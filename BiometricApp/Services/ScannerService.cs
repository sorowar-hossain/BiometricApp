using BiometricApp.Natives;
using FAP50Demo;
using Microsoft.Maui.Controls;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.Text;
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
using FINGER_POSITION = FAP50Demo.FINGER_POSITION;
using GUI_SHOW_MODE = FAP50Demo.GUI_SHOW_MODE;
using IMD_RESULT = FAP50Demo.IMD_RESULT;

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
        private bool _isScanRunning = false;
        private CancellationTokenSource _ctsScan;
        private string baseDir, videoBasePath, ValidImgFPath;
        private bool isFingerValided = false;
        private bool isFingerON_ScanDone = false;

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
                StartScan();
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
                StopScan();
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
                StartScan();
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
                StopScan();
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
                StartScan();
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
                StopScan();
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
        private void StartScan()
        {
            if (_isScanRunning) return;

            _ctsScan = new CancellationTokenSource();
            _isScanRunning = true;
            _ = ScanLoopAsync(_ctsScan.Token);
        }
        private async Task ScanLoopAsync(CancellationToken token)
        {
            bool isReScan = false;
            var img_status = default(FAP50Demo.ImageStatus);
            FINGER_POSITION[] pos = { FINGER_POSITION.UNKNOW_FINGER };
            img_status.show_mode = GUI_SHOW_MODE.FLAT;
            try
            {
                while (true)
                {
                    if (token.IsCancellationRequested)
                        break;

                    var res = imd_fap50.get_image_status(ref img_status);
                    if (res != IMD_RESULT.SUCCESS)
                    {
                        int delaya = res == IMD_RESULT.SUCCESS ? 33 : 100;

                        await Task.Delay(delaya, token);
                        continue;
                    }

                    if (img_status.show_mode == GUI_SHOW_MODE.FLAT)
                    {
                        Handle_Flat_Mode_Scan(ref img_status);
                    }
                    int delay = res == IMD_RESULT.SUCCESS ? 33 : 100;

                    await Task.Delay(delay, token);
                }
            }
            catch (TaskCanceledException)
            {

            }
        }
        private void Handle_Flat_Mode_Scan(ref FAP50Demo.ImageStatus img_status)
        {
            bool fNeedReScan = false;
            if (img_status.is_finger_on == false && img_status.is_flat_done)
            {
                imd_fap50.scan_cancel();
                var p = new FAP50Demo.ImageProperty
                {
                    mode = img_status.show_mode,
                    pos = img_status.finger_position,
                    this_scan = true
                };
                var res = imd_fap50.get_image(ref p);
                ValidImgFPath = FindValidImageFile(ref p, out bool isValid);
                ShowValidFinger();

                switch (res)
                {
                    case IMD_RESULT.PUT_WRONG_HAND:
                        break;

                    case IMD_RESULT.POOR_QUALITY_AND_CANTACT_IRON:
                    case IMD_RESULT.POOR_NFIQ_QUALITY:
                        break;

                    case IMD_RESULT.POOR_QUALITY_AND_WRONG_HAND:
                        break;
                    default:
                        break;
                }

                if (res == IMD_RESULT.SUCCESS ||
                   (res != IMD_RESULT.SUCCESS && fNeedReScan == false))
                {
                    fNeedReScan = true;
                }

                if (fNeedReScan)
                {
                    img_status = default;
                    fNeedReScan = false;
                }
                isFingerON_ScanDone = false;
            }
        }
        private void ShowValidFinger()
        {
            using Mat score_img = LoadImage(ValidImgFPath);
            if (score_img.Empty()) return;

            using Mat rotated_img = new Mat();
            Cv2.Rotate(score_img, rotated_img, RotateFlags.Rotate90Clockwise);

            Cv2.ImEncode(".jpg", rotated_img, out byte[] imgBytes,
                new ImageEncodingParam(ImwriteFlags.JpegQuality, 60));

            if (IMD_FAP50_SDK_PANEL.IsConnected)
            {
                _ = Task.Run(() =>
                {
                    IMD_FAP50_SDK_PANEL.send_jpg_fap50_panel(imgBytes, (uint)imgBytes.Length);
                });
            }

            Thread.Sleep(500);
        }
        private string FindValidImageFile(ref FAP50Demo.ImageProperty p, out bool isValid)
        {
            string ValidImgPath = "";
            var score = new Score2Num();
            isValid = false;
            int[] managedArray = new int[(int)ScoreArray.MaxSize];
            unsafe
            {
                fixed (int* pArr = p.score_array)
                {
                    Marshal.Copy((IntPtr)pArr, managedArray, 0, managedArray.Length);
                }
            }

            switch (p.score_size)
            {

                case 4:
                    if (p.pos == FINGER_POSITION.RIGHT_FOUR)
                    {

                        score.R1 = p.score_ver == NFIQ_VERSION.V1 ? managedArray[0] <= p.score_min : managedArray[0] >= p.score_min;
                        score.R2 = p.score_ver == NFIQ_VERSION.V1 ? managedArray[1] <= p.score_min : managedArray[1] >= p.score_min;
                        score.R3 = p.score_ver == NFIQ_VERSION.V1 ? managedArray[2] <= p.score_min : managedArray[2] >= p.score_min;
                        score.R4 = p.score_ver == NFIQ_VERSION.V1 ? managedArray[3] <= p.score_min : managedArray[3] >= p.score_min;

                        isValid = score.R1 && score.R2 && score.R3 && score.R4;
                        ValidImgPath = Path.Combine(baseDir, $"panel/RightHandPanel/Iterations/Iteration_{score.num}.png");
                    }
                    else if (p.pos == FINGER_POSITION.LEFT_FOUR)
                    {
                        score.L1 = p.score_ver == NFIQ_VERSION.V1 ? managedArray[0] <= p.score_min : managedArray[0] >= p.score_min;
                        score.L2 = p.score_ver == NFIQ_VERSION.V1 ? managedArray[1] <= p.score_min : managedArray[1] >= p.score_min;
                        score.L3 = p.score_ver == NFIQ_VERSION.V1 ? managedArray[2] <= p.score_min : managedArray[2] >= p.score_min;
                        score.L4 = p.score_ver == NFIQ_VERSION.V1 ? managedArray[3] <= p.score_min : managedArray[3] >= p.score_min;

                        isValid = score.L1 && score.L2 && score.L3 && score.L4;
                        ValidImgPath = Path.Combine(baseDir, $"panel/LeftHandPanel/Iterations/Iteration_{score.num}.png");
                    }
                    break;

                case 2:
                    if (p.pos == FINGER_POSITION.BOTH_THUMBS)
                    {
                        score.L0 = p.score_ver == NFIQ_VERSION.V1 ? managedArray[0] <= p.score_min : managedArray[0] >= p.score_min;
                        score.R0 = p.score_ver == NFIQ_VERSION.V1 ? managedArray[1] <= p.score_min : managedArray[1] >= p.score_min;

                        isValid = score.L0 && score.R0;
                        ValidImgPath = Path.Combine(baseDir, $"panel/ThumbsPanel/Iterations/Iteration_{score.num}.png");
                    }
                    break;

                case 1:
                    bool passed = p.score_ver == NFIQ_VERSION.V1 ? managedArray[0] <= p.score_min : managedArray[0] >= p.score_min;
                    isValid = passed;

                    switch (p.pos)
                    {
                        case FINGER_POSITION.RIGHT_THUMB:
                        case FINGER_POSITION.RIGHT_INDEX:
                        case FINGER_POSITION.RIGHT_MIDDLE:
                        case FINGER_POSITION.RIGHT_RING:
                        case FINGER_POSITION.RIGHT_LITTLE:
                            ValidImgPath = Path.Combine(baseDir, $"panel/RightRollingFingers/Iterations/RollFinger-{(passed ? "Done" : "Retry")}.png");
                            break;

                        case FINGER_POSITION.LEFT_THUMB:
                        case FINGER_POSITION.LEFT_INDEX:
                        case FINGER_POSITION.LEFT_MIDDLE:
                        case FINGER_POSITION.LEFT_RING:
                        case FINGER_POSITION.LEFT_LITTLE:
                            ValidImgPath = Path.Combine(baseDir, $"panel/LeftRollingFingers/Iterations/RollFinger-{(passed ? "Done" : "Retry")}.png");
                            break;
                    }
                    break;
            }
            return ValidImgPath;
        }
        private Mat LoadImage(string relativePath)
        {
            string fullPath = Path.Combine(baseDir, relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));

            if (!File.Exists(fullPath))
                return new Mat();

            return Cv2.ImRead(fullPath);
        }
        public struct Score2Num
        {
            public byte num;

            public bool R1
            {
                get => (num & (1 << 3)) != 0;
                set => num = value ? (byte)(num | (1 << 3)) : (byte)(num & ~(1 << 3));
            }

            public bool R2
            {
                get => (num & (1 << 2)) != 0;
                set => num = value ? (byte)(num | (1 << 2)) : (byte)(num & ~(1 << 2));
            }

            public bool R3
            {
                get => (num & (1 << 1)) != 0;
                set => num = value ? (byte)(num | (1 << 1)) : (byte)(num & ~(1 << 1));
            }

            public bool R4
            {
                get => (num & (1 << 0)) != 0;
                set => num = value ? (byte)(num | (1 << 0)) : (byte)(num & ~(1 << 0));
            }

            public bool L1
            {
                get => R4;
                set => R4 = value;
            }

            public bool L2
            {
                get => R3;
                set => R3 = value;
            }

            public bool L3
            {
                get => R2;
                set => R2 = value;
            }

            public bool L4
            {
                get => R1;
                set => R1 = value;
            }

            public bool R0
            {
                get => (num & (1 << 0)) != 0;
                set => num = value ? (byte)(num | (1 << 0)) : (byte)(num & ~(1 << 0));
            }

            public bool L0
            {
                get => (num & (1 << 1)) != 0;
                set => num = value ? (byte)(num | (1 << 1)) : (byte)(num & ~(1 << 1));
            }

            public override string ToString()
            {
                return $"num=0x{num:X2} [R4:{R4}, R3:{R3}, R2:{R2}, R1:{R1}]";
            }
        }
        private void StopScan()
        {
            if (!_isScanRunning) return;

            _ctsScan?.Cancel();
            _ctsScan?.Dispose();
            _ctsScan = null;

            _isScanRunning = false;
        }
    }

}
