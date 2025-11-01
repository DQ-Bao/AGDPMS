using AGDPMS.Shared.Models;
using AGDPMS.Shared.Services;
using AGDPMS.Web.Data; 


namespace AGDPMS.Web.Services;

public class SaleService(ProjectRFQDataAccess projectDataAccess, ClientDataAccess clientDataAccess) : ISaleServices
{
    private readonly ProjectRFQDataAccess _projectDataAccess = projectDataAccess;
    private readonly ClientDataAccess _clientDataAccess = clientDataAccess;

    // --- Project RFQ Methods Implementation ---
    public Task<IEnumerable<AppRFQ>> GetProjectsRFQWithClientAsync()
    {
        return _projectDataAccess.GetAllAsync();
    }
    public Task<AppRFQ?> GetProjectRFQByIdAsync(int projectId)
    {
        return _projectDataAccess.GetByIdAsync(projectId);
    }
    public Task<AppRFQ> CreateProjectRFQAsync(AppRFQ project)
    {
        return _projectDataAccess.CreateAsync(project);
    }
    public Task UpdateProjectRFQAsync(AppRFQ project)
    {
        return _projectDataAccess.UpdateAsync(project);
    }
    public Task UpdateProjectRFQStatusAsync(int projectId, ProjectRFQStatus newStatus)
    {
        return _projectDataAccess.UpdateStatusAsync(projectId, newStatus);
    }
    public Task DeleteProjectRFQAsync(int projectId)
    {
        return _projectDataAccess.DeleteAsync(projectId);
    }

    public Task<PagedResult<AppRFQ>> GetProjectsRFQWithClientAsync(string? searchTerm = null, int pageNumber = 1, int pageSize = 10)
    {
        // Pass parameters to the updated DataAccess method
        return _projectDataAccess.GetAllPagedAsync(searchTerm, pageNumber, pageSize);
    }
    // --- Client Methods Implementation ---

    public Task<IEnumerable<AppClient>> GetClientsAsync()
    {
        return _clientDataAccess.GetAllAsync();
    }

    public Task<AppClient?> GetClientByIdAsync(int clientId)
    {
        return _clientDataAccess.GetByIdAsync(clientId);
    }

    public Task<AppClient> CreateClientAsync(AppClient client)
    {
        return _clientDataAccess.CreateAsync(client);
    }

    public Task UpdateClientAsync(AppClient client)
    {
        return _clientDataAccess.UpdateAsync(client);
    }

    public Task DeleteClientAsync(int clientId)
    {
        return _clientDataAccess.DeleteAsync(clientId);
    }
    public Task<PagedResult<AppClient>> GetClientsAsync(string? searchTerm = null, int pageNumber = 1, int pageSize = 10)
    {
        // Pass parameters to the updated DataAccess method
        return _clientDataAccess.GetAllPagedAsync(searchTerm, pageNumber, pageSize);
    }
}