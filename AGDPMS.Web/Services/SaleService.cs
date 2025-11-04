using AGDPMS.Shared.Models;
using AGDPMS.Shared.Services;
using AGDPMS.Web.Data;


namespace AGDPMS.Web.Services;

public class SaleService(ProjectRFQDataAccess projectDataAccess, ClientDataAccess clientDataAccess, ILogger<SaleService> logger) : ISaleServices
{
    private readonly ProjectRFQDataAccess _projectDataAccess = projectDataAccess;
    private readonly ClientDataAccess _clientDataAccess = clientDataAccess;
    private readonly ILogger<SaleService> _logger = logger;

    // --- Project RFQ Methods Implementation ---
    public async Task<IEnumerable<AppRFQ>> GetProjectsRFQWithClientAsync()
    {
        try
        {
            _logger.LogInformation("Getting all projects RFQ with clients");
            return await _projectDataAccess.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all projects RFQ with clients");
            throw;
        }
    }

    public async Task<AppRFQ?> GetProjectRFQByIdAsync(int projectId)
    {
        try
        {
            _logger.LogInformation("Getting project RFQ by ID: {ProjectId}", projectId);
            return await _projectDataAccess.GetByIdAsync(projectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting project RFQ by ID: {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<AppRFQ> CreateProjectRFQAsync(AppRFQ project)
    {
        try
        {
            _logger.LogInformation("Creating new project RFQ");
            return await _projectDataAccess.CreateAsync(project);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating project RFQ");
            throw;
        }
    }

    public async Task UpdateProjectRFQAsync(AppRFQ project)
    {
        try
        {
            _logger.LogInformation("Updating project RFQ with ID: {ProjectId}", project.Id);
            await _projectDataAccess.UpdateAsync(project);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating project RFQ with ID: {ProjectId}", project.Id);
            throw;
        }
    }

    public async Task UpdateProjectRFQStatusAsync(int projectId, ProjectRFQStatus newStatus)
    {
        try
        {
            _logger.LogInformation("Updating project RFQ status for ID: {ProjectId} to {NewStatus}", projectId, newStatus);
            await _projectDataAccess.UpdateStatusAsync(projectId, newStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating project RFQ status for ID: {ProjectId} to {NewStatus}", projectId, newStatus);
            throw;
        }
    }

    public async Task DeleteProjectRFQAsync(int projectId)
    {
        try
        {
            _logger.LogInformation("Deleting project RFQ with ID: {ProjectId}", projectId);
            await _projectDataAccess.DeleteAsync(projectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting project RFQ with ID: {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<PagedResult<AppRFQ>> GetProjectsRFQWithClientAsync(string? searchTerm = null, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            _logger.LogInformation("Getting paged projects RFQ with search term: {SearchTerm}, page: {PageNumber}, page size: {PageSize}",
                searchTerm, pageNumber, pageSize);
            return await _projectDataAccess.GetAllPagedAsync(searchTerm, pageNumber, pageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting paged projects RFQ with search term: {SearchTerm}, page: {PageNumber}, page size: {PageSize}",
                searchTerm, pageNumber, pageSize);
            throw;
        }
    }

    // --- Client Methods Implementation ---

    public async Task<IEnumerable<AppClient>> GetClientsAsync()
    {
        try
        {
            _logger.LogInformation("Getting all clients");
            return await _clientDataAccess.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all clients");
            throw;
        }
    }

    public async Task<AppClient?> GetClientByIdAsync(int clientId)
    {
        try
        {
            _logger.LogInformation("Getting client by ID: {ClientId}", clientId);
            return await _clientDataAccess.GetByIdAsync(clientId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting client by ID: {ClientId}", clientId);
            throw;
        }
    }

    public async Task<AppClient> CreateClientAsync(AppClient client)
    {
        try
        {
            _logger.LogInformation("Creating new client");
            return await _clientDataAccess.CreateAsync(client);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating client");
            throw;
        }
    }

    public async Task UpdateClientAsync(AppClient client)
    {
        try
        {
            _logger.LogInformation("Updating client with ID: {ClientId}", client.Id);
            await _clientDataAccess.UpdateAsync(client);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating client with ID: {ClientId}", client.Id);
            throw;
        }
    }

    public async Task DeleteClientAsync(int clientId)
    {
        try
        {
            _logger.LogInformation("Deleting client with ID: {ClientId}", clientId);
            await _clientDataAccess.DeleteAsync(clientId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting client with ID: {ClientId}", clientId);
            throw;
        }
    }

    public async Task<PagedResult<AppClient>> GetClientsAsync(string? searchTerm = null, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            _logger.LogInformation("Getting paged clients with search term: {SearchTerm}, page: {PageNumber}, page size: {PageSize}",
                searchTerm, pageNumber, pageSize);
            return await _clientDataAccess.GetAllPagedAsync(searchTerm, pageNumber, pageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting paged clients with search term: {SearchTerm}, page: {PageNumber}, page size: {SearchTerm}",
                searchTerm, pageNumber, pageSize);
            throw;
        }
    }
}