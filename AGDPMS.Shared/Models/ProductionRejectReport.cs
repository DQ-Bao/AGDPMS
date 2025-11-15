namespace AGDPMS.Shared.Models;

public class ProductionRejectReport
{
    public int Id { get; set; }
    public int ProductionItemStageId { get; set; }
    public int RejectedByUserId { get; set; }
    public required string Reason { get; set; }
    public DateTime? CreatedAt { get; set; }
}
