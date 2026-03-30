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

        IMDWrapper iMDWrapper = new IMDWrapper(); 
        // Main function to capture fingerprint
        public async Task<bool> CaptureFingerprintLeft()
        {
            try
            {
                // Reset device
                if (iMDWrapper.DeviceReset() != 0)
                    return false;

                // Initial wait for device readiness
                await Task.Delay(5000);

                int timeout = 20000;   // total timeout
                int interval = 300;    // polling interval
                int elapsed = 0;

                while (elapsed < timeout)
                {
                    // 1. Wait if device is busy
                    if (iMDWrapper.IsScanBusy())
                    {
                        await Task.Delay(interval);
                        elapsed += interval;
                        continue;
                    }

                    // 2. Check if finger is placed
                    bool fingerOn;
                    iMDWrapper.GetImageStatus(out fingerOn);

                    if (!fingerOn)
                    {
                        await Task.Delay(interval);
                        elapsed += interval;
                        continue;
                    }

                    // 3. Stabilization delay (VERY IMPORTANT)
                    await Task.Delay(500);

                    // 4. Double-check device is still ready
                    if (iMDWrapper.IsScanBusy())
                        continue;

                    // 5. Perform scan
                    int res = iMDWrapper.ScanLeftFour();

                    if (res == 0)
                    {
                        // 6. Give device time to finalize image
                        await Task.Delay(2000);

                        string folder = @"C:\Biometric_Finger";

                        if (!Directory.Exists(folder))
                            Directory.CreateDirectory(folder);

                        // 7. Save each finger with unique file name
                        int f1 = iMDWrapper.SaveFileLeftIndexFinger($@"{folder}\l.bmp");
                        int f2 = iMDWrapper.SaveFileLeftMiddleFinger($@"{folder}\l.bmp");
                        int f3 = iMDWrapper.SaveFileLeftRingFinger($@"{folder}\l.bmp");
                        int f4 = iMDWrapper.SaveFileLeftlittleFinger($@"{folder}\l.bmp");

                        return true;
                    }
                    else
                    {
                        // Retry after small delay if scan failed
                        await Task.Delay(500);
                    }

                    elapsed += interval;
                }

                return false;
            }
            catch (Exception ex)
            {
                // Optional: log error
                Console.WriteLine($"Error in CaptureFingerprintLeft: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CaptureFingerprintRight()
        {
            try
            {
                // Reset device
                if (iMDWrapper.DeviceReset() != 0)
                    return false;

                // Initial wait for device readiness
                await Task.Delay(5000);

                int timeout = 20000;   // total timeout
                int interval = 300;    // polling interval
                int elapsed = 0;

                while (elapsed < timeout)
                {
                    // 1. Wait if device is busy
                    if (iMDWrapper.IsScanBusy())
                    {
                        await Task.Delay(interval);
                        elapsed += interval;
                        continue;
                    }

                    // 2. Check if finger is placed
                    bool fingerOn;
                   var r= iMDWrapper.GetImageStatus(out fingerOn);

                    if (!fingerOn)
                    {
                        await Task.Delay(interval);
                        elapsed += interval;
                        continue;
                    }

                    // 3. Stabilization delay (VERY IMPORTANT)
                    await Task.Delay(500);

                    // 4. Double-check device is still ready
                    if (iMDWrapper.IsScanBusy())
                        continue;

                    // 5. Perform scan
                    int res = iMDWrapper.ScanRightFour();

                    if (res == 0)
                    {
                        // 6. Give device time to finalize image
                        await Task.Delay(2000);

                        string folder = @"C:\Biometric_Finger";

                        if (!Directory.Exists(folder))
                            Directory.CreateDirectory(folder);

                        // 7. Save each finger with unique file name

                        int f1 = iMDWrapper.SaveFileRightIndexFinger($@"{folder}\r.bmp");
                        int f2 = iMDWrapper.SaveFileRightMiddleFinger($@"{folder}\r.bmp");
                        int f3 = iMDWrapper.SaveFileRightRingFinger($@"{folder}\r.bmp");
                        int f4 = iMDWrapper.SaveFileRightlittleFinger($@"{folder}\r.bmp");

                        return true;
                    }
                    else
                    {
                        // Retry after small delay if scan failed
                        await Task.Delay(500);
                    }

                    elapsed += interval;
                }

                return false;
            }
            catch (Exception ex)
            {
                // Optional: log error
                Console.WriteLine($"Error in CaptureFingerprintLeft: {ex.Message}");
                return false;
            }
        }
      
        public async Task<bool> CaptureFingerprintThum()
        {
            try
            {
                // Reset device
                if (iMDWrapper.DeviceReset() != 0)
                    return false;

                // Initial wait for device readiness
                await Task.Delay(5000);

                int timeout = 20000;   // total timeout
                int interval = 300;    // polling interval
                int elapsed = 0;

                while (elapsed < timeout)
                {
                    // 1. Wait if device is busy
                    if (iMDWrapper.IsScanBusy())
                    {
                        await Task.Delay(interval);
                        elapsed += interval;
                        continue;
                    }

                    // 2. Check if finger is placed
                    bool fingerOn;
                    iMDWrapper.GetImageStatus(out fingerOn);

                    if (!fingerOn)
                    {
                        await Task.Delay(interval);
                        elapsed += interval;
                        continue;
                    }

                    // 3. Stabilization delay (VERY IMPORTANT)
                    await Task.Delay(500);

                    // 4. Double-check device is still ready
                    if (iMDWrapper.IsScanBusy())
                        continue;

                    // 5. Perform scan
                    int res = iMDWrapper.ScanThumbs();

                    if (res == 0)
                    {
                        // 6. Give device time to finalize image
                        await Task.Delay(2000);

                        string folder = @"C:\Biometric_Finger";

                        if (!Directory.Exists(folder))
                            Directory.CreateDirectory(folder);

                        // 7. Save each finger with unique file name

                        int f1 = iMDWrapper.SaveFileThumbLeftFinger($@"{folder}\l.bmp");
                        int f2 = iMDWrapper.SaveFileThumbRightFinger($@"{folder}\r.bmp");

                        return true;
                    }
                    else
                    {
                        // Retry after small delay if scan failed
                        await Task.Delay(500);
                    }

                    elapsed += interval;
                }

                return false;
            }
            catch (Exception ex)
            {
                // Optional: log error
                Console.WriteLine($"Error in CaptureFingerprintLeft: {ex.Message}");
                return false;
            }
        }
    }

}
