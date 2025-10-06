using Dapper;
using System.Data;

namespace AGDPMS.Web.Data;
public class RoleDataAccess(IDbConnection conn)
{
    public Task<IEnumerable<AppRole>> GetAllAsync() => conn.QueryAsync<AppRole>("select * from roles");
}
