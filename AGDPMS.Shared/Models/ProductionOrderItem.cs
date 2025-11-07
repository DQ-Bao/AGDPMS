namespace AGDPMS.Shared.Models;

public class ProductionOrderItem
{
    public int Id { get; set; }
    public int ProductionOrderId { get; set; }
    public int ProductId { get; set; }

    public int LineNo { get; set; }

    public string? QRCode { get; set; }
    public byte[]? QRImage { get; set; }

    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }

    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
