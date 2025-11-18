using System.Data;
using Dapper;
using AGDPMS.Shared.Models;

namespace AGDPMS.Web.Data;

public class ProductionItemStageDataAccess(IDbConnection conn)
{
    public Task<IEnumerable<ProductionItemStage>> ListByItemAsync(int itemId) => conn.QueryAsync<ProductionItemStage>(@"
        select id as Id, production_order_item_id as ProductionOrderItemId, stage_type_id as StageTypeId,
               assigned_qa_user_id as AssignedQaUserId, status as Status,
               planned_start_date as PlannedStartDate, planned_finish_date as PlannedFinishDate,
               actual_start_date as ActualStartDate, actual_finish_date as ActualFinishDate,
               planned_time_hours as PlannedTimeHours, actual_time_hours as ActualTimeHours,
               is_completed as IsCompleted, completed_at as CompletedAt,
               note as Note, created_at as CreatedAt, updated_at as UpdatedAt
        from production_item_stages
        where production_order_item_id = @ItemId
        order by id asc",
        new { ItemId = itemId });

    public Task<ProductionItemStage?> GetByIdAsync(int id) => conn.QueryFirstOrDefaultAsync<ProductionItemStage>(@"
        select id as Id, production_order_item_id as ProductionOrderItemId, stage_type_id as StageTypeId,
               assigned_qa_user_id as AssignedQaUserId, status as Status,
               planned_start_date as PlannedStartDate, planned_finish_date as PlannedFinishDate,
               actual_start_date as ActualStartDate, actual_finish_date as ActualFinishDate,
               planned_time_hours as PlannedTimeHours, actual_time_hours as ActualTimeHours,
               is_completed as IsCompleted, completed_at as CompletedAt,
               note as Note, created_at as CreatedAt, updated_at as UpdatedAt
        from production_item_stages
        where id = @Id",
        new { Id = id });

    public Task<int> CreateAsync(int itemId, int stageTypeId) => conn.ExecuteScalarAsync<int>(@"
        insert into production_item_stages(production_order_item_id, stage_type_id, is_completed, created_at)
        values (@ItemId, @StageTypeId, false, now())
        returning id",
        new { ItemId = itemId, StageTypeId = stageTypeId });

    public Task AssignQaAsync(int stageId, int qaUserId) => conn.ExecuteAsync(@"
        update production_item_stages
        set assigned_qa_user_id = @QaUserId, updated_at = now()
        where id = @Id",
        new { Id = stageId, QaUserId = qaUserId });

    public Task CompleteByPmAsync(int stageId) => conn.ExecuteAsync(@"
        update production_item_stages
        set is_completed = true, status = 2, completed_at = now(), updated_at = now()
        where id = @Id",
        new { Id = stageId });

    public Task UpdatePlanAsync(int stageId, DateTime? plannedStartDate, DateTime? plannedFinishDate, decimal? plannedTimeHours) => conn.ExecuteAsync(@"
        update production_item_stages
        set planned_start_date = @PlannedStartDate,
            planned_finish_date = @PlannedFinishDate,
            planned_time_hours = @PlannedTimeHours,
            updated_at = now()
        where id = @Id",
        new { Id = stageId, PlannedStartDate = plannedStartDate, PlannedFinishDate = plannedFinishDate, PlannedTimeHours = plannedTimeHours });

    public Task UpdateActualDatesAsync(int stageId, DateTime? actualStartDate, DateTime? actualFinishDate, decimal? actualTimeHours) => conn.ExecuteAsync(@"
        update production_item_stages
        set actual_start_date = @ActualStartDate,
            actual_finish_date = @ActualFinishDate,
            actual_time_hours = @ActualTimeHours,
            updated_at = now()
        where id = @Id",
        new { Id = stageId, ActualStartDate = actualStartDate, ActualFinishDate = actualFinishDate, ActualTimeHours = actualTimeHours });

    public Task UpdateStatusAsync(int stageId, StageStatus status) => conn.ExecuteAsync(@"
        update production_item_stages
        set status = @Status, updated_at = now()
        where id = @Id",
        new { Id = stageId, Status = (short)status });

    public Task BulkAssignQaAsync(int itemId, int qaUserId) => conn.ExecuteAsync(@"
        update production_item_stages
        set assigned_qa_user_id = @QaUserId, updated_at = now()
        where production_order_item_id = @ItemId",
        new { ItemId = itemId, QaUserId = qaUserId });
}


