using System.Data;
using Dapper;
using AGDPMS.Shared.Models;

namespace AGDPMS.Web.Data;

public class GlobalLaborCostSettingDataAccess(IDbConnection conn)
{
    public Task<GlobalLaborCostSetting?> GetAsync() => conn.QueryFirstOrDefaultAsync<GlobalLaborCostSetting>(@"
        select id as Id, hourly_rate as HourlyRate, updated_at as UpdatedAt
        from global_labor_cost_settings
        limit 1");

    public async Task UpsertAsync(decimal hourlyRate)
    {
        if (conn.State != System.Data.ConnectionState.Open)
        {
            conn.Open();
        }
        
        // Delete all existing rows (should only be one, but ensure clean state)
        await conn.ExecuteAsync(@"delete from global_labor_cost_settings");
        
        // Insert new single row
        await conn.ExecuteAsync(@"
            insert into global_labor_cost_settings (hourly_rate, updated_at)
            values (@HourlyRate, now())",
            new { HourlyRate = hourlyRate });
    }
}

