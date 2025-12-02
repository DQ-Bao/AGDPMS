using System.Data;
using AGDPMS.Shared.Models;
using Dapper;

namespace AGDPMS.Web.Data;

public class InventoryDataAccess(IDbConnection conn)
{
    public Task<IEnumerable<MaterialType>> GetMaterialTypeAsync()
    {
        string query =
            @"
            select
                mt.id as Id,
                mt.Name as Name
            from material_type mt;
        ";

        return conn.QueryAsync<MaterialType>(query);
    }

    public async Task<IEnumerable<Material>> GetAllMaterialAsync()
    {
        string query_m =
            @"
            select
                m.id as Id,
                m.name as Name,
                m.weight as Weight,

                mt.id as Id,
                mt.name as Name
            from materials m
            join material_type mt
            on m.type = mt.id;
        ";

        string query_ms =
            @"
            select
                ms.id as Id,
                ms.length as Length,
                ms.width as Width,
                ms.stock as Stocks,
                ms.base_price as BasePrice
            from material_stock ms
            where ms.material_id = @MaterialId;
        ";

        IEnumerable<Material> materials = await conn.QueryAsync<Material, MaterialType, Material>(
            query_m,
            (m, mt) =>
            {
                m.Type = mt;
                return m;
            }
        );

        foreach (Material m in materials)
        {
            m.Stocks = (await conn.QueryAsync<MaterialStock>(query_ms, new { MaterialId = m.Id })).ToList();
        }

        return materials;
    }

    public async Task<IEnumerable<Material>> GetMaterialByIdAsync(string id)
    {
        string query =
            @"
        select
            m.id as Id,
            m.name as Name,

            ms.Id as Id,
            ms.length as Length,
            ms.width as Width,
            ms.stock as Stock,

            mt.id as Id,
            mt.name as Name
        from materials m
        join material_type mt
        on m.type = mt.id and m.id like %@Id%
        join material_stock ms
        on m.id = ms.material_id";

        Dictionary<string, Material> dic = new Dictionary<string, Material>();

        var result = await conn.QueryAsync<Material, MaterialStock, MaterialType, Material>(
            query,
            (m, ms, mt) =>
            {
                Material material;
                if (!dic.TryGetValue(m.Id, out material))
                {
                    material = m;
                    material.Type = mt;
                    dic.Add(material.Id, material);
                }
                if (material.Stocks == null)
                {
                    material.Stocks = new List<MaterialStock>();
                }

                ((List<MaterialStock>)material.Stocks).Add(ms);
                return material;
            },
            new { Id = id }
        );

        return dic.Values;
    }

    public async Task<IEnumerable<Material>> GetMaterialByNameAsync(string name)
    {
        string query =
            @"
        select
            m.id as Id,
            m.name as Name,

            ms.Id as Id,
            ms.length as Length,
            ms.width as Width,
            ms.stock as Stock,

            mt.id as Id,
            mt.name as Name
        from materials m
        join material_type mt
        on m.type = mt.id and m.name like %@Name%
        join material_stock ms
        on m.id = ms.material_id";

        Dictionary<string, Material> dic = new Dictionary<string, Material>();

        var result = await conn.QueryAsync<Material, MaterialStock, MaterialType, Material>(
            query,
            (m, ms, mt) =>
            {
                Material material;
                if (!dic.TryGetValue(m.Id, out material))
                {
                    material = m;
                    material.Type = mt;
                    dic.Add(material.Id, material);
                }
                if (material.Stocks == null)
                {
                    material.Stocks = new List<MaterialStock>();
                }

                ((List<MaterialStock>)material.Stocks).Add(ms);
                return material;
            },
            new { Name = name }
        );

        return dic.Values;
    }

    public async Task<IEnumerable<Material>> GetMaterialByTypeAsync(MaterialType type)
    {
        string query =
            @"
        select
            m.id as Id,
            m.name as Name,

            ms.Id as Id,
            ms.length as Length,
            ms.width as Width,
            ms.stock as Stock,

            mt.id as Id,
            mt.name as Name
        from materials m
        join material_type mt
        on m.type = mt.id and mt.id = @Type
        join material_stock ms
        on m.id = ms.material_id";

        Dictionary<string, Material> dic = new Dictionary<string, Material>();

        var result = await conn.QueryAsync<Material, MaterialStock, MaterialType, Material>(
            query,
            (m, ms, mt) =>
            {
                Material material;
                if (!dic.TryGetValue(m.Id, out material))
                {
                    material = m;
                    material.Type = mt;
                    dic.Add(material.Id, material);
                }
                if (material.Stocks == null)
                {
                    material.Stocks = new List<MaterialStock>();
                }

                ((List<MaterialStock>)material.Stocks).Add(ms);
                return material;
            },
            new { Type = type.Id }
        );

        return dic.Values;
    }

    public async Task UpdateMaterial(List<Material> materials)
    {
        string udpate_m =
            @"
            udpate materials
            set
                id = @Id,
                name = @Name,
                type = @Type,
                weight = @Weight
            where material_id = @MaterialId and id = @Id;
        ";

        string update_ms =
            @"
            udpate material_stock
            set
                length = @Length,
                width = @Width,
                stock = @Stock
                base_price = @BasePrice
            where material_id = @MaterialId and id = @Id;
        ";

        foreach (Material m in materials)
        {
            await conn.ExecuteAsync(
                udpate_m,
                new
                {
                    m.Id,
                    m.Name,
                    Type = m.Type.Id,
                    m.Weight,
                }
            );

            foreach (MaterialStock ms in m.Stocks)
            {
                await conn.ExecuteAsync(
                    update_ms,
                    new
                    {
                        ms.Length,
                        ms.Width,
                        ms.Stock,
                        ms.BasePrice,
                        MaterialId = m.Id,
                        ms.Id,
                    }
                );
            }
        }
    }

    public async Task<Material> CreateMaterialAsync(Material material)
    {
        string insert_m =
            @"
            insert into materials(id, name, type, weight)
            values (@Id, @Name, @Type, @Weight);
        ";

        string insert_ms =
            @"
            insert into material_stock(material_id, length, width, stock, base_price)
            values (@MaterialId, @Length, @Width, @Stock, @BasePrice);
        ";

        await conn.ExecuteScalarAsync<int>(
            insert_m,
            new
            {
                material.Id,
                material.Name,
                Type = material.Type.Id,
                material.Weight,
            }
        );
        await conn.ExecuteAsync(insert_ms, material.Stocks);

        return material;
    }
}
