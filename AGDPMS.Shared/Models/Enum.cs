using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGDPMS.Shared.Models;

// Enum để quản lý các trạng thái
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

public enum ProjectRFQStatus { Pending, Scheduled, Active, Completed }

public enum MachineStatus
{
    Operational, // Hoạt động
    NeedsMaintenance, // Cần bảo trì
    Broken // Bị hỏng
}


