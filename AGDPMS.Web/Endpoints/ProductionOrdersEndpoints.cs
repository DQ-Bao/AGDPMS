using AGDPMS.Shared.Models.DTOs;
using AGDPMS.Web.Services;
using AGDPMS.Web.Data;
using Microsoft.AspNetCore.Authorization;

namespace AGDPMS.Web.Endpoints;

public static class ProductionOrdersEndpoints
{
    public static IEndpointRouteBuilder MapProductionOrders(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders");
        
        // Test endpoint to verify routing
        group.MapGet("/test", () => Results.Ok(new { message = "Endpoint is accessible" }))
            .AllowAnonymous();
        
        group = group.RequireAuthorization(new AuthorizeAttribute { Roles = "ProductionManager,QA,Admin" });

        group.MapPost("", async (ProductionOrderService svc, ProductionOrderCreateDto dto, HttpContext ctx) =>
        {
            var userId = 0; // TODO: take from auth
            var spec = new ProductionOrderCreateSpec
            {
                Code = dto.Code,
                ProjectId = dto.ProjectId,
                Items = dto.Items.Select(i => new ProductionOrderCreateSpecItem { ProductId = i.ProductId }).ToList()
            };
            var id = await svc.CreateOrderAsync(spec, userId);
            return Results.Created($"/api/orders/{id}", new { id });
        })
        .RequireAuthorization(new AuthorizeAttribute { Roles = "ProductionManager" })
        .WithName("CreateProductionOrder")
        .WithTags("ProductionOrders");

        group.MapGet("", async (string? projectId, string? q, string? sort, string? dir, ProductionOrderDataAccess orderAccess) =>
        {
            int? pid = null;
            if (!string.IsNullOrWhiteSpace(projectId) && int.TryParse(projectId, out var parsedId))
                pid = parsedId;
            var orders = await orderAccess.ListAsync(pid, q, sort ?? "created_at", dir ?? "desc");
            return Results.Ok(orders);
        });

        group.MapGet("/{id:int}", async (
            int id,
            ProductionOrderDataAccess orderAccess,
            ProductionItemDataAccess itemAccess,
            ProductionItemStageDataAccess stageAccess,
            StageTypeDataAccess stageTypeAccess,
            UserDataAccess userAccess,
            ProductDataAccess productAccess) =>
        {
            var order = await orderAccess.GetByIdAsync(id);
            if (order is null) return Results.NotFound();
            var items = (await itemAccess.ListByOrderAsync(id)).ToList();
            var types = (await stageTypeAccess.GetAllAsync()).ToDictionary(t => t.Id);
            var allUsers = (await userAccess.GetAllAsync()).ToDictionary(u => u.Id);
            var allProducts = (await productAccess.GetAllAsync()).ToDictionary(p => p.Id, p => p.Name);
            var itemsWithStages = new List<object>();
            foreach (var it in items)
            {
                var stages = await stageAccess.ListByItemAsync(it.Id);
                var stageDtos = stages
                    .OrderBy(s => types[s.StageTypeId].DisplayOrder)
                    .Select(s => new
                    {
                        s.Id,
                        s.StageTypeId,
                        StageCode = types[s.StageTypeId].Code,
                        StageName = types[s.StageTypeId].Name,
                        types[s.StageTypeId].DisplayOrder,
                        s.AssignedQaUserId,
                        AssignedQaUserName = s.AssignedQaUserId.HasValue && allUsers.ContainsKey(s.AssignedQaUserId.Value) 
                            ? allUsers[s.AssignedQaUserId.Value].FullName 
                            : null,
                        s.IsCompleted,
                        s.CompletedAt,
                        s.RejectionCount
                    }).ToList();
                var completedStages = stageDtos.Count(s => s.IsCompleted);
                var totalStages = stageDtos.Count;
                itemsWithStages.Add(new
                {
                    it.Id,
                    it.ProductId,
                    ProductName = allProducts.ContainsKey(it.ProductId) ? allProducts[it.ProductId] : null,
                    it.LineNo,
                    it.QRCode,
                    it.IsCompleted,
                    it.CompletedAt,
                    CompletedStages = completedStages,
                    TotalStages = totalStages,
                    Stages = stageDtos
                });
            }
            return Results.Ok(new { order, items = itemsWithStages });
        });

        group.MapPost("/{id:int}/submit", async (int id, ProductionOrderService svc) =>
        {
            await svc.SubmitAsync(id);
            return Results.Ok();
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "ProductionManager" });

        group.MapPost("/{id:int}/cancel", async (int id, ProductionOrderService svc) =>
        {
            await svc.CancelAsync(id);
            return Results.Ok();
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "ProductionManager" });

        group.MapPost("/{id:int}/director/approve", async (int id, ProductionOrderService svc) =>
        {
            await svc.DirectorApproveAsync(id);
            return Results.Ok();
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

        group.MapPost("/{id:int}/director/reject", async (int id, ProductionOrderService svc) =>
        {
            await svc.DirectorRejectAsync(id);
            return Results.Ok();
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

        group.MapPost("/{id:int}/qa/machines-approve", async (int id, ProductionOrderService svc) =>
        {
            await svc.QaMachinesApproveAsync(id);
            return Results.Ok();
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "QA" });

        group.MapPost("/{id:int}/qa/material-approve", async (int id, ProductionOrderService svc) =>
        {
            await svc.QaMaterialApproveAsync(id);
            return Results.Ok();
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "QA" });

        group.MapPost("/{id:int}/finish", async (int id, ProductionOrderService svc) =>
        {
            await svc.FinishAsync(id);
            return Results.Ok();
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "ProductionManager" });

        return app;
    }
}


