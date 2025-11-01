using AGDPMS.Shared.Models;
using AGDPMS.Shared.Services;
using AGDPMS.Web.Data;

namespace AGDPMS.Web.Services;

public class QAService(MachineDataAccess machineAccess, MachineTypeDataAccess machineTypeAccess) : IQAService
{
    public Task<PagedResult<AppMachine>> GetMachinesAsync(string? searchTerm = null, int pageNumber = 1, int pageSize = 10)
        => machineAccess.GetAllPagedAsync(searchTerm, pageNumber, pageSize);

    public Task<AppMachine?> GetMachineByIdAsync(int machineId)
        => machineAccess.GetByIdAsync(machineId);

    public Task<AppMachine> CreateMachineAsync(AppMachine machine)
        => machineAccess.CreateAsync(machine);

    public Task UpdateMachineAsync(AppMachine machine)
        => machineAccess.UpdateAsync(machine);

    public Task UpdateMachineStatusAsync(int machineId, MachineStatus newStatus)
        => machineAccess.UpdateStatusAsync(machineId, newStatus);

    public Task DeleteMachineAsync(int machineId)
        => machineAccess.DeleteAsync(machineId);

    public Task<IEnumerable<AppMachineType>> GetMachineTypesAsync()
        => machineTypeAccess.GetAllAsync();
}