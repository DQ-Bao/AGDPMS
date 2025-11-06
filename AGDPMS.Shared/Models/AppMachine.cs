using System.ComponentModel.DataAnnotations.Schema;

namespace AGDPMS.Shared.Models;

public class AppMachine
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    [Column("machine_type_id")]
    public int MachineTypeId { get; set; }

    public MachineStatus Status { get; set; }

    [Column("entry_date")]
    public DateTime EntryDate { get; set; } 

    [Column("last_maintenance_date")]
    public DateTime? LastMaintenanceDate { get; set; } 

    public AppMachineType MachineType { get; set; } = new();
}