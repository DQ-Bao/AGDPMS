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

        app.MapGet("/api/quotation/{pid:int}/exports", async (int pid, IProductService productService) =>
        {
            var quotationResult = await productService.CalculateQuotationAsync(pid);
            if (!quotationResult.Success || quotationResult.Quotation is null) return Results.NotFound();
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Quotation");

            int currentRow = 1;

            // ===== Title =====
            ws.Cell(currentRow, 1).Value = "QUOTATION";
            ws.Range(currentRow, 1, currentRow, 13).Merge();
            ws.Row(currentRow).Style.Font.Bold = true;
            ws.Row(currentRow).Style.Font.FontSize = 16;
            ws.Row(currentRow).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            currentRow += 2;

            // ===== Header =====
            string[] headers =
            {
                "No",
                "Code",
                "Description",
                "Width",
                "Height",
                "Qty",
                "Weight",
                "Material Price",
                "Labor Cost",
                "Profit %",
                "Tax %",
                "Unit Price",
                "Total Price"
            };

            for (int col = 1; col <= headers.Length; col++)
            {
                ws.Cell(currentRow, col).Value = headers[col - 1];
                ws.Cell(currentRow, col).Style.Font.Bold = true;
                ws.Cell(currentRow, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws.Cell(currentRow, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            currentRow++;

            // ===== Data rows =====
            int index = 1;
            foreach (var d in quotationResult.Quotation.Details)
            {
                ws.Cell(currentRow, 1).Value = index++;
                ws.Cell(currentRow, 2).Value = d.Code;
                ws.Cell(currentRow, 3).Value = d.Description;
                ws.Cell(currentRow, 4).Value = d.Width;
                ws.Cell(currentRow, 5).Value = d.Height;
                ws.Cell(currentRow, 6).Value = d.Quantity;
                ws.Cell(currentRow, 7).Value = d.Weight;
                ws.Cell(currentRow, 8).Value = d.MaterialPrice;
                ws.Cell(currentRow, 9).Value = d.Settings.LaborCost;
                ws.Cell(currentRow, 10).Value = d.Settings.ProfitPercentage;
                ws.Cell(currentRow, 11).Value = d.Settings.TaxPercentage;
                ws.Cell(currentRow, 12).Value = d.UnitPrice;
                ws.Cell(currentRow, 13).Value = d.TotalPrice;

                ws.Range(currentRow, 1, currentRow, 13)
                  .Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                currentRow++;
            }

            // ===== Totals =====
            currentRow++;

            ws.Cell(currentRow, 6).Value = "TOTAL:";
            ws.Cell(currentRow, 6).Style.Font.Bold = true;

            ws.Cell(currentRow, 7).Value = quotationResult.Quotation.TotalWeight;
            ws.Cell(currentRow, 7).Style.Font.Bold = true;

            ws.Cell(currentRow, 13).Value = quotationResult.Quotation.TotalPrice;
            ws.Cell(currentRow, 13).Style.Font.Bold = true;

            // ===== Formatting =====
            ws.Columns().AdjustToContents();
            ws.Column(8).Style.NumberFormat.Format = "#,##0.00";
            ws.Column(9).Style.NumberFormat.Format = "#,##0.00";
            ws.Column(12).Style.NumberFormat.Format = "#,##0.00";
            ws.Column(13).Style.NumberFormat.Format = "#,##0.00";
            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            stream.Position = 0;
            string fileName = $"bao_gia_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return Results.File(stream.ToArray(),
                contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileDownloadName: fileName);
        });

        return app;
    }
}

