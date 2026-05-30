using BiometricApp.Natives;
using FAP50Demo;
using OpenCvSharp;
using System.Diagnostics;
using System.Runtime.InteropServices;
using FINGER_POSITION = FAP50Demo.FINGER_POSITION;
using GUI_SHOW_MODE = FAP50Demo.GUI_SHOW_MODE;
using IMD_RESULT = FAP50Demo.IMD_RESULT;
using Rect = OpenCvSharp.Rect;

namespace BiometricApp.Services
{
    public class ScannerService : IDisposable
    {
        private readonly string basePath = AppSettings.BaseFolder;

        private readonly imd_fap50.Fap50CallbackEvent _callback;

        public string message = "";
        public bool reScan = false;

        public event Action? OnFrameUpdated;

        private readonly SynchronizationContext? _uiContext = SynchronizationContext.Current;

        private string _currentFrameBase64 = "";
        public string CurrentFrameBase64
        {
            get => _currentFrameBase64;
            private set => _currentFrameBase64 = value;
        }

        private volatile bool _scanCompleted = false;

        private bool _isScanRunning = false;

        private CancellationTokenSource? _ctsScan;
        private CancellationTokenSource? _videoCTS;

        private FINGER_POSITION _currentFinger;

        private string baseDir = "";
        private string ValidImgFPath = "";
        private string videoBasePath = "";

        private bool isInternalPannelExist = false;

        #region VIDEO

        public enum VideoShowPhase
        {
            GuideFrame,
            Scanning,
            Validate
        }

        private VideoShowPhase _phase = VideoShowPhase.GuideFrame;

        private readonly Dictionary<FINGER_POSITION, List<Mat>>
            guideFrames = new();

        private readonly Dictionary<FINGER_POSITION, Mat>
            scanning_img = new();

        private readonly Dictionary<int, Mat>
            loading_img = new();

        private int currentFrameIndex = 0;
        private int scanningFrameIndex = 0;

        #endregion
        public enum SampleSequence
        {
            Flat442,
            Flat442R,
            Signature,
            FlatSomefingers
        }

        private readonly Dictionary<string, FINGER_POSITION[]> menuMap =
    new(StringComparer.OrdinalIgnoreCase)
{
    {
        "left_4",
        new[]
        {
            FINGER_POSITION.LEFT_INDEX,
            FINGER_POSITION.LEFT_MIDDLE,
            FINGER_POSITION.LEFT_RING,
            FINGER_POSITION.LEFT_LITTLE
        }
    },
    {
        "right_4",
        new[]
        {
            FINGER_POSITION.RIGHT_INDEX,
            FINGER_POSITION.RIGHT_MIDDLE,
            FINGER_POSITION.RIGHT_RING,
            FINGER_POSITION.RIGHT_LITTLE
        }
    },
    {
        "two_thumbs",
        new[]
        {
            FINGER_POSITION.LEFT_THUMB,
            FINGER_POSITION.RIGHT_THUMB
        }
    }
};

        private readonly SampleSequence workType;

        public ScannerService(SampleSequence workType = SampleSequence.Flat442)
        {
            this.workType = workType;

            _callback = OnFap50Event;
            imd_fap50.set_event(_callback);

            LoadVideoResources();
            InitVideoFrames();

            _currentFinger = FINGER_POSITION.LEFT_INDEX;
            _phase = VideoShowPhase.GuideFrame;

            StartVideoLoop();
        }

        #region PUBLIC METHODS

        public async Task<bool> CaptureFingerprintLeft(string selectedMemberId)
        {
            return await CaptureFingerprint(
                selectedMemberId,
                FINGER_POSITION.LEFT_FOUR,
                new[]
                {
                    (FINGER_POSITION.LEFT_INDEX,  "l_Left_Index.bmp"),
                    (FINGER_POSITION.LEFT_MIDDLE, "l_Left_Middle.bmp"),
                    (FINGER_POSITION.LEFT_RING,   "l_Left_Ring.bmp"),
                    (FINGER_POSITION.LEFT_LITTLE, "l_Left_Little.bmp")
                });
        }

        public async Task<bool> CaptureFingerprintRight(string selectedMemberId)
        {
            return await CaptureFingerprint(
                selectedMemberId,
                FINGER_POSITION.RIGHT_FOUR,
                new[]
                {
                    (FINGER_POSITION.RIGHT_INDEX,  "r_Right_Index.bmp"),
                    (FINGER_POSITION.RIGHT_MIDDLE, "r_Right_Middle.bmp"),
                    (FINGER_POSITION.RIGHT_RING,   "r_Right_Ring.bmp"),
                    (FINGER_POSITION.RIGHT_LITTLE, "r_Right_Little.bmp")
                });
        }

