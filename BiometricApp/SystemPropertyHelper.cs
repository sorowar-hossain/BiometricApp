using BiometricApp.Natives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static BiometricApp.Services.FingerprintService;

namespace BiometricApp
{
    // Helper class to read fixed buffers safely
    public unsafe static class SystemPropertyHelper
    {
        private static string ReadFixedBuffer(byte* buffer, int length)
        {
            int actualLength = 0;
            while (actualLength < length && buffer[actualLength] != 0) actualLength++;
            return System.Text.Encoding.ASCII.GetString(buffer, actualLength);
        }

        public static string GetFirmwareVersion(ref SystemProperty p)
        {
            if (p.fw_ver == null || p.fw_ver.Length < 3) return string.Empty;
            return $"{p.fw_ver[0]}.{p.fw_ver[1]}.{p.fw_ver[2]}";
        }

        public static string GetLibraryVersion(ref SystemProperty p)
        {
            if (p.fap50_lib_ver == null || p.fap50_lib_ver.Length < 3) return string.Empty;
            return $"{p.fap50_lib_ver[0]}.{p.fap50_lib_ver[1]}.{p.fap50_lib_ver[2]}";
        }

        public static string GetProductSN(ref SystemProperty p)
        {
            if (p.product_sn == IntPtr.Zero) return string.Empty;
            return Marshal.PtrToStringAnsi(p.product_sn) ?? string.Empty;
        }

        public static string GetProductModel(ref SystemProperty p)
        {
            if (p.product_model == IntPtr.Zero) return string.Empty;
            return Marshal.PtrToStringAnsi(p.product_model) ?? string.Empty;
        }

        public static string GetProductBrand(ref SystemProperty p)
        {
            if (p.product_brand == IntPtr.Zero) return string.Empty;
            return Marshal.PtrToStringAnsi(p.product_brand) ?? string.Empty;
        }

    }
}
