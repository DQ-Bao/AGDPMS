namespace AGDPMS.Shared.Models;

public class Material
{
    public required string Id { set; get; }
    public required string Name { set; get; }
    public required MaterialType? Type { set; get; }
    public double Weight { set; get; }
    public List<MaterialStock> Stocks { set; get; } = [];

    public bool Equals(Material? other) => other is not null && Id == other.Id;

    public override bool Equals(object? obj) => Equals(obj as Material);

    public override int GetHashCode() => Id.GetHashCode();
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

    public override bool Equals(object? obj) => obj is MaterialType other && Id == other.Id;

    public override int GetHashCode() => Id;

    public override string ToString() => Name;

    public static bool operator ==(MaterialType? left, MaterialType? right)
    {
        if (ReferenceEquals(left, right))
            return true;
        if (left is null || right is null)
            return false;
        return left.Id == right.Id;
    }

    public static bool operator !=(MaterialType? left, MaterialType? right) => !(left == right);
}

public class MaterialStock
{
    public int Id { set; get; }
    public double Length { set; get; }
    public double Width { set; get; }
    public int Stock { set; get; }
    public decimal BasePrice { set; get; }
}

