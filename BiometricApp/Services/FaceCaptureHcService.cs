using BiometricApp.Models;
using FingerprintWrapper;
using OpenCvSharp;
using System.Text.Json;
using FaceONNX;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

#if WINDOWS
using WinRT.Interop;
#endif

namespace BiometricApp.Services;

public class FaceCaptureHcService
{
    private readonly IMDHighCamera cam = new();

    private bool isStarted = false;

    private int cameraIndex = 0;

    private string _latestBase64;

    private CancellationTokenSource _previewCts;

    private bool _autoCropEnabled = false;

    public static double _ImgQuality = 0.0;

    private readonly InferenceSession _detector;
    private readonly InferenceSession _emotion;
    private readonly InferenceSession _gender;


    public FaceCaptureHcService()
    {
        _detector = new InferenceSession(Path.Combine(AppContext.BaseDirectory, "models", "face_detection.onnx"));
        _emotion = new InferenceSession(Path.Combine(AppContext.BaseDirectory, "models", "emotion.onnx"));
        _gender = new InferenceSession(Path.Combine(AppContext.BaseDirectory, "models", "gender.onnx"));
    }

    public void StartCamera()
    {
        if (isStarted)
            return;

        cam.Init();

        int count = cam.GetCameraCount();

        if (count <= 0)
            throw new Exception("No camera found");

#if WINDOWS
        var window =
            (Microsoft.UI.Xaml.Window)App.Current.Windows[0]
            .Handler.PlatformView;

        var hwnd =
            WindowNative.GetWindowHandle(window);

        cam.OpenCamera(cameraIndex, hwnd);
#endif

        isStarted = true;

        StartPreviewSync();
    }

    public void StopCamera()
    {
        try
        {
            _previewCts?.Cancel();

            cam.CloseCamera(cameraIndex);

            cam.Uninit();
        }
        catch
        {
        }

        isStarted = false;
    }

    private void StartPreviewSync()
    {
        _previewCts?.Cancel();

        _previewCts = new CancellationTokenSource();

        var token = _previewCts.Token;

        Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (isStarted)
                    {
                        _latestBase64 =
                            cam.CaptureBase64(cameraIndex);
                    }
                }
                catch
                {
                }

                await Task.Delay(1000, token);
            }
        }, token);
    }

    public string GetLiveBase64()
    {
        return _latestBase64;
    }

    // =============================
    // CAPTURE
    // =============================

    public string CaptureFace()
    {
        if (!isStarted)
            throw new Exception("Camera not started");

        cam.EnableAutoCrop(1, _autoCropEnabled);
        var imgStr = Convert.FromBase64String(cam.CaptureBase64(cameraIndex));
        _ImgQuality = CalculateQualityPercentage(imgStr);
        var face = Cv2.ImDecode(imgStr, ImreadModes.Grayscale);
        GetEmotion(face);
        GetGender(face);
        return cam.CaptureBase64(cameraIndex);
    }

    // =============================
    // AUTO CROP
    // =============================

    public void SetAutoCrop(bool enabled)
    {
        _autoCropEnabled = enabled;
    }

    // =============================
    // ROTATE
    // =============================

    public void RotateLeft()
    {
        if (!isStarted)
            return;

        cam.RotateLeft(cameraIndex);

        RefreshPreview();
    }

    public void RotateRight()
    {
        if (!isStarted)
            return;

        cam.RotateRight(cameraIndex);

        RefreshPreview();
    }

    // =============================
    // ZOOM
    // =============================

    public void ZoomIn()
    {
        if (!isStarted)
            return;

        cam.ZoomIn(0);

        RefreshPreview();
    }

    public void ZoomOut()
    {
        if (!isStarted)
            return;

        cam.ZoomOut(cameraIndex);

        RefreshPreview();
    }

    // =============================
    // RESOLUTION
    // =============================


    public void SetResolution(int resIndex)
    {
        if (!isStarted)
            return;

        cam.SetResolution(cameraIndex, resIndex);

        try
        {
            _latestBase64 =
                cam.CaptureBase64(cameraIndex);
        }
        catch
        {
        }
    }

    // =============================
    // INTERNAL
    // =============================

    private void RefreshPreview()
    {
        try
        {
            _latestBase64 =
                cam.CaptureBase64(cameraIndex);
        }
        catch
        {
        }
    }

    public async Task<string> SaveFace(string faceImageBytes, string path)
    {
        if (faceImageBytes != null)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string jsonPath = Path.Combine(path, "face.json");

            FaceData irisData = new FaceData
            {
                FaceImage = faceImageBytes

            };

            string json = JsonSerializer.Serialize(
                irisData,
                new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText(jsonPath, json);
            return jsonPath;
        }
        return "";
    }

    public double CalculateQualityPercentage(byte[] imageBytes)
    {
        Mat img = Cv2.ImDecode(imageBytes, ImreadModes.Grayscale);

        Mat laplacian = new();
        Cv2.Laplacian(img, laplacian, MatType.CV_64F);

        Cv2.MeanStdDev(laplacian, out _, out Scalar stddev);

        double variance = stddev.Val0 * stddev.Val0;

        // Adjust based on your camera testing
        const double maxSharpness = 500.0;

        double percentage = Math.Min(100.0,
            (variance / maxSharpness) * 100.0);

        return Math.Round(percentage, 2);
    }
    private DenseTensor<float> Preprocess(byte[] imageBytes)
    {
        using var face = Cv2.ImDecode(imageBytes, ImreadModes.Grayscale);

        using var resized = new Mat();
        Cv2.Resize(face, resized, new OpenCvSharp.Size(64, 64));

        var tensor = new DenseTensor<float>(new[] { 1, 1, 64, 64 });

        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                tensor[0, 0, y, x] =
                    resized.At<byte>(y, x) / 255.0f;
            }
        }

        return tensor;
    }
    private string GetEmotion(Mat face)
    {
        Cv2.ImEncode(".jpg", face, out byte[] imageBytes);
        var tensor = Preprocess(imageBytes);

        var inputs = new List<NamedOnnxValue>
    {
        NamedOnnxValue.CreateFromTensor("input", tensor)
    };

        using var result = _emotion.Run(inputs);

        var output = result.First().AsEnumerable<float>().ToArray();

        int maxIndex = Array.IndexOf(output, output.Max());

        string[] labels = { "Angry", "Happy", "Sad", "Surprise", "Neutral" };

        return labels[maxIndex];
    }
    private string GetGender(Mat face)
    {
        Cv2.ImEncode(".jpg", face, out byte[] imageBytes);
        var tensor = Preprocess(imageBytes);

        var inputs = new List<NamedOnnxValue>
    {
        NamedOnnxValue.CreateFromTensor("input", tensor)
    };

        using var result = _gender.Run(inputs);

        var output = result.First().AsEnumerable<float>().ToArray();

        return output[0] > 0.5 ? "Male" : "Female";
    }
}