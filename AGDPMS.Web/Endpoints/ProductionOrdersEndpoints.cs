using AGDPMS.Shared.Models;
using AGDPMS.Shared.Models.DTOs;
using AGDPMS.Web.Services;
using AGDPMS.Web.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Linq;
using System.Globalization;

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
            string? statuses,
            int? status,
            ProductionOrderDataAccess orderAccess,
            ProductionItemDataAccess itemAccess,
            ProductionItemStageDataAccess stageAccess,
            UserDataAccess userAccess,
            ProjectDataAccess projectAccess,
            HttpContext httpContext) =>
        {
            int? pid = null;
            if (!string.IsNullOrWhiteSpace(projectId) && int.TryParse(projectId, out var parsedId))
                pid = parsedId;
            var allOrders = (await orderAccess.ListAsync(pid, q, sort ?? "created_at", dir ?? "desc")).ToList();
            var statusList = new List<int>();
            if (!string.IsNullOrWhiteSpace(statuses))
            {
                statusList = statuses.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                     .Select(s => int.TryParse(s, out var v) ? v : (int?)null)
                                     .Where(v => v.HasValue)
                                     .Select(v => v!.Value)
                                     .ToList();
            }
            else if (status.HasValue)
            {
                statusList.Add(status.Value);
            }

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

            if (statusList.Any())
            {
                orders = orders.Where(o => statusList.Contains((int)o.Status)).ToList();
            }

            var allUsers = (await userAccess.GetAllAsync()).ToDictionary(u => u.Id);

            // Load project names for all referenced projects
            var projectNameMap = new Dictionary<int, string?>();
            var projectIds = orders.Select(o => o.ProjectId).Distinct().ToArray();
            if (projectIds.Length > 0)
            {
                var projects = await projectAccess.GetByIdsAsync(projectIds);
                projectNameMap = projects.ToDictionary(p => p.Id, p => p.Name);
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
                allUsers.TryGetValue(o.AssignedQaUserId ?? -1, out var qaUser);
                projectNameMap.TryGetValue(o.ProjectId, out var projectName);

                result.Add(new
                {
                    o.Id,
                    o.ProjectId,
                    ProjectName = projectName,
                    o.Code,
                    o.Status,
                    o.AssignedQaUserId,
                    AssignedQaUserName = qaUser?.FullName,
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
            var orderQaName = order.AssignedQaUserId.HasValue && allUsers.ContainsKey(order.AssignedQaUserId.Value)
                ? allUsers[order.AssignedQaUserId.Value].FullName
                : null;

            return Results.Ok(new { order, orderQaName, items = itemsWithStages, progressTimeline });
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

        // Dashboard summary for Production Manager and Director
        group.MapGet("/summary", async (
            string? from,
            string? to,
            string? projectId,
            string? statuses,
            ProductionOrderDataAccess orderAccess,
            ProjectDataAccess projectAccess,
            HttpContext httpContext) =>
        {
            DateTime dateTo = DateTime.UtcNow.Date;
            DateTime dateFrom = dateTo.AddDays(-29); // default last 30 days
            if (DateTime.TryParse(from, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsedFrom))
                dateFrom = parsedFrom.Date;
            if (DateTime.TryParse(to, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsedTo))
                dateTo = parsedTo.Date;

            int? pid = null;
            if (!string.IsNullOrWhiteSpace(projectId) && int.TryParse(projectId, out var parsedPid))
                pid = parsedPid;

            var statusList = new List<int>();
            if (!string.IsNullOrWhiteSpace(statuses))
            {
                statusList = statuses.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                     .Select(s => int.TryParse(s, out var v) ? v : (int?)null)
                                     .Where(v => v.HasValue)
                                     .Select(v => v!.Value)
                                     .ToList();
            }

            var allOrders = (await orderAccess.ListAsync(pid, null, "created_at", "desc")).ToList();

            var filtered = allOrders
                .Where(o => o.CreatedAt.HasValue && o.CreatedAt.Value.Date >= dateFrom && o.CreatedAt.Value.Date <= dateTo)
                .ToList();

            if (statusList.Any())
            {
                filtered = filtered.Where(o => statusList.Contains((int)o.Status)).ToList();
            }

            // Load project names
            var projectNames = new Dictionary<int, string?>();
            var pids = filtered.Select(o => o.ProjectId).Distinct().ToArray();
            if (pids.Any())
            {
                var proj = await projectAccess.GetByIdsAsync(pids);
                projectNames = proj.ToDictionary(p => p.Id, p => p.Name);
            }

            // Status counts (exclude DirectorRejected)
            var statusCounts = filtered
                .Where(o => o.Status != ProductionOrderStatus.DirectorRejected)
                .GroupBy(o => o.Status)
                .ToDictionary(g => g.Key.ToString(), g => g.Count());

            var totalOrders = filtered.Count(o => o.Status != ProductionOrderStatus.DirectorRejected);
            var inProduction = filtered.Count(o => o.Status == ProductionOrderStatus.InProduction);
            var finished = filtered.Count(o => o.Status == ProductionOrderStatus.Finished);
            var open = filtered.Count(o =>
                o.Status == ProductionOrderStatus.Draft ||
                o.Status == ProductionOrderStatus.PendingDirectorApproval ||
                o.Status == ProductionOrderStatus.PendingQACheckMachines ||
                o.Status == ProductionOrderStatus.PendingQACheckMaterial);

            // On-time vs late (order-level)
            int onTime = 0, late = 0;
            foreach (var o in filtered)
            {
                if (!o.PlannedFinishDate.HasValue) continue;
                var planned = o.PlannedFinishDate.Value;
                if (o.Status == ProductionOrderStatus.Finished && o.FinishedAt.HasValue)
                {
                    if (o.FinishedAt.Value <= planned) onTime++; else late++;
                }
                else if (o.Status == ProductionOrderStatus.InProduction || o.Status == ProductionOrderStatus.PendingQACheckMachines || o.Status == ProductionOrderStatus.PendingQACheckMaterial)
                {
                    if (DateTime.UtcNow <= planned) onTime++; else late++;
                }
            }

            // Timeline by week (ISO week) - label by week start date (dd/MM)
            var timeline = new Dictionary<string, (int Started, int Finished)>();
            foreach (var o in filtered)
            {
                if (o.StartedAt.HasValue)
                {
                    var week = ISOWeek.GetWeekOfYear(o.StartedAt.Value);
                    var year = o.StartedAt.Value.Year;
                    var monday = ISOWeek.ToDateTime(year, week, DayOfWeek.Monday);
                    var key = monday.ToString("dd/MM");
                    timeline.TryGetValue(key, out var entry);
                    entry.Started++;
                    timeline[key] = entry;
                }
                if (o.FinishedAt.HasValue)
                {
                    var week = ISOWeek.GetWeekOfYear(o.FinishedAt.Value);
                    var year = o.FinishedAt.Value.Year;
                    var monday = ISOWeek.ToDateTime(year, week, DayOfWeek.Monday);
                    var key = monday.ToString("dd/MM");
                    timeline.TryGetValue(key, out var entry);
                    entry.Finished++;
                    timeline[key] = entry;
                }
            }
            var timelineList = timeline.OrderBy(k => k.Key).Select(k => new { Week = k.Key, Started = k.Value.Started, Finished = k.Value.Finished }).ToList();

            // Avg cycle time (days) for finished
            var finishedDurations = filtered
                .Where(o => o.Status == ProductionOrderStatus.Finished && o.StartedAt.HasValue && o.FinishedAt.HasValue)
                .Select(o => (o.FinishedAt.Value - o.StartedAt.Value).TotalDays);
            double? avgCycleDays = finishedDurations.Any() ? finishedDurations.Average() : null;

            // Recent orders
            var recent = filtered
                .Where(o => o.Status != ProductionOrderStatus.DirectorRejected)
                .OrderByDescending(o => o.CreatedAt)
                .Take(10)
                .Select(o =>
                {
                    projectNames.TryGetValue(o.ProjectId, out var pname);
                    var planned = o.PlannedFinishDate;
                    bool? isLate = null;
                    if (planned.HasValue)
                    {
                        if (o.Status == ProductionOrderStatus.Finished && o.FinishedAt.HasValue)
                            isLate = o.FinishedAt.Value > planned.Value;
                        else if (o.Status == ProductionOrderStatus.InProduction || o.Status == ProductionOrderStatus.PendingQACheckMachines || o.Status == ProductionOrderStatus.PendingQACheckMaterial)
                            isLate = DateTime.UtcNow > planned.Value;
                    }
                    return new
                    {
                        o.Id,
                        o.Code,
                        o.ProjectId,
                        ProjectName = pname,
                        o.Status,
                        o.PlannedFinishDate,
                        o.StartedAt,
                        o.FinishedAt,
                        IsLate = isLate
                    };
                }).ToList();

            var summary = new
            {
                DateFrom = dateFrom,
                DateTo = dateTo,
                TotalOrders = totalOrders,
                OpenOrders = open,
                InProduction = inProduction,
                Finished = finished,
                AvgCycleDays = avgCycleDays,
                StatusCounts = statusCounts,
                OnTimeLate = new { OnTime = onTime, Late = late },
                Timeline = timelineList,
                RecentOrders = recent
            };

            return Results.Ok(summary);
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Production Manager,Director" });

        group.MapPost("/{id:int}/submit", async (int id, ProductionOrderService svc) =>
        {
            await svc.SubmitAsync(id);
            return Results.Ok();
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Production Manager,Director" });

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

        group.MapPut("/{id:int}/assign-qa", async (int id, AssignQaOrderDto dto, ProductionOrderService svc) =>
        {
            await svc.AssignQaAsync(id, dto.QaUserId);
            return Results.Ok();
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Production Manager,Director" });

        group.MapPut("/{id:int}/plan", async (int id, UpdateOrderPlanDto dto, ProductionOrderService svc) =>
        {
            await svc.UpdateOrderPlanAsync(id, dto.PlannedStartDate, dto.PlannedFinishDate);
            return Results.Ok();
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Production Manager" });

        return app;
    }
}
