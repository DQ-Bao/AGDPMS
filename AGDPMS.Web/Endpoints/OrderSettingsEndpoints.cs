using AGDPMS.Shared.Models;
using AGDPMS.Shared.Models.DTOs;
using AGDPMS.Web.Data;
using Microsoft.AspNetCore.Authorization;
using AGDPMS.Web.Services;

namespace AGDPMS.Web.Endpoints;

public static class OrderSettingsEndpoints
{
    public static IEndpointRouteBuilder MapOrderSettings(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/order-settings")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Production Manager,Director" });

        group.MapGet("/stage-time", async (StageTypeDataAccess stageTypes, GlobalStageTimeSettingDataAccess timeAccess) =>
        {
            var types = (await stageTypes.GetAllAsync()).OrderBy(t => t.Id).ToList();
            var timeSettings = (await timeAccess.GetAllAsync()).ToDictionary(t => t.StageTypeId);

            var result = types.Select(t =>
            {
                timeSettings.TryGetValue(t.Id, out var setting);
                return BuildStageTimeView(t, setting);
            });

            return Results.Ok(result);
        });

        group.MapPut("/stage-time", async (List<StageTimeSettingUpdateDto> dto, GlobalStageTimeSettingDataAccess timeAccess) =>
        {
            if (dto is null || dto.Count == 0) return Results.BadRequest("No settings provided");

            foreach (var item in dto)
            {
                var setting = new GlobalStageTimeSetting
                {
                    StageTypeId = item.StageTypeId,
                    SetupMinutes = item.SetupMinutes ?? (decimal)StageTimeConfig.SetupMinutes,
                    FinishMinutes = item.FinishMinutes ?? (decimal)StageTimeConfig.FinishMinutes
                };

                ApplyDoMinutes(item, setting);
                await timeAccess.UpsertAsync(setting);
            }

            return Results.Ok();
        });

        group.MapGet("/labor-cost", async (GlobalLaborCostSettingDataAccess laborAccess) =>
        {
            var setting = await laborAccess.GetAsync();
            var unifiedRate = setting?.HourlyRate ?? 100000m;
            
            return Results.Ok(new UnifiedLaborCostDto { HourlyRate = unifiedRate });
        });

        group.MapPut("/labor-cost", async (UnifiedLaborCostDto dto, GlobalLaborCostSettingDataAccess laborAccess) =>
        {
            if (dto is null) return Results.BadRequest("No settings provided");

            await laborAccess.UpsertAsync(dto.HourlyRate);

            return Results.Ok();
        });

        return app;
    }

    private static StageTimeSettingViewDto BuildStageTimeView(StageType stageType, GlobalStageTimeSetting? setting)
    {
        var (doLabel, doValue) = GetDoLabelAndValue(stageType.Code, setting);
        return new StageTimeSettingViewDto
        {
            StageTypeId = stageType.Id,
            StageTypeCode = stageType.Code,
            StageTypeName = stageType.Name,
            SetupMinutes = setting?.SetupMinutes ?? (decimal)StageTimeConfig.SetupMinutes,
            FinishMinutes = setting?.FinishMinutes ?? (decimal)StageTimeConfig.FinishMinutes,
            DoMinutes = doValue,
            DoLabel = doLabel
        };
    }

    private static (string DoLabel, decimal? DoValue) GetDoLabelAndValue(string code, GlobalStageTimeSetting? setting)
    {
        return code switch
        {
            "CUT_AL" => ("Phút/đơn vị nhôm", setting?.CutAlMinutesPerUnit ?? (decimal)StageTimeConfig.CutAlMinutesPerUnit),
            "MILL_LOCK" => ("Phút/nhóm 4 thanh", setting?.MillLockMinutesPerGroup ?? (decimal)StageTimeConfig.MillLockMinutesPerGroup),
            "DOOR_CORNER_CUT" => ("Phút/góc", setting?.CornerCutMinutesPerCorner ?? (decimal)StageTimeConfig.CornerCutMinutesPerCorner),
            "ASSEMBLE_FRAME" => ("Phút/góc", setting?.AssembleFrameMinutesPerCorner ?? (decimal)StageTimeConfig.AssembleFrameMinutesPerCorner),
            "CUT_GLASS" => ("Phút/m² kính", setting?.CutGlassMinutesPerSquareMeter ?? (decimal)StageTimeConfig.CutGlassMinutesPerSquareMeter),
            "GLASS_INSTALL" => ("Phút/tấm kính", setting?.GlassInstallMinutesPerUnit ?? (decimal)StageTimeConfig.GlassInstallMinutesPerUnit),
            "PRESS_GASKET" => ("Phút/gioăng", setting?.GasketMinutesPerUnit ?? (decimal)StageTimeConfig.GasketMinutesPerUnit),
            "INSTALL_ACCESSORIES" => ("Phút/phụ kiện", setting?.AccessoryMinutesPerUnit ?? (decimal)StageTimeConfig.AccessoryMinutesPerUnit),
            "CUT_FLUSH" => ("Phút/đơn vị", setting?.CutFlushMinutesPerUnit ?? (decimal)StageTimeConfig.CutFlushMinutesPerUnit),
            "FINISH_SILICON" => ("Phút/m keo", setting?.SiliconMinutesPerMeter ?? (decimal)StageTimeConfig.SiliconMinutesPerMeter),
            _ => ("Phút/thao tác", null)
        };
    }

    private static void ApplyDoMinutes(StageTimeSettingUpdateDto dto, GlobalStageTimeSetting setting)
    {
        if (!dto.DoMinutes.HasValue) return;

        // Map to the correct column based on stage code
        switch (dto.StageTypeCode?.ToUpperInvariant())
        {
            case "CUT_AL":
                setting.CutAlMinutesPerUnit = dto.DoMinutes;
                break;
            case "MILL_LOCK":
                setting.MillLockMinutesPerGroup = dto.DoMinutes;
                break;
            case "DOOR_CORNER_CUT":
                setting.CornerCutMinutesPerCorner = dto.DoMinutes;
                break;
            case "ASSEMBLE_FRAME":
                setting.AssembleFrameMinutesPerCorner = dto.DoMinutes;
                break;
            case "CUT_GLASS":
                setting.CutGlassMinutesPerSquareMeter = dto.DoMinutes;
                break;
            case "GLASS_INSTALL":
                setting.GlassInstallMinutesPerUnit = dto.DoMinutes;
                break;
            case "PRESS_GASKET":
                setting.GasketMinutesPerUnit = dto.DoMinutes;
                break;
            case "INSTALL_ACCESSORIES":
                setting.AccessoryMinutesPerUnit = dto.DoMinutes;
                break;
            case "CUT_FLUSH":
                setting.CutFlushMinutesPerUnit = dto.DoMinutes;
                break;
            case "FINISH_SILICON":
                setting.SiliconMinutesPerMeter = dto.DoMinutes;
                break;
            default:
                break;
        }
    }
}

