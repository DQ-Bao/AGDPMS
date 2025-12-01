using QRCoder;

namespace AGDPMS.Web.Services;

public class QrService
{
    public (string url, byte[] pngBytes) GenerateForItem(int itemId)
    {
        var url = $"/items/{itemId}";
        try
        {
            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            var png = new PngByteQRCode(data).GetGraphic(10);
            return (url, png);
        }
        catch
        {
            return (url, Array.Empty<byte>());
        }
    }
}


