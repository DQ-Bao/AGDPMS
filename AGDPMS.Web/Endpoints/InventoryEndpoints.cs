using AGDPMS.Shared.Services;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;

namespace AGDPMS.Web.Endpoints;

public static class InventoryEndpoints
{
    public static IEndpointRouteBuilder MapInventory(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/inventory");

        group.MapGet("/receipts/export", async (IInventoryService inventoryService) =>
        {
            var result = await inventoryService.GetStockReceipt();
            if (!result.Success || result.StockReceipts == null || !result.StockReceipts.Any())
            {
                return Results.NotFound(new { message = "Không có dữ liệu để xuất" });
            }

            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Phiếu nhập kho");

            int currentRow = 1;

            // Title
            ws.Cell(currentRow, 1).Value = "Phiếu nhập kho";
            ws.Range(currentRow, 1, currentRow, 7).Merge();
            ws.Row(currentRow).Style.Font.Bold = true;
            ws.Row(currentRow).Style.Font.FontSize = 16;
            ws.Row(currentRow).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            currentRow += 2;

            // Headers
            string[] headers = { "No", "Mã phiếu", "Mã vật tư", "Tên vật tư", "SL nhập", "Giá", "Ngày" };
            for (int col = 1; col <= headers.Length; col++)
            {
                var cell = ws.Cell(currentRow, col);
                cell.Value = headers[col - 1];
                cell.Style.Font.Bold = true;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            currentRow++;

            // Data rows
            int index = 1;
            var materials = await inventoryService.GetMaterial();
            var materialDict = materials.Materials?.ToDictionary(m => m.Id, m => m.Name) ?? new Dictionary<string, string>();

            foreach (var t in result.StockReceipts.OrderBy(t => t.Date))
            {
                var materialName = materialDict.GetValueOrDefault(t.MaterialStock?.MaterialId ?? "", "");

                ws.Cell(currentRow, 1).Value = index++;
                ws.Cell(currentRow, 2).Value = t.VoucherCode ?? "";
                ws.Cell(currentRow, 3).Value = t.MaterialStock?.MaterialId ?? "";
                ws.Cell(currentRow, 4).Value = materialName;
                ws.Cell(currentRow, 5).Value = t.QuantityChange;
                ws.Cell(currentRow, 6).Value = t.Price;
                ws.Cell(currentRow, 7).Value = t.Date;

                ws.Range(currentRow, 1, currentRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                if (t.QuantityChange < 0)
                {
                    ws.Cell(currentRow, 5).Style.Font.FontColor = XLColor.Red;
                    ws.Cell(currentRow, 6).Style.Font.FontColor = XLColor.Red;
                }

                currentRow++;
            }

            // Formatting
            ws.Columns().AdjustToContents();
            ws.Column(6).Style.NumberFormat.Format = "#,##0.00";
            ws.Column(7).Style.NumberFormat.Format = "dd/MM/yyyy";

            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"phieu_nhap_kho_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return Results.File(stream.ToArray(), 
                contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileDownloadName: fileName);
        });

        group.MapGet("/issues/export", async (IInventoryService inventoryService) =>
        {
            var result = await inventoryService.GetStockIssue();
            if (!result.Success || result.StockIssues == null || !result.StockIssues.Any())
            {
                return Results.NotFound(new { message = "Không có dữ liệu để xuất" });
            }

            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Phiếu xuất kho");

            int currentRow = 1;

            // Title
            ws.Cell(currentRow, 1).Value = "Phiếu xuất kho";
            ws.Range(currentRow, 1, currentRow, 7).Merge();
            ws.Row(currentRow).Style.Font.Bold = true;
            ws.Row(currentRow).Style.Font.FontSize = 16;
            ws.Row(currentRow).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            currentRow += 2;

            // Headers
            string[] headers = { "No", "Mã phiếu", "Mã vật tư", "Tên vật tư", "SL xuất", "Giá", "Ngày" };
            for (int col = 1; col <= headers.Length; col++)
            {
                var cell = ws.Cell(currentRow, col);
                cell.Value = headers[col - 1];
                cell.Style.Font.Bold = true;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            currentRow++;

            // Data rows
            int index = 1;
            var materials = await inventoryService.GetMaterial();
            var materialDict = materials.Materials?.ToDictionary(m => m.Id, m => m.Name) ?? new Dictionary<string, string>();

            foreach (var t in result.StockIssues.OrderBy(t => t.Date))
            {
                var materialName = materialDict.GetValueOrDefault(t.MaterialStock?.MaterialId ?? "", "");

                ws.Cell(currentRow, 1).Value = index++;
                ws.Cell(currentRow, 2).Value = t.VoucherCode ?? "";
                ws.Cell(currentRow, 3).Value = t.MaterialStock?.MaterialId ?? "";
                ws.Cell(currentRow, 4).Value = materialName;
                ws.Cell(currentRow, 5).Value = t.QuantityChange;
                ws.Cell(currentRow, 6).Value = t.Price;
                ws.Cell(currentRow, 7).Value = t.Date;

                ws.Range(currentRow, 1, currentRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                if (t.QuantityChange < 0)
                {
                    ws.Cell(currentRow, 5).Style.Font.FontColor = XLColor.Red;
                    ws.Cell(currentRow, 6).Style.Font.FontColor = XLColor.Red;
                }

                currentRow++;
            }

            // Formatting
            ws.Columns().AdjustToContents();
            ws.Column(6).Style.NumberFormat.Format = "#,##0.00";
            ws.Column(7).Style.NumberFormat.Format = "dd/MM/yyyy";

            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"phieu_xuat_kho_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return Results.File(stream.ToArray(), 
                contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileDownloadName: fileName);
        });

        return app;
    }
}

