namespace AGDPMS.Shared.Models;

public class WarrantyStamp
{
    public string ProductName { get; set; } = "...............................";
    public string OrderCode { get; set; } = "........................";
    public string OrderItemCode { get; set; } = "W6T2";
    public int? OrderItemId { get; set; }
    public DateTime? InstallDate { get; set; }

    // New: set this to a data URI (e.g. "data:image/png;base64,...")
    // or an absolute URL (e.g. "/api/items/123/qr") when a real QR is available.
    public string? QrImage { get; set; }
}