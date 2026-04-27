using BiometricApp.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;

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


            // Load config
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("BiometricApp.appsettings.json");

            if (stream == null)
                throw new Exception("appsettings.json not found as embedded resource");

            var config = new ConfigurationBuilder()
                .AddJsonStream(stream)
                .Build();

            builder.Configuration.AddConfiguration(config);

            // Register HttpClient with BaseAddress
            builder.Services.AddHttpClient<ApiService>(client =>
            {
                var baseUrl = builder.Configuration["ApiSettings:BaseUrl"];
                client.BaseAddress = new Uri(baseUrl);
            });
            builder.Services.AddSingleton<UserService>();
            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddSingleton<ScannerService>();
            builder.Services.AddSingleton<ImageService>();
            builder.Services.AddSingleton<Fap50FingerprintService>();
            builder.Services.AddSingleton<IrisService>(); 
            builder.Services.AddSingleton<FaceService>();
            builder.Services.AddSingleton<SignatureService>();
            builder.Services.AddSingleton<FaceCaptureHcService>();
            



            builder.Services.AddSingleton<LocalStorageService>();
            builder.Services.AddSingleton<LocalUserService>();
            builder.Services.AddSingleton<SqlDatabaseService>();
            builder.Services.AddSingleton<PostgreSqlDatabaseService>();
            




#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
