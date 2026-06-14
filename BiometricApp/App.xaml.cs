#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Windowing;
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

#if WINDOWS
            window.Created += (s, e) =>
            {
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
