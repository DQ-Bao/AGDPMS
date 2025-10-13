using Dapper;
using System.Data;
using AGDPMS.Shared.Models;

namespace AGDPMS.Web.Data;

public class RoleDataAccess(IDbConnection conn)
{
    public Task<IEnumerable<AppRole>> GetAllAsync() => conn.QueryAsync<AppRole>("select * from roles");
}
