using AGDPMS.Shared.Models;

namespace AGDPMS.Shared.Services;

public interface INotificationService
{
    event Func<Task>? OnNotificationReceived;

    Task AddNotificationAsync(Notification notification);

    Task<List<Notification>> GetNotificationsAsync();

    Task<int> GetUnreadCountAsync();

    Task MarkAsReadAsync(int notificationId);
}