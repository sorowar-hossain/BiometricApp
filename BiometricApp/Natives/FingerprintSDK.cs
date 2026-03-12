using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BiometricApp.Natives 
{
    // DLL wrapper
    public enum IMD_RESULT
    {
        SUCCESS = 0,
        FAILURE = 1
    }
    public enum GUI_SHOW_MODE
    {
        NONE = 0,
        SHOW = 1,
        // Add other modes from SDK
    }

   
    public enum FINGER_POSITION
    {
        UNKNOWN = 0,
        LEFT_THUMB = 1,
        RIGHT_THUMB = 2
        // Add others if SDK supports more
    }

    public enum RESULT
    {
        SUCCESS = 0,
        ERROR = -1,
        NULL_POINTER = -2
        // Add others from SDK
    }


    //[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)] // default packing
    //public unsafe struct SystemProperty
    //{
    //    public fixed byte product_sn[32];       // product serial number
    //    public fixed byte product_model[32];    // product model
    //    public fixed byte fw_ver[32];           // firmware version
    //    public ushort chip_id;                  // 2-byte chip ID
    //    public fixed byte fap50_lib_ver[32];    // library version
    //}

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public unsafe struct SystemProperty
    {
        //public FINGER_POSITION finger_position;

        public int png_compress_ratio;
        public int jp2_quality;
        public float wsq_bit_rate;
        public IntPtr wsq_comment_text; // char*

        //public VERSION nfiq_ver;
        public int nfiq_score_minimum_acceptable;
        public int nfiq2_score_minimum_acceptable;

        [MarshalAs(UnmanagedType.I1)]
        public bool speech_en;

        public int speech_language;
        public int speech_volume;

        [MarshalAs(UnmanagedType.I1)]
        public bool life_check_en;

        public uint scan_timeout_ms;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public int[] signature_clear_roi;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public int[] signature_confirm_roi;

        public int image_width;
        public int image_height;
        public int image_bit_per_pix;
        public int image_pix_per_inch;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] fap50_lib_ver;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] opencv_lib_ver;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] nbis_lib_ver;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] nfiq2_lib_ver;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] guid;

        public ushort chip_id;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] fw_ver;


        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] led_ver;

        public IntPtr product_sn;    // char*
        public IntPtr product_brand; // char*
        public IntPtr product_model; // char*

        [MarshalAs(UnmanagedType.I1)]
        public bool scan_by_manual;
    }

    public static class FingerprintSDK
    {
        // Change CallingConvention to Cdecl if the DLL uses C calling convention
        //[DllImport("lib_imd_fap50_method.dll", EntryPoint = "get_system_property", CallingConvention = CallingConvention.StdCall)]
        //public static extern IMD_RESULT get_system_property(ref SystemProperty p);

        [DllImport("lib_imd_fap50_method.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool connect_fap50_panel(string host, ushort port);

        [DllImport("lib_imd_fap50_method.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void disconnect_fap50_panel();

        [DllImport("lib_imd_fap50_method.dll", EntryPoint = "device_reset", CallingConvention = CallingConvention.Cdecl)]
        public static extern IMD_RESULT device_reset();

        [DllImport("lib_imd_fap50_method.dll", EntryPoint = "get_system_property", CallingConvention = CallingConvention.Cdecl)]
        public static extern IMD_RESULT get_system_property(ref SystemProperty p);

        [DllImport("Device_Wrapper.dll", EntryPoint = "DeviceReset", CallingConvention = CallingConvention.StdCall)]
        public static extern IMD_RESULT DeviceReset();

        [DllImport("Device_Wrapper.dll", EntryPoint = "ScanStart", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ScanStart();

        //[DllImport("Device_Wrapper.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern int ScanStart();

        [DllImport("Device_Wrapper.dll", EntryPoint = "IsScanBusy", CallingConvention = CallingConvention.StdCall)]
        public static extern int IsScanBusy();


        [DllImport("lib_imd_fap50_method.dll", EntryPoint = "scan_start", CallingConvention = CallingConvention.StdCall)]
        public static extern RESULT scan_start(
          GUI_SHOW_MODE mode,
          [In, Out] FINGER_POSITION[] pos_buf,
          int num
      );

        [DllImport("lib_imd_fap50_method.dll", EntryPoint = "is_scan_busy", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool is_scan_busy();

        [DllImport("lib_imd_fap50_method.dll", EntryPoint = "scan_cancel", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT scan_cancel();



    }
}
