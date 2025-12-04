using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGDPMS.Shared.Models;

public class MaterialPlanning
{
    public string Id { set; get; } = string.Empty;
    public string Name { set; get; } = string.Empty;
    public double Length { set; get; }
    public double Weight { set; get; }
    public string Color { set; get; } = string.Empty;
    public string Vendor { set; get; } = string.Empty;
    public int Stock { set; get; }
    public int Receipt { set; get; }
}
