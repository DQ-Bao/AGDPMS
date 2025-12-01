using AGDPMS.Shared.Models;
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
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Production Manager,Qa,Director" });

        group.MapPut("/{stageId:int}/assign-qa", async (int stageId, AssignStageQaDto dto, StageService svc) =>
        {
            await svc.AssignQaAsync(stageId, dto.QaUserId);
            return Results.Ok();
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Production Manager" });

        // PM can mark a stage completed
        group.MapPost("/{stageId:int}/pm-complete", async (int stageId, StageService svc) =>
        {
            await svc.CompleteByPmAsync(stageId);
            return Results.Ok();
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Production Manager" });

        // Issue management
        group.MapPost("/{stageId:int}/issues", async (int stageId, CreateIssueReportDto dto, ProductionIssueReportDataAccess issueAccess, HttpContext httpContext) =>
        {
            var userIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var createdByUserId))
            {
                return Results.BadRequest("Invalid user");
            }
            var id = await issueAccess.CreateAsync(stageId, createdByUserId, dto.Priority, dto.Reason);
            return Results.Created($"/api/stages/{stageId}/issues/{id}", new { id });
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Qa,Director" });

        group.MapGet("/{stageId:int}/issues", async (int stageId, ProductionIssueReportDataAccess issueAccess) =>
        {
            var issues = await issueAccess.GetByStageIdAsync(stageId);
            return Results.Ok(issues);
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Production Manager,Qa,Director" });

        group.MapPut("/issues/{issueId:int}/resolve", async (int issueId, ProductionIssueReportDataAccess issueAccess, HttpContext httpContext) =>
        {
            var userIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var resolvedByUserId))
            {
                return Results.BadRequest("Invalid user");
            }
            await issueAccess.ResolveIssueAsync(issueId, resolvedByUserId);
            return Results.Ok();
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Production Manager" });

        // Progress and planning
        group.MapPut("/{stageId:int}/plan", async (int stageId, UpdateStagePlanDto dto, StageService svc) =>
        {
            try
            {
                await svc.UpdatePlanAsync(stageId, dto.PlannedStartDate, dto.PlannedFinishDate, dto.PlannedTimeHours);
                return Results.Ok();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Production Manager" });

        group.MapPut("/{stageId:int}/dates", async (int stageId, UpdateStageDatesDto dto, StageService svc) =>
        {
            await svc.UpdateActualDatesAsync(stageId, dto.ActualStartDate, dto.ActualFinishDate, dto.ActualTimeHours);
            return Results.Ok();
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Production Manager" });

        group.MapPut("/{stageId:int}/status", async (int stageId, UpdateStageStatusDto dto, StageService svc) =>
        {
            await svc.UpdateStatusAsync(stageId, (StageStatus)dto.Status);
            return Results.Ok();
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Production Manager" });

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

        itemGroup.MapPost("/{itemId:int}/assign-qa-bulk", async (int itemId, AssignItemQaDto dto, StageService svc) =>
        {
            await svc.BulkAssignQaToItemAsync(itemId, dto.QaUserId);
            return Results.Ok();
        });

        itemGroup.MapPut("/{itemId:int}/plan", async (int itemId, UpdateItemPlanDto dto, StageService svc) =>
        {
            try
            {
                await svc.UpdateItemPlanAsync(itemId, dto.PlannedStartDate, dto.PlannedFinishDate);
                return Results.Ok();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        });

        itemGroup.MapPut("/{itemId:int}/actuals", async (int itemId, UpdateItemActualDto dto, StageService svc) =>
        {
            try
            {
                await svc.UpdateItemActualsAsync(itemId, dto.ActualStartDate, dto.ActualFinishDate);
                return Results.Ok();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        });

        itemGroup.MapPut("/{itemId:int}/completion", async (int itemId, UpdateItemCompletionDto dto, StageService svc) =>
        {
            try
            {
                await svc.SetItemCompletionStatusAsync(itemId, dto.IsCompleted);
                return Results.Ok();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        });

        return app;
    }
}


