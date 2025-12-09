using AGDPMS.Shared.Models;
using AGDPMS.Shared.Models.DTOs;
using AGDPMS.Web.Services;
using AGDPMS.Web.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AGDPMS.Web.Endpoints;

public static class ProductionOrdersEndpoints
{
    public static IEndpointRouteBuilder MapProductionOrders(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders");
        
        // Test endpoint to verify routing
        group.MapGet("/test", () => Results.Ok(new { message = "Endpoint is accessible" }))
            .AllowAnonymous();
        
        group = group.RequireAuthorization(new AuthorizeAttribute { Roles = "Production Manager,QA,Director" });

        group.MapPost("", async (ProductionOrderService svc, ProductionOrderCreateDto dto, HttpContext ctx) =>
        {
            var userIdClaim = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = 0;
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var parsedUserId))
                userId = parsedUserId;
            
            var spec = new ProductionOrderCreateSpec
            {
                ProjectId = dto.ProjectId,
                Items = dto.Items.Select(i => new ProductionOrderCreateSpecItem { CavityId = i.CavityId, Quantity = i.Quantity }).ToList(),
                TimeSettings = dto.TimeSettings
            };
            var id = await svc.CreateOrderAsync(spec, userId);
            return Results.Created($"/api/orders/{id}", new { id });
        })
        .RequireAuthorization(new AuthorizeAttribute { Roles = "Production Manager" })
        .WithName("CreateProductionOrder")
        .WithTags("ProductionOrders");

        group.MapGet("", async (
            string? projectId,
            string? q,
            string? sort,
            string? dir,
            int? status,
            ProductionOrderDataAccess orderAccess,
            ProductionItemDataAccess itemAccess,
            ProductionItemStageDataAccess stageAccess,
            HttpContext httpContext) =>
        {
            int? pid = null;
            if (!string.IsNullOrWhiteSpace(projectId) && int.TryParse(projectId, out var parsedId))
                pid = parsedId;
            var allOrders = (await orderAccess.ListAsync(pid, q, sort ?? "created_at", dir ?? "desc")).ToList();

            // Get user role and ID for filtering
            var userRole = httpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var userIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            int? userId = null;
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var parsedUserId))
                userId = parsedUserId;

            // Filter orders based on role
            var orders = new List<ProductionOrder>();
            if (userRole == "Director" || userRole == "Admin")
            {
                // Director can see all orders EXCEPT Draft
                orders = allOrders.Where(o => o.Status != ProductionOrderStatus.Draft).ToList();
            }
            else if (userRole == "Production Manager" || userRole == "ProductionManager")
            {
                // PM can see all orders
                orders = allOrders.ToList();
            }
            else if (userRole == "QA" || userRole == "Qa")
            {
                // QA can see: PendingQACheckMachines, PendingQACheckMaterial, or InProduction with assigned items
                var qaOrders = allOrders.Where(o => 
                    o.Status == ProductionOrderStatus.PendingQACheckMachines ||
                    o.Status == ProductionOrderStatus.PendingQACheckMaterial).ToList();
                
                // Also include InProduction orders where QA is assigned to any stage
                if (userId.HasValue)
                {
                    var inProductionOrders = allOrders.Where(o => o.Status == ProductionOrderStatus.InProduction).ToList();
                    foreach (var order in inProductionOrders)
                    {
                        var items = await itemAccess.ListByOrderAsync(order.Id);
                        foreach (var item in items)
                        {
                            var stages = await stageAccess.ListByItemAsync(item.Id);
                            if (stages.Any(s => s.AssignedQaUserId == userId.Value))
                            {
                                qaOrders.Add(order);
                                break;
                            }
                        }
                    }
                }
                orders = qaOrders;
            }
            else
            {
                // Other roles see nothing
                orders = new List<ProductionOrder>();
            }

            if (status.HasValue)
            {
                orders = orders.Where(o => (int)o.Status == status.Value).ToList();
            }

            // compute hasPendingQa per order: any stage assigned and not completed
            var result = new List<object>();
            foreach (var o in orders)
            {
                var items = await itemAccess.ListByOrderAsync(o.Id);
                var hasPendingQa = false;
                foreach (var it in items)
                {
                    var stages = await stageAccess.ListByItemAsync(it.Id);
                    if (stages.Any(s => s.AssignedQaUserId.HasValue && !s.IsCompleted))
                    {
                        hasPendingQa = true;
                        break;
                    }
                }
                result.Add(new
                {
                    o.Id,
                    o.ProjectId,
                    o.Code,
                    o.Status,
                    o.PlannedStartDate,
                    o.PlannedFinishDate,
                    o.CreatedAt,
                    o.SubmittedAt,
                    o.DirectorDecisionAt,
                    o.QAMachinesCheckedAt,
                    o.QAMaterialCheckedAt,
                    o.StartedAt,
                    o.FinishedAt,
                    o.UpdatedAt,
                    HasPendingQa = hasPendingQa
                });
            }
            return Results.Ok(result);
        });

        group.MapGet("/{id:int}", async (
            int id,
            ProductionOrderDataAccess orderAccess,
            ProductionItemDataAccess itemAccess,
            ProductionItemStageDataAccess stageAccess,
            StageTypeDataAccess stageTypeAccess,
            UserDataAccess userAccess,
            CavityDataAccess cavityAccess) =>
        {
            var order = await orderAccess.GetByIdAsync(id);
            if (order is null) return Results.NotFound();
            var items = (await itemAccess.ListByOrderAsync(id)).ToList();
            var types = (await stageTypeAccess.GetAllAsync()).ToDictionary(t => t.Id);
            // Create order mapping based on code order in data.sql
            var stageTypeOrder = new Dictionary<string, int>
            {
                { "CUT_AL", 1 },
                { "MILL_LOCK", 2 },
                { "DOOR_CORNER_CUT", 3 },
                { "ASSEMBLE_FRAME", 4 },
                { "GLASS_INSTALL", 5 },
                { "PRESS_GASKET", 6 },
                { "INSTALL_ACCESSORIES", 7 },
                { "CUT_FLUSH", 8 },
                { "FINISH_SILICON", 9 }
            };
            var allUsers = (await userAccess.GetAllAsync()).ToDictionary(u => u.Id);
            var allCavities = (await cavityAccess.GetAllFromProjectAsync(order.ProjectId)).ToDictionary(c => c.Id);
            var itemsWithStages = new List<object>();
            var completionDates = new List<DateTime>();
            foreach (var it in items)
            {
                var stages = (await stageAccess.ListByItemAsync(it.Id)).ToList();
                // Filter: Hide stages with planned_time_hours = 0 or NULL unless order is Draft
                var filteredStages = stages.Where(s =>
                    (s.PlannedTimeHours > 0 || s.PlannedTimeHours.HasValue) ||
                    order.Status == ProductionOrderStatus.Draft
                ).ToList();
                var stageDtos = filteredStages
                    .OrderBy(s =>
                    {
                        if (types.ContainsKey(s.StageTypeId))
                        {
                            var code = types[s.StageTypeId].Code;
                            return stageTypeOrder.ContainsKey(code) ? stageTypeOrder[code] : 999;
                        }
                        return 999;
                    })
                    .ThenBy(s => s.StageTypeId)
                    .Select(s => new
                    {
                        s.Id,
                        s.StageTypeId,
                        StageCode = types.ContainsKey(s.StageTypeId) ? types[s.StageTypeId].Code : string.Empty,
                        StageName = types.ContainsKey(s.StageTypeId) ? types[s.StageTypeId].Name : $"Stage Type {s.StageTypeId}",
                        s.PlannedStartDate,
                        s.PlannedFinishDate,
                        s.ActualStartDate,
                        s.ActualFinishDate,
                        s.PlannedTimeHours,
                        s.ActualTimeHours,
                        s.AssignedQaUserId,
                        AssignedQaUserName = s.AssignedQaUserId.HasValue && allUsers.ContainsKey(s.AssignedQaUserId.Value) 
                            ? allUsers[s.AssignedQaUserId.Value].FullName 
                            : null,
                        s.IsCompleted,
                        s.CompletedAt
                    }).ToList();

                // Calculate completed stages - only check IsCompleted flag
                var completedStages = stageDtos.Count(s => s.IsCompleted);
                var totalStages = stageDtos.Count;
                var needsQa = stageDtos.Any(s => s.AssignedQaUserId != null && !s.IsCompleted);
                var totalPlannedHours = stageDtos.Sum(s => (decimal?)(s.PlannedTimeHours ?? 0m)) ?? 0m;
                var totalActualHours = stageDtos.Sum(s => (decimal?)(s.ActualTimeHours ?? 0m)) ?? 0m;
                var now = DateTime.UtcNow;
                var isLate = false;
                var isOverdue = false;
                var daysLate = 0;

                if (it.PlannedFinishDate.HasValue)
                {
                    var planFinish = it.PlannedFinishDate.Value;
                    // Item is late if actual finish date is later than planned finish date (even if not completed)
                    if (it.ActualFinishDate.HasValue && it.ActualFinishDate.Value > planFinish)
                    {
                        isLate = true;
                        daysLate = (int)Math.Ceiling((it.ActualFinishDate.Value - planFinish).TotalDays);
                    }
                    // Or if not completed and current time is past planned finish date
                    else if (!it.IsCompleted && now > planFinish)
                    {
                        isLate = true;
                        isOverdue = true;
                        daysLate = (int)Math.Ceiling((now - planFinish).TotalDays);
                    }
                }

                if (it.IsCompleted && it.CompletedAt.HasValue)
                {
                    completionDates.Add(it.CompletedAt.Value);
                }

                var cavity = allCavities.ContainsKey(it.CavityId) ? allCavities[it.CavityId] : null;
                itemsWithStages.Add(new
                {
                    it.Id,
                    it.CavityId,
                    CavityCode = cavity?.Code,
                    CavityName = cavity != null ? $"{cavity.Code} - {cavity.Description ?? cavity.Location ?? ""}" : null,
                    it.Code,
                    it.LineNo,
                    it.QRCode,
                    it.IsCompleted,
                    it.IsStored,
                    it.CompletedAt,
                    it.PlannedStartDate,
                    it.PlannedFinishDate,
                    it.ActualStartDate,
                    it.ActualFinishDate,
                    PlannedTimeHours = totalPlannedHours,
                    ActualTimeHours = totalActualHours,
                    NeedsQa = needsQa,
                    IsLate = isLate,
                    IsOverdue = isOverdue,
                    DaysLate = daysLate,
                    CompletedStages = completedStages,
                    TotalStages = totalStages,
                    Stages = stageDtos.Select(s => new
                    {
                        s.Id,
                        s.StageTypeId,
                        s.StageName,
                        s.IsCompleted,
                        s.PlannedStartDate,
                        s.PlannedFinishDate,
                        s.ActualStartDate,
                        s.ActualFinishDate,
                        s.AssignedQaUserId,
                        s.AssignedQaUserName,
                        s.PlannedTimeHours,
                        s.ActualTimeHours
                    }).ToList()
                });
            }
            var progressTimeline = completionDates
                .GroupBy(d => d.Date)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    Date = g.Key,
                    CompletedCount = g.Count()
                }).ToList();
            return Results.Ok(new { order, items = itemsWithStages, progressTimeline });
        });

        // Project cost management (EVM) data
        group.MapGet("/{id:int}/cost-management", async (int id, bool? includeLaborCost, ProjectCostManagementService svc) =>
        {
            try
            {
                var dto = await svc.CalculateEVMAsync(id, includeLaborCost ?? true);
                return Results.Ok(dto);
            }
            catch (InvalidOperationException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Production Manager,Director" });

        group.MapPost("/{id:int}/submit", async (int id, ProductionOrderService svc) =>
        {
            await svc.SubmitAsync(id);
            return Results.Ok();
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Production Manager" });

        group.MapPost("/{id:int}/cancel", async (int id, ProductionOrderService svc) =>
        {
            await svc.CancelAsync(id);
            return Results.Ok();
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Production Manager" });

        group.MapPost("/{id:int}/director/approve", async (int id, ProductionOrderService svc) =>
        {
            await svc.DirectorApproveAsync(id);
            return Results.Ok();
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Director" });

        group.MapPost("/{id:int}/director/reject", async (int id, ProductionOrderService svc) =>
        {
            await svc.DirectorRejectAsync(id);
            return Results.Ok();
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Director" });

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
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Production Manager" });

        group.MapPost("/{id:int}/revert-to-draft", async (int id, ProductionOrderService svc) =>
        {
            await svc.RevertToDraftAsync(id);
            return Results.Ok();
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Production Manager" });

        group.MapPut("/{id:int}/plan", async (int id, UpdateOrderPlanDto dto, ProductionOrderService svc) =>
        {
            await svc.UpdateOrderPlanAsync(id, dto.PlannedStartDate, dto.PlannedFinishDate);
            return Results.Ok();
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Production Manager" });

        return app;
    }
}
