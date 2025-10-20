using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGDPMS.Shared.Models;

public class AppRFQ
{
    public int Id { get; set; } 
    public string ProjectRFQName { get; set; }
    public string Location { get; set; }
    public int ClientId { get; set; }
    public string DesignCompany { get; set; }
    public DateTime CompletionDate { get; set; }

    public DateTime CreatedAt { get; set; }

    //  DesignFilePath (đường dẫn)
    public string DesignFilePath { get; set; }
    public string DocumentPath { get; set; }
    public ProjectRFQStatus Status { get; set; }
    public AppClient Client { get; set; } = new();

}

