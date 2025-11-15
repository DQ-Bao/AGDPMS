using AGDPMS.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGDPMS.Shared.Services;

public interface IInventoryService
{
    Task<GetMaterialResult> GetMaterial();
    Task<GetMaterialResult> GetMaterialById(string id);
    Task<GetMaterialResult> GetMaterialByType(MaterialType type);
    Task<GetMaterialResult> GetMaterialByName(string name);

}

public class GetMaterialResult : BaseResult
{
    public IEnumerable<Material> Materials { get; set; } = [];
}