using AGDPMS.Shared.Models;
using AGDPMS.Shared.Services;
using AGDPMS.Web.Data;

namespace AGDPMS.Web.Services;

internal class WebProductService(CavityDataAccess cavityDataAccess) : IProductService
{
    public async Task<GetProjectCavitiesResult> GetProjectCavitiesAsync(int projectId)
    {
        try
        {
            var cavities = await cavityDataAccess.GetAllFromProjectAsync(projectId);
            return new GetProjectCavitiesResult { Success = true, Cavities = cavities };
        }
        catch (Exception ex)
        {
            return new GetProjectCavitiesResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<GetCavityWithBOMsResult> GetCavityWithBOMsAsync(int cavityId)
    {
        try
        {
            var cavity = await cavityDataAccess.GetByIdWithBOMsAsync(cavityId);
            if (cavity is null)
                return new GetCavityWithBOMsResult { Success = false, ErrorMessage = "Không tìm thấy cửa" };
            return new GetCavityWithBOMsResult { Success = true, Cavity = cavity };
        }
        catch (Exception ex)
        {
            return new GetCavityWithBOMsResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<UpdateCavityResult> UpdateCavityAsync(Cavity cavity)
    {
        try
        {
            await cavityDataAccess.UpdateAsync(cavity);
            return new UpdateCavityResult { Success = true };
        }
        catch (Exception ex)
        {
            return new UpdateCavityResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<DeleteCavityResult> DeleteCavityAsync(int cavityId)
    {
        try
        {
            await cavityDataAccess.DeleteAsync(cavityId);
            return new DeleteCavityResult { Success = true };
        }
        catch (Exception ex)
        {
            return new DeleteCavityResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<AddCavitiesFromWStarDesignResult> AddCavitiesFromWStarDesignAsync(int projectId, IEnumerable<WStarCavity> cavities)
    {
        try
        {
            var error = await cavityDataAccess.AddOrUpdateBatchAsync(cavities.Select(c => new Cavity
            {
                Code = c.Description,
                ProjectId = projectId,
                Width = c.Width,
                Height = c.Height,
                Location = c.Location,
                Quantity = c.Quantity,
                AluminumVendor = c.WindowType,
                BOMs = [..c.Materials.Select(m => new CavityBOM
                {
                    CavityId = 0, // Invalid value, set later
                    Material = new Material
                    {
                        Id = m.Symbol,
                        Name = m.Description,
                        Type = m.MatType switch // Hack, revise this
                        {
                            "Profile" => new MaterialType { Id = 1 },
                            "Kinh" => new MaterialType { Id = 2 },
                            "PKKK" => new MaterialType { Id = 3 },
                            "Gioang" => new MaterialType { Id = 4 },
                            "Vat tu phu" => new MaterialType { Id = 5 },
                            _ => null,
                        }
                    },
                    Quantity = m.Num,
                    Length = m.Length,
                    Width = m.Width,
                    Color = m.Color,
                    Unit = m.Unit,
                })]
            }));
            return new AddCavitiesFromWStarDesignResult { Success = error is null, ErrorMessage = error };
        }
        catch (Exception ex)
        {
            return new AddCavitiesFromWStarDesignResult { Success = false, ErrorMessage = ex.Message };
        }
    }
}
