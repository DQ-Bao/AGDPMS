using AGDPMS.Shared.Models.DTOs;
using AGDPMS.Web.Data;
using AGDPMS.Web.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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

        // PM can mark a stage completed without QA
        group.MapPost("/{stageId:int}/pm-complete", async (int stageId, StageService svc) =>
        {
            await svc.CompleteStageByPmAsync(stageId);
            return Results.Ok();
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Production Manager" });

        group.MapPost("/{stageId:int}/reject", async (int stageId, StageDecisionDto dto, StageService svc, ProductionRejectReportDataAccess rejectAccess, HttpContext httpContext) =>
        {
            var userIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var rejectedByUserId))
            {
                return Results.BadRequest("Invalid user");
            }
            await svc.RejectAsync(stageId, dto.Reason ?? string.Empty, rejectedByUserId, rejectAccess);
            return Results.Ok();
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Qa" });

        group.MapPost("/{stageId:int}/cancel", async (int stageId, StageService svc) =>
        {
            await svc.CancelStageAsync(stageId);
            return Results.Ok();
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Production Manager" });

        group.MapGet("/{stageId:int}/latest-reject-reason", async (int stageId, ProductionRejectReportDataAccess rejectAccess) =>
        {
            var report = await rejectAccess.GetLatestByStageIdAsync(stageId);
            if (report is null)
            {
                return Results.NotFound();
            }
            return Results.Ok(new { Id = report.Id, Reason = report.Reason, CreatedAt = report.CreatedAt, RejectedByUserId = report.RejectedByUserId });
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Production Manager,Qa" });

        group.MapPut("/reject-reports/{reportId:int}", async (int reportId, UpdateRejectReasonDto dto, ProductionRejectReportDataAccess rejectAccess, HttpContext httpContext) =>
        {
            var userIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var currentUserId))
            {
                return Results.BadRequest("Invalid user");
            }
            
            // Get the report to check ownership
            var report = await rejectAccess.GetByIdAsync(reportId);
            if (report is null)
            {
                return Results.NotFound();
            }
            
            // Only the author can edit
            if (report.RejectedByUserId != currentUserId)
            {
                return Results.Forbid();
            }
            
            await rejectAccess.UpdateReasonAsync(reportId, dto.Reason);
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

        itemGroup.MapPost("/{itemId:int}/stages", async (int itemId, int? stageTypeId, StageService svc) =>
        {
            if (stageTypeId.HasValue)
            {
                var id = await svc.AddStageToItemAsync(itemId, stageTypeId.Value);
                return Results.Created($"/api/stages/{id}", new { id });
            }
            else
            {
                return Results.BadRequest("stageTypeId is required");
            }
        });

        return app;
    }
}


