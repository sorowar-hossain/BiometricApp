using BiometricApp.Natives;
using Microsoft.Maui.Controls;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using FAP50Demo;
using IMD_RESULT = FAP50Demo.IMD_RESULT;
using GUI_SHOW_MODE = FAP50Demo.GUI_SHOW_MODE;
using FINGER_POSITION = FAP50Demo.FINGER_POSITION;

namespace BiometricApp.Services
{
    public class ScannerService
    {

        //IMDWrapper iMDWrapper = new IMDWrapper();
        string basePath = AppSettings.BaseFolder;
        // Main function to capture fingerprint

        private imd_fap50.Fap50CallbackEvent _callback;
        string[,] score = new string[(int)Fap50FingerprintService.GUI_SHOW_MODE.SIZE, (int)Fap50FingerprintService.FINGER_POSITION.SIZE];
        System.Drawing.Brush fontBrush = new SolidBrush(System.Drawing.Color.DarkBlue);
        Fap50FingerprintService.GUI_SHOW_MODE now_mode = Fap50FingerprintService.GUI_SHOW_MODE.NONE;
        //
        public string[] StandbyVideoPaths { get; private set; }
        private Timer StandbyVideoShowTimer;
        // standby video file list default path is "trunk3\x64\Release\panel\video\rossi_plus_fixed"
        private List<string> fileList = new List<string>();
        private int current_standby_Img_Index = 0;
        private Timer ShowDemoVideoTimer;
        private bool isInternalPannelExist = false;
        private bool isStandbyTimerWorks = false;

        private volatile bool _scanCompleted = false;
        private FINGER_POSITION _currentFinger;
        private string _currentFolder;

        public ScannerService()
        {
            _callback = OnFap50Event;
            imd_fap50.set_event(_callback);
        }

        public async Task<bool> CaptureFingerprintLeft(string selectedMemberId)
        {
            try
            {
                _scanCompleted = false;

                // Reset device
                IMD_RESULT res = imd_fap50.device_reset();

                if (res != IMD_RESULT.SUCCESS)
                {
                    Console.WriteLine($"Device reset failed: {res}");
                    return false;
                }

                // Connect panel if available
                Connect_Pannel();

                // Wait device initialization
                await Task.Delay(2000);

                // Start LEFT FOUR scan
                FINGER_POSITION finger = FINGER_POSITION.LEFT_FOUR;

                res = imd_fap50.scan_start(GUI_SHOW_MODE.FLAT, ref finger, 1);

                if (res != IMD_RESULT.SUCCESS)
                {
                    Console.WriteLine($"Scan start failed: {res}");
                    return false;
                }

                // Wait capture complete
                // WAIT FOR CALLBACK (NOT FIXED DELAY)
                int timeout = 15000;
                int waited = 0;

                while (!_scanCompleted && waited < timeout)
                {
                    await Task.Delay(200);
                    waited += 200;
                }

                // Create output folder
                string folder = Path.Combine(basePath, selectedMemberId);

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                IMD_RESULT f1 = imd_fap50.save_file(GUI_SHOW_MODE.FLAT, FINGER_POSITION.LEFT_INDEX, Path.Combine(folder, "l_Left_Index.bmp"));
                IMD_RESULT f2 = imd_fap50.save_file(GUI_SHOW_MODE.FLAT, FINGER_POSITION.LEFT_MIDDLE, Path.Combine(folder, "l_Left_Middle.bmp"));
                IMD_RESULT f3 = imd_fap50.save_file(GUI_SHOW_MODE.FLAT, FINGER_POSITION.LEFT_RING, Path.Combine(folder, "l_Left_Ring.bmp"));
                IMD_RESULT f4 = imd_fap50.save_file(GUI_SHOW_MODE.FLAT, FINGER_POSITION.LEFT_LITTLE, Path.Combine(folder, "l_Left_Little.bmp"));

                if (f1 != IMD_RESULT.SUCCESS ||
                    f2 != IMD_RESULT.SUCCESS ||
                    f3 != IMD_RESULT.SUCCESS ||
                    f4 != IMD_RESULT.SUCCESS)
                {
                    Console.WriteLine("Save file failed.");
                    return false;
                }

                Console.WriteLine("Left fingerprint capture successful.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CaptureFingerprintLeft Error: {ex}");
                return false;
            }
            finally
            {
                
            }
        }

