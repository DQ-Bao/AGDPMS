using System.Data;
using Dapper;
using AGDPMS.Shared.Models;

namespace AGDPMS.Web.Data;

public class ProductionOrderSettingDataAccess(IDbConnection conn)
{
    public Task<IEnumerable<ProductionOrderSetting>> GetByOrderIdAsync(int orderId) => conn.QueryAsync<ProductionOrderSetting>(@"
        select id as Id, production_order_id as ProductionOrderId, stage_type_id as StageTypeId,
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
               created_at as CreatedAt, updated_at as UpdatedAt
        from production_order_settings
        where production_order_id = @OrderId",
        new { OrderId = orderId });

    public async Task UpsertAsync(ProductionOrderSetting setting)
    {
        if (conn.State != System.Data.ConnectionState.Open)
        {
            conn.Open();
        }
        
        await conn.ExecuteAsync(@"
            insert into production_order_settings (
                production_order_id, stage_type_id, setup_minutes, finish_minutes,
                cut_al_minutes_per_unit, mill_lock_minutes_per_group,
                corner_cut_minutes_per_corner, assemble_frame_minutes_per_corner,
                cut_glass_minutes_per_square_meter, glass_install_minutes_per_unit,
                gasket_minutes_per_unit, accessory_minutes_per_unit,
                cut_flush_minutes_per_unit, silicon_minutes_per_meter,
                created_at, updated_at
            )
            values (
                @ProductionOrderId, @StageTypeId, @SetupMinutes, @FinishMinutes,
                @CutAlMinutesPerUnit, @MillLockMinutesPerGroup,
                @CornerCutMinutesPerCorner, @AssembleFrameMinutesPerCorner,
                @CutGlassMinutesPerSquareMeter, @GlassInstallMinutesPerUnit,
                @GasketMinutesPerUnit, @AccessoryMinutesPerUnit,
                @CutFlushMinutesPerUnit, @SiliconMinutesPerMeter,
                now(), now()
            )
            on conflict (production_order_id, stage_type_id)
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

    public Task DeleteByOrderIdAsync(int orderId) => conn.ExecuteAsync(@"
        delete from production_order_settings
        where production_order_id = @OrderId",
        new { OrderId = orderId });
}

