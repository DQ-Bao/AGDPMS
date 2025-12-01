using AGDPMS.Shared.Models;
using AGDPMS.Shared.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Linq;

namespace AGDPMS.Shared.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hub;
    private readonly List<Notification> _notifications = new();
    private int _nextId = 1;
    private readonly object _sync = new();

    public event Func<Task>? OnNotificationReceived;

    public NotificationService(IHubContext<NotificationHub> hub)
    {
        _hub = hub;

        // seed
        _notifications.Add(new Notification
        {
            Id = _nextId++,
            Message = "Máy Cắt CNC 01 báo lỗi 'Nhiệt độ quá cao'.",
            Url = "/qa/machines",
            Timestamp = DateTime.Now.AddMinutes(-5),
            IsRead = false
        });
        _notifications.Add(new Notification
        {
            Id = _nextId++,
            Message = "Dự án Vinhome vừa được cập nhật sang 'Hoạt động'.",
            Url = "/sale/projectrfq/detail/1",
            Timestamp = DateTime.Now.AddMinutes(-30),
            IsRead = false
        });
        _notifications.Add(new Notification
        {
            Id = _nextId++,
            Message = "Khách hàng 'Albert Cook' đã được thêm.",
            Url = "/sale/customercontact/detail/1",
            Timestamp = DateTime.Now.AddHours(-2),
            IsRead = true
        });
    }

    public async Task AddNotificationAsync(Notification notification)
    {
        lock (_sync)
        {
            notification.Id = _nextId++;
            notification.Timestamp = DateTime.Now;
            notification.IsRead = false;
            _notifications.Insert(0, notification);
        }

        // If RecipientUserId is set, broadcast to that user's group; otherwise broadcast to all.
        if (!string.IsNullOrEmpty(notification.RecipientUserId))
        {
            var group = GetGroupName(notification.RecipientUserId);
            await _hub.Clients.Group(group).SendAsync("ReceiveNotification", notification);
        }
        else
        {
            await _hub.Clients.All.SendAsync("ReceiveNotification", notification);
        }

        if (OnNotificationReceived != null)
            await OnNotificationReceived.Invoke();
    }

    public Task<List<Notification>> GetNotificationsAsync()
    {
        var sortedList = _notifications.OrderByDescending(n => n.Timestamp).ToList();
        return Task.FromResult(sortedList);
    }

    public Task<int> GetUnreadCountAsync()
    {
        var count = _notifications.Count(n => !n.IsRead);
        return Task.FromResult(count);
    }

    public async Task MarkAsReadAsync(int notificationId)
    {
        Notification? notification = null;
        lock (_sync)
        {
            notification = _notifications.FirstOrDefault(n => n.Id == notificationId);
            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;
            }
        }

        if (notification != null)
        {
            if (!string.IsNullOrEmpty(notification.RecipientUserId))
            {
                await _hub.Clients.Group(GetGroupName(notification.RecipientUserId)).SendAsync("NotificationUpdated", notification);
            }
            else
            {
                await _hub.Clients.All.SendAsync("NotificationUpdated", notification);
            }

            if (OnNotificationReceived != null)
                await OnNotificationReceived.Invoke();
        }
    }

    private static string GetGroupName(string userId) => $"user-{userId}";
}
