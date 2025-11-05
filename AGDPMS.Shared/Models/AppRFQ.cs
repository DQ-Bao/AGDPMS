using System.ComponentModel.DataAnnotations; // <-- THÊM DÒNG NÀY

namespace AGDPMS.Shared.Models;

public class AppRFQ
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên dự án.")]
    public string ProjectRFQName { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập địa điểm.")]
    public string Location { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn một khách hàng.")]
    public int ClientId { get; set; }

    public string? DesignCompany { get; set; } 

    [Required(ErrorMessage = "Vui lòng chọn ngày hoàn thành.")]
    public DateTime CompletionDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? DesignFilePath { get; set; }
    public string? DocumentPath { get; set; }

    public ProjectRFQStatus Status { get; set; }
    public AppClient Client { get; set; } = new();
}