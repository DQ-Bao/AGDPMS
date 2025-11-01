using AGDPMS.Shared.Models;

namespace AGDPMS.Shared.Services;

public interface IQAService
{
    // Machine Methods
    Task<PagedResult<AppMachine>> GetMachinesAsync(string? searchTerm = null, int pageNumber = 1, int pageSize = 10);
    Task<AppMachine?> GetMachineByIdAsync(int machineId);
    Task<AppMachine> CreateMachineAsync(AppMachine machine);
    Task UpdateMachineAsync(AppMachine machine);
    Task UpdateMachineStatusAsync(int machineId, MachineStatus newStatus);
    Task DeleteMachineAsync(int machineId);

    // Machine Type Methods
    Task<IEnumerable<AppMachineType>> GetMachineTypesAsync();
}