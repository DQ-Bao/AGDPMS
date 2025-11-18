using System.Data;
using Dapper;
using AGDPMS.Shared.Models;

namespace AGDPMS.Web.Data;

public class ProductionOrderDataAccess(IDbConnection conn)
{
    public async Task<string> GenerateNextCodeAsync()
    {
        var today = DateTime.Now.ToString("yyyyMMdd");
        var prefix = $"PO-{today}-";
        
        var latestCode = await conn.QueryFirstOrDefaultAsync<string>(@"
            select code
            from production_orders
            where code like @Prefix
            order by code desc
            limit 1",
            new { Prefix = $"{prefix}%" });
        
        int sequence = 1;
        if (!string.IsNullOrEmpty(latestCode) && latestCode.Length > prefix.Length)
        {
            var sequenceStr = latestCode.Substring(prefix.Length);
            if (int.TryParse(sequenceStr, out int lastSequence))
            {
                sequence = lastSequence + 1;
            }
        }
        
        return $"{prefix}{sequence:D3}";
    }

    public async Task<int> CreateAsync(ProductionOrder order)
    {
        var id = await conn.ExecuteScalarAsync<int>(@"
            insert into production_orders(project_id, code, status, is_cancelled, created_at, created_by)
            values (@ProjectId, @Code, @Status, false, now(), @CreatedBy)
            returning id",
            new { order.ProjectId, order.Code, Status = (short)order.Status, order.CreatedBy });
        return id;
    }

    public Task<IEnumerable<ProductionOrder>> ListAsync(int? projectId, string? q, string? sort, string? dir) => conn.QueryAsync<ProductionOrder>(@"
        select id as Id, project_id as ProjectId, code as Code,
               status as Status, is_cancelled as IsCancelled,
               planned_start_date as PlannedStartDate, planned_finish_date as PlannedFinishDate,
               submitted_at as SubmittedAt, director_decision_at as DirectorDecisionAt,
               qa_machines_checked_at as QAMachinesCheckedAt, qa_material_checked_at as QAMaterialCheckedAt,
               started_at as StartedAt, finished_at as FinishedAt,
               created_at as CreatedAt, created_by as CreatedBy,
               updated_at as UpdatedAt, updated_by as UpdatedBy
        from production_orders
        where (@ProjectId is null or project_id = @ProjectId)
          and (@Q is null or @Q = '' or coalesce(code,'') ilike '%'||@Q||'%')
        order by 
          case when @Sort = 'code' and @Dir = 'asc' then code end asc,
          case when @Sort = 'code' and @Dir = 'desc' then code end desc,
          case when @Sort = 'created_at' and @Dir = 'asc' then created_at end asc,
          case when @Sort = 'created_at' and @Dir = 'desc' then created_at end desc,
          created_at desc",
        new { ProjectId = projectId, Q = q, Sort = sort, Dir = dir });

    public Task<ProductionOrder?> GetByIdAsync(int id) => conn.QueryFirstOrDefaultAsync<ProductionOrder>(@"
        select id as Id, project_id as ProjectId, code as Code,
               status as Status, is_cancelled as IsCancelled,
               planned_start_date as PlannedStartDate, planned_finish_date as PlannedFinishDate,
               submitted_at as SubmittedAt, director_decision_at as DirectorDecisionAt,
               qa_machines_checked_at as QAMachinesCheckedAt, qa_material_checked_at as QAMaterialCheckedAt,
               started_at as StartedAt, finished_at as FinishedAt,
               created_at as CreatedAt, created_by as CreatedBy,
               updated_at as UpdatedAt, updated_by as UpdatedBy
        from production_orders
        where id = @Id",
        new { Id = id });

    public Task UpdatePlanAsync(int id, DateTime? plannedStartDate, DateTime? plannedFinishDate) => conn.ExecuteAsync(@"
        update production_orders
        set planned_start_date = @PlannedStartDate, planned_finish_date = @PlannedFinishDate, updated_at = now()
        where id = @Id",
        new { Id = id, PlannedStartDate = plannedStartDate, PlannedFinishDate = plannedFinishDate });

    public Task RevertToDraftAsync(int id) => conn.ExecuteAsync(@"
        update production_orders
        set status = @Status, updated_at = now()
        where id = @Id",
        new { Id = id, Status = (short)ProductionOrderStatus.Draft });

    public Task UpdateStatusAsync(int id, ProductionOrderStatus status) => conn.ExecuteAsync(@"
        update production_orders
        set status = @Status, updated_at = now()
        where id = @Id",
        new { Id = id, Status = (short)status });

    public Task MarkCancelledAsync(int id) => conn.ExecuteAsync(@"
        update production_orders
        set is_cancelled = true, status = @Status, updated_at = now()
        where id = @Id",
        new { Id = id, Status = (short)ProductionOrderStatus.Cancelled });

    public Task SubmitAsync(int id) => conn.ExecuteAsync(@"
        update production_orders
        set status = @Status, submitted_at = now(), updated_at = now()
        where id = @Id",
        new { Id = id, Status = (short)ProductionOrderStatus.PendingDirectorApproval });

    public Task DirectorApproveAsync(int id) => conn.ExecuteAsync(@"
        update production_orders
        set status = @Status, director_decision_at = now(), updated_at = now()
        where id = @Id",
        new { Id = id, Status = (short)ProductionOrderStatus.PendingQACheckMachines });

    public Task DirectorRejectAsync(int id) => conn.ExecuteAsync(@"
        update production_orders
        set status = @Status, director_decision_at = now(), updated_at = now()
        where id = @Id",
        new { Id = id, Status = (short)ProductionOrderStatus.DirectorRejected });

    public Task QaMachinesApproveAsync(int id) => conn.ExecuteAsync(@"
        update production_orders
        set status = @Status, qa_machines_checked_at = now(), updated_at = now()
        where id = @Id",
        new { Id = id, Status = (short)ProductionOrderStatus.PendingQACheckMaterial });

    public Task QaMaterialApproveAsync(int id) => conn.ExecuteAsync(@"
        update production_orders
        set status = @Status, qa_material_checked_at = now(), started_at = now(), updated_at = now()
        where id = @Id",
        new { Id = id, Status = (short)ProductionOrderStatus.InProduction });

    public Task MarkFinishedAsync(int id) => conn.ExecuteAsync(@"
        update production_orders
        set status = @Status, finished_at = now(), updated_at = now()
        where id = @Id",
        new { Id = id, Status = (short)ProductionOrderStatus.Finished });
}


