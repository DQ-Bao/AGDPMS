namespace AGDPMS.Shared.Models.DTOs;

public class StageTypeDto
{
    public string Code { get; set; } = string.Empty; // English code
    public string Name { get; set; } = string.Empty; // Vietnamese name
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
}


