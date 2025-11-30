using AGDPMS.Shared.Models;
using AGDPMS.Web.Data;

namespace AGDPMS.Web.Services;

public class StageService(
    ProductionItemDataAccess itemAccess,
    StageTypeDataAccess stageTypeAccess,
    ProductionItemStageDataAccess stageAccess,
    ProductionOrderDataAccess orderAccess)
{
    public async Task InitializeDefaultStagesForItemAsync(int itemId)
    {
        var defaults = await stageTypeAccess.GetAllAsync();
        var activeDefaults = defaults.Where(s => s.IsActive && s.IsDefault).OrderBy(s => s.Id).ToList();
        var existing = await stageAccess.ListByItemAsync(itemId);
        var existingTypeIds = existing.Select(e => e.StageTypeId).ToHashSet();
        foreach (var st in activeDefaults)
        {
            if (!existingTypeIds.Contains(st.Id))
            {
                await stageAccess.CreateAsync(itemId, st.Id);
            }
        }
    }

    public async Task AutoCreateStagesForItemAsync(int itemId)
    {
        var allActive = await stageTypeAccess.GetAllAsync();
        var activeStages = allActive.Where(s => s.IsActive).OrderBy(s => s.Id).ToList();
        var existing = await stageAccess.ListByItemAsync(itemId);
        var existingTypeIds = existing.Select(e => e.StageTypeId).ToHashSet();
        foreach (var st in activeStages)
        {
            if (!existingTypeIds.Contains(st.Id))
            {
                await stageAccess.CreateAsync(itemId, st.Id);
            }
        }
    }

    public async Task AssignQaAsync(int stageId, int qaUserId)
    {
        var stage = await stageAccess.GetByIdAsync(stageId) ?? throw new InvalidOperationException("Stage not found");
        if (stage.IsCompleted) throw new InvalidOperationException("Stage already completed");
        await stageAccess.AssignQaAsync(stageId, qaUserId);
    }

    public async Task CompleteByPmAsync(int stageId)
    {
        var stage = await stageAccess.GetByIdAsync(stageId) ?? throw new InvalidOperationException("Stage not found");
        if (stage.IsCompleted) return;
        await stageAccess.CompleteByPmAsync(stageId);
        await TryAutoCompleteItemAsync(stage.ProductionOrderItemId);
    }

    public Task ForceCompleteItemAsync(int itemId) => itemAccess.SetCompletedAsync(itemId);

    public async Task SetItemCompletionStatusAsync(int itemId, bool isCompleted)
    {
        var item = await itemAccess.GetByIdAsync(itemId) ?? throw new InvalidOperationException("Item not found");
        var order = await orderAccess.GetByIdAsync(item.ProductionOrderId) ?? throw new InvalidOperationException("Order not found");
        if (order.Status == ProductionOrderStatus.Cancelled)
        {
            throw new InvalidOperationException("Không thể cập nhật trạng thái hoàn thành khi lệnh đã hủy");
        }
        if (order.Status != ProductionOrderStatus.InProduction)
        {
            throw new InvalidOperationException("Chỉ có thể đánh dấu hoàn thành khi lệnh đang sản xuất (InProduction)");
        }
        await itemAccess.SetCompletionStatusAsync(itemId, isCompleted);
    }

    public async Task<int> AddStageToItemAsync(int itemId, int stageTypeId)
    {
        // ensure no duplicate type per item
        var existing = await stageAccess.ListByItemAsync(itemId);
        if (existing.Any(s => s.StageTypeId == stageTypeId))
            throw new InvalidOperationException("Stage type already exists for this item");
        var id = await stageAccess.CreateAsync(itemId, stageTypeId);
        return id;
    }

    public async Task UpdatePlanAsync(int stageId, DateTime? plannedStartDate, DateTime? plannedFinishDate, decimal? plannedTimeHours)
    {
        // Chỉ cho phép cập nhật kế hoạch khi order ở trạng thái Draft
        var stage = await stageAccess.GetByIdAsync(stageId) ?? throw new InvalidOperationException("Stage not found");
        var item = await itemAccess.GetByIdAsync(stage.ProductionOrderItemId) ?? throw new InvalidOperationException("Item not found");
        var order = await orderAccess.GetByIdAsync(item.ProductionOrderId) ?? throw new InvalidOperationException("Order not found");
        
        if (order.Status != ProductionOrderStatus.Draft)
        {
            throw new InvalidOperationException("Chỉ có thể cập nhật kế hoạch khi order ở trạng thái Draft");
        }
        
        await stageAccess.UpdatePlanAsync(stageId, plannedStartDate, plannedFinishDate, plannedTimeHours);
    }

    public Task UpdateActualDatesAsync(int stageId, DateTime? actualStartDate, DateTime? actualFinishDate, decimal? actualTimeHours)
    {
        return stageAccess.UpdateActualDatesAsync(stageId, actualStartDate, actualFinishDate, actualTimeHours);
    }

    public Task BulkAssignQaToItemAsync(int itemId, int qaUserId)
    {
        return stageAccess.BulkAssignQaAsync(itemId, qaUserId);
    }

    public async Task UpdateItemPlanAsync(int itemId, DateTime? plannedStartDate, DateTime? plannedFinishDate)
    {
        var item = await itemAccess.GetByIdAsync(itemId) ?? throw new InvalidOperationException("Item not found");
        var order = await orderAccess.GetByIdAsync(item.ProductionOrderId) ?? throw new InvalidOperationException("Order not found");
        if (order.Status != ProductionOrderStatus.Draft)
        {
            throw new InvalidOperationException("Chỉ có thể cập nhật kế hoạch khi order ở trạng thái Draft");
        }
        await itemAccess.UpdatePlanAsync(itemId, plannedStartDate, plannedFinishDate);
    }

    public async Task UpdateItemActualsAsync(int itemId, DateTime? actualStartDate, DateTime? actualFinishDate)
    {
        var item = await itemAccess.GetByIdAsync(itemId) ?? throw new InvalidOperationException("Item not found");
        var order = await orderAccess.GetByIdAsync(item.ProductionOrderId) ?? throw new InvalidOperationException("Order not found");
        if (order.Status != ProductionOrderStatus.InProduction)
        {
            throw new InvalidOperationException("Chỉ có thể cập nhật thông tin thực tế khi order đang sản xuất");
        }
        await itemAccess.UpdateActualsAsync(itemId, actualStartDate, actualFinishDate);
    }

    private async Task TryAutoCompleteItemAsync(int itemId)
    {
        var stages = await stageAccess.ListByItemAsync(itemId);
        if (!stages.Any()) return;
        if (stages.All(s => s.IsCompleted))
        {
            await itemAccess.SetCompletedAsync(itemId);

            // if all items in the order are completed, auto-finish order
            var item = await itemAccess.GetByIdAsync(itemId);
            if (item is not null)
            {
                var siblings = await itemAccess.ListByOrderAsync(item.ProductionOrderId);
                if (siblings.All(i => i.IsCompleted))
                {
                    await orderAccess.MarkFinishedAsync(item.ProductionOrderId);
                }
            }
        }
    }
}


