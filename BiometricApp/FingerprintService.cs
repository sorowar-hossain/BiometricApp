
using BiometricApp.Natives;
using System;
using System.Runtime.InteropServices;
namespace BiometricApp;
public class FingerprintService
{
    public SystemProperty DeviceInfo { get; private set; }


    public int Init(string host = null, ushort port = 0)
    {
        // Connect if host/port provided
        if (!string.IsNullOrEmpty(host) && port != 0)
        {
            bool connected = FingerprintSDK.connect_fap50_panel(host, port);
            if (!connected) return -1;
        }

        // Reset device
        IMD_RESULT result = FingerprintSDK.device_reset();
        if (result != IMD_RESULT.SUCCESS) return (int)result;

        // Get system property
        SystemProperty p = new SystemProperty();
        result = FingerprintSDK.get_system_property(ref p);
        if (result != IMD_RESULT.SUCCESS) return (int)result;

        DeviceInfo = p;
        return 0; // success
    }

    public int DeviceReset()
    {
        // Reset device
        IMD_RESULT result = FingerprintSDK.device_reset();
        if (result != IMD_RESULT.SUCCESS) return (int)result;

        // Get system property
        SystemProperty p = new SystemProperty();
        result = FingerprintSDK.get_system_property(ref p);
        if (result != IMD_RESULT.SUCCESS) return (int)result;

        DeviceInfo = p;
        return 0; // success
    }

    public bool ScanStart()
    {
        IMD_RESULT result = FingerprintSDK.scan_start();
        return result == IMD_RESULT.SUCCESS;
    }

    public void Disconnect()
    {
        FingerprintSDK.disconnect_fap50_panel();
    }
}
