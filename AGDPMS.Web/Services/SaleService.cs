using AGDPMS.Shared.Models;
using AGDPMS.Shared.Services;
using AGDPMS.Web.Data;

namespace AGDPMS.Web.Services;

public class SaleService(ProjectRFQDataAccess projectDataAccess, ClientDataAccess clientDataAccess, ILogger<SaleService> logger, INotificationService notificationService, UserDataAccess userDataAccess) : ISaleServices
{
    private readonly ProjectRFQDataAccess _projectDataAccess = projectDataAccess;
    private readonly ClientDataAccess _clientDataAccess = clientDataAccess;
    private readonly ILogger<SaleService> _logger = logger;
    private readonly INotificationService _notificationService = notificationService;
    private readonly UserDataAccess _userDataAccess = userDataAccess;

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
            return Enumerable.Empty<AppRFQ>();
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
            return null; 
        }
    }
    public async Task<AppRFQ> CreateProjectRFQAsync(AppRFQ project)
    {
        try
        {
            _logger.LogInformation("Creating new project RFQ");
            var createdProject = await _projectDataAccess.CreateAsync(project);
            
            // Send notification to Admin/Director about new project request
            try
            {
                await NotifyByRolesAsync(new[] { "Director", "Admin" }, 
                    $"Yêu cầu dự án mới: {project.ProjectRFQName} đang chờ phê duyệt", 
                    $"/sale/projectrfq/detail/{createdProject.Id}");
                _logger.LogInformation("Notification sent for new project request: {ProjectId}", createdProject.Id);
            }
            catch (Exception notifEx)
            {
                _logger.LogWarning(notifEx, "Failed to send notification for new project request: {ProjectId}", createdProject.Id);
            }
            
            return createdProject;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating project RFQ");
            return null!; 
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
        }
    }
    // State Machine Methods - Enforce proper state transitions
    
    /// <summary>
    /// Approves a project request (Planning -> Production). Only Admin/Director can approve.
    /// </summary>
    public async Task ApproveProjectAsync(int projectId)
    {
        try
        {
            var project = await _projectDataAccess.GetByIdAsync(projectId);
            if (project == null)
            {
                throw new InvalidOperationException($"Project with ID {projectId} not found.");
            }

            if (project.Status != ProjectRFQStatus.Planning)
            {
                throw new InvalidOperationException($"Cannot approve project. Current status is {project.Status}, expected Planning.");
            }

            _logger.LogInformation("Approving project RFQ ID: {ProjectId}", projectId);
            await _projectDataAccess.UpdateStatusAsync(projectId, ProjectRFQStatus.Production);
            
            // Send notification to Sales about approval
            try
            {
                await NotifyByRolesAsync(new[] { "Sale" }, 
                    $"Dự án '{project.ProjectRFQName}' đã được phê duyệt và chuyển sang sản xuất", 
                    $"/sale/projectrfq/detail/{projectId}");
            }
            catch (Exception notifEx)
            {
                _logger.LogWarning(notifEx, "Failed to send approval notification for project: {ProjectId}", projectId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while approving project RFQ ID: {ProjectId}", projectId);
            throw;
        }
    }

    /// <summary>
    /// Cancels a project (Any status -> Cancelled). Only Admin/Director can cancel.
    /// </summary>
    public async Task CancelProjectAsync(int projectId)
    {
        try
        {
            var project = await _projectDataAccess.GetByIdAsync(projectId);
            if (project == null)
            {
                throw new InvalidOperationException($"Project with ID {projectId} not found.");
            }

            if (project.Status == ProjectRFQStatus.Cancelled)
            {
                throw new InvalidOperationException("Project is already cancelled.");
            }

            if (project.Status == ProjectRFQStatus.Completed)
            {
                throw new InvalidOperationException("Cannot cancel a completed project.");
            }

            _logger.LogInformation("Cancelling project RFQ ID: {ProjectId}", projectId);
            await _projectDataAccess.UpdateStatusAsync(projectId, ProjectRFQStatus.Cancelled);
            
            // Send notification to Sales about cancellation
            try
            {
                await NotifyByRolesAsync(new[] { "Sale" }, 
                    $"Dự án '{project.ProjectRFQName}' đã bị hủy", 
                    $"/sale/projectrfq/detail/{projectId}");
            }
            catch (Exception notifEx)
            {
                _logger.LogWarning(notifEx, "Failed to send cancellation notification for project: {ProjectId}", projectId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while cancelling project RFQ ID: {ProjectId}", projectId);
            throw;
        }
    }

    /// <summary>
    /// Completes production phase (Production -> Deploying). Called automatically when production is marked complete.
    /// </summary>
    public async Task CompleteProductionAsync(int projectId)
    {
        try
        {
            var project = await _projectDataAccess.GetByIdAsync(projectId);
            if (project == null)
            {
                throw new InvalidOperationException($"Project with ID {projectId} not found.");
            }

            if (project.Status != ProjectRFQStatus.Production)
            {
                throw new InvalidOperationException($"Cannot complete production. Current status is {project.Status}, expected Production.");
            }

            _logger.LogInformation("Completing production for project RFQ ID: {ProjectId}, transitioning to Deploying", projectId);
            await _projectDataAccess.UpdateStatusAsync(projectId, ProjectRFQStatus.Deploying);
            
            // Send notification
            try
            {
                await NotifyByRolesAsync(new[] { "Director", "Admin", "Sale" }, 
                    $"Dự án '{project.ProjectRFQName}' đã hoàn thành sản xuất, chuyển sang giai đoạn lắp đặt", 
                    $"/sale/projectrfq/detail/{projectId}");
            }
            catch (Exception notifEx)
            {
                _logger.LogWarning(notifEx, "Failed to send production completion notification for project: {ProjectId}", projectId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while completing production for project RFQ ID: {ProjectId}", projectId);
            throw;
        }
    }

    /// <summary>
    /// Completes deployment phase (Deploying -> Completed). Called automatically when deployment is complete.
    /// </summary>
    public async Task CompleteDeployAsync(int projectId)
    {
        try
        {
            var project = await _projectDataAccess.GetByIdAsync(projectId);
            if (project == null)
            {
                throw new InvalidOperationException($"Project with ID {projectId} not found.");
            }

            if (project.Status != ProjectRFQStatus.Deploying)
            {
                throw new InvalidOperationException($"Cannot complete deployment. Current status is {project.Status}, expected Deploying.");
            }

            _logger.LogInformation("Completing deployment for project RFQ ID: {ProjectId}, transitioning to Completed", projectId);
            await _projectDataAccess.UpdateStatusAsync(projectId, ProjectRFQStatus.Completed);
            
            // Send notification
            try
            {
                await NotifyByRolesAsync(new[] { "Director", "Admin", "Sale" }, 
                    $"Dự án '{project.ProjectRFQName}' đã hoàn thành", 
                    $"/sale/projectrfq/detail/{projectId}");
            }
            catch (Exception notifEx)
            {
                _logger.LogWarning(notifEx, "Failed to send completion notification for project: {ProjectId}", projectId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while completing deployment for project RFQ ID: {ProjectId}", projectId);
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
            return new PagedResult<AppRFQ>();
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
            return Enumerable.Empty<AppClient>();
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
            return null;
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
            return null!;
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
            _logger.LogError(ex, "Error occurred while getting paged clients with search term: {SearchTerm}, page: {PageNumber}, page size: {PageSize}", // Sửa lỗi typo {SearchTerm} -> {PageSize}
                searchTerm, pageNumber, pageSize);
            return new PagedResult<AppClient>();
        }
    }

    private async Task NotifyByRolesAsync(IEnumerable<string> roles, string message, string url)
    {
        try
        {
            var allUsers = await _userDataAccess.GetAllAsync();
            var roleSet = roles.ToHashSet(StringComparer.OrdinalIgnoreCase);
            var recipients = allUsers.Where(u => roleSet.Contains(u.Role.Name ?? string.Empty)).ToList();
            foreach (var user in recipients)
            {
                await _notificationService.AddNotificationAsync(new Notification
                {
                    Message = message,
                    Url = url,
                    RecipientUserId = user.Id.ToString()
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send notification by roles: {Message}", ex.Message);
        }
    }
}