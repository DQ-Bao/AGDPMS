namespace AGDPMS.Shared.Models.DTOs;

public class ProjectCostManagementDto
{
    // Basic EVM metrics
    public decimal PV { get; set; } // Planned Value
    public decimal EV { get; set; } // Earned Value
    public decimal AC { get; set; } // Actual Cost
    public decimal BAC { get; set; } // Budget at Completion
    
    // Variances
    public decimal CV { get; set; } // Cost Variance (EV - AC)
    public decimal SV { get; set; } // Schedule Variance (EV - PV)
    
    // Performance Indices
    public decimal? CPI { get; set; } // Cost Performance Index (EV / AC)
    public decimal? SPI { get; set; } // Schedule Performance Index (EV / PV)
    
    // Forecasts
    public decimal? EAC { get; set; } // Estimate at Completion
    public decimal? ETC { get; set; } // Estimate to Complete
    public decimal? VAC { get; set; } // Variance at Completion
    public decimal? TCPI { get; set; } // To Complete Performance Index
    
    // Breakdown
    public decimal PlannedLaborCost { get; set; }
    public decimal ActualLaborCost { get; set; }
    public decimal PlannedMaterialCost { get; set; }
    public decimal ActualMaterialCost { get; set; }
    
    // Per-item breakdown
    public List<ItemCostBreakdownDto> ItemBreakdowns { get; set; } = new();
}

public class ItemCostBreakdownDto
{
    public int ItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string CavityName { get; set; } = string.Empty;
    public decimal PlannedLaborCost { get; set; }
    public decimal ActualLaborCost { get; set; }
    public decimal PlannedMaterialCost { get; set; }
    public decimal ActualMaterialCost { get; set; }
    public decimal PlannedTotalCost { get; set; }
    public decimal ActualTotalCost { get; set; }
}

