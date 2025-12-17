using AGDPMS.Shared.Models;
using AGDPMS.Shared.Models.DTOs;
using AGDPMS.Shared.Services;
using AGDPMS.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace AGDPMS.Web.Hubs;

[Authorize]
public class NotificationHub(NotificationDataAccess notificationDataAccess) : Hub<INotificationClient>
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrWhiteSpace(userId)) await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");
        var roles = Context.User?.FindAll(ClaimTypes.Role).Select(r => r.Value) ?? [];
        foreach (var role in roles) await Groups.AddToGroupAsync(Context.ConnectionId, $"role:{role}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrWhiteSpace(userId)) await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user:{userId}");
        var roles = Context.User?.FindAll(ClaimTypes.Role).Select(r => r.Value) ?? [];
        foreach (var role in roles) await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"role:{role}");
        await base.OnDisconnectedAsync(exception);
    }

    public async Task AddNotification(NotificationDto dto)
    {
        var saved = await notificationDataAccess.CreateAsync(dto.ToDomain());
        await SendAsync(NotificationDto.FromDomain(saved));
    }

    public async Task MarkAsRead(int notificationId)
    {
        var userId = int.Parse(Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!);
        string roleName = Context.User!.FindFirstValue(ClaimTypes.Role)!;
        var notifications = await notificationDataAccess.GetUserNotificationsAsync(userId, roleName);
        var notification = notifications.FirstOrDefault(n => n.Id == notificationId);
        if (notification is null || notification.IsRead) return;
        notification.IsRead = true;
        await notificationDataAccess.UpdateAsync(notification);
    }

    private Task SendAsync(NotificationDto notification) => notification.ToDomain().Target switch
    {
        NotificationTarget.AllTarget => Clients.All.ReceiveNotification(notification),
        NotificationTarget.UserTarget u => Clients.Group($"user:{u.UserId}").ReceiveNotification(notification),
        NotificationTarget.RoleTarget r => Clients.Group($"role:{r.Role.Name}").ReceiveNotification(notification),
        _ => Task.CompletedTask
    };
}