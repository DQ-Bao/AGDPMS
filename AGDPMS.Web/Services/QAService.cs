using AGDPMS.Shared.Models;
using AGDPMS.Shared.Services;
using AGDPMS.Web.Data;


namespace AGDPMS.Web.Services;

public class QAService(
    MachineDataAccess machineAccess,
    MachineTypeDataAccess machineTypeAccess,
    INotificationService notificationService,
    ILogger<QAService> logger) : IQAService
{
    private readonly MachineDataAccess _machineAccess = machineAccess;
    private readonly MachineTypeDataAccess _machineTypeAccess = machineTypeAccess;
    private readonly INotificationService _notificationService = notificationService;
    private readonly ILogger<QAService> _logger = logger;

    public async Task<PagedResult<AppMachine>> GetMachinesAsync(string? searchTerm = null, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            _logger.LogInformation("Getting paged machines with search term: {SearchTerm}, page: {PageNumber}, page size: {PageSize}",
                searchTerm, pageNumber, pageSize);
            return await _machineAccess.GetAllPagedAsync(searchTerm, pageNumber, pageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting paged machines with search term: {SearchTerm}, page: {PageNumber}, page size: {PageSize}",
                searchTerm, pageNumber, pageSize);
            throw;
        }
    }

    public async Task<AppMachine?> GetMachineByIdAsync(int machineId)
    {
        try
        {
            _logger.LogInformation("Getting machine by ID: {MachineId}", machineId);
            return await _machineAccess.GetByIdAsync(machineId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting machine by ID: {MachineId}", machineId);
            throw;
        }
    }

    public async Task<AppMachine> CreateMachineAsync(AppMachine machine)
    {
        try
        {
            _logger.LogInformation("Creating new machine: {MachineName}", machine.Name);
            return await _machineAccess.CreateAsync(machine);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating machine: {MachineName}", machine.Name);
            throw;
        }
    }

    public async Task UpdateMachineAsync(AppMachine machine)
    {
        try
        {
            _logger.LogInformation("Updating machine with ID: {MachineId}, Name: {MachineName}", machine.Id, machine.Name);
            await _machineAccess.UpdateAsync(machine);

            // KÍCH HOẠT THÔNG BÁO
            try
            {
                await _notificationService.AddNotificationAsync(new Notification
                {
                    Message = $"Máy '{machine.Name}' vừa được cập nhật trạng thái.",
                    Url = $"/qa/machines" // Hoặc trang detail nếu bạn đã tạo
                });
                _logger.LogInformation("Notification sent for machine update: {MachineId}", machine.Id);
            }
            catch (Exception notifEx)
            {
                _logger.LogWarning(notifEx, "Failed to send notification for machine update: {MachineId}", machine.Id);
                // Không throw ở đây để không ảnh hưởng đến operation chính
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating machine with ID: {MachineId}, Name: {MachineName}", machine.Id, machine.Name);
            throw;
        }
    }

    public async Task UpdateMachineStatusAsync(int machineId, MachineStatus newStatus)
    {
        try
        {
            _logger.LogInformation("Updating machine status for ID: {MachineId} to {NewStatus}", machineId, newStatus);
            await _machineAccess.UpdateStatusAsync(machineId, newStatus);

            // KÍCH HOẠT THÔNG BÁO
            try
            {
                await _notificationService.AddNotificationAsync(new Notification
                {
                    Message = $"Máy (ID: {machineId}) đã đổi trạng thái thành {newStatus}.",
                    Url = "/qa/machines"
                });
                _logger.LogInformation("Notification sent for machine status update: {MachineId}", machineId);
            }
            catch (Exception notifEx)
            {
                _logger.LogWarning(notifEx, "Failed to send notification for machine status update: {MachineId}", machineId);
                // Không throw ở đây để không ảnh hưởng đến operation chính
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating machine status for ID: {MachineId} to {NewStatus}", machineId, newStatus);
            throw;
        }
    }

    public async Task DeleteMachineAsync(int machineId)
    {
        try
        {
            _logger.LogInformation("Deleting machine with ID: {MachineId}", machineId);
            await _machineAccess.DeleteAsync(machineId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting machine with ID: {MachineId}", machineId);
            throw;
        }
    }

    public async Task<IEnumerable<AppMachineType>> GetMachineTypesAsync()
    {
        try
        {
            _logger.LogInformation("Getting all machine types");
            return await _machineTypeAccess.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all machine types");
            throw;
        }
    }
}