using QRCoder;
using Microsoft.AspNetCore.Http;

namespace AGDPMS.Web.Services;

public class QrService(IHttpContextAccessor httpContextAccessor)
{
    public (string url, byte[] pngBytes) GenerateForItem(int orderId, int itemId)
    {
        // Generate full URL to item detail page
        var httpContext = httpContextAccessor.HttpContext;
        var baseUrl = "";
        
        if (httpContext != null)
        {
            var request = httpContext.Request;
            baseUrl = $"{request.Scheme}://{request.Host}";
        }
        
        // URL format: /production/orders/{orderId}/items/{itemId}
        var relativeUrl = $"/production/orders/{orderId}/items/{itemId}";
        var fullUrl = string.IsNullOrEmpty(baseUrl) ? relativeUrl : $"{baseUrl}{relativeUrl}";
        
        try
        {
            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(fullUrl, QRCodeGenerator.ECCLevel.Q);
            var png = new PngByteQRCode(data).GetGraphic(10);
            return (fullUrl, png);
        }
        catch
        {
            return (fullUrl, Array.Empty<byte>());
        }
    }
}


