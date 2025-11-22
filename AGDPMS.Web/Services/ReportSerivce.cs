using AGDPMS.Shared.Services;
using ClosedXML.Excel;

namespace AGDPMS.Web.Services;

public class ReportSerivce : IReportService
{
    string receipt = Path.Combine(AppContext.BaseDirectory, "Templates", "nhap_kho.xlsx");
    string issue = Path.Combine(AppContext.BaseDirectory, "Templates", "xuat_kho.xlsx");
    string quotation = Path.Combine(AppContext.BaseDirectory, "Templates", "bao_gia.xlsx");
    string material_planning = Path.Combine(AppContext.BaseDirectory, "Templates", "du_tru_vat_lieu.xlsx");

    void GenerateReciept(IEnumerable<StockTransaction> transactions);
    void GenerateIssue(IEnumerable<StockTransaction> transactions);
    void GenerateQuotation(Quotation quotation);
    void GenerateMaterialPlanning(IEnumerable<MaterialPlanning> plannings);
}
