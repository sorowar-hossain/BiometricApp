using BiometricApp.Natives;
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
        public bool StartScan2()
        {
            var result = FingerprintSDK.ScanStart(); 
            return result == 0;
        }

        public bool IsBusy()
        {
           
             return FingerprintSDK.is_scan_busy();
           
        }

        public RESULT CancelScan()
        {
            return FingerprintSDK.scan_cancel();
        }


        public bool StartScan(int num = 1, GUI_SHOW_MODE mode = GUI_SHOW_MODE.SHOW)
        {
            // Create buffer for results
            FINGER_POSITION[] positions = new FINGER_POSITION[num];

            RESULT result = FingerprintSDK.scan_start(mode, positions, num);

            if (result == RESULT.SUCCESS)
            {
                // positions now contains scanned fingerprint positions
                foreach (var pos in positions)
                {
                    //Console.WriteLine($"Finger: x={pos.x}, y={pos.y}, w={pos.width}, h={pos.height}");
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
