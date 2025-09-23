using AGDPMS.Web.Data;
using Microsoft.AspNetCore.Identity;
using Npgsql;
using System.Data;

namespace AGDPMS.Web;

public static class ConfigureServices
{
    public static IServiceCollection AddDataAccesses(this IServiceCollection services, string connectionString)
    {
        services.AddScoped<IDbConnection>(_ => new NpgsqlConnection(connectionString));
        services.AddScoped<IUserStore<AppUser>, DapperUserStore>();
        return services;
    }
}