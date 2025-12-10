using AGDPMS.Web.Data;
using System.Data;
using Dapper;
using AGDPMS.Shared.Models;

namespace AGDPMS.Web.Endpoints;

public static class LookupEndpoints
{
    public static IEndpointRouteBuilder MapLookup(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/lookup");

        group.MapGet("/projects", async (string? q, ProjectDataAccess access) =>
        {
            var list = await access.SearchAsync(q);
            return Results.Ok(list.Select(t => new { id = t.Id, name = t.Name }));
        });

        group.MapGet("/cavities", async (int projectId, string? q, CavityDataAccess access) =>
        {
            var cavities = await access.GetAllFromProjectAsync(projectId);
            var filtered = cavities;
            if (!string.IsNullOrWhiteSpace(q))
            {
                var qLower = q.ToLower();
                filtered = cavities.Where(c => 
                    (c.Code?.ToLower().Contains(qLower) ?? false) ||
                    (c.Description?.ToLower().Contains(qLower) ?? false) ||
                    (c.Location?.ToLower().Contains(qLower) ?? false) ||
                    (c.WindowType?.ToLower().Contains(qLower) ?? false)
                );
            }
            return Results.Ok(filtered.Select(c => new { 
                id = c.Id, 
                code = c.Code,
                description = c.Description,
                location = c.Location,
                windowType = c.WindowType,
                width = c.Width,
                height = c.Height,
                quantity = c.Quantity
            }));
        });

        group.MapGet("/qa-users", async (string? q, string? scope, UserDataAccess access, IDbConnection conn) =>
        {
            var useOrderScope = string.Equals(scope, "order", StringComparison.OrdinalIgnoreCase);

            // naive search by fullname/phone for users with QA role
            var users = await access.GetAllAsync();
            var qa = users.Where(u => string.Equals(u.Role.Name, "QA", StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(q))
            {
                qa = qa.Where(u => (u.FullName?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false)
                                 || (u.PhoneNumber?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false));
            }
            
            // Workload
            var qaList = qa.ToList();
            var qaIds = qaList.Select(u => u.Id).ToList();
            
            var itemCounts = new Dictionary<int, int>();
            if (qaIds.Any())
            {
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    conn.Open();
                }
                
                var counts = useOrderScope
                    ? await conn.QueryAsync<(int? QaUserId, int Count)>(@"
                        select po.assigned_qa_user_id as QaUserId,
                               sum(
                                   (case when po.qa_machines_checked_at is null then 1 else 0 end) +
                                   (case when po.qa_material_checked_at is null then 1 else 0 end)
                               ) as Count
                        from production_orders po
                        where po.assigned_qa_user_id = any(@QaIds)
                          and po.status between @StatusStart and @StatusEnd
                          and po.is_cancelled = false
                        group by po.assigned_qa_user_id",
                        new
                        {
                            QaIds = qaIds,
                            StatusStart = (short)ProductionOrderStatus.PendingQACheckMachines,
                            StatusEnd = (short)ProductionOrderStatus.InProduction
                        })
                    : await conn.QueryAsync<(int? QaUserId, int Count)>(@"
                        select pis.assigned_qa_user_id as QaUserId, count(distinct pis.id) as Count
                        from production_item_stages pis
                        join production_order_items poi on poi.id = pis.production_order_item_id
                        join production_orders po on po.id = poi.production_order_id
                        where pis.assigned_qa_user_id = any(@QaIds)
                          and pis.is_completed = false
                          and coalesce(pis.planned_time_hours, 0) > 0
                          and po.status between @StatusStart and @StatusEnd
                          and po.is_cancelled = false
                          and not exists (
                              select 1 from stage_reviews sr
                              where sr.production_item_stage_id = pis.id
                                and sr.status in ('pending', 'in_progress')
                          )
                        group by pis.assigned_qa_user_id",
                        new
                        {
                            QaIds = qaIds,
                            StatusStart = (short)ProductionOrderStatus.PendingQACheckMachines,
                            StatusEnd = (short)ProductionOrderStatus.InProduction
                        });
                
                foreach (var result in counts)
                {
                    if (result.QaUserId.HasValue)
                    {
                        itemCounts[result.QaUserId.Value] = result.Count;
                    }
                }
            }
            
            return Results.Ok(qaList.Select(u => new 
            { 
                id = u.Id, 
                name = u.FullName, 
                phone = u.PhoneNumber,
                pendingItemsCount = itemCounts.GetValueOrDefault(u.Id, 0)
            }));
        });

        return app;
    }
}


