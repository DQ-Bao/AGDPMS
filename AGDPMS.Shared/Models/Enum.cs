﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGDPMS.Shared.Models;

// Enum để quản lý các trạng thái
public enum ProjectRFQStatus { Pending, Scheduled, Active, Completed }

public enum MachineStatus
{
    Operational, // Hoạt động
    NeedsMaintenance, // Cần bảo trì
    Broken // Bị hỏng
}


