using AGDPMS.Shared.Models;
using AGDPMS.Shared.Services;
using AGDPMS.Web.Data;
using Google.OrTools.ConstraintSolver;

namespace AGDPMS.Web.Services;

public class InventoryService(InventoryDataAccess inventoryDataAccess) : IInventoryService
{
    public async Task<GetMaterialTypeResult> GetMaterialType()
    {
        try
        {
            return new GetMaterialTypeResult
            {
                Success = true,
                MaterialTypes = await inventoryDataAccess.GetMaterialTypeAsync()
            };

        }
        catch (Exception e)
        {
            return new GetMaterialTypeResult { Success = false, ErrorMessage = e.Message };
        }
    }
    public async Task<GetMaterialResult> GetMaterial()
    {
        try
        {
            return new GetMaterialResult
            {
                Success = true,
                Materials = await inventoryDataAccess.GetAllMaterialAsync()
            };

        } catch (Exception e)
        {
            return new GetMaterialResult { Success = false, ErrorMessage = e.Message };
        }

    }

    public async Task<GetMaterialResult> GetMaterialById(string id)
    {
        try
        {
            return new GetMaterialResult
            {
                Success = true,
                Materials = await inventoryDataAccess.GetMaterialByIdAsync(id)
            };

        }
        catch (Exception e)
        {
            return new GetMaterialResult { Success = false, ErrorMessage = e.Message };
        }
    }

    public async Task<GetMaterialResult> GetMaterialByName(string name)
    {

        try
        {
            return new GetMaterialResult
            {
                Success = true,
                Materials = await inventoryDataAccess.GetMaterialByNameAsync(name)
            };

        }
        catch (Exception e)
        {
            return new GetMaterialResult { Success = false, ErrorMessage = e.Message };
        }
    }

    public async Task<GetMaterialResult> GetMaterialByType(MaterialType type)
    {

        try
        {
            return new GetMaterialResult
            {
                Success = true,
                Materials = await inventoryDataAccess.GetMaterialByTypeAsync(type)
            };

        }
        catch (Exception e)
        {
            return new GetMaterialResult { Success = false, ErrorMessage = e.Message };
        }
    }

    public async Task<GetStockReceiptResult> GetStockReceipt()
    {
        try
        {
            return new GetStockReceiptResult
            {
                Success = true,
                StockReceipts = await inventoryDataAccess.GetStockReceiptAsync()
            };

        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
            return new GetStockReceiptResult { Success = false, ErrorMessage = e.Message };
        }
    }

    public async Task<GetStockIssueResult> GetStockIssue()
    {
        try
        {
            return new GetStockIssueResult
            {
                Success = true,
                StockIssues = await inventoryDataAccess.GetStockIssueAsync()
            };

        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
            return new GetStockIssueResult { Success = false, ErrorMessage = e.Message };
        }
    }

    public async Task<BaseResult> AddMaterial(Material material)
    {
        try
        {
            await inventoryDataAccess.CreateMaterialAsync(material);
            return new BaseResult { Success = true};
        }
        catch(Exception e)
        {
            return new BaseResult { Success = false, ErrorMessage = e.Message };
        }
    }

    public async Task<BaseResult> AddStockReceipt(StockReceipt stockReceipt)
    {
        try
        {
            await inventoryDataAccess.CreateStockImportAsync(stockReceipt);
            return new BaseResult { Success = true };
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
            return new BaseResult { Success = false, ErrorMessage = e.Message };
        }
    }

    public async Task<BaseResult> AddStockIssue(StockIssue stockReceipt)
    {

        try
        {
            await inventoryDataAccess.CreateStockImportAsync(stockReceipt);
            return new BaseResult { Success = true };
        }
        catch (Exception e)
        {
            return new BaseResult { Success = false, ErrorMessage = e.Message };
        }
    }
}
