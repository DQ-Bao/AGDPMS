using AGDPMS.Shared.Models;
using Dapper;
using System.Data;
using System.Text;

namespace AGDPMS.Web.Data;

public class ClientDataAccess(IDbConnection conn)
{
    // Lấy tất cả khách hàng
    public Task<IEnumerable<AppClient>> GetAllAsync() =>
        conn.QueryAsync<AppClient>(@"
            SELECT id as Id, name as Name, address as Address, phone as Phone, email as Email
            FROM clients
            ORDER BY name
        ");

    // Lấy khách hàng theo ID
    public Task<AppClient?> GetByIdAsync(int id) =>
        conn.QueryFirstOrDefaultAsync<AppClient>(@"
            SELECT id as Id, name as Name, address as Address, phone as Phone, email as Email
            FROM clients
            WHERE id = @Id
        ", new { Id = id });

    // Tạo khách hàng mới
    public async Task<AppClient> CreateAsync(AppClient client)
    {
        var id = await conn.ExecuteScalarAsync<int>(@"
            INSERT INTO clients (name, address, phone, email)
            VALUES (@Name, @Address, @Phone, @Email)
            RETURNING id
        ", client);

        client.Id = id;
        return client;
    }

    // Cập nhật khách hàng
    public Task UpdateAsync(AppClient client) =>
        conn.ExecuteAsync(@"
            UPDATE clients
            SET name = @Name,
                address = @Address,
                phone = @Phone,
                email = @Email
            WHERE id = @Id
        ", client);

    // Xóa khách hàng
    public Task DeleteAsync(int id) =>
        conn.ExecuteAsync(@"
            DELETE FROM clients
            WHERE id = @Id
        ", new { Id = id });
 
    public async Task<PagedResult<AppClient>> GetAllPagedAsync(string? searchTerm = null, int pageNumber = 1, int pageSize = 10)
    {
        var parameters = new DynamicParameters();
        var sqlBuilder = new StringBuilder();
        var countSqlBuilder = new StringBuilder();

        string baseSelect = @"
            SELECT id as Id, name as Name, address as Address, phone as Phone, email as Email
            FROM clients
        ";
        string baseCount = @"SELECT COUNT(*) FROM clients";

        sqlBuilder.Append(baseSelect);
        countSqlBuilder.Append(baseCount);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            string whereClause = @"
                WHERE (name ILIKE @SearchTerm OR 
                       address ILIKE @SearchTerm OR 
                       phone ILIKE @SearchTerm OR 
                       email ILIKE @SearchTerm)
            "; 
            sqlBuilder.Append(whereClause);
            countSqlBuilder.Append(whereClause);
            parameters.Add("SearchTerm", $"%{searchTerm}%");
        }

        sqlBuilder.Append(" ORDER BY name"); 

        int offset = (pageNumber - 1) * pageSize;
        sqlBuilder.Append(" LIMIT @PageSize OFFSET @Offset");
        parameters.Add("PageSize", pageSize);
        parameters.Add("Offset", offset);

        // Execute both queries
        using var multi = await conn.QueryMultipleAsync(
            $"{countSqlBuilder}; {sqlBuilder}", parameters);

        int totalCount = await multi.ReadSingleAsync<int>();
        var items = (await multi.ReadAsync<AppClient>()).ToList();

        return new PagedResult<AppClient>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}