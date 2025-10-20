using AGDPMS.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AGDPMS.Shared.Services;

public interface ISaleServices
{
    // --- Project RFQ Methods ---
    Task<IEnumerable<AppRFQ>> GetProjectsRFQWithClientAsync(); 
    Task<AppRFQ?> GetProjectRFQByIdAsync(int projectId);
    Task<AppRFQ> CreateProjectRFQAsync(AppRFQ project); 
    Task UpdateProjectRFQAsync(AppRFQ project);
    Task UpdateProjectRFQStatusAsync(int projectId, ProjectRFQStatus newStatus);
    Task DeleteProjectRFQAsync(int projectId);
    Task<PagedResult<AppRFQ>> GetProjectsRFQWithClientAsync(string? searchTerm = null, int pageNumber = 1, int pageSize = 10);

    // --- Client Methods ---

    Task<IEnumerable<AppClient>> GetClientsAsync();
    Task<AppClient?> GetClientByIdAsync(int clientId);
    Task<AppClient> CreateClientAsync(AppClient client);
    Task UpdateClientAsync(AppClient client);
    Task DeleteClientAsync(int clientId);
    Task<PagedResult<AppClient>> GetClientsAsync(string? searchTerm = null, int pageNumber = 1, int pageSize = 10);
}