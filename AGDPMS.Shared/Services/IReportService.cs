using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGDPMS.Shared.Services;

public interface IReportService
{
    void GenerateReciept(IEnumerable<StockTransaction> transactions);
    void GenerateIssue(IEnumerable<StockTransaction> transactions);
    void GenerateQuotation(Quotation quotation);
    void GenerateMaterialPlanning(IEnumerable<MaterialPlanning> plannings);
}
