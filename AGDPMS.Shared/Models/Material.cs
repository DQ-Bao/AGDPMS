namespace AGDPMS.Shared.Models;

public class Material
{
    public int Id { set; get; }
    public required string Code { set; get; }
    public required string Name { set; get; }
    public required MaterialType? Type { set; get; }
    public double Weight { set; get; }
    public string? Vendor { set; get; }
    public IEnumerable<MaterialStock> Stock { set; get; } = new List<MaterialStock>();
}

public class MaterialType
{
    public int Id { set; get; }
    public required string Name { set; get; }

    public static readonly MaterialType Aluminum = new() { Id = 1, Name = "aluminum" };
    public static readonly MaterialType Glass = new() { Id = 2, Name = "glass" };
    public static readonly MaterialType Accessory = new() { Id = 3, Name = "accessory" };
    public static readonly MaterialType Gasket = new() { Id = 4, Name = "gasket" };
    public static readonly MaterialType Auxiliary = new() { Id = 5, Name = "auxiliary" };
    public static IEnumerable<MaterialType> All => [Aluminum, Glass, Accessory, Gasket, Auxiliary];

    public static MaterialType? FromId(int id) => All.FirstOrDefault(m => m.Id == id);

    public static MaterialType? FromName(string name) =>
        All.FirstOrDefault(m => string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase));

    public override string ToString() => Name;
}

public class MaterialStock
{
    double Length { set; get; }
    double Width { set; get; }

    //double Weight { set; get; }
    int Stock { set; get; }
}

