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

public sealed class CavityProfileSummary
{
    public sealed record ProfileCut(double CutLength, int Quantity);
    public sealed class CutSolution
    {
        public double StockLength { get; set; }
        public int Quantity { get; set; }
        public List<ProfileCut> Pattern { get; set; } = [];
        public double Waste => Math.Round(StockLength - Pattern.Sum(p => p.CutLength * p.Quantity), 2);
        public double Efficiency => Math.Round((StockLength - Waste) / StockLength * 100, 2);
    }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public required List<ProfileCut> Cuts { get; set; }
    public required List<CutSolution> CutSolutions { get; set; }
    public Dictionary<double, int> BarsUsedByStockLength => CutSolutions
        .GroupBy(s => s.StockLength)
        .ToDictionary(g => g.Key, g => g.Sum(s => s.Quantity));
    public int TotalBarsUsed => BarsUsedByStockLength.Values.Sum();
    public double TotalDemandLength => Cuts.Sum(c => c.CutLength * c.Quantity);
    public double TotalUsedLength => CutSolutions.Sum(s => s.StockLength * s.Quantity);
    public required double WeightPerMeter { get; set; }
    public double TotalWeight => WeightPerMeter * TotalUsedLength;
    public required Dictionary<double, int> BarsInStockByStockLength { get; set; }
    public Dictionary<double, int> BarsNeedByStockLength => BarsUsedByStockLength
        .ToDictionary(kv => kv.Key, kv =>
        {
            BarsInStockByStockLength.TryGetValue(kv.Key, out var inStock);
            return Math.Max(kv.Value - inStock, 0);
        });
    public required Dictionary<double, decimal> UnitPriceByStockLength { get; set; }
    public decimal TotalPrice => BarsUsedByStockLength
        .Sum(kv =>
        {
            UnitPriceByStockLength.TryGetValue(kv.Key, out var pricePerBar);
            return kv.Value * pricePerBar;
        });
}

public sealed class GlassSummary
{
    public sealed class GlassCut
    {
        public required double Width { get; set; }
        public required double Length { get; set; }
        public required int Quantity { get; set; }
        public double TotalPerimeter => (Width + Length) * 2 * Quantity;
        public double TotalSize => Width * Length * Quantity;
    }
    public List<GlassCut> Cuts { get; set; } = [];
    public required string ProfileCode { get; set; }
    public required string ProfileName { get; set; }
}