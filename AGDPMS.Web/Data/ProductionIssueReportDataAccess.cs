using System.Data;
using Dapper;
using AGDPMS.Shared.Models;

namespace AGDPMS.Web.Data;

public class ProductionIssueReportDataAccess(IDbConnection conn)
{
    public Task<int> CreateAsync(int stageId, int createdByUserId, IssuePriority priority, string reason) => conn.ExecuteScalarAsync<int>(@"
        insert into production_issue_reports(production_item_stage_id, created_by_user_id, status, priority, reason, created_at)
        values (@StageId, @CreatedByUserId, 0, @Priority, @Reason, now())
        returning id",
        new { StageId = stageId, CreatedByUserId = createdByUserId, Priority = (short)priority, Reason = reason });

    public Task<ProductionIssueReport?> GetByIdAsync(int id) => conn.QueryFirstOrDefaultAsync<ProductionIssueReport>(@"
        select id as Id, production_item_stage_id as ProductionItemStageId, created_by_user_id as CreatedByUserId,
               status as Status, priority as Priority, reason as Reason,
               resolved_at as ResolvedAt, resolved_by_user_id as ResolvedByUserId,
               created_at as CreatedAt
        from production_issue_reports
        where id = @Id",
        new { Id = id });

    public Task<IEnumerable<ProductionIssueReport>> GetByStageIdAsync(int stageId) => conn.QueryAsync<ProductionIssueReport>(@"
        select id as Id, production_item_stage_id as ProductionItemStageId, created_by_user_id as CreatedByUserId,
               status as Status, priority as Priority, reason as Reason,
               resolved_at as ResolvedAt, resolved_by_user_id as ResolvedByUserId,
               created_at as CreatedAt
        from production_issue_reports
        where production_item_stage_id = @StageId
        order by created_at desc",
        new { StageId = stageId });

    public Task<IEnumerable<ProductionIssueReport>> GetOpenIssuesByStageIdAsync(int stageId) => conn.QueryAsync<ProductionIssueReport>(@"
        select id as Id, production_item_stage_id as ProductionItemStageId, created_by_user_id as CreatedByUserId,
               status as Status, priority as Priority, reason as Reason,
               resolved_at as ResolvedAt, resolved_by_user_id as ResolvedByUserId,
               created_at as CreatedAt
        from production_issue_reports
        where production_item_stage_id = @StageId and status = 0
        order by created_at desc",
        new { StageId = stageId });

    public Task ResolveIssueAsync(int issueId, int resolvedByUserId) => conn.ExecuteAsync(@"
        update production_issue_reports
        set status = 1, resolved_at = now(), resolved_by_user_id = @ResolvedByUserId
        where id = @Id",
        new { Id = issueId, ResolvedByUserId = resolvedByUserId });
}

