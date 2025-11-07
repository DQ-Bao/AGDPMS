using System.Data;
using Dapper;
using AGDPMS.Shared.Models;

namespace AGDPMS.Web.Data;

public class ProductionItemStageDataAccess(IDbConnection conn)
{
    public Task<IEnumerable<ProductionItemStage>> ListByItemAsync(int itemId) => conn.QueryAsync<ProductionItemStage>(@"
        select id as Id, production_order_item_id as ProductionOrderItemId, stage_type_id as StageTypeId,
               assigned_qa_user_id as AssignedQaUserId, is_completed as IsCompleted, completed_at as CompletedAt,
               rejection_count as RejectionCount, note as Note, created_at as CreatedAt, updated_at as UpdatedAt
        from production_item_stages
        where production_order_item_id = @ItemId
        order by id asc",
        new { ItemId = itemId });

    public Task<ProductionItemStage?> GetByIdAsync(int id) => conn.QueryFirstOrDefaultAsync<ProductionItemStage>(@"
        select id as Id, production_order_item_id as ProductionOrderItemId, stage_type_id as StageTypeId,
               assigned_qa_user_id as AssignedQaUserId, is_completed as IsCompleted, completed_at as CompletedAt,
               rejection_count as RejectionCount, note as Note, created_at as CreatedAt, updated_at as UpdatedAt
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

    public Task ApproveAsync(int stageId) => conn.ExecuteAsync(@"
        update production_item_stages
        set is_completed = true, completed_at = now(), updated_at = now()
        where id = @Id",
        new { Id = stageId });

    public Task RejectAsync(int stageId) => conn.ExecuteAsync(@"
        update production_item_stages
        set assigned_qa_user_id = null, rejection_count = rejection_count + 1, updated_at = now()
        where id = @Id",
        new { Id = stageId });
}


