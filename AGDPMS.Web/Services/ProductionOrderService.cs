using AGDPMS.Shared.Models;
using AGDPMS.Shared.Models.DTOs;
using AGDPMS.Shared.Services;
using AGDPMS.Web.Data;
using System.Data;
using Dapper;

namespace AGDPMS.Web.Services;

public class ProductionOrderService(
    ProductionOrderDataAccess orders,
    ProductionItemDataAccess items,
    StageTypeDataAccess stageTypes,
    StageService stageService,
    QrService qrService,
    StageTimeEstimationService stageTimeEstimationService,
    ProductionOrderSettingDataAccess orderSettingAccess,
    UserDataAccess userAccess,
    IDbConnection dbConnection,
    INotificationService notificationService)
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
        var codeSequence = 1;
        foreach (var it in spec.Items)
        {
            // Create N items where N = quantity (each item is one physical unit of the cavity)
            for (int i = 0; i < it.Quantity; i++)
            {
                var itemCode = GenerateItemCode(codeSequence++);
                var itemId = await items.CreateItemAsync(new ProductionOrderItem
                {
                    ProductionOrderId = orderId,
                    CavityId = it.CavityId,
                    Code = itemCode,
                    LineNo = line++
                });
                // Initialize stages for item
                await stageService.InitializeDefaultStagesForItemAsync(itemId);
                // Auto-estimate planned hours based on cavity BOMs
                await stageTimeEstimationService.EstimateAndApplyPlannedHoursAsync(itemId);
                // Auto-assign QA with least pending items to all stages
                await AutoAssignQaToItemStagesAsync(itemId);
                // Generate QR and store - pass both orderId and itemId for full URL
                var (url, png) = qrService.GenerateForItem(orderId, itemId);
                await items.SetQrAsync(itemId, url, png);
            }
        }

        // Save order-specific time settings if provided
        if (spec.TimeSettings != null && spec.TimeSettings.Any())
        {
            foreach (var timeSetting in spec.TimeSettings)
            {
                await orderSettingAccess.UpsertAsync(new ProductionOrderSetting
                {
                    ProductionOrderId = orderId,
                    StageTypeId = timeSetting.StageTypeId,
                    SetupMinutes = timeSetting.SetupMinutes,
                    FinishMinutes = timeSetting.FinishMinutes,
                    CutAlMinutesPerUnit = timeSetting.CutAlMinutesPerUnit,
                    MillLockMinutesPerGroup = timeSetting.MillLockMinutesPerGroup,
                    CornerCutMinutesPerCorner = timeSetting.CornerCutMinutesPerCorner,
                    AssembleFrameMinutesPerCorner = timeSetting.AssembleFrameMinutesPerCorner,
                    CutGlassMinutesPerSquareMeter = timeSetting.CutGlassMinutesPerSquareMeter,
                    GlassInstallMinutesPerUnit = timeSetting.GlassInstallMinutesPerUnit,
                    GasketMinutesPerUnit = timeSetting.GasketMinutesPerUnit,
                    AccessoryMinutesPerUnit = timeSetting.AccessoryMinutesPerUnit,
                    CutFlushMinutesPerUnit = timeSetting.CutFlushMinutesPerUnit,
                    SiliconMinutesPerMeter = timeSetting.SiliconMinutesPerMeter
                });
            }
        }

        return orderId;
    }

    private string GenerateItemCode(int sequence)
    {
        // Generate S001, S002, S003... format (4 chars max)
        if (sequence > 999)
            throw new InvalidOperationException("Cannot generate more than 999 items per order");
        return $"S{sequence:D3}";
    }

    private async Task AutoAssignQaToItemStagesAsync(int itemId)
    {
        // Get all QA users
        var allUsers = await userAccess.GetAllAsync();
        var qaUsers = allUsers.Where(u => string.Equals(u.Role.Name, "QA", StringComparison.OrdinalIgnoreCase)).ToList();
        
        if (!qaUsers.Any())
        {
            // No QA users available, skip assignment
            return;
        }

        // Get counts of pending items for each QA
        var qaIds = qaUsers.Select(u => u.Id).ToList();
        var itemCounts = new Dictionary<int, int>();
        
        if (dbConnection.State != System.Data.ConnectionState.Open)
        {
            dbConnection.Open();
        }

        var counts = await dbConnection.QueryAsync<(int? QaUserId, int Count)>(@"
            select pis.assigned_qa_user_id as QaUserId, count(distinct pis.id) as Count
            from production_item_stages pis
            where pis.assigned_qa_user_id = any(@QaIds)
              and pis.is_completed = false
              and not exists (
                  select 1 from stage_reviews sr
                  where sr.production_item_stage_id = pis.id
                    and sr.status in ('pending', 'in_progress')
              )
            group by pis.assigned_qa_user_id",
            new { QaIds = qaIds });
        
        foreach (var result in counts)
        {
            if (result.QaUserId.HasValue)
            {
                itemCounts[result.QaUserId.Value] = result.Count;
            }
        }

        // Find QA with least pending items (or 0 if not in dictionary)
        var qaWithLeastItems = qaUsers
            .OrderBy(qa => itemCounts.GetValueOrDefault(qa.Id, 0))
            .ThenBy(qa => qa.FullName)
            .First();

        // Assign this QA to all stages of the item
        await stageService.BulkAssignQaToItemAsync(itemId, qaWithLeastItems.Id);
    }

    public async Task SubmitAsync(int orderId)
    {
        var order = await orders.GetByIdAsync(orderId) ?? throw new InvalidOperationException("Order not found");
        if (order.Status != ProductionOrderStatus.Draft)
            throw new InvalidOperationException("Only draft orders can be submitted");
        await orders.SubmitAsync(orderId);
        
        // Send notification to all Directors
        try
        {
            var allUsers = await userAccess.GetAllAsync();
            var directors = allUsers.Where(u => 
                string.Equals(u.Role.Name, "Director", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(u.Role.Name, "Admin", StringComparison.OrdinalIgnoreCase)
            ).ToList();
            
            foreach (var director in directors)
            {
                await notificationService.AddNotificationAsync(new Notification
                {
                    Message = $"Lệnh sản xuất {order.Code} đã được gửi yêu cầu phê duyệt",
                    Url = $"/production/orders/{orderId}",
                    RecipientUserId = director.Id.ToString()
                });
            }
        }
        catch (Exception ex)
        {
            // Log error but don't fail the request
            System.Diagnostics.Debug.WriteLine($"Failed to send notification: {ex.Message}");
        }
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
    public List<ProductionOrderTimeSettingsDto>? TimeSettings { get; set; }
}

public class ProductionOrderCreateSpecItem
{
    public int CavityId { get; set; }
    public int Quantity { get; set; }
}
