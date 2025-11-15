namespace AGDPMS.Shared.Models;

public class StageType
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; } // Vietnamese for UI
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
