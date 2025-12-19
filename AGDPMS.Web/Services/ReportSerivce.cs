using AGDPMS.Shared.Models;
using AGDPMS.Shared.Services;
using ClosedXML.Excel;
//using DocumentFormat.OpenXml.Drawing;

namespace AGDPMS.Web.Services;

public class ReportSerivce : IReportService
{
    private static readonly string templates = Path.Combine(AppContext.BaseDirectory, "Templates");
    private static readonly string exports = Path.Combine(AppContext.BaseDirectory, "Exports");
    private readonly string receipt = Path.Combine(templates, "nhap_kho.xlsx");
    private readonly string issue = Path.Combine(templates, "xuat_kho.xlsx");
    private readonly string quotation = Path.Combine(templates, "bao_gia.xlsx");
    private readonly string material_planning = Path.Combine(templates, "du_tru_vat_lieu.xlsx");

    public ReportSerivce()
    {
        if (!Directory.Exists(exports))
            Directory.CreateDirectory(exports);
    }

    public string GenerateReceipt(IEnumerable<StockReceipt> transactions)
    {
        //using var wb = new XLWorkbook(receipt);
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Phiếu nhập kho");

        int currentRow = 1;

        // ===== Title =====
        ws.Cell(currentRow, 1).Value = "Phiếu nhập kho";
        ws.Range(currentRow, 1, currentRow, 7).Merge();
        ws.Row(currentRow).Style.Font.Bold = true;
        ws.Row(currentRow).Style.Font.FontSize = 16;
        ws.Row(currentRow).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        currentRow += 2;

        // ===== Headers =====
        string[] headers =
        {
            "No",
            "Material ID",
            "Quantity Change",
            "Quantity After",
            "Unit Price",
            "Total Value",
            "Date"
        };

        for (int col = 1; col <= headers.Length; col++)
        {
            var cell = ws.Cell(currentRow, col);
            cell.Value = headers[col - 1];
            cell.Style.Font.Bold = true;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        currentRow++;

        // ===== Data rows =====
        int index = 1;
        decimal grandTotal = 0;

        foreach (var t in transactions.OrderBy(t => t.Date))
        {
            decimal rowTotal = t.QuantityChange * t.Price;
            grandTotal += rowTotal;

            ws.Cell(currentRow, 1).Value = index++;
            ws.Cell(currentRow, 2).Value = t.MaterialId;
            ws.Cell(currentRow, 3).Value = t.QuantityChange;
            ws.Cell(currentRow, 4).Value = t.QuantityAfter;
            ws.Cell(currentRow, 5).Value = t.Price;
            ws.Cell(currentRow, 6).Value = rowTotal;
            ws.Cell(currentRow, 7).Value = t.Date;

            ws.Range(currentRow, 1, currentRow, 7)
              .Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            // Red for stock issue (negative)
            if (t.QuantityChange < 0)
            {
                ws.Cell(currentRow, 3).Style.Font.FontColor = XLColor.Red;
                ws.Cell(currentRow, 6).Style.Font.FontColor = XLColor.Red;
            }

            currentRow++;
        }

        // ===== Summary =====
        currentRow++;

        ws.Cell(currentRow, 5).Value = "TOTAL:";
        ws.Cell(currentRow, 5).Style.Font.Bold = true;

        ws.Cell(currentRow, 6).Value = grandTotal;
        ws.Cell(currentRow, 6).Style.Font.Bold = true;

        // ===== Formatting =====
        ws.Columns().AdjustToContents();

        ws.Column(5).Style.NumberFormat.Format = "#,##0.00";
        ws.Column(6).Style.NumberFormat.Format = "#,##0.00";
        ws.Column(7).Style.NumberFormat.Format = "yyyy-mm-dd";

        ws.Columns().AdjustToContents();

        string filePath = Path.Combine(
            exports,
            $"xuat_kho_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
        );

        wb.SaveAs(filePath);
        return filePath;
    }

    public string GenerateIssue(IEnumerable<StockIssue> transactions)
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Stock Issue");

        int currentRow = 1;

        // ===== Title =====
        ws.Cell(currentRow, 1).Value = "STOCK ISSUE REPORT";
        ws.Range(currentRow, 1, currentRow, 7).Merge();
        ws.Row(currentRow).Style.Font.Bold = true;
        ws.Row(currentRow).Style.Font.FontSize = 16;
        ws.Row(currentRow).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        currentRow += 2;

        // ===== Headers =====
        string[] headers =
        {
            "No",
            "Material ID",
            "Quantity Change",
            "Quantity After",
            "Unit Price",
            "Total Value",
            "Date"
        };

        for (int col = 1; col <= headers.Length; col++)
        {
            var cell = ws.Cell(currentRow, col);
            cell.Value = headers[col - 1];
            cell.Style.Font.Bold = true;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        currentRow++;

        // ===== Data rows =====
        int index = 1;
        decimal grandTotal = 0;

        foreach (var t in transactions.OrderBy(t => t.Date))
        {
            decimal rowTotal = t.QuantityChange * t.Price;
            grandTotal += rowTotal;

            ws.Cell(currentRow, 1).Value = index++;
            ws.Cell(currentRow, 2).Value = t.MaterialId;
            ws.Cell(currentRow, 3).Value = t.QuantityChange;
            ws.Cell(currentRow, 4).Value = t.QuantityAfter;
            ws.Cell(currentRow, 5).Value = t.Price;
            ws.Cell(currentRow, 6).Value = rowTotal;
            ws.Cell(currentRow, 7).Value = t.Date;

            ws.Range(currentRow, 1, currentRow, 7)
              .Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            // Red for stock issue (negative)
            if (t.QuantityChange < 0)
            {
                ws.Cell(currentRow, 3).Style.Font.FontColor = XLColor.Red;
                ws.Cell(currentRow, 6).Style.Font.FontColor = XLColor.Red;
            }

            currentRow++;
        }

        // ===== Summary =====
        currentRow++;

        ws.Cell(currentRow, 5).Value = "TOTAL:";
        ws.Cell(currentRow, 5).Style.Font.Bold = true;

        ws.Cell(currentRow, 6).Value = grandTotal;
        ws.Cell(currentRow, 6).Style.Font.Bold = true;

        // ===== Formatting =====
        ws.Columns().AdjustToContents();

        ws.Column(5).Style.NumberFormat.Format = "#,##0.00";
        ws.Column(6).Style.NumberFormat.Format = "#,##0.00";
        ws.Column(7).Style.NumberFormat.Format = "yyyy-mm-dd hh:mm";

        string filePath = Path.Combine(
            exports,
            $"xuat_kho_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
        );

        wb.SaveAs(filePath);
        return filePath;
    }



    public string GenerateQuotation(Quotation quotationData)
    {
        //using var wb = new XLWorkbook(quotation);
        using var wb = new XLWorkbook();
        var ws = wb.Worksheet(1);

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
        foreach (var d in quotationData.Details)
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

        ws.Cell(currentRow, 7).Value = quotationData.TotalWeight;
        ws.Cell(currentRow, 7).Style.Font.Bold = true;

        ws.Cell(currentRow, 13).Value = quotationData.TotalPrice;
        ws.Cell(currentRow, 13).Style.Font.Bold = true;

        // ===== Formatting =====
        ws.Columns().AdjustToContents();
        ws.Column(8).Style.NumberFormat.Format = "#,##0.00";
        ws.Column(9).Style.NumberFormat.Format = "#,##0.00";
        ws.Column(12).Style.NumberFormat.Format = "#,##0.00";
        ws.Column(13).Style.NumberFormat.Format = "#,##0.00";

        string filePath = Path.Combine(
            exports,
            $"bao_gia_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
        );

        wb.SaveAs(filePath);
        return filePath;
    }


    public string GenerateMaterialPlanning(IEnumerable<MaterialPlanning> plannings)
    {
        using var wb = new XLWorkbook(material_planning);
        var ws = wb.Worksheet(1);

        // Fill data here

        string filePath = Path.Combine(exports, $"du_tru_vat_tu_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        wb.SaveAs(filePath);
        return filePath;
    }

}
