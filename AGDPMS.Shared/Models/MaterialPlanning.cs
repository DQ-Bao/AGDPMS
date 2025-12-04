namespace AGDPMS.Shared.Models;

public class MaterialPlanning
{
    public int Id { get; set; }
    public required int UserId { get; set; }
    public required int ProjectId { get; set; }
    public MaterialPlanningStatus Status { get; set; } = MaterialPlanningStatus.Pending;
    public List<MaterialPlanningDetails> Details { get; set; } = [];
}

public enum MaterialPlanningStatus { Pending, Cancelled, Completed }

public class MaterialPlanningDetails
{
    public int Id { get; set; }
    public int PlanningId { get; set; }
    public required Material Material { get; set; }
    public double Length { get; set; }
    public double Width { get; set; }
    public required int Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
}