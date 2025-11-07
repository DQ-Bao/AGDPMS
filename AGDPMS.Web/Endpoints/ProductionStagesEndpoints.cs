using AGDPMS.Shared.Models.DTOs;
using AGDPMS.Web.Data;
using AGDPMS.Web.Services;
using Microsoft.AspNetCore.Authorization;

namespace AGDPMS.Web.Endpoints;

public static class ProductionStagesEndpoints
{
    public static IEndpointRouteBuilder MapProductionStages(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/stages")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Production Manager,Qa" });

        group.MapPost("/{stageId:int}/assign-qa", async (int stageId, AssignStageQaDto dto, StageService svc) =>
        {
            await svc.AssignQaAsync(stageId, dto.QaUserId);
            return Results.Ok();
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Production Manager" });

        group.MapPost("/{stageId:int}/approve", async (int stageId, StageService svc) =>
        {
            await svc.ApproveAsync(stageId);
            return Results.Ok();
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Qa" });

        group.MapPost("/{stageId:int}/reject", async (int stageId, StageDecisionDto dto, StageService svc, ProductionRejectReportDataAccess rejectAccess) =>
        {
            await svc.RejectAsync(stageId, rejectedByUserId: 0, reason: dto.Reason ?? string.Empty, rejectAccess);
            return Results.Ok();
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Qa" });

        var itemGroup = app.MapGroup("/api/items").RequireAuthorization(new AuthorizeAttribute { Roles = "Production Manager" });
        itemGroup.MapPost("/{itemId:int}/complete", async (int itemId, StageService svc) =>
        {
            await svc.ForceCompleteItemAsync(itemId);
            return Results.Ok();
        });

        // helper to fetch order id from item id (for UI convenience)
        app.MapGet("/api/items/{itemId:int}/order-id", async (int itemId, ProductionItemDataAccess items) =>
        {
            var it = await items.GetByIdAsync(itemId);
            return Results.Ok(it?.ProductionOrderId ?? 0);
        });

        itemGroup.MapPost("/{itemId:int}/stages", async (int itemId, int stageTypeId, StageService svc) =>
        {
            var id = await svc.AddStageToItemAsync(itemId, stageTypeId);
            return Results.Created($"/api/stages/{id}", new { id });
        });

        return app;
    }
}


