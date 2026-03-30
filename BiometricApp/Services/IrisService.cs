using FingerprintWrapper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BiometricApp.Services
{
    public class IrisService
    {
        IMDIrisWrapper iris = new IMDIrisWrapper();


        // Attach the iris device
        public void AttachDevice()
        {
            iris.Attach(0, "EyeIrisService");
        }


        // Capture iris images

        public (byte[] leftImage, byte[] rightImage, int leftQuality, int rightQuality) Capture()
        {
            // Initialize variables because 'ref' requires pre-assigned values
            byte[] leftImage = null;
            byte[] rightImage = null;
            int leftQuality = 0;
            int rightQuality = 0;

            // Call the unsafe method using 'ref'
            iris.Capture(ref leftImage, ref rightImage, ref leftQuality, ref rightQuality);

            return (leftImage, rightImage, leftQuality, rightQuality);
        }


        // Save captured images to disk

        public void SaveImages(byte[] leftImage, byte[] rightImage, string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string leftPath = Path.Combine(directoryPath, "left_iris.png");
            string rightPath = Path.Combine(directoryPath, "right_iris.png");


            //IMDIrisWrapper.SaveToFile(leftImage, leftPath);
            // IMDIrisWrapper.SaveToFile(rightImage, rightPath);
            SaveRawToPng(leftImage, leftPath, 640, 480);
            SaveRawToPng(rightImage, rightPath, 640, 480);
        }


        // Identify iris

        public (int leftIndex, int rightIndex, byte[] leftImage, byte[] rightImage) Identify()
        {
            // Initialize variables because 'ref' requires pre-assigned values
            byte[] leftImage = null;
            byte[] rightImage = null;
            int leftIndex = -1;
            int rightIndex = -1;

            // Call unsafe DLL method
            iris.Identify(ref leftImage, ref rightImage, ref leftIndex, ref rightIndex);

            // Return results as a tuple
            return (leftIndex, rightIndex, leftImage, rightImage);
        }

        public static bool SaveRawToPng(byte[] raw, string path, int width, int height)
        {
            if (raw == null || raw.Length == 0)
                return false;

            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format8bppIndexed);

            // Set grayscale palette
            ColorPalette palette = bmp.Palette;
            for (int i = 0; i < 256; i++)
                palette.Entries[i] = System.Drawing.Color.FromArgb(i, i, i);
            bmp.Palette = palette;

            // Copy raw data into bitmap
            BitmapData data = bmp.LockBits(
        new Rectangle(0, 0, width, height),
        ImageLockMode.WriteOnly,
        PixelFormat.Format8bppIndexed);

            Marshal.Copy(raw, 0, data.Scan0, raw.Length);
            bmp.UnlockBits(data);

            bmp.Save(path, System.Drawing.Imaging.ImageFormat.Png);
            bmp.Dispose();

            return true;
        }

    }
}
