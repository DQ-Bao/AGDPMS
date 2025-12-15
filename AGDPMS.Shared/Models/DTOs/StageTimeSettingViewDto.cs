namespace AGDPMS.Shared.Models.DTOs;

public class StageTimeSettingViewDto
{
    public int StageTypeId { get; set; }
    public string StageTypeCode { get; set; } = string.Empty;
    public string StageTypeName { get; set; } = string.Empty;
    public decimal SetupMinutes { get; set; }
    public decimal FinishMinutes { get; set; }
    public decimal? DoMinutes { get; set; }
    public string DoLabel { get; set; } = string.Empty;
}

public class StageTimeSettingUpdateDto
{
    public int StageTypeId { get; set; }
    public string? StageTypeCode { get; set; }
    public decimal? SetupMinutes { get; set; }
    public decimal? FinishMinutes { get; set; }
    public decimal? DoMinutes { get; set; }
}

