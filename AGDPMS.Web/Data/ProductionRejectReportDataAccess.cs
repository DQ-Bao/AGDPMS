using System.Data;
using Dapper;
using AGDPMS.Shared.Models;

namespace AGDPMS.Web.Data;

public class ProductionRejectReportDataAccess(IDbConnection conn)
{
    public async Task<int> CreateAsync(ProductionRejectReport report)
    {
        var id = await conn.ExecuteScalarAsync<int>(@"
            insert into production_reject_reports(production_item_stage_id, rejected_by_user_id, reason, created_at)
            values (@ProductionItemStageId, @RejectedByUserId, @Reason, now())
            returning id",
            new { report.ProductionItemStageId, report.RejectedByUserId, report.Reason });
        return id;
    }
}


