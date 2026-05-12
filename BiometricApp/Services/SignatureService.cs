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
                Console.WriteLine("Device Reset");
                int reset = dev.DeviceReset();
                Console.WriteLine($"Reset: {reset}");

                await Task.Delay(2000);

                Console.WriteLine("Checking Busy");
                bool busy = dev.IsScanBusy();
                Console.WriteLine($"Busy: {busy}");

                Console.WriteLine("Starting Signature");

                int start = dev.StartSignature();

                Console.WriteLine($"Start Result: {start}");

                return "";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return "";
            }
        }
    } 
}
