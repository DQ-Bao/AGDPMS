using AGDPMS.Web.Data;
using System.Data;
using Dapper;
using AGDPMS.Shared.Models;
using System.Text;

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

        group.MapGet("/order-items-for-stamps", async (
            string? q,
            string? statuses,
            string? items,
            ProductionItemDataAccess itemAccess,
            ProductionOrderDataAccess orderAccess,
            CavityDataAccess cavityAccess,
            IDbConnection conn) =>
        {
            if (conn.State != System.Data.ConnectionState.Open)
            {
                conn.Open();
            }

            // Parse item IDs filter
            var itemIdList = new List<int>();
            if (!string.IsNullOrWhiteSpace(items))
            {
                itemIdList = items.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(id => int.TryParse(id, out var v) ? v : (int?)null)
                    .Where(id => id.HasValue)
                    .Select(id => id!.Value)
                    .ToList();
            }

            // Parse stage status filter (checkbox - multiple values)
            var statusList = new List<StageStatus>();
            if (!string.IsNullOrWhiteSpace(statuses))
            {
                var statusValues = statuses.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(s => int.TryParse(s, out var v) ? v : (int?)null)
                    .Where(v => v.HasValue && Enum.IsDefined(typeof(StageStatus), v!.Value))
                    .Select(v => (StageStatus)v!.Value)
                    .ToList();
                statusList = statusValues;
            }

            // Build query - use item status (StageStatus) from production_order_items
            var sql = @"
                select 
                    poi.id as ItemId,
                    poi.code as ItemCode,
                    poi.qr_image as QRImage,
                    po.id as OrderId,
                    po.code as OrderCode,
                    po.status as OrderStatus,
                    poi.cavity_id as CavityId,
                    coalesce(poi.status, 0) as StageStatus
                from production_order_items poi
                join production_orders po on po.id = poi.production_order_id
                where po.is_cancelled = false";

            var parameters = new DynamicParameters();

            if (itemIdList.Any())
            {
                sql += " and poi.id = any(@ItemIds)";
                parameters.Add("ItemIds", itemIdList.ToArray());
            }

            if (statusList.Any())
            {
                sql += " and coalesce(pis.status, 0) = any(@Statuses)";
                parameters.Add("Statuses", statusList.Select(s => (short)s).ToArray());
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                sql += " and (poi.code ilike @Search or po.code ilike @Search)";
                parameters.Add("Search", $"%{q}%");
            }

            sql += " order by po.code, poi.line_no";

            var queryResults = await conn.QueryAsync<(int ItemId, string ItemCode, byte[]? QRImage, int OrderId, string? OrderCode, ProductionOrderStatus OrderStatus, int CavityId, short StageStatus)>(sql, parameters);

            // Get all cavities
            var allCavityIds = queryResults.Select(i => i.CavityId).Distinct().ToList();
            var cavities = new Dictionary<int, Cavity>();
            if (allCavityIds.Any())
            {
                var cavitiesList = await conn.QueryAsync<Cavity>(@"
                    select id as Id, code as Code, project_id as ProjectId, description as Description,
                           width as Width, height as Height, location as Location, quantity as Quantity, window_type as WindowType
                    from cavities
                    where id = any(@CavityIds)",
                    new { CavityIds = allCavityIds.ToArray() });
                foreach (var cavity in cavitiesList)
                {
                    cavities[cavity.Id] = cavity;
                }
            }

            // Group by order
            var ordersDict = new Dictionary<int, OrderWithItemsDto>();
            foreach (var item in queryResults)
            {
                if (!ordersDict.ContainsKey(item.OrderId))
                {
                    ordersDict[item.OrderId] = new OrderWithItemsDto
                    {
                        orderId = item.OrderId,
                        orderCode = item.OrderCode,
                        items = new List<OrderItemDto>()
                    };
                }

                var cavity = cavities.GetValueOrDefault(item.CavityId);
                var productName = cavity?.Description ?? cavity?.Code ?? "N/A";
                var qrImageDataUri = item.QRImage != null && item.QRImage.Length > 0
                    ? $"data:image/png;base64,{Convert.ToBase64String(item.QRImage)}"
                    : null;

                ordersDict[item.OrderId].items.Add(new OrderItemDto
                {
                    itemId = item.ItemId,
                    itemCode = item.ItemCode,
                    orderId = item.OrderId,
                    orderCode = item.OrderCode,
                    orderStatus = item.OrderStatus,
                    stageStatus = (StageStatus)item.StageStatus,
                    productName = productName,
                    qrImage = qrImageDataUri
                });
            }

            return Results.Ok(ordersDict.Values.OrderBy(o => o.orderCode).ToList());
        });

        return app;
    }

    private class OrderWithItemsDto
    {
        public int orderId { get; set; }
        public string? orderCode { get; set; }
        public List<OrderItemDto> items { get; set; } = new();
    }

    private class OrderItemDto
    {
        public int itemId { get; set; }
        public string itemCode { get; set; } = "";
        public int orderId { get; set; }
        public string? orderCode { get; set; }
        public ProductionOrderStatus orderStatus { get; set; }
        public StageStatus stageStatus { get; set; }
        public string productName { get; set; } = "";
        public string? qrImage { get; set; }
    }
}


