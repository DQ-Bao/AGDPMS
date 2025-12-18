using AGDPMS.Services;
using AGDPMS.Shared.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using ZXing.Net.Maui.Controls;

namespace AGDPMS;

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
            })
            .UseBarcodeReader();

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif
        builder.Services.AddAuthorizationCore();
        builder.Services.AddScoped<CustomAuthenticationStateProvider>();
        builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<CustomAuthenticationStateProvider>());

        builder.Services.AddSingleton<IFormFactor, FormFactor>();
        builder.Services.AddTransient<AuthHeaderHandler>();
        builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("ApiClient2"));
        builder.Services.AddHttpClient("ApiClient2", client =>
        {
            client.BaseAddress = new Uri("https://cole-bra-eden-lectures.trycloudflare.com");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddHttpMessageHandler<AuthHeaderHandler>()
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });
        builder.Services.AddHttpClient("ApiClient", client =>
        {
            client.BaseAddress = new Uri("https://cole-bra-eden-lectures.trycloudflare.com/api/");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddHttpMessageHandler<AuthHeaderHandler>()
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        }); // NOTE: For development only - accept self-signed certificates
        builder.Services.AddScoped<IAuthService, MobileAuthService>();
        builder.Services.AddScoped<IUserService, MobileUserService>();
        builder.Services.AddScoped<INotificationService, MobileNotificationService>();
        builder.Services.AddSingleton<QrScanService>();

        return builder.Build();
    }
}
