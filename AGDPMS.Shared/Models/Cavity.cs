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
    public string? WindowType { get; set; }
    public List<CavityBOM> BOMs { get; set; } = [];
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

public sealed class CavityProfileSummary : Material
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
    public sealed record StockUsage(MaterialStock Stock, int Used)
    {
        public int StockId => Stock.Id;
        public double StockLength => Stock.Length;
        public int InStock => Stock.Stock;
        public int Need => Math.Max(Used - Stock.Stock, 0);
        public decimal UnitPrice
        {
            get => Stock.BasePrice;
            set => Stock.BasePrice = value;
        }
    }

    public CavityProfileSummary(Material material)
    {
        if (material.Type != MaterialType.Aluminum)
            throw new ArgumentException("CavityProfileSummary can only be created with an aluminum material.");
        Id = material.Id;
        Name = material.Name;
        Type = material.Type;
        Weight = material.Weight;
        Stocks = material.Stocks;
    }

    public required List<ProfileCut> Cuts { get; set; }
    public required List<CutSolution> CutSolutions { get; set; }
    public Dictionary<double, int> BarsUsedByStockLength => CutSolutions
        .GroupBy(s => s.StockLength)
        .ToDictionary(g => g.Key, g => g.Sum(s => s.Quantity));
    public int TotalBarsUsed => BarsUsedByStockLength.Values.Sum();
    public double TotalDemandLength => Cuts.Sum(c => c.CutLength * c.Quantity);
    public double TotalUsedLength => CutSolutions.Sum(s => s.StockLength * s.Quantity);
    public double TotalWeight => Weight * (TotalUsedLength / 1000);
    public List<StockUsage> StockUsages => [.. Stocks
        .Select(stock =>
        {
            BarsUsedByStockLength.TryGetValue(stock.Length, out var used);
            return new StockUsage(stock, used);
        })];
    public decimal TotalPrice => StockUsages.Sum(su => su.Used * su.UnitPrice);
}

public sealed class CavityGlassSummary : Material
{
    public sealed class GlassCut
    {
        public required double Width { get; set; }
        public required double Length { get; set; }
        public required int Quantity { get; set; }
        public int StockId { get; set; }
        public required int Stock { get; set; }
        public int Need => Math.Max(Quantity - Stock, 0);
        public double TotalPerimeter => (Width + Length) * 2 * Quantity;
        public double TotalSize => Width * Length * Quantity;
        public string? Color { get; set; }
        public List<string> CavityCodes { get; set; } = [];
        public required decimal UnitPrice { get; set; }
    }

    public CavityGlassSummary(Material material)
    {
        if (material.Type != MaterialType.Glass)
            throw new ArgumentException("CavityGlassSummary can only be created with an glass material.");
        Id = material.Id;
        Name = material.Name;
        Type = material.Type;
        Weight = material.Weight;
        Stocks = material.Stocks;
    }

    public required List<GlassCut> Cuts { get; set; }
    public double TotalPerimeter => Cuts.Sum(c => c.TotalPerimeter);
    public double TotalSize => Cuts.Sum(c => c.TotalSize);
    public decimal TotalPrice =>  Cuts.Sum(c => c.UnitPrice * c.Quantity);
}

public sealed class CavityOtherMaterialSummary : Material
{
    public CavityOtherMaterialSummary(Material material)
    {
        if (material.Type != MaterialType.Gasket && material.Type != MaterialType.Accessory && material.Type != MaterialType.Auxiliary)
            throw new ArgumentException("CavityGlassSummary can only be created with an material other than aluminum and glass.");
        Id = material.Id;
        Name = material.Name;
        Type = material.Type;
        Weight = material.Weight;
        Stocks = material.Stocks;
    }

    public double? GasketLength {  get; set; }
    public required MaterialStock Stock { get; set; }
    public int Quantity { get; set; }
    public string? Unit { get; set; }
    public string? Color { get; set; }
    public double NeedGasketLength => GasketLength is not null ? Math.Max(GasketLength.Value - Stock.Length, 0) : 0;
    public int Need => Math.Max(Quantity - Stock.Stock, 0);
    public required decimal UnitPrice { get; set; }
    public decimal TotalPrice => GasketLength is null ? UnitPrice * Quantity : UnitPrice * (decimal)GasketLength * Quantity;
}

public class Quotation
{
    public class QuotationDetails
    {
        public required string Code { get; set; }
        public string Description { get; set; } = string.Empty;
        public required double Width { get; set; }
        public required double Height { get; set; }
        public required int Quantity { get; set; }
        public required double Weight { get; set; }
        public required decimal MaterialPrice { get; set; }
        public required decimal LaborCost { get; set; }
        public required decimal ProfitPercentage { get; set; }
        public required decimal TaxPercentage { get; set; }
        public required decimal TransportCost { get; set; }
        public required decimal Contingency { get; set; }
        public decimal UnitPrice =>
            MaterialPrice +
            LaborCost +
            (MaterialPrice * ProfitPercentage * 0.01M) +
            (MaterialPrice * TaxPercentage * 0.01M) +
            TransportCost +
            Contingency;
        public decimal TotalPrice => UnitPrice * Quantity;
    }
    public List<QuotationDetails> Details { get; set; } = [];
    public double TotalWeight => Details.Sum(d => d.Weight);
    public decimal TotalPrice => Details.Sum(d => d.TotalPrice);
}