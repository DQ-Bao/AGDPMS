using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace AGDPMS.Shared.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(userId));
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGroupName(userId));
        }
        await base.OnDisconnectedAsync(exception);
    }

    // Optional explicit join/leave methods
    public Task JoinUserGroup(string userId) => Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(userId));
    public Task LeaveUserGroup(string userId) => Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGroupName(userId));

    private static string GetGroupName(string userId) => $"user-{userId}";
}