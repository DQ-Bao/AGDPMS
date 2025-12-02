using System.Data;
using Dapper;
using AGDPMS.Shared.Models;

namespace AGDPMS.Web.Data;

public class StageTypeDataAccess(IDbConnection conn)
{
    public Task<IEnumerable<StageType>> GetAllAsync() => conn.QueryAsync<StageType>(@"
        select id as Id, code as Code, name as Name,
               is_active as IsActive, is_default as IsDefault,
               created_at as CreatedAt, updated_at as UpdatedAt
        from stage_types
        order by id asc");

    public async Task<int> CreateAsync(StageType model) => await conn.ExecuteScalarAsync<int>(@"
        insert into stage_types(code, name, is_active, is_default)
        values (@Code, @Name, @IsActive, @IsDefault)
        returning id",
        new { model.Code, model.Name, model.IsActive, model.IsDefault });

    public Task UpdateAsync(StageType model) => conn.ExecuteAsync(@"
        update stage_types
        set code = @Code,
            name = @Name,
            is_active = @IsActive,
            is_default = @IsDefault,
            updated_at = now()
        where id = @Id",
        new { model.Code, model.Name, model.IsActive, model.IsDefault, model.Id });
}


