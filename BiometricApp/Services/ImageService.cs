using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiometricApp.Services
{
    public class ImageService
    {

        enum GUI_SHOW_MODE 
        { 
            GUI_SHOW_MODE_NONE,
            GUI_SHOW_MODE_CAPTURE,    //scan fingerprints immediately
            GUI_SHOW_MODE_ROLL,
            GUI_SHOW_MODE_FLAT,
            GUI_SHOW_MODE_SIGNATURE,
            GUI_SHOW_MODE_SIGNATURE_BY_PEN,
        };

        enum FINGER_POSITION
        {
            FINTER_POSITION_UNKNOW_FINGER = 0,

            FINGER_POSITION_RIGHT_THUMB,
            FINGER_POSITION_RIGHT_INDEX,
            FINGER_POSITION_RIGHT_MIDDLE,
            FINGER_POSITION_RIGHT_RING,
            FINGER_POSITION_RIGHT_LITTLE,

            FINGER_POSITION_LEFT_THUMB,
            FINGER_POSITION_LEFT_INDEX,
            FINGER_POSITION_LEFT_MIDDLE,
            FINGER_POSITION_LEFT_RING,
            FINGER_POSITION_LEFT_LITTLE,

            FINGER_POSITION_RIGHT_FOUR = 13,
            FINGER_POSITION_LEFT_FOUR,
            FINGER_POSITION_BOTH_THUMBS,
            FINGER_POSITION_SOME_FINGERS,
            FINGER_POSITION_SIGNATURE,
            FINGER_POSITION_RIGHT_FULL,
            FINGER_POSITION_LEFT_FULL,
            FINGER_POSITION_SIZE,
        };
    }
}
