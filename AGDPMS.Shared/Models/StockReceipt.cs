namespace AGDPMS.Shared.Models;

public class StockReceipt
{
    public int Id { get; set; }
    public string MaterialId { get; set; } = string.Empty;
    public string VoucherCode { get; set; } = string.Empty;
    public int QuantityChange { get; set; }
    public int QuantityAfter { get; set; }
    public decimal Price { get; set; }
    public DateTime Date { get; set; }
}