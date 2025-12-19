namespace AGDPMS.Shared.Models;

public class AppClient
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // Not Null
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}
