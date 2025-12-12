using AGDPMS.Shared.Models;

namespace AGDPMS.Shared.Services;

public interface IReportService
{
    string GenerateReceipt(IEnumerable<StockReceipt> transactions);
    string GenerateIssue(IEnumerable<StockIssue> transactions);
    string GenerateQuotation(Quotation quotation);
    string GenerateMaterialPlanning(IEnumerable<MaterialPlanning> plannings);
}