        public async Task<bool> CaptureFingerprintThumb(string selectedMemberId)
        {
            return await CaptureFingerprint(
                selectedMemberId,
                FINGER_POSITION.BOTH_THUMBS,
                new[]
                {
                    (FINGER_POSITION.LEFT_THUMB,  "l_Left_Thumb.bmp"),
                    (FINGER_POSITION.RIGHT_THUMB, "r_Right_Thumb.bmp")
                });
        }

        #endregion

        #region MAIN CAPTURE

        private async Task<bool> CaptureFingerprint(
            string selectedMemberId,
            FINGER_POSITION fingerPosition,
            (FINGER_POSITION pos, string file)[] saveFiles)
        {
            try
            {
                message = "";
                reScan = false;
                _scanCompleted = false;

                IMD_RESULT res = imd_fap50.device_reset();

                if (res != IMD_RESULT.SUCCESS)
                {
                    message = "Device reset failed";
                    return false;
                }

                Connect_Pannel();

                await Task.Delay(1500);

                if(fingerPosition.ToString().ToLower() == "left_four")
                {
                    fingerPosition = FINGER_POSITION.LEFT_INDEX;
                }
                else if (fingerPosition.ToString().ToLower() == "right_four")
                {
                    fingerPosition = FINGER_POSITION.RIGHT_INDEX;
                }
                else if (fingerPosition.ToString().ToLower() == "left_thumb")
                {
                    fingerPosition = FINGER_POSITION.LEFT_THUMB;
                }

                _currentFinger = fingerPosition;

                _phase = VideoShowPhase.GuideFrame;

                StartVideoLoop();

                await Task.Delay(800);

                StartScan();

                FINGER_POSITION finger = fingerPosition;

                res = imd_fap50.scan_start(
                    GUI_SHOW_MODE.FLAT,
                    ref finger,
                    1);

                if (res != IMD_RESULT.SUCCESS)
                {
                    message = "Scan start failed";
                    StopScan();
                    return false;
                }

                _phase = VideoShowPhase.Scanning;

                bool completed = await WaitScanComplete();

                StopScan();

                if (!completed)
                {
                    message = "Scan timeout";
                    return false;
                }

                if (reScan)
                {
                    return false;
                }

                string folder =
                    Path.Combine(basePath, selectedMemberId);

                Directory.CreateDirectory(folder);

                foreach (var item in saveFiles)
                {
                    string path =
                        Path.Combine(folder, item.file);

                    IMD_RESULT saveRes =
                        imd_fap50.save_file(
                            GUI_SHOW_MODE.FLAT,
                            item.pos,
                            path);

                    if (saveRes != IMD_RESULT.SUCCESS)
                    {
                        message = "Save failed";
                        return false;
                    }
                }

                message = "Fingerprint capture success";

                return true;
            }
            catch (Exception ex)
            {
                message = ex.Message;

                return false;
            }
            finally
            {
                StopScan();
            }
        }

        #endregion

        #region CALLBACK

        private void OnFap50Event(IMD_RESULT e)
        {
            _scanCompleted = e == IMD_RESULT.SUCCESS;
        }

        #endregion

        #region VIDEO LOOP

        private void StartVideoLoop()
        {
            _videoCTS = new CancellationTokenSource();

            _ = VideoLoopAsync(_videoCTS.Token);
        }

        private async Task VideoLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                switch (_phase)
                {
                    case VideoShowPhase.GuideFrame:
                        await ShowGuideFrame();
                        break;

                    case VideoShowPhase.Scanning:
                        await ShowScanningFrame();
                        break;

                    case VideoShowPhase.Validate:
                        await ShowValidateFrame();
                        break;
                }

