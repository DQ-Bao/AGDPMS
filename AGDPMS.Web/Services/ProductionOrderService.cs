using AGDPMS.Shared.Models;
using AGDPMS.Web.Data;

namespace AGDPMS.Web.Services;

public class ProductionOrderService(
    ProductionOrderDataAccess orders,
    ProductionItemDataAccess items,
    StageTypeDataAccess stageTypes,
    StageService stageService,
    QrService qrService)
{
    public async Task<int> CreateOrderAsync(ProductionOrderCreateSpec spec, int createdBy)
    {
        var code = await orders.GenerateNextCodeAsync();
        var order = new ProductionOrder
        {
            Code = code,
            ProjectId = spec.ProjectId,
            Status = ProductionOrderStatus.Draft,
            CreatedBy = createdBy
        };
        var orderId = await orders.CreateAsync(order);

        // line numbering starts at 1
        var line = 1;
        foreach (var it in spec.Items)
        {
            var itemId = await items.CreateItemAsync(new ProductionOrderItem
            {
                ProductionOrderId = orderId,
                ProductId = it.ProductId,
                LineNo = line++
            });
            // Initialize stages for item
            await stageService.InitializeDefaultStagesForItemAsync(itemId);
            // Generate QR and store - pass both orderId and itemId for full URL
            var (url, png) = qrService.GenerateForItem(orderId, itemId);
            await items.SetQrAsync(itemId, url, png);
        }
        return orderId;
    }

    public async Task SubmitAsync(int orderId)
    {
        var order = await orders.GetByIdAsync(orderId) ?? throw new InvalidOperationException("Order not found");
        if (order.Status != ProductionOrderStatus.Draft)
            throw new InvalidOperationException("Only draft orders can be submitted");
        await orders.SubmitAsync(orderId);
    }

    public async Task CancelAsync(int orderId)
    {
        await orders.MarkCancelledAsync(orderId);
    }

    public async Task DirectorApproveAsync(int orderId)
    {
        var order = await orders.GetByIdAsync(orderId) ?? throw new InvalidOperationException("Order not found");
        if (order.Status != ProductionOrderStatus.PendingDirectorApproval)
            throw new InvalidOperationException("Invalid state for director approval");
        await orders.DirectorApproveAsync(orderId);
    }

    public async Task DirectorRejectAsync(int orderId)
    {
        var order = await orders.GetByIdAsync(orderId) ?? throw new InvalidOperationException("Order not found");
        if (order.Status != ProductionOrderStatus.PendingDirectorApproval)
            throw new InvalidOperationException("Invalid state for director rejection");
        await orders.DirectorRejectAsync(orderId);
    }

    public async Task QaMachinesApproveAsync(int orderId)
    {
        var order = await orders.GetByIdAsync(orderId) ?? throw new InvalidOperationException("Order not found");
        if (order.Status != ProductionOrderStatus.PendingQACheckMachines)
            throw new InvalidOperationException("Invalid state for QA machines check");
        await orders.QaMachinesApproveAsync(orderId);
    }

    public async Task QaMaterialApproveAsync(int orderId)
    {
        var order = await orders.GetByIdAsync(orderId) ?? throw new InvalidOperationException("Order not found");
        if (order.Status != ProductionOrderStatus.PendingQACheckMaterial)
            throw new InvalidOperationException("Invalid state for QA material check");
        await orders.QaMaterialApproveAsync(orderId);
    }

    public async Task FinishAsync(int orderId)
    {
        var order = await orders.GetByIdAsync(orderId) ?? throw new InvalidOperationException("Order not found");
        if (order.Status != ProductionOrderStatus.InProduction)
            throw new InvalidOperationException("Only orders in production can be finished");
        await orders.MarkFinishedAsync(orderId);
    }

    public async Task UpdateOrderPlanAsync(int orderId, DateTime? plannedStartDate, DateTime? plannedFinishDate)
    {
        var order = await orders.GetByIdAsync(orderId) ?? throw new InvalidOperationException("Order not found");
        await orders.UpdatePlanAsync(orderId, plannedStartDate, plannedFinishDate);
    }

    public async Task RevertToDraftAsync(int orderId)
    {
        var order = await orders.GetByIdAsync(orderId) ?? throw new InvalidOperationException("Order not found");
        if (order.Status != ProductionOrderStatus.PendingDirectorApproval)
            throw new InvalidOperationException("Only orders pending director approval can be reverted to draft");
        await orders.RevertToDraftAsync(orderId);
    }
}

public class ProductionOrderCreateSpec
{
    public int ProjectId { get; set; }
    public List<ProductionOrderCreateSpecItem> Items { get; set; } = new();
}

public class ProductionOrderCreateSpecItem
{
    public int ProductId { get; set; }
}


