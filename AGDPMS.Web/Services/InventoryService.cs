using AGDPMS.Shared.Models;
using AGDPMS.Shared.Services;
using AGDPMS.Web.Data;
<<<<<<< Updated upstream
using System.Xml.Linq;

namespace AGDPMS.Web.Services;

public class InventoryService : IInventoryService
{
    InventoryDataAccess _inventoryDataAccess;
    public Task<IEnumerable<Material>> GetMaterial()
    {
        return _inventoryDataAccess.GetAllMaterialAsync();
    }

    public Task<IEnumerable<Material>> GetMaterialById(string id)
    {
        return _inventoryDataAccess.GetMaterialByIdAsync(id);
    }

    public Task<IEnumerable<Material>> GetMaterialByName(string name)
    {
        return _inventoryDataAccess.GetMaterialByNameAsync(name);
    }

    public Task<IEnumerable<Material>> GetMaterialByType(MaterialType type)
    {
        return _inventoryDataAccess.GetMaterialByTypeAsync(type);
=======

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
>>>>>>> Stashed changes
    }

    public async Task<PagedResult<Material>> GetMaterialPage(int pageNumber = 1, int pageSize = 10)
    {
        IEnumerable<Material> items = await _inventoryDataAccess.GetAllMaterialAsync();
        int totalCount = items.Count();

        return new PagedResult<Material>
        {
            Items = items.ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<Material>> GetMaterialPageById(string id, int pageNumber = 1, int pageSize = 10)
    {
        IEnumerable<Material> items = await _inventoryDataAccess.GetMaterialByIdAsync(id);
        int totalCount = items.Count();

        return new PagedResult<Material>
        {
            Items = items.ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<Material>> GetMaterialPageByName(string name, int pageNumber = 1, int pageSize = 10)
    {
        IEnumerable<Material> items = await _inventoryDataAccess.GetMaterialByNameAsync(name);
        int totalCount = items.Count();

        return new PagedResult<Material>
        {
            Items = items.ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<Material>> GetMaterialPageByType(MaterialType type, int pageNumber = 1, int pageSize = 10)
    {
        IEnumerable<Material> items = await _inventoryDataAccess.GetMaterialByTypeAsync(type);
        int totalCount = items.Count();

        return new PagedResult<Material>
        {
            Items = items.ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
