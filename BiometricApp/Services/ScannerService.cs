using BiometricApp.Natives;
using FingerprintWrapper;
using Microsoft.Maui.Controls;
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
    public class ScannerService
    {
        private const string FingerprintFolder = "fingerprints";
        private const int SCORE_ARRAY_SIZE=4;
        IMDWrapper iMDWrapper = new IMDWrapper(); 
      

        // Main function to capture fingerprint
        public async Task<bool> CaptureFingerprintLeft() 
        {
            if (iMDWrapper.DeviceReset() !=0)
                return false;

            // ✅ Wait at least 5 seconds before first image capture attempt
            await Task.Delay(5000);

            int timeout = 10000; // total timeout (after the 5s wait)
            int interval = 200;  // check every 500ms
            int elapsed = 0;

            while (elapsed < timeout)
            {
                // ✅ Check if device is busy
                if (iMDWrapper.IsScanBusy())
                {
                    await Task.Delay(interval);
                    elapsed += interval;
                    continue;
                }

                bool fingerOn;
                iMDWrapper.GetImageStatus(out fingerOn);

                if (!fingerOn)
                {
                    await Task.Delay(interval);
                    elapsed += interval;
                    continue;
                }
                int res = iMDWrapper.ScanLeftFour();


                if (res ==0)
                {
                    // process image

                    string folder = @"C:\Biometric_Finger";

                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    int s1=  iMDWrapper.SaveFile($@"{folder}\left_index.bmp");
                    iMDWrapper.SaveFile($@"{folder}\left_middle.bmp");
                    iMDWrapper.SaveFile($@"{folder}\left_ring.bmp");
                    iMDWrapper.SaveFile($@"{folder}\left_little.bmp");

                  
                    return true;
                }

                await Task.Delay(interval);
                elapsed += interval;
            }

            return false;
        }

        public async Task<bool> CaptureFingerprintRight() 
        {
            if (iMDWrapper.DeviceReset() != 0)
                return false;

            // ✅ Wait at least 5 seconds before first image capture attempt
            await Task.Delay(5000);

            int timeout = 10000; // total timeout (after the 5s wait)
            int interval = 200;  // check every 500ms
            int elapsed = 0;

            while (elapsed < timeout)
            {
                // ✅ Check if device is busy
                if (iMDWrapper.IsScanBusy())
                {
                    await Task.Delay(interval);
                    elapsed += interval;
                    continue;
                }

                bool fingerOn;
                iMDWrapper.GetImageStatus(out fingerOn);

                if (!fingerOn)
                {
                    await Task.Delay(interval);
                    elapsed += interval;
                    continue;
                }
                int res = iMDWrapper.ScanRightFour();


                if (res == 0)
                {
                    // process image
                    string folder = @"C:\Biometric_Finger";
                   // string folder = @"C:\Users\mohsi\Downloads";

                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    int s1 = iMDWrapper.SaveFile($@"{ folder}\right_index.bmp");
                    iMDWrapper.SaveFile($@"{folder}\right_middle.bmp");
                    iMDWrapper.SaveFile($@"{folder}\right_ring.bmp");
                    iMDWrapper.SaveFile($@"{folder}\right_little.bmp");


                    return true;
                }

                await Task.Delay(interval);
                elapsed += interval;
            }

            return false;
        }
    }

}
