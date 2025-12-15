namespace AGDPMS.Shared.Models.DTOs;

public class LaborCostSettingDto
{
    public int StageTypeId { get; set; }
    public string StageTypeCode { get; set; } = string.Empty;
    public string StageTypeName { get; set; } = string.Empty;
    public decimal HourlyRate { get; set; }
}

