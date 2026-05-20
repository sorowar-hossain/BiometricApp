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
                // reset device
                imd_fap50.device_reset();

                // create folder
                string folder = @"C:\Signature";
                Directory.CreateDirectory(folder);

                string path = Path.Combine(folder, "sig.png");

                // start signature mode
                FINGER_POSITION pos = FINGER_POSITION.SIGNATURE;

                IMD_RESULT startRes = imd_fap50.scan_start(
                    GUI_SHOW_MODE.SIGNATURE,
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
                    GUI_SHOW_MODE.SIGNATURE,
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
    } 
}
