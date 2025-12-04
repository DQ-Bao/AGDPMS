using AGDPMS.Shared.Models;
using Dapper;
using System.Data;

namespace AGDPMS.Web.Data;

public class CavityDataAccess(IDbConnection conn)
{
    public Task<IEnumerable<Cavity>> GetAllFromProjectAsync(int projectId) =>
        conn.QueryAsync<Cavity>(@"
            select cav.id as Id, cav.code as Code, cav.project_id as ProjectId, cav.description as Description, 
                   cav.width as Width, cav.height as Height, cav.location as Location, cav.quantity as Quantity, cav.window_type as WindowType
            from cavities as cav
            where cav.project_id = @ProjectId",
            new { ProjectId = projectId });

    public Task<IEnumerable<string>> GetCodeByIdAsync(int cavityId, IDbTransaction? tran = null) =>
        conn.QueryAsync<string>(@"select code from cavities where id = @CavityId", new { CavityId = cavityId }, tran);

    public async Task<Cavity?> GetByIdWithBOMsAsync(int cavityId, IDbTransaction? tran = null)
    {
        Cavity? cavity = null;
        List<CavityBOM> boms = [];
        await conn.QueryAsync<Cavity, CavityBOM, Material, MaterialType, Cavity>(@"
            select cav.id as Id, cav.code as Code, cav.project_id as ProjectId, cav.description as Description, 
                   cav.width as Width, cav.height as Height, cav.location as Location, cav.quantity as Quantity, cav.window_type as WindowType,

                   bom.id as Id, bom.cavity_id as CavityId, bom.quantity as Quantity, bom.length as Length, bom.width as Width,
                   bom.color as Color, bom.unit as Unit,

                   m.id as Id, m.name as Name, m.weight as Weight,
                   mt.id as Id, mt.name as Name
            from cavities as cav
            left join cavity_boms as bom on cav.id = bom.cavity_id
            left join materials as m on bom.material_id = m.id
            left join material_type as mt on m.type = mt.id
            where cav.id = @CavityId",
            (cav, bom, material, materialType) =>
            {
                if (cavity == null)
                {
                    cavity = cav;
                    cavity.BOMs = boms;
                }
                if (bom is not null && material is not null)
                {
                    material.Type = materialType;
                    bom.Material = material;
                    boms.Add(bom);
                }
                return cav;
            },
            new { CavityId = cavityId }, tran);
        return cavity;
    }

    public async Task<Cavity?> GetByCodeAsync(int projectId, string code, IDbTransaction? tran = null) =>
        (await conn.QueryAsync<Cavity>(@"
            select cav.id as Id, cav.code as Code, cav.project_id as ProjectId, cav.description as Description, 
                   cav.width as Width, cav.height as Height, cav.location as Location, cav.quantity as Quantity, cav.window_type as WindowType
            from cavities as cav
            where cav.project_id = @ProjectId and cav.code = @Code",
            new { ProjectId = projectId, Code = code }, tran)).FirstOrDefault();

