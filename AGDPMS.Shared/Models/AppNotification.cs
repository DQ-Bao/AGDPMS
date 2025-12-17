namespace AGDPMS.Shared.Models;

public class Notification
{
    public int Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Url { get; set; } = "#";
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public bool IsRead { get; set; } = false;
    public NotificationTarget Target { get; set; } = NotificationTarget.All();
}

public abstract record NotificationTarget
{
    private NotificationTarget() { }
    public sealed record AllTarget() : NotificationTarget;
    public sealed record UserTarget(int UserId) : NotificationTarget;
    public sealed record RoleTarget(AppRole Role) : NotificationTarget;
    public static NotificationTarget All() => new AllTarget();
    public static NotificationTarget User(int userId) => new UserTarget(userId);
    public static NotificationTarget ForRole(AppRole role) => new RoleTarget(role);
    public static NotificationTarget FromValues(int? userId, AppRole? role)
    {
        if (userId.HasValue) return User(userId.Value);
        if (role is not null) return ForRole(role);
        return All();
    }
    public (int? userId, AppRole? role) ToValues() => this switch
    {
        AllTarget => (null, null),
        UserTarget u => (u.UserId, null),
        RoleTarget r => (null, r.Role),
        _ => throw new InvalidOperationException()
    };
}