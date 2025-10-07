using AGDPMS.Services;
using AGDPMS.Shared.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Maui;
using Microsoft.AspNetCore.Components.WebView.Maui;

namespace AGDPMS
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit();

            builder.ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

            // Add Blazor services
            builder.Services.AddMauiBlazorWebView();

            // Add device-specific services used by the AGDPMS.Shared project
            builder.Services.AddSingleton<IFormFactor, FormFactor>();
            builder.Services
                .AddHttpClient<IApiClient, AGDPMS.Services.ApiClient>()
                .ConfigureHttpClient(client =>
                {
                    // Use HTTP everywhere in Development to avoid device TLS headaches
                    if (DeviceInfo.Platform == DevicePlatform.Android)
                    {
                        // Emulator uses 10.0.2.2, physical device uses 127.0.0.1 with adb reverse
                        client.BaseAddress = new Uri(DeviceInfo.DeviceType == DeviceType.Virtual ? "http://10.0.2.2:5273" : "http://127.0.0.1:5273");
                    }
                    else
                    {
                        client.BaseAddress = new Uri("http://localhost:5273");
                    }
                });

            // Register user session abstraction
            builder.Services.AddScoped<IUserSession, MauiUserSession>();

            // Navigation history service for Android back navigation
            builder.Services.AddSingleton<NavigationHistoryService>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
