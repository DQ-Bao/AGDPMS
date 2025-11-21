using AGDPMS.Shared.Models;

namespace AGDPMS.Shared.Services;

public interface IProductService
{
    Task<GetAllProjectsResult> GetAllProjectsAsync();
    Task<GetProjectCavitiesResult> GetProjectCavitiesAsync(int projectId);
    Task<GetCavityWithBOMsResult> GetCavityWithBOMsAsync(int cavityId);
    Task<UpdateCavityResult> UpdateCavityAsync(Cavity cavity);
    Task<DeleteCavityResult> DeleteCavityAsync(int cavityId);
    Task<AddCavitiesFromWStarDesignResult> AddCavitiesFromWStarDesignAsync(int projectId, IEnumerable<WStarCavity> cavities);
    Task<GetCavityProfileSummariesResult> GetCavityProfileSummariesAsync(int projectId, double stockLengthForOptimize);
    Task<GetCavityGlassAndOtherMaterialSummariesResult> GetCavityGlassAndOtherMaterialSummariesAsync(int projectId);
    Task<UpdateProfileMaterialWeightResult> UpdateProfileMaterialWeightAsync(string materialId, double weight);
    Task<AddOrUpdateProfileMaterialBasePriceResult> AddOrUpdateProfileMaterialBasePriceAsync(string materialId, double stockLength, int stockId, decimal price);
    Task<AddOrUpdateGlassMaterialBasePriceResult> AddOrUpdateGlassMaterialBasePriceAsync(string materialId, double stockWidth, double stockLength, int stockId, decimal price);
    Task<AddOrUpdateOtherMaterialBasePriceResult> AddOrUpdateOtherMaterialBasePriceAsync(string materialId, int stockId, decimal price);
}

public sealed class GetAllProjectsResult
{
    public required bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public IEnumerable<AppRFQ> Projects { get; set; } = [];
}

public sealed class GetProjectCavitiesResult
{
    public required bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public IEnumerable<Cavity> Cavities { get; set; } = [];
}

public sealed class GetCavityWithBOMsResult
{
    public required bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public Cavity? Cavity { get; set; }
}

public sealed class UpdateCavityResult
{
    public required bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

public sealed class DeleteCavityResult
{
    public required bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

public sealed class AddCavitiesFromWStarDesignResult
{
    public required bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

public sealed class GetCavityProfileSummariesResult
{
    public required bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public IEnumerable<CavityProfileSummary> Summaries { get; set; } = [];
}

public sealed class GetCavityGlassAndOtherMaterialSummariesResult
{
    public required bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public IEnumerable<CavityGlassSummary> Glasses { get; set; } = [];
    public IEnumerable<CavityOtherMaterialSummary> Others { get; set; } = [];
}

public sealed class UpdateProfileMaterialWeightResult
{
    public required bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

public sealed class AddOrUpdateProfileMaterialBasePriceResult
{
    public required bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

public sealed class AddOrUpdateGlassMaterialBasePriceResult
{
    public required bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

public sealed class AddOrUpdateOtherMaterialBasePriceResult
{
    public required bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}