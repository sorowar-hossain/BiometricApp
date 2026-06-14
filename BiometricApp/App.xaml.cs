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

#if WINDOWS
            window.Created += async (s, e) =>
            {
                for (int i = 0; i < 10; i++)
                {
                    if (DisplayArea.FindAll().Count > 1)
                        break;

                    await Task.Delay(1000);
                }

                MoveToSecondMonitor(window);
            };
#endif

            return window;
        }
#if WINDOWS
        private void MoveToSecondMonitor(Window window)
        {
            var mauiWindow = window.Handler.PlatformView;
            var hwnd = WindowNative.GetWindowHandle(mauiWindow);
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);

            var appWindow = AppWindow.GetFromWindowId(windowId);

            var displays = DisplayArea.FindAll();

            if (displays.Count <= 1)
                return;

            // Get current display
            var currentDisplay = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Primary);

            // Pick a different display (external monitor)
            var target = displays.FirstOrDefault(d =>
                d != currentDisplay &&
                d.DisplayId.Value != currentDisplay.DisplayId.Value);

            if (target == null)
                return;

            var workArea = target.WorkArea;

            appWindow.MoveAndResize(new RectInt32(
                workArea.X,
                workArea.Y,
                workArea.Width,
                workArea.Height
            ));
        }
#endif
    }
}
