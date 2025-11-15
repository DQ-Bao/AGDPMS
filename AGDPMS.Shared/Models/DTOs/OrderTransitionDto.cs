namespace AGDPMS.Shared.Models.DTOs;

public class OrderTransitionDto
{
    public int OrderId { get; set; }
    public string Action { get; set; } = string.Empty; // submit | cancel | finish
    public string? Note { get; set; }
}


