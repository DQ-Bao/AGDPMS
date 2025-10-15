using AGDPMS.Services;
using AGDPMS.Shared.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;

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
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif
        builder.Services.AddAuthorizationCore();
        builder.Services.AddScoped<CustomAuthenticationStateProvider>();
        builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<CustomAuthenticationStateProvider>());

        builder.Services.AddSingleton<IFormFactor, FormFactor>();
        builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("ApiClient"));
        builder.Services.AddHttpClient("ApiClient", client =>
        {
            client.BaseAddress = new Uri("https://bedford-blvd-empirical-requirements.trycloudflare.com/api/");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddHttpMessageHandler<AuthHeaderHandler>()
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        }); // NOTE: For development only - accept self-signed certificates
        builder.Services.AddScoped<IAuthService, MobileAuthService>();

        return builder.Build();
    }
}