        public async Task<bool> CaptureFingerprintRight(string selectedMemberId)
        {
            try
            {
                _scanCompleted = false;

                // Reset device
                IMD_RESULT res = imd_fap50.device_reset();

                if (res != IMD_RESULT.SUCCESS)
                {
                    Console.WriteLine($"Device reset failed: {res}");
                    return false;
                }

                // Connect panel if available
                Connect_Pannel();

                // Wait device initialization
                await Task.Delay(2000);

                // Start LEFT FOUR scan
                FINGER_POSITION finger = FINGER_POSITION.RIGHT_FOUR;

                res = imd_fap50.scan_start(GUI_SHOW_MODE.FLAT, ref finger, 1);

                if (res != IMD_RESULT.SUCCESS)
                {
                    Console.WriteLine($"Scan start failed: {res}");
                    return false;
                }

                // Wait capture complete
                // WAIT FOR CALLBACK (NOT FIXED DELAY)
                int timeout = 15000;
                int waited = 0;

                while (!_scanCompleted && waited < timeout)
                {
                    await Task.Delay(200);
                    waited += 200;
                }

                // Create output folder
                string folder = Path.Combine(basePath, selectedMemberId);

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                IMD_RESULT f1 = imd_fap50.save_file(GUI_SHOW_MODE.FLAT, FINGER_POSITION.RIGHT_INDEX, Path.Combine(folder, "r_Right_Index.bmp"));
                IMD_RESULT f2 = imd_fap50.save_file(GUI_SHOW_MODE.FLAT, FINGER_POSITION.RIGHT_MIDDLE, Path.Combine(folder, "r_Right_Middle.bmp"));
                IMD_RESULT f3 = imd_fap50.save_file(GUI_SHOW_MODE.FLAT, FINGER_POSITION.RIGHT_RING, Path.Combine(folder, "r_Right_Ring.bmp"));
                IMD_RESULT f4 = imd_fap50.save_file(GUI_SHOW_MODE.FLAT, FINGER_POSITION.RIGHT_LITTLE, Path.Combine(folder, "r_Right_Little.bmp"));

                if (f1 != IMD_RESULT.SUCCESS ||
                    f2 != IMD_RESULT.SUCCESS ||
                    f3 != IMD_RESULT.SUCCESS ||
                    f4 != IMD_RESULT.SUCCESS)
                {
                    Console.WriteLine("Save file failed.");
                    return false;
                }

                Console.WriteLine("Left fingerprint capture successful.");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CaptureFingerprintLeft Error: {ex}");
                return false;
            }
            finally
            {

            }
        }

