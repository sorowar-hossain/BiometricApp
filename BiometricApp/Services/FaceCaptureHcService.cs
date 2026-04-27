
using FingerprintWrapper;
namespace BiometricApp.Services
{
    public class FaceCaptureHcService
    {
        private readonly IMDHighCamera cam = new IMDHighCamera();
        private bool isStarted = false;

        public void StartCamera()
        {
            if (isStarted) return;

            cam.Init();

            int count = cam.GetCameraCount();
            if (count <= 0)
                throw new Exception("No camera found");

            cam.OpenCamera(0, IntPtr.Zero);

            isStarted = true;
        }

        public string CaptureFace()
        {
            if (!isStarted)
                throw new Exception("Camera not started");

            string filePath = Path.Combine(FileSystem.CacheDirectory, "face.jpg");

            //SetResolution(index, resIndex);
            //index → which camera device(e.g., 0 = first camera, 1 = second)
            //resIndex → which resolution option(not width / height directly, but a preset index)
            //resIndex = 0 → 640x480
            //resIndex = 1 → 1280x720
            //resIndex = 2 → 1920x1080
            cam.SetResolution(0, 0);

            //EnableAutoCrop(int index, bool enable)
            //index → camera device
            //enable = true → auto - crop ON
            //enable = false → auto - crop OFF

            cam.EnableAutoCrop(0, true);
            cam.Capture(0, filePath);

            return filePath;
        }

        public void StopCamera()
        {
            try
            {
                cam.CloseCamera(0);
                cam.Uninit();
            }
            catch { }

            isStarted = false;
        }
    }
}
