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

    public string GenerateReceipt(IEnumerable<StockTransaction> transactions)
    {
        using var wb = new XLWorkbook(receipt);
        var ws = wb.Worksheet(1);

        // Fill data here if needed
        // int row = 5;
        // foreach (var t in transactions)
        // {
        //     ws.Cell(row, 1).Value = t.ItemName;
        //     ws.Cell(row, 2).Value = t.Quantity;
        //     row++;
        // }

        string filePath = Path.Combine(exports, $"nhap_kho_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        wb.SaveAs(filePath);
        return filePath;
    }
    public string GenerateReceipt(Receipt receipt)
    {
        throw new NotImplementedException();
    }

    public string GenerateIssue(IEnumerable<StockTransaction> transactions)
    {
        using var wb = new XLWorkbook(issue);
        var ws = wb.Worksheet(1);

        // Fill data here

        string filePath = Path.Combine(exports, $"xuat_kho_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        wb.SaveAs(filePath);
        return filePath;
    }

    public string GenerateIssue(Issue issue)
    {
        throw new NotImplementedException();
    }

    public string GenerateQuotation(Quotation quotationData)
    {
        using var wb = new XLWorkbook(quotation);
        var ws = wb.Worksheet(1);

        // Fill data here

        string filePath = Path.Combine(exports, $"bao_gia_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
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
