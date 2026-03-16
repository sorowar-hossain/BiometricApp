using BiometricApp.Services;
using Microsoft.Extensions.Logging;

namespace BiometricApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddSingleton<ScannerService>();
            builder.Services.AddSingleton<ImageService>();
            builder.Services.AddSingleton<Fap50FingerprintService>();
            builder.Services.AddSingleton<FingerprintService2>();
            builder.Services.AddSingleton<FaceService>();


#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