        public async Task<bool> CaptureFingerprintThum(string selectedMemberId)
        {
            try
            {
                _scanCompleted = false;

                // Reset device
                IMD_RESULT res = imd_fap50.device_reset();

                if (res != IMD_RESULT.SUCCESS)
                {
                    Console.WriteLine($"Device reset failed: {res}");
                    return false;
                }

                // Connect panel if available
                Connect_Pannel();

                // Wait device initialization
                await Task.Delay(2000);

                // Start LEFT FOUR scan
                FINGER_POSITION finger = FINGER_POSITION.BOTH_THUMBS;

                res = imd_fap50.scan_start(GUI_SHOW_MODE.FLAT, ref finger, 1);

                if (res != IMD_RESULT.SUCCESS)
                {
                    Console.WriteLine($"Scan start failed: {res}");
                    return false;
                }

                // Wait capture complete
                // WAIT FOR CALLBACK (NOT FIXED DELAY)
                int timeout = 15000;
                int waited = 0;

                while (!_scanCompleted && waited < timeout)
                {
                    await Task.Delay(200);
                    waited += 200;
                }

                // Create output folder
                string folder = Path.Combine(basePath, selectedMemberId);

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                IMD_RESULT f1 = imd_fap50.save_file(GUI_SHOW_MODE.FLAT, FINGER_POSITION.LEFT_THUMB, Path.Combine(folder, "l_Left_Thumb.bmp"));
                IMD_RESULT f2 = imd_fap50.save_file(GUI_SHOW_MODE.FLAT, FINGER_POSITION.RIGHT_THUMB, Path.Combine(folder, "r_Right_Thumb.bmp"));

                if (f1 != IMD_RESULT.SUCCESS ||
                    f2 != IMD_RESULT.SUCCESS)
                {
                    Console.WriteLine("Save file failed.");
                    return false;
                }

                Console.WriteLine("Left fingerprint capture successful.");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CaptureFingerprintLeft Error: {ex}");
                return false;
            }
            finally
            {

            }
        }
        void dbg(string msg)
        {
#if DEBUG
            Debug.WriteLine(msg);  // 在調試模式下輸出
#else
            Trace.WriteLine(msg);  // 在發佈模式下也輸出
#endif
        }
        public static class IMD_FAP50_SDK_PANEL
        {
            private static bool isConnected = false;
            [DllImport("lib_imd_fap50_method.dll", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool connect_fap50_panel(string host, ushort port);

            [DllImport("lib_imd_fap50_method.dll", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool send_jpg_fap50_panel(byte[] buffer, uint size);

            [DllImport("lib_imd_fap50_method.dll", CallingConvention = CallingConvention.Cdecl)]
            public static extern void disconnect_fap50_panel();
            /*
            public static bool Connect(string host, ushort port)
            {
                if (!isConnected)
                {
                    isConnected = connect_fap50_panel(host, port);
                }
                return isConnected;
            }*/

            public static bool Connect(string host, ushort port)
            {
                if (!isConnected)
                {
                    var sw = Stopwatch.StartNew();

                    isConnected = connect_fap50_panel(host, port);

                    sw.Stop();
                    Trace.WriteLine($"[TIMING] connect_fap50_panel took {sw.ElapsedMilliseconds} ms");
                }
                return isConnected;
            }

            public static void Disconnect()
            {
                if (isConnected)
                {
                    disconnect_fap50_panel();
                    isConnected = false;
                }
            }
            public static bool IsConnected => isConnected;
        }
        private void Connect_Pannel()
        {
            if (!isInternalPannelExist)
                return;

            bool fconnectResult = false;
            if (!IMD_FAP50_SDK_PANEL.IsConnected)
            {
                fconnectResult = IMD_FAP50_SDK_PANEL.Connect("192.168.100.10", 1812);
                if (!fconnectResult)
                    dbg("Cannot connect to Panel via 192.168.100.10");
            }
            else
            {
                dbg("The Pannel is connected!!");
                return;
            }
        }
        private void Disconnect_Pannel()
        {
            if (!isInternalPannelExist)
                return;
            if (IMD_FAP50_SDK_PANEL.IsConnected)
            {
                IMD_FAP50_SDK_PANEL.Disconnect();
                dbg("Disconnected from FAP50 Panel.");
            }
            dbg($"Pannel is disconnected = {IMD_FAP50_SDK_PANEL.IsConnected}");
        }

        private void OnFap50Event(IMD_RESULT e)
        {
            if (e == IMD_RESULT.SUCCESS)
            {
                _scanCompleted = true;
            }
            else
            {
                _scanCompleted = false;
            }
        }
        bool IsLeft(FINGER_POSITION pos)
        {
            return pos == FINGER_POSITION.LEFT_INDEX ||
                   pos == FINGER_POSITION.LEFT_MIDDLE ||
                   pos == FINGER_POSITION.LEFT_RING ||
                   pos == FINGER_POSITION.LEFT_LITTLE ||
                   pos == FINGER_POSITION.LEFT_THUMB ||
                   pos == FINGER_POSITION.LEFT_FOUR;
        }
    }

}
