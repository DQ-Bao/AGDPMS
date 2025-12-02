namespace AGDPMS.Shared.Models.DTOs;

public class CreateIssueReportDto
{
    public int StageId { get; set; }
    public IssuePriority Priority { get; set; }
    public string Reason { get; set; } = string.Empty;
}

