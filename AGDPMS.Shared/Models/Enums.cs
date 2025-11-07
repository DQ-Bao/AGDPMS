namespace AGDPMS.Shared.Models;

public enum ProductionOrderStatus : short
{
    Draft = 0,
    PendingDirectorApproval = 1,
    DirectorRejected = 2,
    PendingQACheckMachines = 3,
    PendingQACheckMaterial = 4,
    InProduction = 5,
    Finished = 6,
    Cancelled = 9
}
