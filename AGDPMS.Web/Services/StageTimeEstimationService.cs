using AGDPMS.Shared.Models;
using AGDPMS.Web.Data;

namespace AGDPMS.Web.Services;

/// <summary>
/// Estimates PlannedTimeHours for stages of a production item based on its cavity BOMs.
/// Each ProductionOrderItem is treated as 1 physical unit of the cavity (no cavity quantity applied).
/// </summary>
public class StageTimeEstimationService(
    ProductionItemDataAccess itemAccess,
    CavityDataAccess cavityAccess,
    ProductionItemStageDataAccess stageAccess,
    StageTypeDataAccess stageTypeAccess,
    StageService stageService,
    ProductionOrderSettingDataAccess orderSettingAccess,
    GlobalStageTimeSettingDataAccess globalTimeSettingAccess)
{
    public async Task EstimateAndApplyPlannedHoursAsync(int itemId)
    {
        var item = await itemAccess.GetByIdAsync(itemId);
        if (item is null) return;

        // Load time settings (order-specific first, then global)
        var orderSettings = (await orderSettingAccess.GetByOrderIdAsync(item.ProductionOrderId)).ToDictionary(s => s.StageTypeId);
        var globalSettings = (await globalTimeSettingAccess.GetAllAsync()).ToDictionary(s => s.StageTypeId);

        // Load cavity with BOMs for this item
        var cavity = await cavityAccess.GetByIdWithBOMsAsync(item.CavityId);
        if (cavity is null || cavity.BOMs is null || cavity.BOMs.Count == 0)
            return;

        var stages = (await stageAccess.ListByItemAsync(itemId)).ToList();
        if (stages.Count == 0)
            return;

        var stageTypes = (await stageTypeAccess.GetAllAsync()).ToDictionary(st => st.Id);

        // Aggregate BOM quantities for this single cavity/item (no cavity quantity multiplier).
        double aluminumQty = 0;
        double glassQty = 0;
        double gasketQty = 0;
        double accessoryQty = 0;

        double glassAreaM2 = 0;      // width * height * qty, converted from mm^2
        double siliconLengthM = 0;   // length * qty, converted from mm to m

        foreach (var bom in cavity.BOMs)
        {
            if (bom.Material?.Type is null) continue;

            var qty = (double)bom.Quantity;
            var lengthMm = (double)bom.Length;
            var widthMm = (double)bom.Width;

            if (bom.Material.Type == MaterialType.Aluminum)
            {
                aluminumQty += qty;
                // For silicon, use aluminum profile length as an approximation of sealant run.
                if (lengthMm > 0)
                    siliconLengthM += qty * (lengthMm / 1000.0);
            }
            else if (bom.Material.Type == MaterialType.Glass)
            {
                glassQty += qty;
                if (lengthMm > 0 && widthMm > 0)
                {
                    var areaMm2 = qty * lengthMm * widthMm;
                    glassAreaM2 += areaMm2 / 1_000_000.0;
                }
            }
            else if (bom.Material.Type == MaterialType.Gasket)
            {
                gasketQty += qty;
            }
            else if (bom.Material.Type == MaterialType.Accessory)
            {
                accessoryQty += qty;
            }
        }

        // Estimate each stage and write PlannedTimeHours / PlannedUnits using StageService (enforces Draft state).
        foreach (var stage in stages)
        {
            if (!stageTypes.TryGetValue(stage.StageTypeId, out var stageType))
                continue;

            var code = stageType.Code;
            var timeConfig = GetTimeConfig(code, stage.StageTypeId, orderSettings, globalSettings);
            decimal? hours = EstimateHoursForStage(
                code,
                timeConfig,
                aluminumQty,
                glassQty,
                gasketQty,
                accessoryQty,
                glassAreaM2,
                siliconLengthM);
            int? plannedUnits = EstimatePlannedUnitsForStage(
                code,
                aluminumQty,
                glassQty,
                gasketQty,
                accessoryQty);

                // Keep existing planned dates, only update PlannedTimeHours and PlannedUnits
            try
            {
                    await stageService.UpdatePlanAsync(stage.Id, stage.PlannedStartDate, stage.PlannedFinishDate, hours);
                    if (plannedUnits.HasValue)
                    {
                        await stageAccess.UpdatePlannedUnitsAsync(stage.Id, plannedUnits.Value);
                    }
            }
            catch
            {
                // If order is not in Draft or any rule fails, ignore and continue.
            }
        }
    }

    private static decimal? EstimateHoursForStage(
        string stageCode,
        StageTimeConfigValues timeConfig,
        double aluminumQty,
        double glassQty,
        double gasketQty,
        double accessoryQty,
        double glassAreaM2,
        double siliconLengthM)
    {
        // If there is no relevant material, return null so the stage can be hidden/ignored.
        double doMinutes = 0;

        switch (stageCode)
        {
            case "CUT_AL":
                if (aluminumQty <= 0) return null;
                doMinutes = aluminumQty * timeConfig.DoMinutes;
                break;

            case "MILL_LOCK":
                if (aluminumQty <= 0) return null;
                doMinutes = (aluminumQty / 4.0) * timeConfig.DoMinutes;
                break;

            case "DOOR_CORNER_CUT":
                doMinutes = StageTimeConfig.DefaultCornerCountPerItem * timeConfig.DoMinutes;
                break;

            case "ASSEMBLE_FRAME":
                doMinutes = StageTimeConfig.DefaultCornerCountPerItem * timeConfig.DoMinutes;
                break;

            case "CUT_GLASS":
                if (glassAreaM2 <= 0) return null;
                doMinutes = glassAreaM2 * timeConfig.DoMinutes;
                break;

            case "GLASS_INSTALL":
                if (glassQty <= 0) return null;
                doMinutes = glassQty * timeConfig.DoMinutes;
                break;

            case "PRESS_GASKET":
                if (gasketQty <= 0) return null;
                doMinutes = gasketQty * timeConfig.DoMinutes;
                break;

            case "INSTALL_ACCESSORIES":
                if (accessoryQty <= 0) return null;
                doMinutes = accessoryQty * timeConfig.DoMinutes;
                break;

            case "CUT_FLUSH":
                if (aluminumQty <= 0) return null;
                doMinutes = aluminumQty * timeConfig.DoMinutes;
                break;

            case "FINISH_SILICON":
                if (siliconLengthM <= 0) return null;
                doMinutes = siliconLengthM * timeConfig.DoMinutes;
                break;

            default:
                // For other stages we don't have a formula yet.
                return null;
        }

        var totalMinutes = (double)timeConfig.SetupMinutes + doMinutes + (double)timeConfig.FinishMinutes;
        var hours = Math.Round(totalMinutes / 60.0, 2);
        if (hours <= 0) return null;
        return (decimal)hours;
    }

    private static int? EstimatePlannedUnitsForStage(
        string stageCode,
        double aluminumQty,
        double glassQty,
        double gasketQty,
        double accessoryQty)
    {
        return stageCode switch
        {
            "CUT_AL" or "CUT_FLUSH" => aluminumQty > 0 ? (int?)Math.Round(aluminumQty) : null,
            "MILL_LOCK"             => aluminumQty > 0 ? (int?)Math.Max(1, Math.Round(aluminumQty / 4.0)) : null,
            "DOOR_CORNER_CUT"
              or "ASSEMBLE_FRAME"   => StageTimeConfig.DefaultCornerCountPerItem,
            "CUT_GLASS"
              or "GLASS_INSTALL"    => glassQty > 0 ? (int?)Math.Round(glassQty) : null,
            "PRESS_GASKET"          => gasketQty > 0 ? (int?)Math.Round(gasketQty) : null,
            "INSTALL_ACCESSORIES"   => accessoryQty > 0 ? (int?)Math.Round(accessoryQty) : null,
            "FINISH_SILICON"        => aluminumQty > 0 ? (int?)Math.Round(aluminumQty) : null,
            _                       => null
        };
    }

    private record StageTimeConfigValues(decimal SetupMinutes, decimal FinishMinutes, double DoMinutes);

    private StageTimeConfigValues GetTimeConfig(
        string stageCode,
        int stageTypeId,
        Dictionary<int, ProductionOrderSetting> orderSettings,
        Dictionary<int, GlobalStageTimeSetting> globalSettings)
    {
        orderSettings.TryGetValue(stageTypeId, out var orderSetting);
        globalSettings.TryGetValue(stageTypeId, out var globalSetting);

        decimal setup = orderSetting?.SetupMinutes
            ?? globalSetting?.SetupMinutes
            ?? (decimal)StageTimeConfig.SetupMinutes;

        decimal finish = orderSetting?.FinishMinutes
            ?? globalSetting?.FinishMinutes
            ?? (decimal)StageTimeConfig.FinishMinutes;

        decimal? doMinutes = stageCode switch
        {
            "CUT_AL" => orderSetting?.CutAlMinutesPerUnit ?? globalSetting?.CutAlMinutesPerUnit ?? (decimal)StageTimeConfig.CutAlMinutesPerUnit,
            "MILL_LOCK" => orderSetting?.MillLockMinutesPerGroup ?? globalSetting?.MillLockMinutesPerGroup ?? (decimal)StageTimeConfig.MillLockMinutesPerGroup,
            "DOOR_CORNER_CUT" => orderSetting?.CornerCutMinutesPerCorner ?? globalSetting?.CornerCutMinutesPerCorner ?? (decimal)StageTimeConfig.CornerCutMinutesPerCorner,
            "ASSEMBLE_FRAME" => orderSetting?.AssembleFrameMinutesPerCorner ?? globalSetting?.AssembleFrameMinutesPerCorner ?? (decimal)StageTimeConfig.AssembleFrameMinutesPerCorner,
            "CUT_GLASS" => orderSetting?.CutGlassMinutesPerSquareMeter ?? globalSetting?.CutGlassMinutesPerSquareMeter ?? (decimal)StageTimeConfig.CutGlassMinutesPerSquareMeter,
            "GLASS_INSTALL" => orderSetting?.GlassInstallMinutesPerUnit ?? globalSetting?.GlassInstallMinutesPerUnit ?? (decimal)StageTimeConfig.GlassInstallMinutesPerUnit,
            "PRESS_GASKET" => orderSetting?.GasketMinutesPerUnit ?? globalSetting?.GasketMinutesPerUnit ?? (decimal)StageTimeConfig.GasketMinutesPerUnit,
            "INSTALL_ACCESSORIES" => orderSetting?.AccessoryMinutesPerUnit ?? globalSetting?.AccessoryMinutesPerUnit ?? (decimal)StageTimeConfig.AccessoryMinutesPerUnit,
            "CUT_FLUSH" => orderSetting?.CutFlushMinutesPerUnit ?? globalSetting?.CutFlushMinutesPerUnit ?? (decimal)StageTimeConfig.CutFlushMinutesPerUnit,
            "FINISH_SILICON" => orderSetting?.SiliconMinutesPerMeter ?? globalSetting?.SiliconMinutesPerMeter ?? (decimal)StageTimeConfig.SiliconMinutesPerMeter,
            _ => null
        };

        // Fall back to 0 if not defined to avoid null double conversion
        var doVal = (double)(doMinutes ?? 0m);
        return new StageTimeConfigValues(setup, finish, doVal);
    }
}



