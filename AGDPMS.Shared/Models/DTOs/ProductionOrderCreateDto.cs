namespace AGDPMS.Shared.Models.DTOs;

public class ProductionOrderCreateDto
{
    public string Code { get; set; } = string.Empty;
    public int ProjectId { get; set; }
    public List<ProductionOrderCreateItemDto> Items { get; set; } = new();
}

public class ProductionOrderCreateItemDto
{
    public int ProductId { get; set; }
}


