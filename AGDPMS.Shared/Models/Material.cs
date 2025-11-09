namespace AGDPMS.Shared.Models;

public class Material
{
    public required string Id { set; get; }
    public string Name { set; get; } = string.Empty;
    public required MaterialType? Type { set; get; }
    public IEnumerable<MaterialStock> Stock { set; get; } = new List<MaterialStock>();

}

public class MaterialStock
{
    double Length { set; get; }
    double Width { set; get; }
    //double Weight { set; get; }
    int Stock { set; get; }
}