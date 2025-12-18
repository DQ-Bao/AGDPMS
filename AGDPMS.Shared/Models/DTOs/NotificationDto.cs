namespace AGDPMS.Shared.Models.DTOs;

public class NotificationDto
{
    public int Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Url { get; set; } = "#";
    public DateTime Timestamp { get; set; }
    public bool IsRead { get; set; }
    public int? UserId { get; set; }
    public AppRole? Role { get; set; }

    public static NotificationDto FromDomain(Notification notification)
    {
        var (userId, role) = notification.Target.ToValues();
        return new NotificationDto
        {
            Id = notification.Id,
            Message = notification.Message,
            Url = notification.Url,
            Timestamp = notification.Timestamp,
            IsRead = notification.IsRead,
            UserId = userId,
            Role = role
        };
    }

    public Notification ToDomain() => new()
    {
        Id = Id,
        Message = Message,
        Url = Url,
        Timestamp = Timestamp,
        IsRead = IsRead,
        Target = NotificationTarget.FromValues(UserId, Role)
    };
}