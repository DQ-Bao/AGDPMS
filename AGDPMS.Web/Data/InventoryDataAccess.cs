using System.Data;
using AGDPMS.Shared.Models;
using Dapper;

namespace AGDPMS.Web.Data;

public class InventoryDataAccess(IDbConnection conn)
{
    public Task<IEnumerable<MaterialType>> GetMaterialTypeAsync()
    {
<<<<<<< HEAD
        string query = @"
=======
        string query =
            @"
>>>>>>> merging
            select
                mt.id as Id,
                mt.Name as Name
            from material_type mt;
        ";

        return conn.QueryAsync<MaterialType>(query);
    }

    public async Task<IEnumerable<Material>> GetAllMaterialAsync()
    {
<<<<<<< HEAD
        string query_m = @"
=======
        string query_m =
            @"
>>>>>>> merging
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

<<<<<<< HEAD
        string query_ms = @"
=======
        string query_ms =
            @"
>>>>>>> merging
            select
                ms.id as Id,
                ms.length as Length,
                ms.width as Width,
<<<<<<< HEAD
                ms.stock as Stock,
=======
                ms.stock as Stocks,
>>>>>>> merging
                ms.base_price as BasePrice
            from material_stock ms
            where ms.material_id = @MaterialId;
        ";

        IEnumerable<Material> materials = await conn.QueryAsync<Material, MaterialType, Material>(
            query_m,
<<<<<<< HEAD
            (m, mt) => {
=======
            (m, mt) =>
            {
>>>>>>> merging
                m.Type = mt;
                return m;
            }
        );

        foreach (Material m in materials)
        {
<<<<<<< HEAD
            m.Stock = await conn.QueryAsync<MaterialStock>(
                query_ms,
                new { MaterialId = m.Id }
            );
=======
            m.Stocks = (await conn.QueryAsync<MaterialStock>(query_ms, new { MaterialId = m.Id })).ToList();
>>>>>>> merging
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
<<<<<<< HEAD
            (m, ms, mt) => {
=======
            (m, ms, mt) =>
            {
>>>>>>> merging
                Material material;
                if (!dic.TryGetValue(m.Id, out material))
                {
                    material = m;
                    material.Type = mt;
                    dic.Add(material.Id, material);
                }
<<<<<<< HEAD
                if (material.Stock == null)
                {
                    material.Stock = new List<MaterialStock>();
                }

                ((List<MaterialStock>)material.Stock).Add(ms);
                return material;
            },
            new
            {
                Id = id
            }
=======
                if (material.Stocks == null)
                {
                    material.Stocks = new List<MaterialStock>();
                }

                ((List<MaterialStock>)material.Stocks).Add(ms);
                return material;
            },
            new { Id = id }
>>>>>>> merging
        );

        return dic.Values;
    }

    public async Task<IEnumerable<Material>> GetMaterialByNameAsync(string name)
    {
<<<<<<< HEAD
        string query = @"
=======
        string query =
            @"
>>>>>>> merging
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
<<<<<<< HEAD
            (m, ms, mt) => {
=======
            (m, ms, mt) =>
            {
>>>>>>> merging
                Material material;
                if (!dic.TryGetValue(m.Id, out material))
                {
                    material = m;
                    material.Type = mt;
                    dic.Add(material.Id, material);
                }
<<<<<<< HEAD
                if (material.Stock == null)
                {
                    material.Stock = new List<MaterialStock>();
                }

                ((List<MaterialStock>)material.Stock).Add(ms);
                return material;
            },
            new
            {
                Name = name
            }
=======
                if (material.Stocks == null)
                {
                    material.Stocks = new List<MaterialStock>();
                }

                ((List<MaterialStock>)material.Stocks).Add(ms);
                return material;
            },
            new { Name = name }
>>>>>>> merging
        );

        return dic.Values;
    }

    public async Task<IEnumerable<Material>> GetMaterialByTypeAsync(MaterialType type)
    {
<<<<<<< HEAD
        string query = @"
=======
        string query =
            @"
>>>>>>> merging
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
<<<<<<< HEAD
            (m, ms, mt) => {
=======
            (m, ms, mt) =>
            {
>>>>>>> merging
                Material material;
                if (!dic.TryGetValue(m.Id, out material))
                {
                    material = m;
                    material.Type = mt;
                    dic.Add(material.Id, material);
                }
<<<<<<< HEAD
                if (material.Stock == null)
                {
                    material.Stock = new List<MaterialStock>();
                }

                ((List<MaterialStock>)material.Stock).Add(ms);
                return material;
            },
            new
            {
                Type = type.Id
            }
=======
                if (material.Stocks == null)
                {
                    material.Stocks = new List<MaterialStock>();
                }

                ((List<MaterialStock>)material.Stocks).Add(ms);
                return material;
            },
            new { Type = type.Id }
>>>>>>> merging
        );

        return dic.Values;
    }

    public async Task UpdateMaterial(List<Material> materials)
    {
<<<<<<< HEAD
        string udpate_m = @"
=======
        string udpate_m =
            @"
>>>>>>> merging
            udpate materials
            set
                id = @Id,
                name = @Name,
                type = @Type,
                weight = @Weight
            where material_id = @MaterialId and id = @Id;
        ";

<<<<<<< HEAD
        string update_ms = @"
=======
        string update_ms =
            @"
>>>>>>> merging
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
<<<<<<< HEAD
                    m.Weight
                }
            );

            foreach (MaterialStock ms in m.Stock)
=======
                    m.Weight,
                }
            );

            foreach (MaterialStock ms in m.Stocks)
>>>>>>> merging
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
<<<<<<< HEAD
                        ms.Id
                    }
                );
            }

=======
                        ms.Id,
                    }
                );
            }
>>>>>>> merging
        }
    }

    public async Task<Material> CreateMaterialAsync(Material material)
    {
<<<<<<< HEAD
        string insert_m = @"
=======
        string insert_m =
            @"
>>>>>>> merging
            insert into materials(id, name, type, weight)
            values (@Id, @Name, @Type, @Weight);
        ";

<<<<<<< HEAD
        string insert_ms = @"
=======
        string insert_ms =
            @"
>>>>>>> merging
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
<<<<<<< HEAD
                material.Weight
            }
        );
        await conn.ExecuteAsync(
            insert_ms,
            material.Stock
        );
=======
                material.Weight,
            }
        );
        await conn.ExecuteAsync(insert_ms, material.Stocks);
>>>>>>> merging

        return material;
    }
}
