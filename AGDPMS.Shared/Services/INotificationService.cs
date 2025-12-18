using AGDPMS.Shared.Models;

namespace AGDPMS.Shared.Services;

public interface INotificationService : IAsyncDisposable
{
    event Func<Notification, Task>? OnNotificationReceived;
    Task<StartStopNotificationServiceResult> StartAsync();
    Task<StartStopNotificationServiceResult> StopAsync();
    Task<GetNotificationsResult> GetNotificationsAsync();
    Task<AddNotificationResult> AddNotificationAsync(Notification notification);
    Task<MarkAsReadResult> MarkAsReadAsync(int notificationId);
}

public sealed class StartStopNotificationServiceResult
{
    public required bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

public sealed class GetNotificationsResult
{
    public required bool Success { get; set; }
    public List<Notification> Notifications { get; set; } = [];
    public string? ErrorMessage { get; set; }
}

public sealed class AddNotificationResult
{
    public required bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

public sealed class MarkAsReadResult
{
    public required bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}