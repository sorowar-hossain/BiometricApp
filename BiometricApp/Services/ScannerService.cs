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

                    int f1=  iMDWrapper.SaveFileLeftIndexFinger($@"{folder}\left_index.bmp");
                    int f2 = iMDWrapper.SaveFileLeftMiddleFinger($@"{folder}\left_middle.bmp");
                    int f3 = iMDWrapper.SaveFileLeftRingFinger($@"{folder}\left_ring.bmp");
                    int f4 = iMDWrapper.SaveFileLeftlittleFinger($@"{folder}\left_little.bmp"); 

                  
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

                    int f1 = iMDWrapper.SaveFileRightIndexFinger( $@"{folder}\right_index.bmp");
                    int f2 = iMDWrapper.SaveFileRightMiddleFinger( $@"{folder}\right_middle.bmp");
                    int f3 = iMDWrapper.SaveFileRightRingFinger( $@"{folder}\right_ring.bmp");
                    int f4 = iMDWrapper.SaveFileRightlittleFinger( $@"{folder}\right_little.bmp");


                    return true;
                }

                await Task.Delay(interval);
                elapsed += interval;
            }

            return false;
        }

        public async Task<bool> CaptureFingerprintThum() 
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
                int res = iMDWrapper.ScanThumbs();


                if (res == 0)
                {
                    // process image
                    string folder = @"C:\Biometric_Finger";
                    // string folder = @"C:\Users\mohsi\Downloads";

                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    int f1  = iMDWrapper.SaveFileThumbLeftFinger($@"{folder}\thum_left.bmp");
                    int f2 = iMDWrapper.SaveFileThumbRightFinger($@"{folder}\thum_right.bmp"); 
                  


                    return true;
                }

                await Task.Delay(interval);
                elapsed += interval;
            }

            return false;
        }
    }

}
