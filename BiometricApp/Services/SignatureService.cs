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
        IMDWrapper dev =new IMDWrapper();
        // 1. Start

        public async Task<string> CaptureSignatureAsync()
        {
           int start= dev.StartSignature();

            bool done = false;

            while (!done)
            {
                // Example: check device status
                // dev.GetSignatureStatus(out done);

                await Task.Delay(200); // VERY IMPORTANT (non-blocking)
            }
            int isConfirm = dev.ConfirmSignature();

            string path = "C:\\sign.png";
            int isSaved = dev.SaveSignature(path);

            return path;
        }
    }
       
}
