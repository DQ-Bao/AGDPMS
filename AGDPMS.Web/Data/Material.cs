namespace AGDPMS.Web.Data;

public class Material
{
    public required string Id { set; get; }
    public string Name { set; get; } = string.Empty;
    public required MaterialType Type { set; get; }
    public int Stock { set; get; }
    public decimal Weight { set; get; }
    public decimal Thickness { set; get; }

}
