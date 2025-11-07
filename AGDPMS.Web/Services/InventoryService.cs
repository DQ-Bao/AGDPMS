using AGDPMS.Shared.Models;
using AGDPMS.Shared.Services;
using AGDPMS.Web.Data;

namespace AGDPMS.Web.Services;

public class InventoryService(InventoryDataAccess _inventoryDataAccess) : IInventoryService
{
    public async Task<GetMaterialResult> GetMaterial()
    {
        try
        {
            return new GetMaterialResult
            {
                Success = true,
                Materials = await _inventoryDataAccess.GetAllMaterialAsync()
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
                Materials = await _inventoryDataAccess.GetMaterialByIdAsync(id)
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
                Materials = await _inventoryDataAccess.GetMaterialByNameAsync(name)
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
                Materials = await _inventoryDataAccess.GetMaterialByTypeAsync(type)
            };

        }
        catch (Exception e)
        {
            return new GetMaterialResult { Success = false, ErrorMessage = e.Message };
        }
    }
}
