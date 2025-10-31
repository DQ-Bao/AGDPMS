namespace AGDPMS.Shared.Models;

public class Cavity
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public required int ProjectId { get; set; }
    public string? Description { get; set; }
    public required double Width { get; set; }
    public required double Height { get; set; }
    public string? Location { get; set; }
    public required int Quantity { get; set; }
    public string? AluminumVendor { get; set; }
    public IEnumerable<CavityBOM> BOMs { get; set; } = [];
}

public class CavityBOM
{
    public int Id { get; set; }
    public required int CavityId { get; set; }
    public required Material Material { get; set; }
    public required int Quantity { get; set; }
    public required double Length { get; set; }
    public required double Width { get; set; }
    public string? Color { get; set; }
    public string? Unit { get; set; }
}
