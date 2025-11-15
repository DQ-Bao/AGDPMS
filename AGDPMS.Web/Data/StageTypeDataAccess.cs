using System.Data;
using Dapper;
using AGDPMS.Shared.Models;

namespace AGDPMS.Web.Data;

public class StageTypeDataAccess(IDbConnection conn)
{
    public Task<IEnumerable<StageType>> GetAllAsync() => conn.QueryAsync<StageType>(@"
        select id as Id, code as Code, name as Name, display_order as DisplayOrder,
               is_active as IsActive, is_default as IsDefault,
               created_at as CreatedAt, updated_at as UpdatedAt
        from stage_types
        order by display_order asc, id asc");

    public async Task<int> CreateAsync(StageType model) => await conn.ExecuteScalarAsync<int>(@"
        insert into stage_types(code, name, display_order, is_active, is_default)
        values (@Code, @Name, @DisplayOrder, @IsActive, @IsDefault)
        returning id",
        new { model.Code, model.Name, model.DisplayOrder, model.IsActive, model.IsDefault });

    public Task UpdateAsync(StageType model) => conn.ExecuteAsync(@"
        update stage_types
        set code = @Code,
            name = @Name,
            display_order = @DisplayOrder,
            is_active = @IsActive,
            is_default = @IsDefault,
            updated_at = now()
        where id = @Id",
        new { model.Code, model.Name, model.DisplayOrder, model.IsActive, model.IsDefault, model.Id });
}


