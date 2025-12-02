namespace AGDPMS.Shared.Models;

public class ProductionOrderItem
{
    public int Id { get; set; }
    public int ProductionOrderId { get; set; }
    public int ProductId { get; set; }

    public int LineNo { get; set; }

    public StageStatus Status { get; set; }
    public DateTime? PlannedStartDate { get; set; }
    public DateTime? PlannedFinishDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualFinishDate { get; set; }

    public string? QRCode { get; set; }
    public byte[]? QRImage { get; set; }

    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsCanceled { get; set; }

    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
