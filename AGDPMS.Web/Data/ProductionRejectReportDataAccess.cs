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

    public Task<ProductionRejectReport?> GetLatestByStageIdAsync(int stageId) => conn.QueryFirstOrDefaultAsync<ProductionRejectReport>(@"
        select id as Id, production_item_stage_id as ProductionItemStageId, 
               rejected_by_user_id as RejectedByUserId, reason as Reason, created_at as CreatedAt
        from production_reject_reports
        where production_item_stage_id = @StageId
        order by created_at desc
        limit 1",
        new { StageId = stageId });

    public Task<ProductionRejectReport?> GetByIdAsync(int reportId) => conn.QueryFirstOrDefaultAsync<ProductionRejectReport>(@"
        select id as Id, production_item_stage_id as ProductionItemStageId, 
               rejected_by_user_id as RejectedByUserId, reason as Reason, created_at as CreatedAt
        from production_reject_reports
        where id = @Id",
        new { Id = reportId });

    public Task UpdateReasonAsync(int reportId, string reason) => conn.ExecuteAsync(@"
        update production_reject_reports
        set reason = @Reason
        where id = @Id",
        new { Id = reportId, Reason = reason });
}


