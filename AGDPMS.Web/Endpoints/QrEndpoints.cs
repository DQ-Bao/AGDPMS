using AGDPMS.Web.Data;
using Microsoft.AspNetCore.Authorization;

namespace AGDPMS.Web.Endpoints;

public static class QrEndpoints
{
    public static IEndpointRouteBuilder MapQr(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/items");

        group.MapGet("/{itemId:int}/qr", async (int itemId, ProductionItemDataAccess access) =>
        {
            var item = await access.GetByIdAsync(itemId);
            if (item is null) return Results.NotFound();
            if (item.QRImage is null || item.QRImage.Length == 0)
                return Results.Ok(new { url = item.QRCode, hasImage = false });
            return Results.File(item.QRImage, contentType: "image/png");
        });

        return app;
    }
}


