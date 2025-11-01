﻿using AGDPMS.Shared.Models;

namespace AGDPMS.Shared.Services;

public interface IProductService
{
    Task<GetProjectCavitiesResult> GetProjectCavitiesAsync(int projectId);
    Task<GetCavityWithBOMsResult> GetCavityWithBOMsAsync(int cavityId);
    Task<UpdateCavityResult> UpdateCavityAsync(Cavity cavity);
    Task<DeleteCavityResult> DeleteCavityAsync(int cavityId);
    Task<AddCavitiesFromWStarDesignResult> AddCavitiesFromWStarDesignAsync(int projectId, IEnumerable<WStarCavity> cavities);
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
