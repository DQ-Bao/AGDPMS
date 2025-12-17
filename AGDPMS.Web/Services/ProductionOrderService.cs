using AGDPMS.Shared.Models;
using AGDPMS.Shared.Models.DTOs;
using AGDPMS.Shared.Services;
using AGDPMS.Web.Data;
using System.Data;
using Dapper;
using System.Linq;

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
        var qaForOrder = await PickQaForOrderAsync();
        var order = new ProductionOrder
        {
            Code = code,
            ProjectId = spec.ProjectId,
            Status = ProductionOrderStatus.Draft,
            CreatedBy = createdBy,
            AssignedQaUserId = qaForOrder
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
                // Auto-assign QA with least pending items to all stages (reuse order-level QA if chosen)
                await AutoAssignQaToItemStagesAsync(itemId, qaForOrder);
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

    private async Task<int?> PickQaForOrderAsync()
    {
        var allUsers = await userAccess.GetAllAsync();
        var qaUsers = allUsers.Where(u => string.Equals(u.Role.Name, "QA", StringComparison.OrdinalIgnoreCase)).ToList();
        if (!qaUsers.Any()) return null;

        var qaIds = qaUsers.Select(u => u.Id).ToList();
        var workloads = await GetQaOrderWorkloadsAsync(qaIds);

        // Find QA with least workload
        var qaWithLeast = qaUsers
            .OrderBy(u => workloads.GetValueOrDefault(u.Id, 0))
            .ThenBy(u => u.Id) // Use ID instead of name for deterministic but fair distribution
            .FirstOrDefault();
        
        return qaWithLeast?.Id;
    }

    private async Task<Dictionary<int, int>> GetQaOrderWorkloadsAsync(List<int> qaIds)
    {
        var dict = new Dictionary<int, int>();
        if (!qaIds.Any()) return dict;

        if (dbConnection.State != ConnectionState.Open)
        {
            dbConnection.Open();
        }

        // Count ALL orders assigned to QA (including Draft) to get accurate workload
        // This ensures we distribute new orders fairly even when QAs have orders in Draft status
        var rows = await dbConnection.QueryAsync<(int? QaUserId, int Count)>(@"
            select assigned_qa_user_id as QaUserId,
                   count(*) as Count
            from production_orders
            where assigned_qa_user_id = any(@QaIds)
              and is_cancelled = false
              and status <= @MaxStatus
            group by assigned_qa_user_id",
            new
            {
                QaIds = qaIds,
                MaxStatus = (short)ProductionOrderStatus.InProduction // Include Draft, PendingDirectorApproval, etc.
            });

        foreach (var row in rows)
        {
            if (row.QaUserId.HasValue)
            {
                dict[row.QaUserId.Value] = row.Count;
            }
        }
        return dict;
    }

    private async Task AutoAssignQaToItemStagesAsync(int itemId, int? preferredQaId = null)
    {
        if (preferredQaId.HasValue)
        {
            await stageService.BulkAssignQaToItemAsync(itemId, preferredQaId.Value);
            return;
        }

        var allUsers = await userAccess.GetAllAsync();
        var qaUsers = allUsers.Where(u => string.Equals(u.Role.Name, "QA", StringComparison.OrdinalIgnoreCase)).ToList();
        
        if (!qaUsers.Any())
        {
            return;
        }

        var qaIds = qaUsers.Select(u => u.Id).ToList();
        var itemCounts = new Dictionary<int, int>();
        
        if (dbConnection.State != System.Data.ConnectionState.Open)
        {
            dbConnection.Open();
        }

        // Count ALL pending stages assigned to QA (including from Draft orders) for accurate workload
        var counts = await dbConnection.QueryAsync<(int? QaUserId, int Count)>(@"
            select pis.assigned_qa_user_id as QaUserId, count(distinct pis.id) as Count
            from production_item_stages pis
            join production_order_items poi on poi.id = pis.production_order_item_id
            join production_orders po on po.id = poi.production_order_id
            where pis.assigned_qa_user_id = any(@QaIds)
              and pis.is_completed = false
              and po.status <= @MaxStatus
              and po.is_cancelled = false
              and not exists (
                  select 1 from stage_reviews sr
                  where sr.production_item_stage_id = pis.id
                    and sr.status in ('pending', 'in_progress')
              )
            group by pis.assigned_qa_user_id",
            new
            {
                QaIds = qaIds,
                MaxStatus = (short)ProductionOrderStatus.InProduction // Include Draft, PendingDirectorApproval, etc.
            });
        
        foreach (var result in counts)
        {
            if (result.QaUserId.HasValue)
            {
                itemCounts[result.QaUserId.Value] = result.Count;
            }
        }

        // Find QA with least workload, use ID for deterministic distribution when workloads are equal
        var qaWithLeast = qaUsers
            .OrderBy(qa => itemCounts.GetValueOrDefault(qa.Id, 0))
            .ThenBy(qa => qa.Id) // Use ID instead of name for round-robin effect
            .First();

        await stageService.BulkAssignQaToItemAsync(itemId, qaWithLeast.Id);
    }

    public async Task SubmitAsync(int orderId)
    {
        var order = await orders.GetByIdAsync(orderId) ?? throw new InvalidOperationException("Order not found");
        if (order.Status != ProductionOrderStatus.Draft)
            throw new InvalidOperationException("Only draft orders can be submitted");
        await orders.SubmitAsync(orderId);
        await NotifyByRolesAsync(new[] { "Director", "Admin" }, $"Lệnh sản xuất {order.Code} đã được gửi yêu cầu phê duyệt", $"/production/orders/{orderId}");
    }

    public async Task CancelAsync(int orderId)
    {
        var order = await orders.GetByIdAsync(orderId) ?? throw new InvalidOperationException("Order not found");
        if (order.Status is not (ProductionOrderStatus.Draft or ProductionOrderStatus.PendingDirectorApproval or ProductionOrderStatus.PendingQACheckMachines or ProductionOrderStatus.PendingQACheckMaterial or ProductionOrderStatus.InProduction))
            throw new InvalidOperationException("Chỉ có thể hủy lệnh từ Nháp đến Đang sản xuất");
        await orders.MarkCancelledAsync(orderId);
        await NotifyPmAndDirectorsAsync($"Lệnh sản xuất {order.Code} đã bị hủy", $"/production/orders/{orderId}");
    }

    public async Task AssignQaAsync(int orderId, int qaUserId)
    {
        var order = await orders.GetByIdAsync(orderId) ?? throw new InvalidOperationException("Order not found");
        if (order.Status is not (ProductionOrderStatus.Draft or ProductionOrderStatus.PendingDirectorApproval or ProductionOrderStatus.PendingQACheckMachines or ProductionOrderStatus.PendingQACheckMaterial or ProductionOrderStatus.InProduction))
            throw new InvalidOperationException("Chỉ gán QA khi lệnh từ Nháp đến Đang sản xuất");

        await orders.SetAssignedQaAsync(orderId, qaUserId);
    }

    public async Task DirectorApproveAsync(int orderId)
    {
        var order = await orders.GetByIdAsync(orderId) ?? throw new InvalidOperationException("Order not found");
        if (order.Status != ProductionOrderStatus.PendingDirectorApproval)
            throw new InvalidOperationException("Invalid state for director approval");

        if (!order.AssignedQaUserId.HasValue)
        {
            var qaForOrder = await PickQaForOrderAsync();
            if (qaForOrder.HasValue)
            {
                await orders.SetAssignedQaAsync(orderId, qaForOrder.Value);
            }
        }

        await orders.DirectorApproveAsync(orderId);
        await NotifyAllPmAsync($"Lệnh sản xuất {order.Code} đã được giám đốc duyệt", $"/production/orders/{orderId}");
        await NotifyAllQaAsync($"Lệnh sản xuất {order.Code} đang chờ QA kiểm tra máy", $"/production/orders/{orderId}");
    }

    public async Task DirectorRejectAsync(int orderId)
    {
        var order = await orders.GetByIdAsync(orderId) ?? throw new InvalidOperationException("Order not found");
        if (order.Status != ProductionOrderStatus.PendingDirectorApproval)
            throw new InvalidOperationException("Invalid state for director rejection");
        await orders.DirectorRejectAsync(orderId);
        await NotifyAllPmAsync($"Lệnh sản xuất {order.Code} đã bị giám đốc từ chối", $"/production/orders/{orderId}");
    }

    public async Task QaMachinesApproveAsync(int orderId)
    {
        var order = await orders.GetByIdAsync(orderId) ?? throw new InvalidOperationException("Order not found");
        if (order.Status != ProductionOrderStatus.PendingQACheckMachines)
            throw new InvalidOperationException("Invalid state for QA machines check");
        await orders.QaMachinesApproveAsync(orderId);
        await NotifyAllQaAsync($"Lệnh sản xuất {order.Code} đang chờ QA kiểm tra vật tư", $"/production/orders/{orderId}");
    }

    public async Task QaMaterialApproveAsync(int orderId)
    {
        var order = await orders.GetByIdAsync(orderId) ?? throw new InvalidOperationException("Order not found");
        if (order.Status != ProductionOrderStatus.PendingQACheckMaterial)
            throw new InvalidOperationException("Invalid state for QA material check");
        await orders.QaMaterialApproveAsync(orderId);
        await NotifyAllPmAsync($"Lệnh sản xuất {order.Code} đã chuyển sang giai đoạn sản xuất", $"/production/orders/{orderId}");
    }

    public async Task FinishAsync(int orderId)
    {
        var order = await orders.GetByIdAsync(orderId) ?? throw new InvalidOperationException("Order not found");
        if (order.Status != ProductionOrderStatus.InProduction)
            throw new InvalidOperationException("Only orders in production can be finished");
        await orders.MarkFinishedAsync(orderId);
        await NotifyPmAndDirectorsAsync($"Lệnh sản xuất {order.Code} đã hoàn thành", $"/production/orders/{orderId}");
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

    private async Task NotifyAllQaAsync(string message, string url)
    {
        await NotifyByRolesAsync(new[] { "QA", "Qa" }, message, url);
    }

    private async Task NotifyAllPmAsync(string message, string url)
    {
        await NotifyByRolesAsync(new[] { "Production Manager", "ProductionManager" }, message, url);
    }

    private async Task NotifyPmAndDirectorsAsync(string message, string url)
    {
        await NotifyByRolesAsync(new[] { "Production Manager", "ProductionManager", "Director", "Admin" }, message, url);
    }

    private async Task NotifyByRolesAsync(IEnumerable<string> roles, string message, string url)
    {
        try
        {
            var allUsers = await userAccess.GetAllAsync();
            var roleSet = roles.ToHashSet(StringComparer.OrdinalIgnoreCase);
            var recipients = allUsers.Where(u => roleSet.Contains(u.Role.Name ?? string.Empty)).ToList();
            foreach (var user in recipients)
            {
                await notificationService.AddNotificationAsync(new Notification
                {
                    Message = message,
                    Url = url,
                    RecipientUserId = user.Id.ToString()
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to send notification: {ex.Message}");
        }
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
