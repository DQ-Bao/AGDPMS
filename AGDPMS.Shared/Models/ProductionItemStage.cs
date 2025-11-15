namespace AGDPMS.Shared.Models;

public class ProductionItemStage
{
    public int Id { get; set; }
    public int ProductionOrderItemId { get; set; }
    public int StageTypeId { get; set; }

    public int? AssignedQaUserId { get; set; }

    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }

    public int RejectionCount { get; set; }
    public string? Note { get; set; }

    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
