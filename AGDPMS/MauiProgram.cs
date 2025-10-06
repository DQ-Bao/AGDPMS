using AGDPMS.Services;
using AGDPMS.Shared.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Maui;

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

            // Add device-specific services used by the AGDPMS.Shared project
            builder.Services.AddSingleton<IFormFactor, FormFactor>();
            builder.Services.AddHttpClient<IApiClient, AGDPMS.Services.ApiClient>(client =>
            {
                // Use HTTP for local development (matches AGDPMS.Web http profile on port 5273)
                client.BaseAddress = new Uri("http://localhost:5273");
            });

            // Register pages and viewmodels
            builder.Services.AddTransient<AGDPMS.Pages.LoginViewModel>();
            builder.Services.AddTransient<AGDPMS.Pages.LoginPage>();
            builder.Services.AddTransient<AGDPMS.Pages.ForgotPasswordViewModel>();
            builder.Services.AddTransient<AGDPMS.Pages.ForgotPasswordPage>();
            builder.Services.AddTransient<AGDPMS.Pages.AddAccountViewModel>();
            builder.Services.AddTransient<AGDPMS.Pages.AddAccountPage>();
            builder.Services.AddTransient<AGDPMS.Pages.NeedChangePasswordViewModel>();
            builder.Services.AddTransient<AGDPMS.Pages.NeedChangePasswordPage>();
            builder.Services.AddTransient<AGDPMS.Pages.VerifyOtpViewModel>();
            builder.Services.AddTransient<AGDPMS.Pages.VerifyOtpPage>();
            builder.Services.AddTransient<AGDPMS.Pages.ResetPasswordViewModel>();
            builder.Services.AddTransient<AGDPMS.Pages.ResetPasswordPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
