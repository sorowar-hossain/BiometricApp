using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BiometricApp.Models
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct SystemProperty
    {
        public int png_compress_ratio;
        public int jp2_quality;

        public float wsq_bit_rate;

        public IntPtr product_sn;
        public IntPtr product_model;
    }
}
