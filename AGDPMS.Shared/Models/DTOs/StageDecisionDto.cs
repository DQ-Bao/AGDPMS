namespace AGDPMS.Shared.Models.DTOs;

public class StageDecisionDto
{
    public int ItemStageId { get; set; }
    public string Decision { get; set; } = string.Empty; // approve | reject
    public string? Reason { get; set; }
}


