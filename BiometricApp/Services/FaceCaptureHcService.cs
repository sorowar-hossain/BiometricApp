using FingerprintWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            cam.SetResolution(0, 0);
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
