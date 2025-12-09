namespace AGDPMS.Shared.Models.DTOs;

public class UpdateStageTimeSettingsDto
{
    public decimal SetupMinutes { get; set; }
    public decimal FinishMinutes { get; set; }
    public decimal? CutAlMinutesPerUnit { get; set; }
    public decimal? MillLockMinutesPerGroup { get; set; }
    public decimal? CornerCutMinutesPerCorner { get; set; }
    public decimal? AssembleFrameMinutesPerCorner { get; set; }
    public decimal? CutGlassMinutesPerSquareMeter { get; set; }
    public decimal? GlassInstallMinutesPerUnit { get; set; }
    public decimal? GasketMinutesPerUnit { get; set; }
    public decimal? AccessoryMinutesPerUnit { get; set; }
    public decimal? CutFlushMinutesPerUnit { get; set; }
    public decimal? SiliconMinutesPerMeter { get; set; }
}

