namespace AGDPMS.Shared.Models.DTOs;

public class ProductionOrderCreateDto
{
    public int ProjectId { get; set; }
    public List<ProductionOrderCreateItemDto> Items { get; set; } = new();
    public List<ProductionOrderTimeSettingsDto>? TimeSettings { get; set; }
}

public class ProductionOrderCreateItemDto
{
    public int CavityId { get; set; }
    public int Quantity { get; set; }
}


