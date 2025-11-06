using AGDPMS.Shared.Models;
using AGDPMS.Shared.Services;

namespace AGDPMS.Web.Services;

public class NotificationService : INotificationService
{

    private List<Notification> _notifications = new();
    private int _nextId = 1;

    public event Func<Task>? OnNotificationReceived;

    public NotificationService()
    {
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
        notification.Id = _nextId++;
        notification.Timestamp = DateTime.Now;
        notification.IsRead = false;

        _notifications.Insert(0, notification); 

        if (OnNotificationReceived != null)
        {
            await OnNotificationReceived.Invoke();
        }
    }

    public Task<List<Notification>> GetNotificationsAsync()
    {
        var sortedList = _notifications.OrderByDescending(n => n.Timestamp).ToList();
        return Task.FromResult(sortedList);
    }

    public Task<int> GetUnreadCountAsync()
    {
        return Task.FromResult(_notifications.Count(n => !n.IsRead));
    }

    public async Task MarkAsReadAsync(int notificationId)
    {
        var notification = _notifications.FirstOrDefault(n => n.Id == notificationId);
        if (notification != null && !notification.IsRead)
        {
            notification.IsRead = true;
            if (OnNotificationReceived != null)
            {
                await OnNotificationReceived.Invoke();
            }
        }
    }
}