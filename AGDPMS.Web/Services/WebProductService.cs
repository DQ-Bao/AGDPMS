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
                Code = c.Description.Trim(),
                ProjectId = projectId,
                Width = c.Width,
                Height = c.Height,
                Location = c.Location?.Trim(),
                Quantity = c.Quantity,
                WindowType = c.WindowType?.Trim(),
                BOMs = [..c.Materials.Select(m => new CavityBOM
                {
                    CavityId = 0, // Invalid value, set later
                    Material = new Material
                    {
                        Id = m.Symbol.Trim().Replace(" ", string.Empty),
                        Name = m.Description.Trim(),
                        Type = m.MatType switch // Hack, revise this
                        {
                            "Profile" => MaterialType.Aluminum,
                            "Kinh" => MaterialType.Glass,
                            "PKKK" => MaterialType.Accessory,
                            "Gioang" => MaterialType.Gasket,
                            "Vat tu phu" => MaterialType.Auxiliary,
                            _ => null,
                        },
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

    public async Task<GetCavityProfileSummariesResult> GetCavityProfileSummariesAsync(int projectId, double stockLengthForOptimize)
    {
        try
        {
            var boms = await cavityDataAccess.GetBOMsOfTypeAsync(projectId, [MaterialType.Aluminum]);
            var grouped = boms.GroupBy(b => b.BOM.Material);
            var summaries = grouped.Select(g =>
            {
                var cuts = g.GroupBy(bom => bom.BOM.Length)
                    .Select(bomGroup => new CavityProfileSummary.ProfileCut(
                        CutLength: bomGroup.Key,
                        Quantity: bomGroup.Sum(bom => bom.BOM.Quantity * bom.CavityQuantity)
                    ))
                    .OrderBy(cut => cut.CutLength)
                    .ToList();
                double[] stockLengths = [.. g.Key.Stocks.Select(s => s.Length)];
                int[] stockLengthQuantities = [.. g.Key.Stocks.Select(s => s.Stock)];
                double[] cutLengths = [.. cuts.Select(c => c.CutLength)];
                double[] cutLengthDemands = [.. cuts.Select(c => (double)c.Quantity)];
                var opt = ICutOptimizationService.Solve(stockLengthForOptimize, cutLengths, cutLengthDemands); // optimization from stock is not implemented yet.
                List<CavityProfileSummary.CutSolution> cutSolutions = [];
                for (int i = 0; i < opt.patterns.Count; i++)
                {
                    var pattern = opt.patterns[i];
                    int quantity = i < opt.pattern_quantity.Length ? (int)opt.pattern_quantity[i] : 0;
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

                if (!g.Key.Stocks.Any(s => s.Length == stockLengthForOptimize))
                    g.Key.Stocks.Add(new MaterialStock { Length = stockLengthForOptimize });
                
                return new CavityProfileSummary(g.Key)
                {
                    Id = g.Key.Id,
                    Name = g.Key.Name,
                    Type = g.Key.Type,
                    Cuts = cuts,
                    CutSolutions = cutSolutions
                };
            }).OrderBy(s => s.Id).ToList();
            return new GetCavityProfileSummariesResult { Success = true, Summaries = summaries };
        }
        catch (Exception ex)
        {
            return new GetCavityProfileSummariesResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<GetCavityGlassAndOtherMaterialSummariesResult> GetCavityGlassAndOtherMaterialSummariesAsync(int projectId)
    {
        try
        {
            var glassBOMs = await cavityDataAccess.GetBOMsOfTypeAsync(projectId, [MaterialType.Glass]);
            var glassGrouped = glassBOMs.GroupBy(b => b.BOM.Material);
            var glassSummaries = new List<CavityGlassSummary>();
            foreach (var g in glassGrouped)
            {
                var cuts = new List<CavityGlassSummary.GlassCut>();
                foreach (var grp in g.GroupBy(b => (b.BOM.Width, b.BOM.Length)))
                {
                    var stock = grp.First().BOM.Material.Stocks
                                   .FirstOrDefault(s => s.Width == grp.Key.Width && s.Length == grp.Key.Length);
                    var cavityCodeArrays = await Task.WhenAll(grp.Select(b => cavityDataAccess.GetCodeByIdAsync(b.BOM.CavityId)));
                    var cavityCodes = cavityCodeArrays.SelectMany(c => c).ToList();

                    cuts.Add(new CavityGlassSummary.GlassCut
                    {
                        Width = grp.Key.Width,
                        Length = grp.Key.Length,
                        Quantity = grp.Sum(b => b.BOM.Quantity * b.CavityQuantity),
                        StockId = stock?.Id ?? 0,
                        Stock = stock?.Stock ?? 0,
                        UnitPrice = stock?.BasePrice ?? 0m,
                        Color = grp.First().BOM.Color,
                        CavityCodes = cavityCodes
                    });
                }

                glassSummaries.Add(new CavityGlassSummary(g.Key)
                {
                    Id = g.Key.Id,
                    Name = g.Key.Name,
                    Type = g.Key.Type,
                    Cuts = cuts
                });
            }

            var otherBOMs = await cavityDataAccess.GetBOMsOfTypeAsync(projectId, [MaterialType.Accessory, MaterialType.Gasket, MaterialType.Auxiliary]);
            var otherGrouped = otherBOMs.GroupBy(b => b.BOM.Material);
            var otherSummaries = otherGrouped.Select(g =>
            {
                var bom = g.First().BOM;
                var stock = bom.Material.Stocks.FirstOrDefault();

                return new CavityOtherMaterialSummary(g.Key)
                {
                    Id = g.Key.Id,
                    Name = g.Key.Name,
                    Type = bom.Material.Type,
                    Stock = stock ?? new(),
                    Quantity = g.Sum(b => b.BOM.Quantity * b.CavityQuantity),
                    UnitPrice = stock?.BasePrice ?? 0,
                    GasketLength = bom.Material.Type == MaterialType.Gasket ? bom.Length : null,
                    Color = bom.Color,
                    Unit = bom.Unit

                };
            }).ToList();
            return new GetCavityGlassAndOtherMaterialSummariesResult { Success = true, Glasses = glassSummaries, Others = otherSummaries };
        }
        catch (Exception ex)
        {
            return new GetCavityGlassAndOtherMaterialSummariesResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<UpdateProfileMaterialWeightResult> UpdateProfileMaterialWeightAsync(string materialId, double weight)
    {
        try
        {
            await cavityDataAccess.UpdateMaterialWeightAsync(materialId, weight);
            return new UpdateProfileMaterialWeightResult { Success = true };
        }
        catch (Exception ex)
        {
            return new UpdateProfileMaterialWeightResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<AddOrUpdateProfileMaterialBasePriceResult> AddOrUpdateProfileMaterialBasePriceAsync(string materialId, double stockLength, int stockId, decimal price)
    {
        try
        {
            if (stockId == 0) await cavityDataAccess.AddMaterialStockAsync(materialId, new MaterialStock { Length = stockLength, BasePrice = price });
            else await cavityDataAccess.UpdateMaterialStockBasePriceAsync(stockId, price);
            return new AddOrUpdateProfileMaterialBasePriceResult { Success = true };
        }
        catch (Exception ex)
        {
            return new AddOrUpdateProfileMaterialBasePriceResult { Success  = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<AddOrUpdateGlassMaterialBasePriceResult> AddOrUpdateGlassMaterialBasePriceAsync(string materialId, double stockWidth, double stockLength, int stockId, decimal price)
    {
        try
        {
            if (stockId == 0) await cavityDataAccess.AddMaterialStockAsync(materialId, new MaterialStock { Width = stockWidth, Length = stockLength, BasePrice = price });
            else await cavityDataAccess.UpdateMaterialStockBasePriceAsync(stockId, price);
            return new AddOrUpdateGlassMaterialBasePriceResult { Success = true };
        }
        catch (Exception ex)
        {
            return new AddOrUpdateGlassMaterialBasePriceResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<AddOrUpdateOtherMaterialBasePriceResult> AddOrUpdateOtherMaterialBasePriceAsync(string materialId, int stockId, decimal price)
    {
        try
        {
            if (stockId == 0) await cavityDataAccess.AddMaterialStockAsync(materialId, new MaterialStock { BasePrice = price });
            else await cavityDataAccess.UpdateMaterialStockBasePriceAsync(stockId, price);
            return new AddOrUpdateOtherMaterialBasePriceResult { Success = true };
        }
        catch (Exception ex)
        {
            return new AddOrUpdateOtherMaterialBasePriceResult { Success = false, ErrorMessage = ex.Message };
        }
    }
}
