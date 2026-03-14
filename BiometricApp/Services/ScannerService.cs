using BiometricApp.Natives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Bmp;

namespace BiometricApp.Services
{
    public class ScannerService
    {
        private const string FingerprintFolder = "fingerprints";
        private const int SCORE_ARRAY_SIZE=4;
        public bool StartScan2()
        {
            var result = FingerprintSDK.ScanStart();
            return result == 0;
        }

        public bool IsBusy()
        {

            return FingerprintSDK.is_scan_busy();

        }

        public RESULT CancelScan()
        {
            return FingerprintSDK.scan_cancel();
        }


        // Main function to capture fingerprint
        public async Task<bool> CaptureFingerprint()
        {
            //if (!StartScan())
            //    return false;

            // ✅ Wait at least 5 seconds before first image capture attempt
            await Task.Delay(5000);

            int timeout = 10000; // total timeout (after the 5s wait)
            int interval = 500;  // check every 500ms
            int elapsed = 0;

            while (elapsed < timeout)
            {
                ImageProperty imgProp = new ImageProperty
                {
                    score_array = new int[SCORE_ARRAY_SIZE] // use correct SDK size
                };
                RESULT res = FingerprintSDK.get_image(ref imgProp);
                if (res == RESULT.SUCCESS && imgProp.img != IntPtr.Zero)
                {
                    // process image
                    break;
                }

                await Task.Delay(interval);
                elapsed += interval;
            }

            return false;
        }

        //public bool StartScan(int num = 1, GUI_SHOW_MODE mode = GUI_SHOW_MODE.SHOW)
        //{
        //    // Create buffer for results
        //    FINGER_POSITION[] positions = new FINGER_POSITION[num];

        //    RESULT result = FingerprintSDK.scan_start(mode, positions, num);

        //    if (result == RESULT.SUCCESS)
        //    {
        //        // positions now contains scanned fingerprint positions
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}



        private bool GetImage()
        {
            // Get project folder and create fingerprints folder
            string projectRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..");
            projectRoot = Path.GetFullPath(projectRoot);
            string folder = Path.Combine(projectRoot, "fingerprints");
            Directory.CreateDirectory(folder); // create if not exists

            // Generate timestamped filename
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string rawFile = Path.Combine(folder, $"finger_{timestamp}.raw");
            string bmpFile = Path.Combine(folder, $"finger_{timestamp}.bmp");

            // Initialize struct
            ImageProperty imgProp = new ImageProperty
            {
                score_array = new int[SCORE_ARRAY_SIZE] // use correct SDK size
            };

            // Call the DLL
            RESULT result = FingerprintSDK.get_image(ref imgProp);

            if (result == RESULT.SUCCESS)
            {
                // ✅ Check pointer and size BEFORE Marshal.Copy
                if (imgProp.img == IntPtr.Zero || imgProp.width == 0 || imgProp.height == 0)
                {
                    Console.WriteLine("No image data returned from device");
                    return false; // exit safely
                }

                int width = imgProp.width;
                int height = imgProp.height;
                int size = width * height;

                // Copy unmanaged buffer to managed byte array
                byte[] buffer = new byte[size];
                Marshal.Copy(imgProp.img, buffer, 0, size);

                // Save RAW file
                File.WriteAllBytes(rawFile, buffer);

                // Convert RAW -> BMP for viewing
                SaveRawToBmp(buffer, width, height, bmpFile);

                Console.WriteLine($"Fingerprint saved: {bmpFile}");
                return true;
            }

            return false;
        }


        /// Convert RAW grayscale fingerprint to BMP
        /// </summary>
        private void SaveRawToBmp(byte[] raw, int width, int height, string filePath)
        {
            using Image<L8> image = new Image<L8>(width, height);

            // Copy raw data into image
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte value = raw[y * width + x];
                    image[x, y] = new L8(value);
                }
            }

            image.Save(filePath, new BmpEncoder());
        }





    
        enum VERSION
        {
            NFIQ_V1,
            NFIQ_V2,
            NFIQ_VER_SIZE,
        };

        //[StructLayout(LayoutKind.Sequential)]
        //public struct ImageProperty
        //{
        //    public GUI_SHOW_MODE mode;          // probably an int enum
        //    public FINGER_POSITION pos;         // struct
        //    [MarshalAs(UnmanagedType.I1)]
        //    public bool this_scan;              // bool

        //    public IntPtr img;                  // pointer to image buffer
        //    public int width;
        //    public int height;

        //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        //    public int[] score_array;

        //    public int score_size;
        //    public int score_min;
        //    public VERSION score_ver;           // struct
        //}

    }

}
