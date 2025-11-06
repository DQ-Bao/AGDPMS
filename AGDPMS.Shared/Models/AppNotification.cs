using System;

namespace AGDPMS.Shared.Models;

public class Notification
{
    public int Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Url { get; set; } = "#"; // Đường dẫn khi click vào
    public DateTime Timestamp { get; set; }
    public bool IsRead { get; set; } = false;
}