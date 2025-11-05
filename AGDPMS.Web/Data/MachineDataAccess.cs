using Dapper;
using System.Data;
using AGDPMS.Shared.Models;
using System.Text;

namespace AGDPMS.Web.Data;

public class MachineDataAccess(IDbConnection conn)
{
    public async Task<PagedResult<AppMachine>> GetAllPagedAsync(string? searchTerm = null, int pageNumber = 1, int pageSize = 10)
    {
        var parameters = new DynamicParameters();
        var sqlBuilder = new StringBuilder();
        var countSqlBuilder = new StringBuilder();

        string baseSelect = @"
            SELECT 
                m.id as Id, m.name as Name, m.status as Status, 
                m.entry_date as EntryDate, -- ĐÃ THÊM
                m.last_maintenance_date as LastMaintenanceDate,
                m.machine_type_id as MachineTypeId,
                
                t.id as Id, t.name as Name
            FROM machines m
            JOIN machine_types t ON m.machine_type_id = t.id
        ";
        string baseCount = @"SELECT COUNT(*) FROM machines m JOIN machine_types t ON m.machine_type_id = t.id";

        sqlBuilder.Append(baseSelect);
        countSqlBuilder.Append(baseCount);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            string whereClause = @"
                WHERE (m.name ILIKE @SearchTerm OR 
                       t.name ILIKE @SearchTerm OR
                       m.status ILIKE @SearchTerm)
            ";
            sqlBuilder.Append(whereClause);
            countSqlBuilder.Append(whereClause);
            parameters.Add("SearchTerm", $"%{searchTerm}%");
        }

        sqlBuilder.Append(" ORDER BY m.name");
        int offset = (pageNumber - 1) * pageSize;
        sqlBuilder.Append(" LIMIT @PageSize OFFSET @Offset");
        parameters.Add("PageSize", pageSize);
        parameters.Add("Offset", offset);

        using var multi = await conn.QueryMultipleAsync($"{countSqlBuilder}; {sqlBuilder}", parameters);

        int totalCount = await multi.ReadSingleAsync<int>();
        var items = multi.Read<AppMachine, AppMachineType, AppMachine>(
            (machine, type) => { machine.MachineType = type; return machine; },
            splitOn: "Id"
        ).ToList();

        return new PagedResult<AppMachine>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<AppMachine?> GetByIdAsync(int id) =>
        (await conn.QueryAsync<AppMachine, AppMachineType, AppMachine>(@"
            SELECT 
                m.id as Id, m.name as Name, m.status as Status, 
                m.entry_date as EntryDate, -- ĐÃ THÊM
                m.last_maintenance_date as LastMaintenanceDate,
                m.machine_type_id as MachineTypeId,
                t.id as Id, t.name as Name
            FROM machines m
            JOIN machine_types t ON m.machine_type_id = t.id
            WHERE m.id = @Id
        ", (machine, type) => { machine.MachineType = type; return machine; },
        new { Id = id }, splitOn: "Id")).FirstOrDefault();

    public async Task<AppMachine> CreateAsync(AppMachine machine)
    {
        var id = await conn.ExecuteScalarAsync<int>(@"
            INSERT INTO machines (name, machine_type_id, status, entry_date, last_maintenance_date)
            VALUES (@Name, @MachineTypeId, @Status, @EntryDate, @LastMaintenanceDate) -- ĐÃ THÊM entry_date
            RETURNING id
        ", new
        {
            machine.Name,
            machine.MachineTypeId,
            Status = machine.Status.ToString(),
            machine.EntryDate, 
            machine.LastMaintenanceDate 
        });
        machine.Id = id;
        return machine;
    }

    public Task UpdateAsync(AppMachine machine) =>
        conn.ExecuteAsync(@"
            UPDATE machines
            SET name = @Name,
                machine_type_id = @MachineTypeId,
                status = @Status,
                entry_date = @EntryDate, -- ĐÃ THÊM
                last_maintenance_date = @LastMaintenanceDate
            WHERE id = @Id
        ", new
        {
            machine.Name,
            machine.MachineTypeId,
            Status = machine.Status.ToString(),
            machine.EntryDate, 
            machine.LastMaintenanceDate,
            machine.Id
        });

    public Task UpdateStatusAsync(int id, MachineStatus status) =>
        conn.ExecuteAsync(@"
            UPDATE machines SET status = @Status WHERE id = @Id
        ", new { Status = status.ToString(), Id = id });

    public Task DeleteAsync(int id) =>
        conn.ExecuteAsync("DELETE FROM machines WHERE id = @Id", new { Id = id });
}