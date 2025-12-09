using AGDPMS.Web.Data;
using System.Data;
using Dapper;

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

        group.MapGet("/qa-users", async (string? q, UserDataAccess access, IDbConnection conn) =>
        {
            // naive search by fullname/phone for users with QA role
            var users = await access.GetAllAsync();
            var qa = users.Where(u => string.Equals(u.Role.Name, "QA", StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(q))
            {
                qa = qa.Where(u => (u.FullName?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false)
                                 || (u.PhoneNumber?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false));
            }
            
            // Get counts of items not reviewed for each QA
            var qaList = qa.ToList();
            var qaIds = qaList.Select(u => u.Id).ToList();
            
            var itemCounts = new Dictionary<int, int>();
            if (qaIds.Any())
            {
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    conn.Open();
                }
                
                var counts = await conn.QueryAsync<(int? QaUserId, int Count)>(@"
                    select pis.assigned_qa_user_id as QaUserId, count(distinct pis.id) as Count
                    from production_item_stages pis
                    where pis.assigned_qa_user_id = any(@QaIds)
                      and pis.is_completed = false
                      and not exists (
                          select 1 from stage_reviews sr
                          where sr.production_item_stage_id = pis.id
                            and sr.status in ('pending', 'in_progress')
                      )
                    group by pis.assigned_qa_user_id",
                    new { QaIds = qaIds });
                
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


