using BiometricApp.Models;
using FingerprintWrapper;
using OpenCvSharp;
using System.Text.Json;


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
}