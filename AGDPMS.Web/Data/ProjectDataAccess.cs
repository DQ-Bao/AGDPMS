using System.Data;
using Dapper;
using AGDPMS.Shared.Models;

namespace AGDPMS.Web.Data;

public class ProjectDataAccess(IDbConnection conn)
{
    public Task<IEnumerable<(int Id, string? Name)>> SearchAsync(string? q, ProjectRFQStatus? status = null) => conn.QueryAsync<(int, string?)>(@"
        select id, name from projects
        where (@Q is null or @Q = '' or (name ilike '%'||@Q||'%'))
          and (@Status is null or status = @Status)
        order by id desc",
        new { Q = q, Status = status?.ToString() });

    public Task<IEnumerable<(int Id, string? Name)>> GetByIdsAsync(IEnumerable<int> ids) => conn.QueryAsync<(int, string?)>(@"
        select id, name 
        from projects
        where id = any(@Ids)",
        new { Ids = ids.ToArray() });
}


