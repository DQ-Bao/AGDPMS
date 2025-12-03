namespace AGDPMS.Shared.Models.DTOs;

public class UpdateStagePlanDto
{
    public DateTime? PlannedStartDate { get; set; }
    public DateTime? PlannedFinishDate { get; set; }
    public decimal? PlannedTimeHours { get; set; }
}

