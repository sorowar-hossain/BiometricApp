using FingerprintWrapper;

#if WINDOWS
using WinRT.Interop;
using Microsoft.UI.Xaml;
#endif

namespace BiometricApp.Services
{
    public class FaceCaptureHcService
    {
        private readonly IMDHighCamera cam = new IMDHighCamera();
        private bool isStarted = false;

        private int cameraIndex = 0;
        private IntPtr hwnd;
        private CancellationTokenSource _previewCts;
        private string _latestBase64;

#if WINDOWS
        public void Initialize(IntPtr windowHandle)
        {
            hwnd = windowHandle;
        }
#endif

        public void StartCamera()
        {
            if (isStarted) return;

            cam.Init();

            int count = cam.GetCameraCount();
            if (count <= 0)
                throw new Exception("No camera found");

#if WINDOWS
    var window = (Microsoft.UI.Xaml.Window)App.Current.Windows[0].Handler.PlatformView;
    var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

    cam.OpenCamera(cameraIndex, hwnd);
#endif

            isStarted = true;

            StartPreviewSync(); // 🔥 start base64 sync
        }

        public string CaptureFace()
        {
            _previewCts?.Cancel();
            if (!isStarted)
                throw new Exception("Camera not started");

            string filePath = Path.Combine(
                FileSystem.CacheDirectory,
                $"{DateTime.Now:yyyyMMdd_HHmmss}_face.jpg"
            );

            cam.SetResolution(cameraIndex, 0); // 1280x720 recommended
            cam.EnableAutoCrop(cameraIndex, true);

            cam.Capture(cameraIndex, filePath);

            return cam.CaptureBase64(cameraIndex);
        }

        public void StopCamera()
        {
            try
            {
                _previewCts?.Cancel();

                cam.CloseCamera(cameraIndex);
                cam.Uninit();
            }
            catch { }

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
                            _latestBase64 = cam.CaptureBase64(cameraIndex);
                        }
                    }
                    catch
                    {
                        // ignore frame errors (camera busy etc.)
                    }

                    await Task.Delay(300, token); // adjust: 300–800ms
                }
            }, token);
        }
        public string GetLiveBase64()
        {
            return _latestBase64;
        }
    }
}