using FAP50Demo;
using FingerprintWrapper;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.Internal.Vectors;
using OpenCvSharp.Text;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiometricApp.Services
{
    public class SignatureService
    {
        IMDWrapper dev = new IMDWrapper();
        // 1. Start

        public async Task<string> CaptureSignatureAsync()
        {
            try
            {
                // reset device
                imd_fap50.device_reset();

                // create folder
                string folder = @"C:\Signature";
                Directory.CreateDirectory(folder);

                string path = Path.Combine(folder, "sig.png");

                // start signature mode
                FINGER_POSITION pos = FINGER_POSITION.SIGNATURE;

                IMD_RESULT startRes = imd_fap50.scan_start(
                    GUI_SHOW_MODE.SIGN_BY_PEN,
                    ref pos,
                    1);

                if (startRes != IMD_RESULT.SUCCESS)
                {
                    Console.WriteLine($"scan_start failed: {startRes}");
                    return "";
                }

                ImageStatus status = new ImageStatus();

                int timeout = 20000;
                int waited = 0;

                // wait for signing
                while (waited < timeout)
                {
                    IMD_RESULT statusRes =
                        imd_fap50.get_image_status(ref status);

                    if (statusRes == IMD_RESULT.SUCCESS)
                    {
                        if (status.is_signature_done)
                            break;
                    }

                    await Task.Delay(100);
                    waited += 100;
                }

                

                // finalize signature
                imd_fap50.signature(SIGNATURE_ACTION.OK);

                // small delay for sdk finalize
                await Task.Delay(300);

                IMD_RESULT saveRes = imd_fap50.save_file(
                    GUI_SHOW_MODE.SIGN_BY_PEN,
                    FINGER_POSITION.SIGNATURE,
                    path);

                imd_fap50.scan_cancel();

                if (saveRes != IMD_RESULT.SUCCESS)
                {
                    Console.WriteLine($"save_file failed: {saveRes}");
                    return "";
                }

                return path;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                try
                {
                    imd_fap50.scan_cancel();
                }
                catch { }

                return "";
            }
        }

        public static Bitmap U8PtrToBitmap32(IntPtr img, int width, int height)
        {
            // 創建 Mat 指向非託管數據的灰階圖
            //Mat mat = new Mat(height, width, MatType.CV_8UC1, img);//old method
            Mat mat = Mat.FromPixelData(height, width, MatType.CV_8UC1, img);
            //Cv2.ImShow("show image", mat);//dbg

            // 如果需要轉換到 32bpp 格式
            Mat mat32bpp = new Mat();
            Cv2.CvtColor(mat, mat32bpp, ColorConversionCodes.GRAY2BGRA); // 8bpp to 32bpp

            // 將 OpenCV 的 Mat 轉換為 C# 的 Bitmap
            Bitmap bitmap = BitmapConverter.ToBitmap(mat32bpp);
            return bitmap;
        }

        public Task<byte[]> PreviewSignatureAsync()
        {
            if (!imd_fap50.is_scan_busy())
                return Task.FromResult<byte[]>(null);

            ImageStatus img_status = default;
            IMD_RESULT res = imd_fap50.get_image_status(ref img_status);

            if (res != IMD_RESULT.SUCCESS || img_status.img == IntPtr.Zero)
                return Task.FromResult<byte[]>(null);

            Bitmap bmp = U8PtrToBitmap32(img_status.img, 1600, 1000);

            using var ms = new MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return Task.FromResult(ms.ToArray());
        }
    } 
}
