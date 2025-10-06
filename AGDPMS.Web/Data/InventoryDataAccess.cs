using Dapper;
using System.Data;

namespace AGDPMS.Web.Data;

public class InventoryDataAccess(IDbConnection conn)
{
    public Task<IEnumerable<Material>> GetAllMaterialAsync()
    {
        string query = @"
        select
            m.id as Id,
            m.name as Name,
            m.type as Type,
            m.stock as Stock,
            m.weight as Weight,
            m.thickness as Thickness
        from material m;";

        return conn.QueryAsync<Material>(query);
    }
    public Task<IEnumerable<Material>> GetAllAluminumAsync()
    {
        string query = @"
        select
            m.id as Id,
            m.name as Name,
            m.type as Type,
            m.stock as Stock,
            m.weight as Weight,
            m.thickness as Thickness
        from material m
        where m.type = 'aluminum';";

        return conn.QueryAsync<Material>(query);
    }

    public Task<IEnumerable<Material>> GetAllGlassAsync()
    {
        string query = @"
        select
            m.id as Id,
            m.name as Name,
            m.type as Type,
            m.stock as Stock,
            m.weight as Weight,
            m.thickness as Thickness
        from material m
        where m.type = 'glass';";

        return conn.QueryAsync<Material>(query);
    }

    public Task<IEnumerable<Material>> GetAllAccessoryAsync()
    {
        string query = @"
        select
            m.id as Id,
            m.name as Name,
            m.type as Type,
            m.stock as Stock,
            m.weight as Weight,
            m.thickness as Thickness
        from material m
        where m.type = 'accessory';";

        return conn.QueryAsync<Material>(query);
    }

    public async Task<Material> CreateMaterialAsync(Material material)
    {
        string insert = @"
            insert into material(id, name, type, stock, weight, thickness)
            values (@Id, @Name, @Type, @Stock, @Weight, @Thickness);
        ";

        await conn.ExecuteScalarAsync<int>(
            insert,
            new
            {
                material.Id,
                material.Name,
                material.Type,
                material.Stock,
                material.Weight,
                material.Thickness
            }
        );
        return material;
    }
}
