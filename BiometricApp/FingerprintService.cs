
using BiometricApp.Models;
using System;
using System.Runtime.InteropServices;
namespace BiometricApp;
public class FingerprintService
{
    public string OpenDevice()
    {
        SystemProperty p = default;

        IMD_RESULT res = imd_fap50.get_system_property(ref p);

        if (res != IMD_RESULT.SUCCESS)
        {
            res = imd_fap50.device_reset();
            if (res != IMD_RESULT.SUCCESS)
            {
                return $"Reset failed: {res}";
            }
        }

        imd_fap50.get_system_property(ref p);

        string productSN = Marshal.PtrToStringAnsi(p.product_sn);
        string productModel = Marshal.PtrToStringAnsi(p.product_model);

        //unsafe
        //{
        //    return
        //        //$"FW: {(char)p.fw_ver[2]}{p.fw_ver[1]:X}.{p.fw_ver[0]:X2}\n" +
        //        //$"Chip ID: 0x{p.chip_id:X4}\n" +
        //        $"SDK: {p.fap50_lib_ver[2]}.{p.fap50_lib_ver[1]}.{p.fap50_lib_ver[0]}\n" +
        //        $"S/N: {productSN}\n" +
        //        $"Model: {productModel}";
        //}

        return "";
    }

    public enum IMD_RESULT
    {
        SUCCESS = 0,
        FAIL = -1
    }

    public static class imd_fap50
    {
        [DllImport("imd_fap50.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IMD_RESULT get_system_property(ref SystemProperty prop);

        [DllImport("imd_fap50.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IMD_RESULT device_reset();
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct SystemProperty
    {
        public int image_width;
        public int image_height;
        public int image_bit_per_pix;
        public int image_pix_per_inch;

        public fixed Byte fap50_lib_ver[3];
        //public fixed BYTE opencv_lib_ver[3];
        //public fixed BYTE nbis_lib_ver[3];
        //public fixed BYTE nfiq2_lib_ver[3];

        //public fixed BYTE guid[16];

        //public WORD chip_id;

        //public fixed BYTE fw_ver[3];   // 🔹 THIS FIELD WAS MISSING
        //public fixed BYTE led_ver[2];

        public IntPtr product_sn;
        public IntPtr product_brand;
        public IntPtr product_model;
    }
}
