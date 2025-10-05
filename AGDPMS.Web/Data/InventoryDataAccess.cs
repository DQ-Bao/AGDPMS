using Dapper;
using System.Data;

namespace AGDPMS.Web.Data;

public class InventoryDataAccess(IDbConnection conn)
{
    public Task<IEnumerable<Aluminum>> GetAllAluminumAsync()
    {
        string query = @"
        select
            a.aluminum_id as Id,
            a.aluminum_name as Name,
            a.linear_density as LinearDensity,
            a.quantity as Quantity
        from aluminum a;";

        return conn.QueryAsync<Aluminum>(query);
    }

    public async Task<Aluminum?> GetAluminumByIdAsync(string id)
    {
        string query = @"
        select
            a.aluminum_id as Id,
            a.aluminum_name as Name,
            a.linear_density as LinearDensity,
            a.quantity as Quantity
        from aluminum a
        where a.id = @Id;";
        return (await conn.QueryAsync<Aluminum>(query, new { Id = id })).FirstOrDefault();
    }

    public Task<IEnumerable<Aluminum>> GetAluminumByNameAsync(string name)
    {
        string query = @"
        select
            a.aluminum_id as Id,
            a.aluminum_name as Name,
            a.linear_density as LinearDensity,
            a.quantity as Quantity
        from aluminum a
        where a.aluminum_name like '%@Name%';";
        return conn.QueryAsync<Aluminum>(query, new { Name = name });
    }

    public async Task<Aluminum> CreateAluminumAsync(Aluminum aluminum)
    {
        string insert = @"
            insert into aluminum(aluminum_id, aluminum_name, linear_density, quantity)
            values (@Id, @Name, @LinearDensity, @Quantity);
        ";

        await conn.ExecuteScalarAsync<int>(
            insert,
            new
            {
                aluminum.Id,
                aluminum.Name,
                aluminum.LinearDensity,
                aluminum.Quantity
            }
        );
        return aluminum;
    }
}
