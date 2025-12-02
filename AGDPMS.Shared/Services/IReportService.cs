using AGDPMS.Shared.Models;

namespace AGDPMS.Shared.Services;

public interface IReportService
{
    string GenerateReceipt(IEnumerable<StockTransaction> transactions);
    string GenerateReceipt(Receipt receipt);
    string GenerateIssue(IEnumerable<StockTransaction> transactions);
    string GenerateIssue(Issue issue);
    string GenerateQuotation(Quotation quotation);
    string GenerateMaterialPlanning(IEnumerable<MaterialPlanning> plannings);
}
