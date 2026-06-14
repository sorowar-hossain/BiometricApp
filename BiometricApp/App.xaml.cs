#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Windowing;
using System.Diagnostics;
using Windows.Graphics;
using WinRT.Interop;
#endif
namespace BiometricApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new MainPage());
            Process.Start(new ProcessStartInfo
            {
                FileName = @"C:\Windows\System32\DisplaySwitch.exe",
                Arguments = "/extend",
                UseShellExecute = true
            });

            return window;
        }

    }
}
