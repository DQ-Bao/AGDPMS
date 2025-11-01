using Dapper;
using System.Data;
using AGDPMS.Shared.Models;

namespace AGDPMS.Web.Data;

public class MachineTypeDataAccess(IDbConnection conn)
{
    // Dùng để lấy danh sách cho dropdown
    public Task<IEnumerable<AppMachineType>> GetAllAsync() =>
        conn.QueryAsync<AppMachineType>(@"
            SELECT id as Id, name as Name 
            FROM machine_types 
            ORDER BY name
        ");
}