using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace FAP50Demo
{
    using DWORD = UInt32;
    using WORD = UInt16;
    using BYTE = Byte;
    //using BOOL = bool; //c++ bool is 1byte, c# is 4bytes.

    enum IMD_RESULT : int
    {
        /** If IMD_RLT_SUCCESS is returned, it means success.If not, it may be a hint
            or an error.*/
        SUCCESS = 0x00,

        PROMPT = 0x100,
        /** This prompt is triggered when a scan is started.In GUI_SHOW_MODE_CAPTURE,
            there will be no prompt. */
        SCAN_THREAD_START,
        //The device is busy scanning images.
        SCAN_THREAD_IS_BUSY,
        /** After starting to scan, if no finger is placed for more than 30 seconds,
            the device will stop scanning.*/
        SCAN_THREAD_IDLE_TIMEOUT,
        //The prompt is triggered when the device stops.
        SCAN_THREAD_END,

        FAIL = 0x200,
        //This API does not have any implementation.
        NO_IMPLEMENTATION,
        NULL_POINTER,
        FLASH_WRITE_FAIL,
        //There is a problem with the input parameters of the API.
        PARAM_WRONG,
        //No USB devices were found.
        CANT_FIND_ANY_DEVICE,
        //Device reset failed.
        RESET_DEVICE_FAIL,
        //The device's CHIP ID is incorrect.
        CHIP_ID_FAIL,
        //Failed to adjust image background.
        CLAI_TIMEOUT,
        //Exception occurred while reading image from USB.
        USB_READ_IMAGE_EXCEPTION,
        USB_READ_IMAGE_TIMEOUT,
        //No images available.
        NO_AVAILABLE_IMAGE,
        //Hardware's USB is too slow
        USB_TOO_SLOW,
        //Hardware's DCMI is stuck.
        DCMI_IS_STUCK,
        //Scanned images are not yet complete.
        NO_ANY_SCAN_DONE,
        //Scanning images cannot be stopped.
        STOP_SCAN_TIMEOUT,
        //No fingers were detected.
        NO_ANY_FINGER,

        NEED_TO_TAKE_FINGER_OFF,

        FINGER_TOO_TOP = 0x300,
        FINGER_NUM_OVER_FOUR,
        PUT_WRONG_HAND,
        POOR_NFIQ_QUALITY,
        POOR_QUALITY_AND_WRONG_HAND,
        POOR_QUALITY_AND_CANTACT_IRON,
        POOR_CONTACT_IRON,

        OPEN_FILE_FAIL,
        INVALID_BIN_FILE,
        UPDATE_FW_FAIL,
        NOT_SUPPORT,
        FINGER_SHAPE_NOT_GOOD,

        AES_CHK_SUM = 0x400,
        AES_NO_IV_KEY,
        AES_NO_AES_KEY,

        DEVICE_PLUGGED_IN,
        DEVICE_PLUGGED_OUT,
        UNSUPPORTED_FORMAT = 0x500,
        PROCESSING_ERROR,
        INVALID_PARAM,
    };

    public enum NFIQ_VERSION
    {
        V1,
        V2,
        SIZE,
    }
    public enum SIGNATURE_ACTION
    {
        CLEAR,
        OK,
    }
    public enum GUI_SHOW_MODE
    {
        NONE,
        CAPTURE,  //scan fingerprints immediately
        ROLL,
        FLAT,
        SIGNATURE,
        SIGN_BY_PEN,
        SIZE,
    }
    public enum FINGER_POSITION 
    {
        UNKNOW_FINGER = 0,

        RIGHT_THUMB,
        RIGHT_INDEX,
        RIGHT_MIDDLE,
        RIGHT_RING,
        RIGHT_LITTLE,

        LEFT_THUMB,
        LEFT_INDEX,
        LEFT_MIDDLE,
        LEFT_RING,
        LEFT_LITTLE,

        RIGHT_FOUR = 13,
        LEFT_FOUR,
        BOTH_THUMBS,
        SOME_FINGERS,
        SIGNATURE,
        RIGHT_FULL,
        LEFT_FULL,
        SIZE,
    }

    //c++: typedef void (*Fap50CallbackEvent)(IMD_RESULT prompt);
    delegate void Fap50CallbackEvent(IMD_RESULT prompt);

    //public static const int ScoreArraySize = 4;
    enum ScoreArray
    {
        MaxSize = 4,  // 定義枚舉常數
    }

    [StructLayout(LayoutKind.Sequential)]
    struct ImdPoint2d
    {
        public double x, y;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct ImdDiamond
    {
        public ImdPoint2d pa, pb, pc, pd, cntr;
        double angle;
        [MarshalAs(UnmanagedType.U1)]
        public bool valid;

    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct SystemProperty
    {
        /** (TODO) After reset, these variables will be set to default values. */
        FINGER_POSITION finger_position;

        public int png_compress_ratio;     /** The PNG compression ratio is 0~9. (default:9) */
        public int jp2_quality;            /** The Jpeg2000 quality level is 0~1000. (default:750)
								 Zero is loseless compression. */

        public float wsq_bit_rate;         /** The bit rate is set as follows:
								 2.25 yields around 5:1 compression 
								 0.75 yields around 15:1 compression. (default) */
        public IntPtr wsq_comment_text;     /** The WSQ comment default is NULL. */

        public NFIQ_VERSION nfiq_ver;              //default:NFIQ_V1
        public int nfiq_score_minimum_acceptable;  //1~5, default:3
        public int nfiq2_score_minimum_acceptable; //0~100, default:35
  
        [MarshalAs(UnmanagedType.U1)]
        public bool speech_en;                     //default:true
        public int speech_language;                //0:EN, 1:CH 2:ESP 255:user's define
        public int speech_volume;                  //0:small, 1:medium, 2:large 3:mute

        [MarshalAs(UnmanagedType.U1)]
        public bool anti_spoofing;

        [MarshalAs(UnmanagedType.U1)]
        public bool life_check_en;                 //default:false. If debug the prj, need to set 0.
        public DWORD scan_timeout_ms;              //unit:ms default:120*1000ms

        //--- signature buttons
        public fixed int signature_clear_roi[4];     //clear roi:395,830,324,94 (default)
        public fixed int signature_confirm_roi[4];   //confirm roi:886,825,324,94 (default)

        //--- out
        public int image_width, image_height;
        public int image_bit_per_pix, image_pix_per_inch;
        public fixed BYTE fap50_lib_ver[3];
        public fixed BYTE opencv_lib_ver[3];     //OpenCV LIB version
        public fixed BYTE nbis_lib_ver[3];       //NBIS LIB version
        public fixed BYTE nfiq2_lib_ver[3];      //NFIQ2 LIB version
        public fixed BYTE guid[16];
        public WORD chip_id;
        public fixed BYTE fw_ver[3];             //[1]Major, [0]Minor, [2]Type
        public fixed BYTE led_ver[2];            //[1]Major, [0]Minor
        public IntPtr product_sn;                  //Product Serial Number
        public IntPtr product_brand;               //Product Brand
        public IntPtr product_model;               //Product Model
        [MarshalAs(UnmanagedType.U1)]
        public bool scan_by_manual;			        //default is false.
    }

    [StructLayout(LayoutKind.Sequential)]
    unsafe struct ImageProperty
    {
        public GUI_SHOW_MODE mode;
        public FINGER_POSITION pos;

        [MarshalAs(UnmanagedType.U1)]
        public bool this_scan;

        //--- out
        public IntPtr img;
        public int width, height;
        public fixed int score_array[(int)ScoreArray.MaxSize];
        public fixed int spoofing_array[(int)ScoreArray.MaxSize];

        public int score_size;
        public int score_min;
        public NFIQ_VERSION score_ver;
    }

    [StructLayout(LayoutKind.Sequential)]
    unsafe struct ImageStatus
    {
        /** It is up to the user to decide the fingerprint reading mode. */
        public GUI_SHOW_MODE show_mode;
        public FINGER_POSITION finger_position;

        /** Status of roll : If init, the flag is_roll_init will be TRUE.If finished,
            the flag is_roll_done will be TRUE. The value line is the current guideline.*/
        [MarshalAs(UnmanagedType.U1)]
        public bool is_roll_init, is_roll_done;

        /** Status of flat : The flag is_roll_init will be TRUE if is in init.
            If finished, the flag is_flat_done will be TRUE. */
        [MarshalAs(UnmanagedType.U1)]
        public bool is_flat_init, is_flat_done;

        public int finger_num;     //The "finger_num" is the number of finger presses.

        [MarshalAs(UnmanagedType.U1)]
        public bool is_finger_on;  //If any finger is touching, the flag "is_finger_on" is TRUE.

        /** This "contours" is a container for fingerprint outline coordinates, which records the
            coordinates of the rectangle. Use this to extract images for each finger separately.
            (The type void* is vector<ImdDiamond>*) */
        public IntPtr contours;

        public float frame_rate;   //Unit:fps (frame per second). Update every 250ms.
        public IntPtr img;          //The FAP50 image buffer. (1600*1000)
        [MarshalAs(UnmanagedType.U1)]
        public bool is_signature_done;

        public IMD_RESULT result;
    }

    class imd_fap50 
    {
        void dbg(string msg)
        {
#if DEBUG
            Debug.WriteLine(msg);  // 在調試模式下輸出
#else
            Trace.WriteLine(msg);  // 在發佈模式下也輸出
#endif
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void Fap50CallbackEvent(IMD_RESULT e);

        [DllImport("lib_imd_fap50_method.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IMD_RESULT set_event(Fap50CallbackEvent e);

        [DllImport("lib_imd_fap50_method.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IMD_RESULT device_reset();
        [DllImport("lib_imd_fap50_method.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IMD_RESULT get_system_property(ref SystemProperty p);
        [DllImport("lib_imd_fap50_method.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IMD_RESULT set_system_property(ref SystemProperty p);
        [DllImport("lib_imd_fap50_method.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IMD_RESULT scan_start(GUI_SHOW_MODE mode, ref FINGER_POSITION pos_buf, int num);

        [DllImport("lib_imd_fap50_method.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool is_scan_busy();
        [DllImport("lib_imd_fap50_method.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IMD_RESULT scan_cancel();
        [DllImport("lib_imd_fap50_method.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IMD_RESULT get_image_status(ref ImageStatus status);
        [DllImport("lib_imd_fap50_method.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IMD_RESULT save_file(GUI_SHOW_MODE mode, FINGER_POSITION finger_pos, string file_path);
        [DllImport("lib_imd_fap50_method.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IMD_RESULT get_image(ref ImageProperty img_property);
        [DllImport("lib_imd_fap50_method.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IMD_RESULT usb_switch(BYTE usb_sel);
        [DllImport("lib_imd_fap50_method.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IMD_RESULT set_led_speech_standby_mode();
        [DllImport("lib_imd_fap50_method.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IMD_RESULT set_burn_code();
        [DllImport("lib_imd_fap50_method.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IMD_RESULT user_space([MarshalAs(UnmanagedType.I1)] bool is_write, WORD offset, byte[] data, int len);
        [DllImport("lib_imd_fap50_method.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IMD_RESULT signature(SIGNATURE_ACTION action);
    }
}
