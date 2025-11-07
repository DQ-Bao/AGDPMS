using AGDPMS.Shared.Services;
using AGDPMS.Web.Data;
using AGDPMS.Web.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.Data;
using System.Text;

namespace AGDPMS.Web;

public static class ConfigureServices
{
    public static IServiceCollection AddDataAccesses(this IServiceCollection services, string connectionString)
    {
        services.AddScoped<IDbConnection>(_ => new NpgsqlConnection(connectionString));
        services.AddScoped<UserDataAccess>();
        services.AddScoped<InventoryDataAccess>();
        services.AddScoped<RoleDataAccess>();

        services.AddScoped<ClientDataAccess>();
        services.AddScoped<ProjectRFQDataAccess>();

        services.AddScoped<MachineDataAccess>();
        services.AddScoped<MachineTypeDataAccess>();
        services.AddScoped<CavityDataAccess>();
        // production data access
        services.AddScoped<StageTypeDataAccess>();
        services.AddScoped<ProductionOrderDataAccess>();
        services.AddScoped<ProductionItemDataAccess>();
        services.AddScoped<ProductionItemStageDataAccess>();
        services.AddScoped<ProductionRejectReportDataAccess>();
        services.AddScoped<ProjectDataAccess>();
        services.AddScoped<ProductDataAccess>();
        // production services
        services.AddScoped<ProductionOrderService>();
        services.AddScoped<StageService>();
        services.AddScoped<QrService>();
        return services;
    }

    public static IServiceCollection AddSmsSender(this IServiceCollection services, Action<AndroidSmsGatewayOptions> configureOptions)
    {
        services.Configure(configureOptions);
        services.AddHttpClient<AndroidGatewaySmsSender>();
        services.AddScoped<ISmsSender, AndroidGatewaySmsSender>();
        return services;
    }

    public static IServiceCollection AddCookieAndJwtAuth(this IServiceCollection services, Action<JwtOptions> configureOptions)
    {
        var jwtOpts = new JwtOptions();
        configureOptions(jwtOpts);
        if (string.IsNullOrWhiteSpace(jwtOpts.Key) || jwtOpts.Key.Length < 32)
            throw new ArgumentException("JwtOptions.Key must be at least 32 characters long");

        services.AddAuthentication(Constants.AuthScheme)
                .AddCookie(Constants.AuthScheme, opts =>
                {
                    opts.LoginPath = "/login";
                    opts.AccessDeniedPath = "/access-denied";
                    opts.LogoutPath = "/logout";

                    opts.Cookie.Name = Constants.AuthCookie;
                    opts.Cookie.HttpOnly = true;
                    opts.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    opts.Cookie.SameSite = SameSiteMode.Strict;
                    opts.ExpireTimeSpan = TimeSpan.FromDays(1);
                    opts.SlidingExpiration = true;
                })
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opts =>
                {
                    opts.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOpts.Key)),
                        ValidateLifetime = false
                    };
                });
        services.AddAuthorization();

        services.Configure(configureOptions);
        services.AddScoped<JwtService>();
        return services;
    }
}