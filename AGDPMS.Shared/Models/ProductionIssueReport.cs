namespace AGDPMS.Shared.Models;

public class ProductionIssueReport
{
    public int Id { get; set; }
    public int ProductionItemStageId { get; set; }
    public int CreatedByUserId { get; set; }
    
    public IssueStatus Status { get; set; }
    public IssuePriority Priority { get; set; }
    
    public string Reason { get; set; } = string.Empty;
    
    public DateTime? ResolvedAt { get; set; }
    public int? ResolvedByUserId { get; set; }
    
    public DateTime? CreatedAt { get; set; }
}