    public async Task<Cavity> CreateAsync(Cavity cavity, IDbTransaction? tran = null)
    {
        var id = await conn.ExecuteScalarAsync<int>(@"
            insert into cavities(code, project_id, location, quantity, window_type, description, width, height)
            values (@Code, @ProjectId, @Location, @Quantity, @WindowType, @Description, @Width, @Height)
            returning id", cavity, tran);
        cavity.Id = id;
        return cavity;
    }

    public Task UpdateAsync(Cavity cavity, IDbTransaction? tran = null) =>
        conn.ExecuteAsync(@"
            update cavities
            set code = @Code,
                location = @Location,
                quantity = @Quantity,
                window_type = @WindowType,
                description = @Description,
                width = @Width,
                height = @Height
            where id = @Id",
            cavity, tran);

    public async Task DeleteAsync(int cavityId, IDbTransaction? tran = null)
    {
        IDbTransaction? activeTransaction = null;
        try
        {
            if (conn.State != ConnectionState.Open) conn.Open();
            using var localTransaction = tran is null ? conn.BeginTransaction() : null;
            activeTransaction = tran ?? localTransaction;
            await DeleteAllBOMsAsync(cavityId, activeTransaction);
            await conn.ExecuteAsync("delete from cavities where id = @CavityId", new { CavityId = cavityId }, activeTransaction);
            if (tran is null) activeTransaction?.Commit();
        }
        catch
        {
            if (tran is null) activeTransaction?.Rollback();
            throw;
        }
    }

    public async Task AddBOMsAsync(IEnumerable<CavityBOM> boms, IDbTransaction? tran = null)
    {
        if (boms is null || !boms.Any()) return;
        foreach (var bom in boms)
        {
            var existingMat = await conn.QueryFirstOrDefaultAsync<int>(
                "select count(1) from materials where id = @Id", new { bom.Material.Id }, tran);
            if (existingMat == 0)
            {
                // Add the material if didn't exist
                await conn.ExecuteAsync(@"
                    insert into materials(id, name, type)
                    values (@Id, @Name, @Type)",
                    new
                    {
                        bom.Material.Id,
                        bom.Material.Name,
                        Type = bom.Material.Type?.Id,
                    }, tran);
            }
        }
        await conn.ExecuteAsync(@"
            insert into cavity_boms(cavity_id, material_id, quantity, length, width, color, unit)
            values (@CavityId, @MaterialId, @Quantity, @Length, @Width, @Color, @Unit)",
            boms.Select(b => new
            {
                b.CavityId,
                MaterialId = b.Material.Id,
                b.Quantity,
                b.Length,
                b.Width,
                b.Color,
                b.Unit
            }), tran);
    }

    public Task DeleteAllBOMsAsync(int cavityId, IDbTransaction? tran = null) =>
        conn.ExecuteAsync("delete from cavity_boms where cavity_id = @CavityId", new { CavityId = cavityId }, tran);

    public async Task<IEnumerable<(CavityBOM BOM, int CavityQuantity)>> GetBOMsOfTypeAsync(int projectId, IEnumerable<MaterialType> types, IDbTransaction? tran = null)
    {
        var lookup = new Dictionary<int, CavityBOM>();
        var typeNames = types.Select(t => t.Name).ToArray();
        var result = await conn.QueryAsync<CavityBOM, Material, MaterialType, MaterialStock, int, (CavityBOM bom, int cavityQty)>(@"
            select bom.id as Id, bom.cavity_id as CavityId, bom.quantity as Quantity, bom.length as Length, bom.width as Width, bom.color as Color, bom.unit as Unit,
                   m.id as Id, m.name as Name, m.weight as Weight,
                   mt.id as Id, mt.name as Name,
                   ms.id as Id, ms.length as Length, ms.width as Width, ms.stock as Stock, ms.base_price as BasePrice,
                   cav.quantity as CavityQuantity
            from cavity_boms as bom
            join cavities as cav on bom.cavity_id = cav.id and cav.project_id = @ProjectId
            join materials as m on bom.material_id = m.id
            join material_type as mt on m.type = mt.id and mt.name = any(@TypeNames)
            left join material_stock as ms on m.id = ms.material_id
            order by bom.id, m.id",
            (bom, material, materialType, stock, cavityQuantity) =>
            {
                if (!lookup.TryGetValue(bom.Id, out var existing))
                {
                    material.Type = materialType;
                    bom.Material = material;
                    lookup.Add(bom.Id, bom);
                    existing = bom;
                }
                if (stock != null) existing.Material.Stocks.Add(stock);
                return (existing, cavityQuantity);
            },
            new { ProjectId = projectId, TypeNames = typeNames },
            tran,
            splitOn: "Id,CavityQuantity");
        return result;
    }

    public async Task<Dictionary<int, decimal>> GetAveragePricesAsync(IDbTransaction? tran = null) =>
        (await conn.QueryAsync<(int MaterialId, decimal AvgPrice)>(@"
            select material_id as MaterialId, avg(cast(price as numeric)) as AvgPrice
            from stock_import
            group by material_id", transaction: tran)).ToDictionary(x => x.MaterialId, x => x.AvgPrice);
    // Temporary
    public async Task<Dictionary<string, decimal>> GetAveragePricesGroupByCodeAsync(IDbTransaction? tran = null) =>
        (await conn.QueryAsync<(string MaterialCode, decimal AvgPrice)>(@"
            select m.code as MaterialCode, avg(cast(price as numeric)) as AvgPrice
            from stock_import as si
            left join materials as m on si.material_id = m.id
            group by m.code", transaction: tran)).ToDictionary(x => x.MaterialCode, x => x.AvgPrice);

    // Move this to inventory data access
    public Task UpdateMaterialWeightAsync(string materialId, double weight, IDbTransaction? tran = null) =>
        conn.ExecuteAsync("update materials set weight = @Weight where id = @Id", new { Id = materialId, Weight = weight }, tran);

    // Move this to inventory data access
    public Task AddMaterialStockAsync(string materialId, MaterialStock stock, IDbTransaction? tran = null) =>
        conn.ExecuteAsync(@"
            insert into material_stock(material_id, length, base_price)
            values (@MaterialId, @Length, @Price)",
            new { MaterialId = materialId, stock.Length, Price = stock.BasePrice }, tran);
    public Task UpdateMaterialStockBasePriceAsync(int stockId, decimal price, IDbTransaction? tran = null) =>
        conn.ExecuteAsync(@"
            update material_stock
            set base_price = @Price
            where id = @StockId",
            new { StockId = stockId, Price = price }, tran);

    // Move this to inventory data access
    public async Task CreateMaterialPlanningAsync(MaterialPlanning planning, IDbTransaction? tran = null)
    {
        var pId = await conn.ExecuteScalarAsync<int>(@"
            insert into material_plannings(made_by, project_id)
            values (@UserId, @ProjectId)
            returning id", new { planning.UserId, planning.ProjectId }, tran);
        foreach (var d in planning.Details)
        {
            await conn.ExecuteAsync(@"
                insert into material_planning_details(planning_id, material_id, length, width, quantity, unit, note)
                values (@PlanningId, @MaterialId, @Length, @Width, @Quantity, @Unit, @Note)",
                new
                {
                    PlanningId = pId,
                    MaterialId = d.Material.Id,
                    d.Length,
                    d.Width,
                    d.Quantity,
                    d.Unit,
                    d.Note
                }, tran);
        }
    }

    public async Task<string?> AddOrUpdateBatchAsync(IEnumerable<Cavity> cavities)
    {
        if (conn.State != ConnectionState.Open) conn.Open();
        using var tran = conn.BeginTransaction();
        try
        {
            foreach (var cavity in cavities)
            {
                var existingCav = await GetByCodeAsync(cavity.ProjectId, cavity.Code, tran);
                if (existingCav is null)
                {
                    var added = await CreateAsync(cavity, tran);
                    cavity.Id = added.Id;
                }
                else
                {
                    cavity.Id = existingCav.Id;
                    await UpdateAsync(cavity, tran);
                    await DeleteAllBOMsAsync(cavity.Id, tran);
                }
                if (cavity.BOMs is not null && cavity.BOMs.Any())
                {
                    foreach (var bom in cavity.BOMs)
                        bom.CavityId = cavity.Id;
                    await AddBOMsAsync(cavity.BOMs, tran);
                }
            }
            tran.Commit();
            return null;
        }
        catch (Exception ex)
        {
            tran.Rollback();
            return ex.Message;
        }
    }
}
