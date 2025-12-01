using System.Data;
using Dapper;
using AGDPMS.Shared.Models;

namespace AGDPMS.Web.Data;

public class ProductionItemDataAccess(IDbConnection conn)
{
    public Task<ProductionOrderItem?> GetByIdAsync(int id) => conn.QueryFirstOrDefaultAsync<ProductionOrderItem>(@"
        select id as Id, production_order_id as ProductionOrderId, product_id as ProductId,
               line_no as LineNo, status as Status,
               planned_start_date as PlannedStartDate, planned_finish_date as PlannedFinishDate,
               actual_start_date as ActualStartDate, actual_finish_date as ActualFinishDate,
               qr_code as QRCode, qr_image as QRImage,
               is_completed as IsCompleted, completed_at as CompletedAt,
               is_canceled as IsCanceled,
               created_at as CreatedAt, updated_at as UpdatedAt
        from production_order_items
        where id = @Id",
        new { Id = id });

    public async Task<int> CreateItemAsync(ProductionOrderItem item)
    {
        var id = await conn.ExecuteScalarAsync<int>(@"
            insert into production_order_items(production_order_id, product_id, line_no, qr_code, qr_image, is_completed, created_at)
            values (@ProductionOrderId, @ProductId, @LineNo, @QRCode, @QRImage, false, now())
            returning id",
            new { item.ProductionOrderId, item.ProductId, item.LineNo, item.QRCode, item.QRImage });
        return id;
    }

    public Task<IEnumerable<ProductionOrderItem>> ListByOrderAsync(int orderId) => conn.QueryAsync<ProductionOrderItem>(@"
        select id as Id, production_order_id as ProductionOrderId, product_id as ProductId,
               line_no as LineNo, status as Status,
               planned_start_date as PlannedStartDate, planned_finish_date as PlannedFinishDate,
               actual_start_date as ActualStartDate, actual_finish_date as ActualFinishDate,
               qr_code as QRCode, qr_image as QRImage,
               is_completed as IsCompleted, completed_at as CompletedAt,
               is_canceled as IsCanceled,
               created_at as CreatedAt, updated_at as UpdatedAt
        from production_order_items
        where production_order_id = @OrderId
        order by line_no asc",
        new { OrderId = orderId });

    public Task SetCompletedAsync(int itemId) => conn.ExecuteAsync(@"
        update production_order_items
        set is_completed = true, completed_at = now()
        where id = @Id",
        new { Id = itemId });

    public Task SetQrAsync(int itemId, string url, byte[] imageBytes) => conn.ExecuteAsync(@"
        update production_order_items
        set qr_code = @Url, qr_image = @Image, updated_at = now()
        where id = @Id",
        new { Id = itemId, Url = url, Image = imageBytes });

    public Task SetCanceledAsync(int itemId) => conn.ExecuteAsync(@"
        update production_order_items
        set is_canceled = true, updated_at = now()
        where id = @Id",
        new { Id = itemId });

    public Task UpdatePlanAsync(int itemId, DateTime? plannedStartDate, DateTime? plannedFinishDate) => conn.ExecuteAsync(@"
        update production_order_items
        set planned_start_date = @PlannedStartDate,
            planned_finish_date = @PlannedFinishDate,
            updated_at = now()
        where id = @Id",
        new { Id = itemId, PlannedStartDate = plannedStartDate, PlannedFinishDate = plannedFinishDate });

    public Task UpdateActualsAsync(int itemId, DateTime? actualStartDate, DateTime? actualFinishDate) => conn.ExecuteAsync(@"
        update production_order_items
        set actual_start_date = @ActualStartDate,
            actual_finish_date = @ActualFinishDate,
            updated_at = now()
        where id = @Id",
        new { Id = itemId, ActualStartDate = actualStartDate, ActualFinishDate = actualFinishDate });

    public Task SetCompletionStatusAsync(int itemId, bool isCompleted) => conn.ExecuteAsync(@"
        update production_order_items
        set is_completed = @IsCompleted,
            completed_at = case when @IsCompleted then now() else null end,
            updated_at = now()
        where id = @Id",
        new { Id = itemId, IsCompleted = isCompleted });
}