                await Task.Delay(50, token);
            }
        }

        private async Task ShowGuideFrame()
        {
            if (!guideFrames.TryGetValue(
                _currentFinger,
                out var frameList))
                return;

            if (frameList.Count == 0)
                return;

            if (currentFrameIndex >= frameList.Count)
                currentFrameIndex = 0;

            using Mat frame =
                frameList[currentFrameIndex].Clone();

            currentFrameIndex++;

            UpdateFrame(frame);

            await Task.CompletedTask;
        }

        private async Task ShowScanningFrame()
        {
            if (!scanning_img.TryGetValue(
                _currentFinger,
                out var baseMat))
                return;

            using Mat scanFrame = baseMat.Clone();

            int loadingIdx =
                (scanningFrameIndex++ / 8) % 3 + 1;

            if (loading_img.TryGetValue(
                loadingIdx,
                out var loadingMat))
            {
                Rect roi = new Rect(
                    260,
                    48,
                    loadingMat.Width,
                    loadingMat.Height);

                loadingMat.CopyTo(
                    new Mat(scanFrame, roi));
            }

            UpdateFrame(scanFrame);

            await Task.CompletedTask;
        }

        private async Task ShowValidateFrame()
        {
            if (!File.Exists(ValidImgFPath))
                return;

            using Mat img =
                Cv2.ImRead(ValidImgFPath);

            UpdateFrame(img);

            await Task.Delay(1000);

            _phase = VideoShowPhase.GuideFrame;
        }

        private void UpdateFrame(Mat mat)
        {
            Cv2.ImEncode(
                ".jpg",
                mat,
                out byte[] imgBytes,
                new ImageEncodingParam(
                    ImwriteFlags.JpegQuality,
                    60));

            CurrentFrameBase64 =
                $"data:image/jpeg;base64,{Convert.ToBase64String(imgBytes)}";

            // IMPORTANT: ensure UI thread notification
            NotifyUI();
        }

        #endregion

        #region LOAD RESOURCES

        private void LoadVideoResources()
        {
            string exeDir = AppContext.BaseDirectory;

            baseDir = exeDir;
            videoBasePath = Path.Combine(exeDir, @"panel\video");

            guideFrames[FINGER_POSITION.LEFT_FOUR] =
            new List<Mat>
            {
                Cv2.ImRead(
                    Path.Combine(
                        exeDir,
                        "panel/LeftHandPanel/LeftHandPanel-Main.png"))
            };

            guideFrames[FINGER_POSITION.RIGHT_FOUR] =
            new List<Mat>
            {
                Cv2.ImRead(
                    Path.Combine(
                        exeDir,
                        "panel/RightHandPanel/RightHandPanel-Main.png"))
            };

            guideFrames[FINGER_POSITION.BOTH_THUMBS] =
            new List<Mat>
            {
                Cv2.ImRead(
                    Path.Combine(
                        exeDir,
                        "panel/ThumbsPanel/ThumbPanel-Main.png"))
            };

            scanning_img[FINGER_POSITION.LEFT_FOUR] =
                Cv2.ImRead(Path.Combine(
                    exeDir,
                    "panel/LeftHandPanel/LeftHandPanel-Main-Scanning.png"));

            scanning_img[FINGER_POSITION.RIGHT_FOUR] =
                Cv2.ImRead(Path.Combine(
                    exeDir,
                    "panel/RightHandPanel/RightHandPanel-Main-Scanning.png"));

            scanning_img[FINGER_POSITION.BOTH_THUMBS] =
                Cv2.ImRead(Path.Combine(
                    exeDir,
                    "panel/ThumbsPanel/ThumbPanel-Main-Scanning.png"));

            loading_img[1] =
                Cv2.ImRead(Path.Combine(
                    exeDir,
                    "panel/LoadingScreen/WithButton/ScanningFrame-1.png"));

            loading_img[2] =
                Cv2.ImRead(Path.Combine(
                    exeDir,
                    "panel/LoadingScreen/WithButton/ScanningFrame-2.png"));

            loading_img[3] =
                Cv2.ImRead(Path.Combine(
                    exeDir,
                    "panel/LoadingScreen/WithButton/ScanningFrame-3.png"));
        }

        #endregion

        #region SCAN LOOP

        private void StartScan()
        {
            if (_isScanRunning)
                return;

            _ctsScan = new CancellationTokenSource();

            _isScanRunning = true;

            _ = ScanLoopAsync(_ctsScan.Token);
        }

        private void StopScan()
        {
            if (!_isScanRunning)
                return;

            _ctsScan?.Cancel();

            _ctsScan?.Dispose();

            _ctsScan = null;

            _isScanRunning = false;
        }

        private async Task ScanLoopAsync(
            CancellationToken token)
        {
            var img_status =
                default(FAP50Demo.ImageStatus);

            img_status.show_mode =
                GUI_SHOW_MODE.FLAT;

            try
            {
                while (!token.IsCancellationRequested)
                {
                    var res =
                        imd_fap50.get_image_status(
                            ref img_status);

                    if (res == IMD_RESULT.SUCCESS)
                    {
                        if (img_status.show_mode ==
                            GUI_SHOW_MODE.FLAT)
                        {
                            Handle_Flat_Mode_Scan(
                                ref img_status);
                        }
                    }

                    await Task.Delay(
                        res == IMD_RESULT.SUCCESS
                        ? 33
                        : 100,
                        token);
                }
            }
            catch
            {
            }
        }

        private async Task<bool> WaitScanComplete(
            int timeout = 15000)
        {
            int waited = 0;

            while (!_scanCompleted &&
                   waited < timeout)
            {
                await Task.Delay(200);

                waited += 200;
            }

            return _scanCompleted;
        }

        #endregion

        #region FLAT MODE

        private void Handle_Flat_Mode_Scan(
            ref FAP50Demo.ImageStatus img_status)
        {
            if (!img_status.is_finger_on &&
                img_status.is_flat_done)
            {
                imd_fap50.scan_cancel();

                var p =
                    new FAP50Demo.ImageProperty
                    {
                        mode = img_status.show_mode,
                        pos = img_status.finger_position,
                        this_scan = true
                    };

                var res =
                    imd_fap50.get_image(ref p);

                ValidImgFPath =
                    FindValidImageFile(
                        ref p,
                        out bool isValid);

                ShowValidFinger();

                switch (res)
                {
                    case IMD_RESULT.PUT_WRONG_HAND:
                        message = "Wrong hand";
                        reScan = true;
                        break;

                    case IMD_RESULT.POOR_QUALITY_AND_CANTACT_IRON:
                    case IMD_RESULT.POOR_NFIQ_QUALITY:
                        message = "Poor quality";
                        reScan = true;
                        break;

                    case IMD_RESULT.POOR_QUALITY_AND_WRONG_HAND:
                        message = "Wrong hand and poor quality";
                        reScan = true;
                        break;
                }

                _scanCompleted = true;
            }
        }

        private void ShowValidFinger()
        {
            _phase = VideoShowPhase.Validate;

            if (!File.Exists(ValidImgFPath))
                return;

            using Mat score_img =
                Cv2.ImRead(ValidImgFPath);

            UpdateFrame(score_img);
        }

        #endregion

        #region VALID IMAGE

        private string FindValidImageFile(
            ref FAP50Demo.ImageProperty p,
            out bool isValid)
        {
            isValid = false;

            int[] managedArray =
                new int[(int)ScoreArray.MaxSize];

            unsafe
            {
                fixed (int* pArr = p.score_array)
                {
                    Marshal.Copy(
                        (IntPtr)pArr,
                        managedArray,
                        0,
                        managedArray.Length);
                }
            }

            var score = new Score2Num();

            string validImgPath = "";

            switch (p.score_size)
            {
                case 4:

                    if (p.pos == FINGER_POSITION.LEFT_FOUR)
                    {
                        score.L1 = managedArray[0] >= p.score_min;
                        score.L2 = managedArray[1] >= p.score_min;
                        score.L3 = managedArray[2] >= p.score_min;
                        score.L4 = managedArray[3] >= p.score_min;

                        validImgPath =
                            Path.Combine(
                                baseDir,
                                $"panel/LeftHandPanel/Iterations/Iteration_{score.num}.png");
                    }
                    else
                    {
                        score.R1 = managedArray[0] >= p.score_min;
                        score.R2 = managedArray[1] >= p.score_min;
                        score.R3 = managedArray[2] >= p.score_min;
                        score.R4 = managedArray[3] >= p.score_min;

                        validImgPath =
                            Path.Combine(
                                baseDir,
                                $"panel/RightHandPanel/Iterations/Iteration_{score.num}.png");
                    }

                    break;

                case 2:

                    score.L0 = managedArray[0] >= p.score_min;
                    score.R0 = managedArray[1] >= p.score_min;

                    validImgPath =
                        Path.Combine(
                            baseDir,
                            $"panel/ThumbsPanel/Iterations/Iteration_{score.num}.png");

                    break;
            }

            isValid = true;

            return validImgPath;
        }

        #endregion

        #region PANEL

        public static class IMD_FAP50_SDK_PANEL
        {
            private static bool isConnected = false;

            [DllImport("lib_imd_fap50_method.dll",
                CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool connect_fap50_panel(
                string host,
                ushort port);

            [DllImport("lib_imd_fap50_method.dll",
                CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool send_jpg_fap50_panel(
                byte[] buffer,
                uint size);

            [DllImport("lib_imd_fap50_method.dll",
                CallingConvention = CallingConvention.Cdecl)]
            public static extern void disconnect_fap50_panel();

            public static bool Connect(
                string host,
                ushort port)
            {
                if (!isConnected)
                {
                    isConnected =
                        connect_fap50_panel(
                            host,
                            port);
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

            public static bool IsConnected =>
                isConnected;
        }

        private void Connect_Pannel()
        {
            if (!isInternalPannelExist)
                return;

            if (!IMD_FAP50_SDK_PANEL.IsConnected)
            {
                IMD_FAP50_SDK_PANEL.Connect(
                    "192.168.100.10",
                    1812);
            }
        }

        #endregion

        #region SCORE

        public struct Score2Num
        {
            public byte num;

            public bool R1
            {
                get => (num & (1 << 3)) != 0;
                set => num = value
                    ? (byte)(num | (1 << 3))
                    : (byte)(num & ~(1 << 3));
            }

            public bool R2
            {
                get => (num & (1 << 2)) != 0;
                set => num = value
                    ? (byte)(num | (1 << 2))
                    : (byte)(num & ~(1 << 2));
            }

            public bool R3
            {
                get => (num & (1 << 1)) != 0;
                set => num = value
                    ? (byte)(num | (1 << 1))
                    : (byte)(num & ~(1 << 1));
            }

            public bool R4
            {
                get => (num & (1 << 0)) != 0;
                set => num = value
                    ? (byte)(num | (1 << 0))
                    : (byte)(num & ~(1 << 0));
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
                set => num = value
                    ? (byte)(num | (1 << 0))
                    : (byte)(num & ~(1 << 0));
            }

            public bool L0
            {
                get => (num & (1 << 1)) != 0;
                set => num = value
                    ? (byte)(num | (1 << 1))
                    : (byte)(num & ~(1 << 1));
            }
        }

        #endregion

        public void Dispose()
        {
            StopScan();

            _videoCTS?.Cancel();

            _videoCTS?.Dispose();

            foreach (var group in guideFrames.Values)
            {
                foreach (var mat in group)
                {
                    mat.Dispose();
                }
            }

            foreach (var mat in scanning_img.Values)
            {
                mat.Dispose();
            }

            foreach (var mat in loading_img.Values)
            {
                mat.Dispose();
            }
        }
        private void InitVideoFrames()
        {
            guideFrames.Clear();

            List<string> requiredMenus = new();

            switch (workType)
            {
                case SampleSequence.Flat442:
                    requiredMenus.AddRange(new[] { "left_4", "right_4", "two_thumbs" });
                    break;

                case SampleSequence.Flat442R:
                    requiredMenus.AddRange(new[] { "left_4", "right_4", "two_thumbs", "left_roll", "right_roll" });
                    break;

                case SampleSequence.Signature:
                    requiredMenus.Add("signature");
                    break;
            }

            foreach (string menu in requiredMenus)
            {
                if (!menuMap.TryGetValue(menu.ToLower(), out var fingerPos))
                    continue;

                string folderPath = Path.Combine(videoBasePath, menu);
                string listPath = Path.Combine(folderPath, "list.txt");

                if (!File.Exists(listPath))
                    continue;

                var frames = new List<Mat>();

                foreach (string line in File.ReadLines(listPath))
                {
                    string file = line.Trim();

                    if (file == "EOF")
                        break;

                    string fullPath = Path.Combine(folderPath, file);

                    if (!File.Exists(fullPath))
                        continue;

                    var img = Cv2.ImRead(fullPath);

                    if (!img.Empty())
                        frames.Add(img);
                }

                if (frames.Count > 0)
                {
                    foreach (var pos in fingerPos)
                    {
                        guideFrames[pos] =
                            frames.Select(f => f.Clone()).ToList();
                    }
                }
            }
        }
        private void NotifyUI()
        {
            if (_uiContext != null)
            {
                _uiContext.Post(_ =>
                {
                    OnFrameUpdated?.Invoke();
                }, null);
            }
            else
            {
                OnFrameUpdated?.Invoke();
            }
        }
    }
}