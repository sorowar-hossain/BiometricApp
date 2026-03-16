using Microsoft.Maui.Controls.PlatformConfiguration;
using System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;
namespace BiometricApp.Services

{
    public class Fap50FingerprintService
    {
        const string FOLDER = "fingerprints";

        #region ENUMS

        public enum GUI_SHOW_MODE
        {
            NONE,
            CAPTURE,
            ROLL,
            FLAT,
            SIGNATURE,
            SIGN_BY_PEN,
            SIZE
        }

        public enum FINGER_POSITION
        {
            UNKNOW_FINGER = 0,
            RIGHT_THUMB,
            RIGHT_INDEX,
            RIGHT_MIDDLE,
            RIGHT_RING,
            RIGHT_LITTLE,
            LEFT_THUMB,
            LEFT_INDEX,
            LEFT_MIDDLE,
            LEFT_RING,
            LEFT_LITTLE,
            RIGHT_FOUR = 13,
            LEFT_FOUR,
            BOTH_THUMBS,
            SOME_FINGERS,
            SIGNATURE,
            RIGHT_FULL,
            LEFT_FULL,
            SIZE
        }

        public enum IMD_RESULT
        {
            SUCCESS = 0,
            SCAN_THREAD_START = 0x101,
            SCAN_THREAD_END = 0x103,
            NO_ANY_FINGER = 0x20D,
            NO_AVAILABLE_IMAGE = 0x209
        }

        public enum NFIQ_VERSION
        {
            V1,
            V2,
            SIZE
        }

        #endregion

        #region STRUCTS

        [StructLayout(LayoutKind.Sequential)]
        public struct ImageProperty2
        { 
            public GUI_SHOW_MODE mode;
            public FINGER_POSITION pos;

            [MarshalAs(UnmanagedType.U1)]
            public bool this_scan;

            public IntPtr img;
            public int width;
            public int height;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public int[] score_array;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public int[] spoofing_array;

            public int score_size;
            public int score_min;
            public NFIQ_VERSION score_ver;
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct ImageProperty
        {
            public GUI_SHOW_MODE mode;
            public FINGER_POSITION pos;

            [MarshalAs(UnmanagedType.U1)]
            public bool this_scan;

            public IntPtr img;  // DLL allocates

            public int width;
            public int height;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public int[] score_array;

            public int score_size;
            public int score_min;
            public NFIQ_VERSION score_ver;
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct ImageStatus
        {
            public GUI_SHOW_MODE show_mode;
            public FINGER_POSITION finger_position;

            [MarshalAs(UnmanagedType.U1)]
            public bool is_roll_init;

            [MarshalAs(UnmanagedType.U1)]
            public bool is_roll_done;

            [MarshalAs(UnmanagedType.U1)]
            public bool is_flat_init;

            [MarshalAs(UnmanagedType.U1)]
            public bool is_flat_done;

            public int finger_num;

            [MarshalAs(UnmanagedType.U1)]
            public bool is_finger_on;

            public IntPtr contours;

            public float frame_rate;

            public IntPtr img;

            [MarshalAs(UnmanagedType.U1)]
            public bool is_signature_done;

            public IMD_RESULT result;
        }

        #endregion

        #region DLL IMPORTS

        [DllImport("lib_imd_fap50_method.dll", CallingConvention = CallingConvention.StdCall)]
        static extern IMD_RESULT device_reset();

        [DllImport("lib_imd_fap50_method.dll", CallingConvention = CallingConvention.StdCall)]
        static extern IMD_RESULT scan_start(GUI_SHOW_MODE mode, ref FINGER_POSITION pos, int num);

        [DllImport("lib_imd_fap50_method.dll", CallingConvention = CallingConvention.StdCall)]
        static extern IMD_RESULT get_image_status(ref ImageStatus status);

        [DllImport("lib_imd_fap50_method.dll", CallingConvention = CallingConvention.StdCall)]
        static extern IMD_RESULT get_image(ref ImageProperty img);

        #endregion

        public async Task<string?> CaptureFingerprint()
        {
            var reset = device_reset();

            if (reset != IMD_RESULT.SUCCESS)
                return null;

            FINGER_POSITION finger = FINGER_POSITION.LEFT_INDEX;
            FINGER_POSITION pos = FINGER_POSITION.LEFT_FOUR;
            var start = scan_start(GUI_SHOW_MODE.FLAT, ref pos, 1);

            if (start != IMD_RESULT.SUCCESS)
                return null;

            ImageStatus status = new ImageStatus();

            // wait for capture
            while (true)
            {
                var res = get_image_status(ref status);

                if (res != IMD_RESULT.SUCCESS)
                {
                    Console.WriteLine($"Status error: {res}");
                    break;
                }

                // Debug information
                Console.WriteLine($"FingerOn: {status.is_finger_on}  FlatDone: {status.is_flat_done}");
                // is_flat_done = false     The scanner has not finished capturing a valid flat image yet
                 //   is_finger_on = true The scanner detects a finger touching the glass

                if (status.is_flat_done)
                {
                    Console.WriteLine("Capture completed");
                    break;
                }

                await Task.Delay(500);
            }

            ImageProperty img = new ImageProperty 
            {
                score_array = new int[4]
            };

            var result = get_image(ref img);

            if (result != IMD_RESULT.SUCCESS)
                return null;

            if (img.img == IntPtr.Zero)
               return null;

            return SaveFingerprint(img);
        }

        private string SaveFingerprint(ImageProperty img)
        {
            int size = img.width * img.height;

            byte[] buffer = new byte[size];

            Marshal.Copy(img.img, buffer, 0, size);

            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                FOLDER);

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string file = Path.Combine(
                folder,
                $"finger_{DateTime.Now:yyyyMMdd_HHmmss}.raw");

            File.WriteAllBytes(file, buffer);

            return file;
        }
    }
}

