using AGDPMS.Shared.Models;

namespace AGDPMS.Shared.Services;

public interface IReportService
{
    string GenerateReceipt(IEnumerable<StockTransaction> transactions);
    string GenerateIssue(IEnumerable<StockTransaction> transactions);
    string GenerateQuotation(Quotation quotation);
    string GenerateMaterialPlanning(IEnumerable<MaterialPlanning> plannings);
}
