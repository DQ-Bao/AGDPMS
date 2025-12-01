using AGDPMS.Shared.Models;
using AGDPMS.Shared.Models.DTOs;
using AGDPMS.Web.Data;
using Microsoft.AspNetCore.Authorization;

namespace AGDPMS.Web.Endpoints;

public static class StageTypesEndpoints
{
    public static IEndpointRouteBuilder MapStageTypes(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/stage-types")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Production Manager" });

        group.MapGet("", async (StageTypeDataAccess access) =>
        {
            var list = await access.GetAllAsync();
            return Results.Ok(list);
        });

        group.MapPost("", async (StageTypeDataAccess access, StageTypeDto dto) =>
        {
            var model = new StageType
            {
                Code = dto.Code,
                Name = dto.Name,
                IsActive = dto.IsActive,
                IsDefault = dto.IsDefault
            };
            var id = await access.CreateAsync(model);
            return Results.Created($"/api/stage-types/{id}", new { Id = id });
        });

        group.MapPut("/{id:int}", async (int id, StageTypeDataAccess access, StageTypeDto dto) =>
        {
            var model = new StageType
            {
                Id = id,
                Code = dto.Code,
                Name = dto.Name,
                IsActive = dto.IsActive,
                IsDefault = dto.IsDefault
            };
            await access.UpdateAsync(model);
            return Results.Ok();
        });

        return app;
    }
}


