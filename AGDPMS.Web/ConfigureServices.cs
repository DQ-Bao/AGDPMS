using AGDPMS.Web.Components.Account;
using AGDPMS.Web.Data;
using AGDPMS.Shared.Services;
using Npgsql;
using System.Data;

namespace AGDPMS.Web;

public static class ConfigureServices
{
    public static IServiceCollection AddDataAccesses(this IServiceCollection services, string connectionString)
    {
        services.AddScoped<IDbConnection>(_ => new NpgsqlConnection(connectionString));
        services.AddScoped<UserDataAccess>();
        services.AddScoped<RoleDataAccess>();
        return services;
    }

    public static IServiceCollection AddSmsSender(this IServiceCollection services, Action<AndroidSmsGatewayOptions> configureOptions)
    {
        services.Configure(configureOptions);
        services.AddHttpClient<AndroidGatewaySmsSender>();
        services.AddScoped<ISmsSender, AndroidGatewaySmsSender>();
        return services;
    }
}