using AGDPMS.Shared.Models;
using AGDPMS.Shared.Services;
using AGDPMS.Web.Data;

namespace AGDPMS.Web.Services;

internal class WebProductService(
    ProjectRFQDataAccess projectDataAccess,
    CavityDataAccess cavityDataAccess) : IProductService
{
    public async Task<GetAllProjectsResult> GetAllProjectsAsync()
    {
        try
        {
            var projects = await projectDataAccess.GetAllAsync();
            return new GetAllProjectsResult { Success = true, Projects = projects };
        }
        catch (Exception ex)
        {
            return new GetAllProjectsResult { Success = false, ErrorMessage = ex.Message };
        }
    }

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
                        Code = m.Symbol,
                        Name = m.Description,
                        Type = m.MatType switch // Hack, revise this
                        {
                            "Profile" => MaterialType.Aluminum,
                            "Kinh" => MaterialType.Glass,
                            "PKKK" => MaterialType.Accessory,
                            "Gioang" => MaterialType.Gasket,
                            "Vat tu phu" => MaterialType.Auxiliary,
                            _ => null,
                        },
                        StockLength = m.MatType switch
                        {
                            "Profile" => 6000, // Hardcoded, should let users set the default
                            _ => 0,
                        },
                        Vendor = m.MatVendor
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

    public async Task<GetCavityProfileSummariesResult> GetCavityProfileSummariesAsync(int projectId)
    {
        try
        {
            var boms = await cavityDataAccess.GetBOMsOfTypeAsync(projectId, MaterialType.Aluminum);
            var avgPrices = await cavityDataAccess.GetAveragePricesGroupByCodeAsync();
            var grouped = boms.GroupBy(b => new { b.Material.Code, b.Material.Name, b.Material.Weight, b.Material.Stock });
            var summaries = grouped.Select(g =>
            {
                var cuts = g.GroupBy(x => x.Length)
                    .Select(x => new CavityProfileSummary.ProfileCut(CutLength: x.Key, Quantity: x.Sum(b => b.Quantity)))
                    .OrderBy(x => x.CutLength)
                    .ToList();
                double[] lengths = [.. cuts.Select(c => c.CutLength)];
                double[] demands = [.. cuts.Select(c => (double)c.Quantity)];
                var opt = ICutOptimizationService.Solve(6000, lengths, demands); // Hardcoded stock length
                List<CavityProfileSummary.CutSolution> cutSolutions = [];
                for (int i = 0; i < opt.patterns.Count; i++)
                {
                    var pattern = opt.patterns[i];
                    int quantity = 0;
                    if (i < opt.pattern_quantity.Length) quantity = (int)opt.pattern_quantity[i];
                    List<CavityProfileSummary.ProfileCut> patternCuts = [];
                    for (int j = 0; j < opt.lengths.Length; j++)
                    {
                        int count = (int)pattern[j];
                        if (count > 0) patternCuts.Add(new CavityProfileSummary.ProfileCut(opt.lengths[j], count));
                    }
                    if (quantity > 0)
                    {
                        cutSolutions.Add(new CavityProfileSummary.CutSolution
                        {
                            StockLength = opt.stock_len,
                            Quantity = quantity,
                            Pattern = patternCuts
                        });
                    }
                }
                decimal unitPrice = avgPrices.TryGetValue(g.Key.Code, out var price) ? price : 0;
                return new CavityProfileSummary
                {
                    Code = g.Key.Code,
                    Name = g.Key.Name,
                    Cuts = cuts,
                    CutSolutions = cutSolutions,
                    WeightPerMeter = g.Key.Weight,
                    BarsInStockByStockLength = new()
                    {
                        [6000] = g.Key.Stock
                    },
                    UnitPriceByStockLength = new()
                    {
                        [6000] = unitPrice
                    }
                };
            }).OrderBy(x => x.Code).ToList();
            return new GetCavityProfileSummariesResult { Success = true, Summaries = summaries };
        }
        catch (Exception ex)
        {
            return new GetCavityProfileSummariesResult { Success = false, ErrorMessage = ex.Message };
        }
    }
}
