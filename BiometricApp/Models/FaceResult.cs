using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiometricApp.Models
{
    public class FaceResult
    {
        public OpenCvSharp.Rect Rect { get; set; }
        public Mat Face { get; set; } = new();
    }
}
