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
    // State machine methods - status transitions are controlled
    Task ApproveProjectAsync(int projectId); // Planning -> Production (Admin only)
    Task CancelProjectAsync(int projectId); // Any -> Cancelled (Admin only)
    Task CompleteProductionAsync(int projectId); // Production -> Deploying (automatic)
    Task CompleteDeployAsync(int projectId); // Deploying -> Completed (automatic)
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