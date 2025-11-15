using System.Data;
using Dapper;

namespace AGDPMS.Web.Data;

public class ProjectDataAccess(IDbConnection conn)
{
    public Task<IEnumerable<(int Id, string? Name)>> SearchAsync(string? q) => conn.QueryAsync<(int, string?)>(@"
        select id, name from projects
        where @Q is null or @Q = '' or (name ilike '%'||@Q||'%')
        order by id desc",
        new { Q = q });
}


