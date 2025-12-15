namespace AGDPMS.Shared.Models.DTOs;

public class SubmitStageReviewDto
{
    public string? Notes { get; set; }
    public List<StageReviewCriteriaResultDto> CriteriaResults { get; set; } = new();
    public bool? IsPassed { get; set; } // Manual pass/fail decision by QA
    public List<string>? Attachments { get; set; }
}

public class StageReviewCriteriaResultDto
{
    public int CriteriaId { get; set; }
    public bool? IsPassed { get; set; }
    public string? Value { get; set; }
    public string? Notes { get; set; }
    public string? Severity { get; set; } // low, medium, high, critical
    public string? Content { get; set; }
    public List<string>? Attachments { get; set; } // List of file URLs or paths
}

