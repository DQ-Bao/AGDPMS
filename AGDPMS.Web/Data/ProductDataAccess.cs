using System.Data;
using Dapper;

namespace AGDPMS.Web.Data;

public class ProductDataAccess(IDbConnection conn)
{
    public Task<IEnumerable<(int Id, string? Name)>> SearchByProjectAsync(int projectId, string? q) => conn.QueryAsync<(int, string?)>(@"
        select id, name from products
        where project_id = @ProjectId
          and (@Q is null or @Q = '' or (coalesce(name,'') ilike '%'||@Q||'%' or cast(id as text) ilike '%'||@Q||'%'))
        order by id desc",
        new { ProjectId = projectId, Q = q });

    public Task<IEnumerable<(int Id, string? Name)>> GetAllAsync() => conn.QueryAsync<(int, string?)>(@"
        select id, name from products
        order by id");
}


