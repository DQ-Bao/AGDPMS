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
        where a.aluminum_id = @Id;";
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

    public Task<IEnumerable<Glass>> GetAllGlassAsync()
    {
        string query = @"
        select
            g.glass_id as Id,
            g.glass_name as Name,
            g.thickness as Thickness,
            g.quantity as Quantity
        from glass g;";

        return conn.QueryAsync<Glass>(query);
    }

    public async Task<Glass?> GetGlassByIdAsync(string id)
    {
        string query = @"
        select
            g.glass_id as Id,
            g.glass_name as Name,
            g.thickness as Thickness,
            g.quantity as Quantity
        from glass g
        where g.glass_id = @Id;";
        return (await conn.QueryAsync<Glass>(query, new { Id = id })).FirstOrDefault();
    }

    public Task<IEnumerable<Glass>> GetGlassByNameAsync(string name)
    {
        string query = @"
        select
            g.glass_id as Id,
            g.glass_name as Name,
            g.thickness as Thickness,
            g.quantity as Quantity
        from glass g
        where g.glass_name like '%@Name%';";
        return conn.QueryAsync<Glass>(query, new { Name = name });
    }

    public async Task<Glass> CreateGlassAsync(Glass glass)
    {
        string insert = @"
            insert into glass(glass_id, glass_name, thickness, quantity)
            values (@Id, @Name, @Thickness, @Quantity);
        ";

        await conn.ExecuteScalarAsync<int>(
            insert,
            new
            {
                glass.Id,
                glass.Name,
                glass.Thickness,
                glass.Quantity
            }
        );
        return glass;
    }

    public Task<IEnumerable<Accessory>> GetAllAccessoryAsync()
    {
        string query = @"
        select
            a.accessory_id as Id,
            a.accessory_name as Name,
            a.quantity as Quantity
        from accessory a;";

        return conn.QueryAsync<Accessory>(query);
    }

    public async Task<Accessory?> GetAccessoryByIdAsync(string id)
    {
        string query = @"
        select
            a.accessory_id as Id,
            a.accessory_name as Name,
            a.quantity as Quantity
        from accessory a
        where a.accessory_id = @Id;";
        return (await conn.QueryAsync<Accessory>(query, new { Id = id })).FirstOrDefault();
    }

    public Task<IEnumerable<Accessory>> GetAccessoryByNameAsync(string name)
    {
        string query = @"
        select
            a.accessory_id as Id,
            a.accessory_name as Name,
            a.quantity as Quantity
        from accessory a
        where g.accessory_name like '%@Name%';";
        return conn.QueryAsync<Accessory>(query, new { Name = name });
    }

    public async Task<Accessory> CreateAccessoryAsync(Accessory accessory)
    {
        string insert = @"
            insert into accessory(glass_id, glass_name, quantity)
            values (@Id, @Name, @Quantity);
        ";

        await conn.ExecuteScalarAsync<int>(
            insert,
            new
            {
                accessory.Id,
                accessory.Name,
                accessory.Quantity
            }
        );
        return accessory;
    }
}
