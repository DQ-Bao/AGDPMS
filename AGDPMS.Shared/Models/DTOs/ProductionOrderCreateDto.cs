namespace AGDPMS.Shared.Models.DTOs;

public class ProductionOrderCreateDto
{
    public int ProjectId { get; set; }
    public List<ProductionOrderCreateItemDto> Items { get; set; } = new();
}

public class ProductionOrderCreateItemDto
{
    public int ProductId { get; set; }
}


