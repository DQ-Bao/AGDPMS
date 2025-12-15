using System.Data;
using Dapper;
using AGDPMS.Shared.Models;

namespace AGDPMS.Web.Data;

public class GlobalStageTimeSettingDataAccess(IDbConnection conn)
{
    public Task<IEnumerable<GlobalStageTimeSetting>> GetAllAsync() => conn.QueryAsync<GlobalStageTimeSetting>(@"
        select id as Id, stage_type_id as StageTypeId,
               setup_minutes as SetupMinutes, finish_minutes as FinishMinutes,
               cut_al_minutes_per_unit as CutAlMinutesPerUnit,
               mill_lock_minutes_per_group as MillLockMinutesPerGroup,
               corner_cut_minutes_per_corner as CornerCutMinutesPerCorner,
               assemble_frame_minutes_per_corner as AssembleFrameMinutesPerCorner,
               cut_glass_minutes_per_square_meter as CutGlassMinutesPerSquareMeter,
               glass_install_minutes_per_unit as GlassInstallMinutesPerUnit,
               gasket_minutes_per_unit as GasketMinutesPerUnit,
               accessory_minutes_per_unit as AccessoryMinutesPerUnit,
               cut_flush_minutes_per_unit as CutFlushMinutesPerUnit,
               silicon_minutes_per_meter as SiliconMinutesPerMeter,
               updated_at as UpdatedAt
        from global_stage_time_settings
        order by stage_type_id");

    public Task<GlobalStageTimeSetting?> GetByStageTypeIdAsync(int stageTypeId) => conn.QueryFirstOrDefaultAsync<GlobalStageTimeSetting>(@"
        select id as Id, stage_type_id as StageTypeId,
               setup_minutes as SetupMinutes, finish_minutes as FinishMinutes,
               cut_al_minutes_per_unit as CutAlMinutesPerUnit,
               mill_lock_minutes_per_group as MillLockMinutesPerGroup,
               corner_cut_minutes_per_corner as CornerCutMinutesPerCorner,
               assemble_frame_minutes_per_corner as AssembleFrameMinutesPerCorner,
               cut_glass_minutes_per_square_meter as CutGlassMinutesPerSquareMeter,
               glass_install_minutes_per_unit as GlassInstallMinutesPerUnit,
               gasket_minutes_per_unit as GasketMinutesPerUnit,
               accessory_minutes_per_unit as AccessoryMinutesPerUnit,
               cut_flush_minutes_per_unit as CutFlushMinutesPerUnit,
               silicon_minutes_per_meter as SiliconMinutesPerMeter,
               updated_at as UpdatedAt
        from global_stage_time_settings
        where stage_type_id = @StageTypeId",
        new { StageTypeId = stageTypeId });

    public async Task UpsertAsync(GlobalStageTimeSetting setting)
    {
        if (conn.State != System.Data.ConnectionState.Open)
        {
            conn.Open();
        }
        
        await conn.ExecuteAsync(@"
            insert into global_stage_time_settings (
                stage_type_id, setup_minutes, finish_minutes,
                cut_al_minutes_per_unit, mill_lock_minutes_per_group,
                corner_cut_minutes_per_corner, assemble_frame_minutes_per_corner,
                cut_glass_minutes_per_square_meter, glass_install_minutes_per_unit,
                gasket_minutes_per_unit, accessory_minutes_per_unit,
                cut_flush_minutes_per_unit, silicon_minutes_per_meter,
                updated_at
            )
            values (
                @StageTypeId, @SetupMinutes, @FinishMinutes,
                @CutAlMinutesPerUnit, @MillLockMinutesPerGroup,
                @CornerCutMinutesPerCorner, @AssembleFrameMinutesPerCorner,
                @CutGlassMinutesPerSquareMeter, @GlassInstallMinutesPerUnit,
                @GasketMinutesPerUnit, @AccessoryMinutesPerUnit,
                @CutFlushMinutesPerUnit, @SiliconMinutesPerMeter,
                now()
            )
            on conflict (stage_type_id)
            do update set
                setup_minutes = @SetupMinutes,
                finish_minutes = @FinishMinutes,
                cut_al_minutes_per_unit = @CutAlMinutesPerUnit,
                mill_lock_minutes_per_group = @MillLockMinutesPerGroup,
                corner_cut_minutes_per_corner = @CornerCutMinutesPerCorner,
                assemble_frame_minutes_per_corner = @AssembleFrameMinutesPerCorner,
                cut_glass_minutes_per_square_meter = @CutGlassMinutesPerSquareMeter,
                glass_install_minutes_per_unit = @GlassInstallMinutesPerUnit,
                gasket_minutes_per_unit = @GasketMinutesPerUnit,
                accessory_minutes_per_unit = @AccessoryMinutesPerUnit,
                cut_flush_minutes_per_unit = @CutFlushMinutesPerUnit,
                silicon_minutes_per_meter = @SiliconMinutesPerMeter,
                updated_at = now()",
            setting);
    }
}

