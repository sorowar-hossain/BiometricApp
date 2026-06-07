using BiometricApp.Models;
using FingerprintWrapper;
using OpenCvSharp;
using System.Text.Json;
using FaceONNX;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Windows.Media.FaceAnalysis;
using System.Drawing;

using OpenCvSharp.Extensions;
using Windows.Perception.People;




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
    public static double _GenderMScore = 0.0;
    public static double _GenderFScore = 0.0;
    public static double _AngryEmoScore = 0.0;
    public static double _SadEmoScore = 0.0;
    public static double _HappyEmoScore = 0.0;
    public static double _SurpriseEmoScore = 0.0;

    public static InferenceSession _emotion;
    public static InferenceSession _gender;
    public static FaceONNX.FaceDetector _detector;


    public static double sharpness = 0.0;
    public static double brightness = 0.0;
    public static double contrast = 0.0;

    public FaceCaptureHcService()
    {
        _detector = new FaceONNX.FaceDetector();
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
        
        var face = Cv2.ImDecode(imgStr, ImreadModes.Grayscale);
        
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

        cam.ZoomIn(cameraIndex);

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
            RefreshPreview();
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

    public static double GetSharpnessScore(Mat gray)
    {
        using var lap = new Mat();

        Cv2.Laplacian(gray, lap, MatType.CV_64F);

        Cv2.MeanStdDev(lap, out _, out Scalar stddev);

        double variance = stddev.Val0 * stddev.Val0;

        const double maxSharpness = 500;

        return Math.Min(100,
            variance / maxSharpness * 100);
    }
    public static double GetBrightnessScore(Mat gray)
    {
        Scalar mean = Cv2.Mean(gray);

        double brightness = mean.Val0;

        double ideal = 128;

        double diff = Math.Abs(brightness - ideal);

        return Math.Max(0,
            100 - diff / 128 * 100);
    }
    public static double GetContrastScore(Mat gray)
    {
        Cv2.MeanStdDev(gray, out _, out Scalar stddev);

        double contrast = stddev.Val0;

        return Math.Min(100,
            contrast / 60.0 * 100);
    }
    public static double GetFaceSizeScore(OpenCvSharp.Rect faceRect, OpenCvSharp.Size imageSize)
    {
        double ratio =
            (double)faceRect.Height /
            imageSize.Height;

        double ideal = 0.7;

        double diff = Math.Abs(ratio - ideal);

        return Math.Max(0,
            100 - diff / ideal * 100);
    }
    public static double GetCenterScore(OpenCvSharp.Rect faceRect, OpenCvSharp.Size imgSize)
    {
        double cx = faceRect.X + faceRect.Width / 2.0;
        double cy = faceRect.Y + faceRect.Height / 2.0;

        double dx = Math.Abs(cx - imgSize.Width / 2.0);
        double dy = Math.Abs(cy - imgSize.Height / 2.0);

        double dist =
            Math.Sqrt(dx * dx + dy * dy);

        double maxDist =
            Math.Sqrt(
                imgSize.Width * imgSize.Width +
                imgSize.Height * imgSize.Height) / 2;

        return Math.Max(0,
            100 - dist / maxDist * 100);
    }

    public static double CalculateQualityPercentage(byte[] imageBytes)
    {
        using var img = Cv2.ImDecode(imageBytes, ImreadModes.Grayscale);

        sharpness = GetSharpnessScore(img);
        brightness = GetBrightnessScore(img);
        contrast = GetContrastScore(img);

        // Normalize weights to 100%
        double quality =
              sharpness * 0.50
            + brightness * 0.30
            + contrast * 0.20;

        return Math.Round(quality, 2);
    }
    
    public static DenseTensor<float> PreprocessGender(byte[] imageBytes)
    {
        using var face = Cv2.ImDecode(imageBytes, ImreadModes.Grayscale);
        var resized = new Mat();
        Cv2.Resize(face, resized, new OpenCvSharp.Size(224, 224));

        var rgb = new Mat();
        Cv2.CvtColor(resized, rgb, ColorConversionCodes.BGR2RGB);

        var tensor = new DenseTensor<float>(new[] { 1, 3, 224, 224 });

        for (int y = 0; y < 224; y++)
        {
            for (int x = 0; x < 224; x++)
            {
                var pixel = rgb.At<Vec3b>(y, x);

                tensor[0, 0, y, x] = pixel.Item0 / 255f; // R
                tensor[0, 1, y, x] = pixel.Item1 / 255f; // G
                tensor[0, 2, y, x] = pixel.Item2 / 255f; // B
            }
        }

        return tensor;
    }
    public static string GetGender(Mat face)
    {
        Cv2.ImEncode(".jpg", face, out byte[] imageBytes);
        var tensor = PreprocessGender(imageBytes);

        var inputs = new List<NamedOnnxValue>
    {
        NamedOnnxValue.CreateFromTensor(
    _gender.InputMetadata.Keys.First(),
    tensor)
    };

        using var result = _gender.Run(inputs);

        var output = result.First().AsEnumerable<float>().ToArray();

        return output[0] > 0.5 ? "Male" : "Female";
    }
}