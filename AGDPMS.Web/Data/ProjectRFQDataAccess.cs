using Dapper;
using System.Data;
using AGDPMS.Shared.Models;
using System.Text;

namespace AGDPMS.Web.Data;

public class ProjectRFQDataAccess(IDbConnection conn)
{
    public Task<IEnumerable<AppRFQ>> GetAllAsync() =>
        conn.QueryAsync<AppRFQ, AppClient, AppRFQ>(@"
            SELECT 
                p.id as Id, 
                p.name as ProjectRFQName, 
                p.location as Location,
                p.design_company as DesignCompany, 
                p.completion_date as CompletionDate,
                p.created_at as CreatedAt,
                p.design_file_path as DesignFilePath,
                p.status as Status,                 
                p.document_path as DocumentPath,    
                p.client_id as ClientId,

                c.id as Id,
                c.name as Name
            FROM projects_rfq p
            JOIN clients c ON p.client_id = c.id
            ORDER BY p.created_at DESC
        ", (project, client) =>
        {
            project.Client = client;
            return project;
        });

    public async Task<AppRFQ?> GetByIdAsync(int id) =>
          (await conn.QueryAsync<AppRFQ, AppClient, AppRFQ>(@"
            SELECT 
                p.id as Id, 
                p.name as ProjectRFQName, 
                p.location as Location,
                p.design_company as DesignCompany, 
                p.completion_date as CompletionDate,
                p.created_at as CreatedAt,
                p.design_file_path as DesignFilePath,
                p.status as Status,                
                p.document_path as DocumentPath,    
                p.client_id as ClientId,

                c.id as Id,
                c.name as Name,
                c.address as Address, 
                c.phone as Phone,     
                c.email as Email      
            FROM projects_rfq p
            JOIN clients c ON p.client_id = c.id
            WHERE p.id = @Id
        ", (project, client) =>
          {
              project.Client = client;
              return project;
          }, new { Id = id })).FirstOrDefault();

    public async Task<AppRFQ> CreateAsync(AppRFQ project)
    {
        var id = await conn.ExecuteScalarAsync<int>(@"
            INSERT INTO projects_rfq 
            (name, location, client_id, design_company, completion_date, design_file_path, status, document_path) 
            VALUES 
            (@ProjectRFQName, @Location, @ClientId, @DesignCompany, @CompletionDate, @DesignFilePath, @Status, @DocumentPath)
            RETURNING id
        ", new
        {
            project.ProjectRFQName,
            project.Location,
            project.ClientId,
            project.DesignCompany,
            project.CompletionDate,
            project.DesignFilePath,
            Status = project.Status.ToString(),
            project.DocumentPath
        });

        project.Id = id;
        return project;
    }

    public Task UpdateAsync(AppRFQ project) =>
        conn.ExecuteAsync(@"
            UPDATE projects_rfq
            SET name = @ProjectRFQName,
                location = @Location,
                client_id = @ClientId,
                design_company = @DesignCompany,
                completion_date = @CompletionDate,
                design_file_path = @DesignFilePath,
                status = @Status,                 
                document_path = @DocumentPath     
            WHERE id = @Id
        ", new
        {
            project.ProjectRFQName,
            project.Location,
            project.ClientId,
            project.DesignCompany,
            project.CompletionDate,
            project.DesignFilePath,
            Status = project.Status.ToString(),
            project.DocumentPath,
            project.Id
        });

    public Task UpdateStatusAsync(int id, ProjectRFQStatus status) =>
        conn.ExecuteAsync(@"
            UPDATE projects_rfq
            SET status = @Status                
            WHERE id = @Id
        ", new { Status = status.ToString(), Id = id });

    // Xóa dự án
    public Task DeleteAsync(int id) =>
        conn.ExecuteAsync(@"
            DELETE FROM projects_rfq
            WHERE id = @Id
        ", new { Id = id });

    public async Task<PagedResult<AppRFQ>> GetAllPagedAsync(string? searchTerm = null, int pageNumber = 1, int pageSize = 10)
    {
        var parameters = new DynamicParameters();
        var sqlBuilder = new StringBuilder();
        var countSqlBuilder = new StringBuilder();

        string baseSelect = @"
            SELECT 
                p.id as Id, p.name as ProjectRFQName, p.location as Location,
                p.design_company as DesignCompany, p.completion_date as CompletionDate,
                p.created_at as CreatedAt, p.design_file_path as DesignFilePath,
                p.status as Status,                 
                p.document_path as DocumentPath,    
                p.client_id as ClientId,
                c.id as Id, c.name as Name 
            FROM projects_rfq p
            JOIN clients c ON p.client_id = c.id
        ";
        string baseCount = @"SELECT COUNT(*) FROM projects_rfq p JOIN clients c ON p.client_id = c.id";

        sqlBuilder.Append(baseSelect);
        countSqlBuilder.Append(baseCount);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            string whereClause = @"
                WHERE (p.name ILIKE @SearchTerm OR 
                       p.location ILIKE @SearchTerm OR 
                       c.name ILIKE @SearchTerm)
            ";

            sqlBuilder.Append(whereClause);
            countSqlBuilder.Append(whereClause);
            parameters.Add("SearchTerm", $"%{searchTerm}%");
        }

        sqlBuilder.Append(" ORDER BY p.created_at DESC");

        int offset = (pageNumber - 1) * pageSize;
        sqlBuilder.Append(" LIMIT @PageSize OFFSET @Offset");
        parameters.Add("PageSize", pageSize);
        parameters.Add("Offset", offset);

        using var multi = await conn.QueryMultipleAsync(
            $"{countSqlBuilder}; {sqlBuilder}", parameters);

        int totalCount = await multi.ReadSingleAsync<int>();

        var items = multi.Read<AppRFQ, AppClient, AppRFQ>(
            (project, client) => { project.Client = client; return project; },
            splitOn: "Id"
        ).ToList();

        return new PagedResult<AppRFQ>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}