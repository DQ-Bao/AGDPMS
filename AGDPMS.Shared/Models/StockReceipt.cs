namespace AGDPMS.Shared.Models;

public class StockReceipt
{
    public string Code { set; get; } = string.Empty;
    public string Vendor { set; get; } = string.Empty;
    public DateOnly CreationDate { set; get; }
    public string Note { set; get; } = string.Empty;
    public IEnumerable<Material> MaterialReceipts { set; get; } = new List<Material>() { };
}