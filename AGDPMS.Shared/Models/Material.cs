namespace AGDPMS.Shared.Models;

public class Material
{
    public required string Id { set; get; }
    public string Name { set; get; } = string.Empty;

    public double Weight { set; get; }
    public required MaterialType? Type { set; get; }
    public IEnumerable<MaterialStock> Stock { set; get; } = new List<MaterialStock>();

}

public class MaterialStock
{
    public int Id { set; get; }
    public double Length { set; get; }
    public double Width { set; get; }
    //double Weight { set; get; }
    public int Stock { set; get; }

    public double BasePrice { set; get; }
}