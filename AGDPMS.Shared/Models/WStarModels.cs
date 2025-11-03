namespace AGDPMS.Shared.Models;

public sealed class WStarProject
{
    public required string Code { get; set; }
    public required DateTime CreateDate { get; set; }
    public List<WStarCavity> Cavities { get; set; } = [];
}

public sealed class WStarCavity
{
    public required string Code { get; set; }
    public required string Description { get; set; }
    public string? WindowType { get; set; }
    public required int Quantity { get; set; }
    public string? Location { get; set; }
    public required double Width { get; set; }
    public required double Height { get; set; }
    public List<WStarBOMAccessory> Materials { get; set; } = [];
}

public sealed class WStarBOMAccessory
{
    public required string Code { get; set; }
    public required string Description { get; set; }
    public required string Symbol { get; set; }
    public required string MatType { get; set; }
    public required int Num { get; set; }
    public required double Length { get; set; }
    public required double Width { get; set; }
    public string? Color { get; set; }
    public string? Unit { get; set; }
}