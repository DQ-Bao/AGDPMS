using AGDPMS.Shared.Models;
using AGDPMS.Shared.Models.DTOs;
using AGDPMS.Web.Data;

namespace AGDPMS.Web.Services;

public class ProjectCostManagementService(
    ProductionOrderDataAccess orderAccess,
    ProductionItemDataAccess itemAccess,
    ProductionItemStageDataAccess stageAccess,
    CavityDataAccess cavityAccess,
    GlobalLaborCostSettingDataAccess laborCostAccess,
    ProductionOrderSettingDataAccess orderSettingAccess,
    GlobalStageTimeSettingDataAccess globalTimeSettingAccess)
{
    public async Task<ProjectCostManagementDto> CalculateEVMAsync(int orderId, bool includeLaborCost = true)
    {
        var order = await orderAccess.GetByIdAsync(orderId);
        if (order == null)
            throw new InvalidOperationException("Order not found");

        var items = (await itemAccess.ListByOrderAsync(orderId)).ToList();
        var allStages = new List<ProductionItemStage>();
        foreach (var item in items)
        {
            var stages = await stageAccess.ListByItemAsync(item.Id);
            allStages.AddRange(stages);
        }

        // Get labor cost settings (global) and use a unified rate across all stages
        var laborCostSetting = await laborCostAccess.GetAsync();
        var unifiedHourlyRate = laborCostSetting?.HourlyRate ?? 100000m;
        
        // Get order-specific time settings if available (for future use if needed)
        var orderSettings = (await orderSettingAccess.GetByOrderIdAsync(orderId)).ToDictionary(s => s.StageTypeId);
        var globalTimeSettings = (await globalTimeSettingAccess.GetAllAsync()).ToDictionary(s => s.StageTypeId);

        var now = DateTime.UtcNow;
        var orderStartDate = order.PlannedStartDate ?? order.CreatedAt ?? now;
        var orderFinishDate = order.PlannedFinishDate ?? orderStartDate.AddDays(30);

        // Calculate BAC (Budget at Completion) - total planned cost
        decimal bac = 0;
        decimal plannedLaborCost = 0;
        decimal plannedMaterialCost = 0;
        decimal actualLaborCost = 0;
        decimal actualMaterialCost = 0;
        decimal ev = 0; // Earned Value
        decimal pv = 0; // Planned Value
        decimal ac = 0; // Actual Cost

        var itemBreakdowns = new List<ItemCostBreakdownDto>();

        foreach (var item in items)
        {
            var cavity = await cavityAccess.GetByIdWithBOMsAsync(item.CavityId);
            if (cavity == null) continue;

            var itemStages = allStages.Where(s => s.ProductionOrderItemId == item.Id).ToList();
            
            decimal itemPlannedLaborCost = 0;
            decimal itemActualLaborCost = 0;
            decimal itemPlannedMaterialCost = 0;
            decimal itemActualMaterialCost = 0;

            // Calculate material costs from cavity BOMs
            if (cavity.BOMs != null)
            {
                foreach (var bom in cavity.BOMs)
                {
                    if (bom.Material?.Stocks != null && bom.Material.Stocks.Any())
                    {
                        // Use first stock's base price (or average if multiple)
                        var avgPrice = bom.Material.Stocks.Average(s => s.BasePrice);
                        var materialCost = avgPrice * bom.Quantity;
                        itemPlannedMaterialCost += materialCost;
                        itemActualMaterialCost += materialCost; // Assume material cost is same for planned/actual
                    }
                }
            }

            // Calculate labor costs from stages (only if includeLaborCost is true)
            if (includeLaborCost)
            {
                foreach (var stage in itemStages)
                {
                    // Planned labor cost
                    if (stage.PlannedTimeHours.HasValue && stage.PlannedTimeHours.Value > 0)
                    {
                        itemPlannedLaborCost += stage.PlannedTimeHours.Value * unifiedHourlyRate;
                    }

                    // Actual labor cost
                    if (stage.ActualTimeHours.HasValue && stage.ActualTimeHours.Value > 0)
                    {
                        itemActualLaborCost += stage.ActualTimeHours.Value * unifiedHourlyRate;
                    }

                    // Calculate PV: Planned value up to current date
                    if (stage.PlannedTimeHours.HasValue && stage.PlannedStartDate.HasValue && stage.PlannedFinishDate.HasValue)
                    {
                        var stagePlannedCost = stage.PlannedTimeHours.Value * unifiedHourlyRate;
                        var stageDuration = (stage.PlannedFinishDate.Value - stage.PlannedStartDate.Value).TotalDays;
                        if (stageDuration > 0 && now >= stage.PlannedStartDate.Value)
                        {
                            var progressDays = Math.Min((now - stage.PlannedStartDate.Value).TotalDays, stageDuration);
                            var progressRatio = (decimal)(progressDays / stageDuration);
                            pv += stagePlannedCost * Math.Min(progressRatio, 1.0m);
                        }
                    }

                    // Calculate EV: Earned value for completed stages
                    if (stage.IsCompleted && stage.PlannedTimeHours.HasValue)
                    {
                        ev += stage.PlannedTimeHours.Value * unifiedHourlyRate;
                    }
                }
            }

            // Add material costs to PV/EV proportionally
            var itemTotalPlannedCost = itemPlannedLaborCost + itemPlannedMaterialCost;
            if (itemTotalPlannedCost > 0)
            {
                var laborRatio = itemPlannedLaborCost / itemTotalPlannedCost;
                var materialRatio = itemPlannedMaterialCost / itemTotalPlannedCost;
                
                // PV includes material cost proportionally
                if (includeLaborCost)
                {
                    var itemPVLabor = itemStages
                        .Where(s => s.PlannedTimeHours.HasValue && s.PlannedStartDate.HasValue && s.PlannedFinishDate.HasValue)
                        .Sum(s =>
                        {
                            var cost = s.PlannedTimeHours.Value * unifiedHourlyRate;
                            var duration = (s.PlannedFinishDate.Value - s.PlannedStartDate.Value).TotalDays;
                            if (duration > 0 && now >= s.PlannedStartDate.Value)
                            {
                                var progressDays = Math.Min((now - s.PlannedStartDate.Value).TotalDays, duration);
                                return cost * (decimal)(progressDays / duration);
                            }
                            return 0;
                        });
                    pv += itemPVLabor + (itemPlannedMaterialCost * materialRatio * (item.IsCompleted ? 1.0m : 0.5m));
                }
                else
                {
                    // Only material cost for PV
                    pv += itemPlannedMaterialCost * (item.IsCompleted ? 1.0m : 0.5m);
                }
                
                // EV includes material cost for completed items
                if (item.IsCompleted)
                {
                    ev += itemPlannedMaterialCost;
                }
            }
            else if (itemPlannedMaterialCost > 0)
            {
                // Only material cost, no labor
                pv += itemPlannedMaterialCost * (item.IsCompleted ? 1.0m : 0.5m);
                if (item.IsCompleted)
                {
                    ev += itemPlannedMaterialCost;
                }
            }

            plannedLaborCost += itemPlannedLaborCost;
            actualLaborCost += itemActualLaborCost;
            plannedMaterialCost += itemPlannedMaterialCost;
            actualMaterialCost += itemActualMaterialCost;
            ac += itemActualLaborCost + itemActualMaterialCost;

            itemBreakdowns.Add(new ItemCostBreakdownDto
            {
                ItemId = item.Id,
                ItemCode = item.Code,
                CavityName = cavity.Code + " - " + (cavity.Description ?? cavity.Location ?? ""),
                PlannedLaborCost = itemPlannedLaborCost,
                ActualLaborCost = itemActualLaborCost,
                PlannedMaterialCost = itemPlannedMaterialCost,
                ActualMaterialCost = itemActualMaterialCost,
                PlannedTotalCost = itemPlannedLaborCost + itemPlannedMaterialCost,
                ActualTotalCost = itemActualLaborCost + itemActualMaterialCost
            });
        }

        bac = plannedLaborCost + plannedMaterialCost;

        // Calculate variances
        var cv = ev - ac; // Cost Variance
        var sv = ev - pv; // Schedule Variance

        // Calculate performance indices
        decimal? cpi = ac > 0 ? ev / ac : null;
        decimal? spi = pv > 0 ? ev / pv : null;

        // Calculate forecasts
        decimal? eac = null;
        decimal? etc = null;
        decimal? vac = null;
        decimal? tcpi = null;

        if (cpi.HasValue && cpi.Value > 0)
        {
            eac = ac + (bac - ev) / cpi.Value;
            etc = eac - ac;
            vac = bac - eac;
        }

        if (bac > ac)
        {
            tcpi = (bac - ev) / (bac - ac);
        }

        return new ProjectCostManagementDto
        {
            PV = pv,
            EV = ev,
            AC = ac,
            BAC = bac,
            CV = cv,
            SV = sv,
            CPI = cpi,
            SPI = spi,
            EAC = eac,
            ETC = etc,
            VAC = vac,
            TCPI = tcpi,
            PlannedLaborCost = plannedLaborCost,
            ActualLaborCost = actualLaborCost,
            PlannedMaterialCost = plannedMaterialCost,
            ActualMaterialCost = actualMaterialCost,
            ItemBreakdowns = itemBreakdowns
        };
    }
}

