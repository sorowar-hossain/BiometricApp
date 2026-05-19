using FAP50Demo;
using FingerprintWrapper;
using System;
using System.Collections.Generic;
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
                imd_fap50.device_reset();

                ImageStatus status = new ImageStatus();

                // start signature mode
                FINGER_POSITION pos = FINGER_POSITION.SIGNATURE;
                imd_fap50.scan_start(GUI_SHOW_MODE.SIGNATURE, ref pos, 1);

                // wait for user to finish
                int timeout = 20000;
                int waited = 0;

                while (waited < timeout)
                {
                    imd_fap50.get_image_status(ref status);

                    if (status.is_signature_done)
                        break;

                    await Task.Delay(100);
                    waited += 100;
                }

                if (!status.is_signature_done)
                {
                    Console.WriteLine("Signature timeout");
                    return "";
                }

                string path = @"C:\Signature\sig.png";

                var res = imd_fap50.save_file(
                    GUI_SHOW_MODE.SIGNATURE,
                    FINGER_POSITION.SIGNATURE,
                    path
                );

                return res == IMD_RESULT.SUCCESS ? path : "";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return "";
            }
        }
    } 
}
