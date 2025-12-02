namespace AGDPMS.Shared.Models;

public class ProductionOrder
{
    public int Id { get; set; }
    public int ProjectId { get; set; }

    public string? Code { get; set; }

    public ProductionOrderStatus Status { get; set; }
    public bool IsCancelled { get; set; }

    public DateTime? PlannedStartDate { get; set; }
    public DateTime? PlannedFinishDate { get; set; }

    public DateTime? SubmittedAt { get; set; }
    public DateTime? DirectorDecisionAt { get; set; }
    public DateTime? QAMachinesCheckedAt { get; set; }
    public DateTime? QAMaterialCheckedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }

    public DateTime? CreatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
}
