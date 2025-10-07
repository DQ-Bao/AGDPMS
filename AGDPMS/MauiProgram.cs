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
            builder.Services.AddHttpClient<IApiClient, AGDPMS.Services.ApiClient>(client =>
            {
                // Use HTTP for local development (matches AGDPMS.Web http profile on port 5273)
                client.BaseAddress = new Uri("http://localhost:5273");
            });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
