namespace AGDPMS.Shared.Models;

public class ProductionItemStage
{
    public int Id { get; set; }
    public int ProductionOrderItemId { get; set; }
    public int StageTypeId { get; set; }
    public int? AssignedQaUserId { get; set; }

    public DateTime? PlannedStartDate { get; set; }
    public DateTime? PlannedFinishDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualFinishDate { get; set; }
    public decimal? PlannedTimeHours { get; set; }
    public decimal? ActualTimeHours { get; set; }
    public int? PlannedUnits { get; set; }
    public int? ActualUnits { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }

    public string? Note { get; set; }

    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
