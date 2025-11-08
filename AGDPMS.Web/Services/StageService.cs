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
        var activeDefaults = defaults.Where(s => s.IsActive && s.IsDefault).OrderBy(s => s.DisplayOrder).ToList();
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

    public async Task AssignQaAsync(int stageId, int qaUserId)
    {
        var stage = await stageAccess.GetByIdAsync(stageId) ?? throw new InvalidOperationException("Stage not found");
        if (stage.IsCompleted) throw new InvalidOperationException("Stage already completed");
        await stageAccess.AssignQaAsync(stageId, qaUserId);
    }

    public async Task ApproveAsync(int stageId)
    {
        var stage = await stageAccess.GetByIdAsync(stageId) ?? throw new InvalidOperationException("Stage not found");
        if (stage.IsCompleted) return;

        await stageAccess.ApproveAsync(stageId);

        // After stage complete, check if all stages completed -> mark item completed
        await TryAutoCompleteItemAsync(stage.ProductionOrderItemId);
    }

    public async Task RejectAsync(int stageId, string reason, int rejectedByUserId, ProductionRejectReportDataAccess rejectAccess)
    {
        var stage = await stageAccess.GetByIdAsync(stageId) ?? throw new InvalidOperationException("Stage not found");
        if (stage.IsCompleted) throw new InvalidOperationException("Cannot reject a completed stage");

        await rejectAccess.CreateAsync(new ProductionRejectReport
        {
            ProductionItemStageId = stageId,
            RejectedByUserId = rejectedByUserId,
            Reason = reason
        });
        await stageAccess.RejectAsync(stageId);
    }

    public async Task CompleteStageByPmAsync(int stageId)
    {
        var stage = await stageAccess.GetByIdAsync(stageId) ?? throw new InvalidOperationException("Stage not found");
        if (stage.IsCompleted) return;
        await stageAccess.ApproveAsync(stageId);
        await TryAutoCompleteItemAsync(stage.ProductionOrderItemId);
    }

    public Task ForceCompleteItemAsync(int itemId) => itemAccess.SetCompletedAsync(itemId);

    public async Task<int> AddStageToItemAsync(int itemId, int stageTypeId)
    {
        // ensure no duplicate type per item
        var existing = await stageAccess.ListByItemAsync(itemId);
        if (existing.Any(s => s.StageTypeId == stageTypeId))
            throw new InvalidOperationException("Stage type already exists for this item");
        var id = await stageAccess.CreateAsync(itemId, stageTypeId);
        return id;
    }

    public async Task CancelStageAsync(int stageId)
    {
        var stage = await stageAccess.GetByIdAsync(stageId) ?? throw new InvalidOperationException("Stage not found");
        if (stage.IsCompleted) throw new InvalidOperationException("Cannot cancel a completed stage");
        await stageAccess.CancelAsync(stageId);
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


