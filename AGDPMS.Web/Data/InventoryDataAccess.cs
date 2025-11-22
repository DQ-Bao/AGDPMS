using Dapper;
using System.Data;
using AGDPMS.Shared.Models;

namespace AGDPMS.Web.Data;

public class InventoryDataAccess(IDbConnection conn)
{
    public Task<IEnumerable<Material>> GetAllMaterialAsync()
    {
        string query = @"
        select
            m.id as Id,
            m.name as Name,
            m.stock as Stock,
            m.weight as Weight,
            m.thickness as Thickness,

            mt.id as Id,
            mt.name as Name
        from material m
        join material_type mt
        on m.type = mt.id;";

        return conn.QueryAsync<Material, MaterialType, Material>(
            query,
            (m, mt) => {
                m.Type = mt;
                return m;
            }
        );
    }
    public Task<IEnumerable<Material>> GetAllAluminumAsync()
    {
        string query = @"
        select
            m.id as Id,
            m.name as Name,
            m.stock as Stock,
            m.weight as Weight,
            m.thickness as Thickness,

            mt.id as Id,
            mt.name as Name
        from material m
        join material_type mt
        on m.type = mt.id and mt.name = 'aluminum';";

        return conn.QueryAsync<Material, MaterialType, Material>(
            query,
            (m, mt) => {
                m.Type = mt;
                return m;
            }
        );
    }

    public Task<IEnumerable<Material>> GetAllGlassAsync()
    {
        string query = @"
        select
            m.id as Id,
            m.name as Name,
            m.stock as Stock,
            m.weight as Weight,
            m.thickness as Thickness,

            mt.id as Id,
            mt.name as Name
        from material m
        join material_type mt
        on m.type = mt.id and mt.name = 'glass';";

        return conn.QueryAsync<Material, MaterialType, Material>(
            query,
            (m, mt) => {
                m.Type = mt;
                return m;
            }
        );
    }

    public Task<IEnumerable<Material>> GetAllAccessoryAsync()
    {
        string query = @"
        select
            m.id as Id,
            m.name as Name,
            m.stock as Stock,
            m.weight as Weight,
            m.thickness as Thickness,

            mt.id as Id,
            mt.name as Name
        from material m
        join material_type mt
        on m.type = mt.id and mt.name = 'accessory';";

        return conn.QueryAsync<Material, MaterialType, Material>(
            query,
            (m, mt) => {
                m.Type = mt;
                return m;
            }
        );
    }

    //public async Task<Material> CreateMaterialAsync(Material material)
    //{
    //    string insert = @"
    //        insert into material(id, name, type, stock, weight, thickness)
    //        values (@Id, @Name, @Type, @Stock, @Weight, @Thickness);
    //    ";

    //    await conn.ExecuteScalarAsync<int>(
    //        insert,
    //        new
    //        {
    //            material.Id,
    //            material.Name,
    //            Type = material.Type.Id,
    //            material.Stock,
    //            material.Weight,
    //            material.Thickness
    //        }
    //    );
    //    return material;
    //}
}
