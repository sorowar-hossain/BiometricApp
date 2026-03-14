using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BiometricApp.Services
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Formats.Bmp;

    public class ImageService
    {
        private const int SCORE_ARRAY_SIZE = 4; // match DLL
        private const string FINGERPRINT_FOLDER = "fingerprints";

        // ------------------- DLL Imports -------------------
        [DllImport("lib_imd_fap50_method.dll", EntryPoint = "scan_start", CallingConvention = CallingConvention.Cdecl)]
        private static extern RESULT scan_start(GUI_SHOW_MODE mode, [In, Out] FingerRect[] positions, int num);

        [DllImport("lib_imd_fap50_method.dll", EntryPoint = "get_image", CallingConvention = CallingConvention.Cdecl)]
        private static extern RESULT get_image(ref ImageProperty img_property);

        // ------------------- Structs -------------------

        // Struct returned by scan_start with coordinates
        [StructLayout(LayoutKind.Sequential)]
        public struct FingerRect
        {
            public int x, y, width, height;
        }

        // Version enum
        public enum VERSION
        {
            NFIQ_V1 = 0,
            NFIQ_V2 = 1,
            NFIQ_VER_SIZE = 2
        }

        // Logical finger identifier enum (DLL FINGER_POSITION)
        public enum FINGER_POSITION
        {
            FINTER_POSITION_UNKNOW_FINGER = 0,
            FINGER_POSITION_RIGHT_THUMB,
            FINGER_POSITION_RIGHT_INDEX,
            FINGER_POSITION_RIGHT_MIDDLE,
            FINGER_POSITION_RIGHT_RING,
            FINGER_POSITION_RIGHT_LITTLE,
            FINGER_POSITION_LEFT_THUMB,
            FINGER_POSITION_LEFT_INDEX,
            FINGER_POSITION_LEFT_MIDDLE,
            FINGER_POSITION_LEFT_RING,
            FINGER_POSITION_LEFT_LITTLE,
            FINGER_POSITION_RIGHT_FOUR = 13,
            FINGER_POSITION_LEFT_FOUR,
            FINGER_POSITION_BOTH_THUMBS,
            FINGER_POSITION_SOME_FINGERS,
            FINGER_POSITION_SIGNATURE,
            FINGER_POSITION_RIGHT_FULL,
            FINGER_POSITION_LEFT_FULL,
            FINGER_POSITION_SIZE
        }

        // GUI mode enum exactly like DLL
        public enum GUI_SHOW_MODE
        {
            GUI_SHOW_MODE_NONE = 0,
            GUI_SHOW_MODE_CAPTURE = 1,          // scan fingerprints immediately
            GUI_SHOW_MODE_ROLL = 2,
            GUI_SHOW_MODE_FLAT = 3,
            GUI_SHOW_MODE_SIGNATURE = 4,
            GUI_SHOW_MODE_SIGNATURE_BY_PEN = 5
        }

        // RESULT enum
        public enum RESULT
        {
            SUCCESS = 0,
            FAILURE = 1
        }

        // Struct returned by get_image
        [StructLayout(LayoutKind.Sequential)]
        public struct ImageProperty
        {
            public GUI_SHOW_MODE mode;
            public FINGER_POSITION pos;

            [MarshalAs(UnmanagedType.I1)]
            public bool this_scan;

            public IntPtr img;      // pointer to image buffer
            public int width;
            public int height;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = SCORE_ARRAY_SIZE)]
            public int[] score_array;

            public int score_size;
            public int score_min;
            public VERSION score_ver;
        }

        // ------------------- Public Methods -------------------

        public bool StartScan()
        {
            FingerRect[] positions = new FingerRect[1];
            RESULT result = scan_start(GUI_SHOW_MODE.GUI_SHOW_MODE_CAPTURE, positions, 1);
            return result == RESULT.SUCCESS;
        }

        public async Task<bool> CaptureFingerprint() 
        {
            if (!StartScan())
                return false;

            // Wait for the user to place a finger
            await Task.Delay(5000); // wait at least 5 seconds

            int timeout = 15000; // 15 seconds max
            int interval = 500;  // check every 500ms
            int elapsed = 0;

            while (elapsed < timeout)
            {
                ImageProperty imgProp = new ImageProperty
                {
                    score_array = new int[SCORE_ARRAY_SIZE]
                };

                RESULT res = get_image(ref imgProp);

                if (res == RESULT.SUCCESS && imgProp.img != IntPtr.Zero && imgProp.width > 0 && imgProp.height > 0)
                {
                    SaveFingerprint(imgProp);
                    return true;
                }

                await Task.Delay(interval);
                elapsed += interval;
            }

            return false; // timeout without a valid image
        }

        // ------------------- Private Methods -------------------

        private void SaveFingerprint(ImageProperty imgProp)
        {
            int width = imgProp.width;
            int height = imgProp.height;
            int size = width * height;

            byte[] buffer = new byte[size];
            Marshal.Copy(imgProp.img, buffer, 0, size);

            // Ensure folder exists
            string projectRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..");
            projectRoot = Path.GetFullPath(projectRoot);
            string folder = Path.Combine(projectRoot, FINGERPRINT_FOLDER);
            Directory.CreateDirectory(folder);

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string rawFile = Path.Combine(folder, $"finger_{timestamp}.raw");
            string bmpFile = Path.Combine(folder, $"finger_{timestamp}.bmp");

            // Save RAW
            File.WriteAllBytes(rawFile, buffer);

            // Convert RAW -> BMP
            SaveRawToBmp(buffer, width, height, bmpFile);

            Console.WriteLine($"Fingerprint saved: {bmpFile}");
        }

        private void SaveRawToBmp(byte[] raw, int width, int height, string filePath)
        {
            using Image<L8> image = new Image<L8>(width, height);

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
    }
}

