using AGDPMS.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGDPMS.Shared.Services;

public interface IInventoryService
{
    Task<GetMaterialTypeResult> GetMaterialType();
    Task<GetMaterialResult> GetMaterial();
    Task<GetMaterialResult> GetMaterialById(string id);
    Task<GetMaterialResult> GetMaterialByType(MaterialType type);
    Task<GetMaterialResult> GetMaterialByName(string name);
    Task<BaseResult> AddMaterial(Material material);
    Task<GetStockReceiptResult> GetStockReceipt();
    Task<BaseResult> AddStockReceipt(StockReceipt stockReceipt);
    Task<GetStockIssueResult> GetStockIssue();
    Task<BaseResult> AddStockIssue(StockIssue stockReceipt);
}

public class GetMaterialTypeResult : BaseResult
{
    public IEnumerable<MaterialType> MaterialTypes { get; set; } = [];
}

public class GetStockReceiptResult : BaseResult
{
    public IEnumerable<StockReceipt> StockReceipts { get; set; } = [];
}

public class GetStockIssueResult : BaseResult
{
    public IEnumerable<StockIssue> StockIssues { get; set; } = [];
}
public class GetMaterialResult : BaseResult
{
    public IEnumerable<Material> Materials { get; set; } = [];
}

public class GetMaterialPlanningResult : BaseResult
{
    public IEnumerable<MaterialPlanning> MaterialPlannings { get; set; } = [];
}