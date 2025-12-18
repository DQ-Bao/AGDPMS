using AGDPMS.Shared.Models;
using AGDPMS.Shared.Models.DTOs;
using AGDPMS.Shared.Services;
using AGDPMS.Web.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net;
using System.Security.Claims;

namespace AGDPMS.Web.Services;

public class WebNotificationService : INotificationService
{
    public event Func<Notification, Task>? OnNotificationReceived;
    private readonly HubConnection _connection;
    private readonly NotificationDataAccess _dataAccess;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public WebNotificationService(NavigationManager nav, NotificationDataAccess dataAccess, IHttpContextAccessor httpContextAccessor)
    {
        _dataAccess = dataAccess;
        _httpContextAccessor = httpContextAccessor;
        if (_connection is not null) return;
        var cookies = new Dictionary<string, string>();
        _httpContextAccessor.HttpContext?.Request.Cookies.ToList().ForEach(c => cookies.Add(c.Key, c.Value));
        _connection = new HubConnectionBuilder()
            .WithUrl(nav.ToAbsoluteUri("/hubs/notifications"), opts =>
            {
                opts.UseDefaultCredentials = true;
                var cookieContainer = cookies.Count != 0 ? new CookieContainer(cookies.Count) : new CookieContainer();
                foreach (var cookie in cookies)
                    cookieContainer.Add(new Cookie(
                        cookie.Key,
                        WebUtility.UrlEncode(cookie.Value),
                        path: "/",
                        domain: nav.ToAbsoluteUri("/").Host));
                opts.Cookies = cookieContainer;
                opts.HttpMessageHandlerFactory = _ => new HttpClientHandler 
                {
                    PreAuthenticate = true,
                    CookieContainer = cookieContainer,
                    UseCookies = true,
                    UseDefaultCredentials = true
                };
            })
            .WithAutomaticReconnect()
            .Build();
        _connection.On<NotificationDto>(
            nameof(INotificationClient.ReceiveNotification),
            async notification =>
            {
                if (OnNotificationReceived is not null) await OnNotificationReceived.Invoke(notification.ToDomain());
            });
    }

    public async Task<StartStopNotificationServiceResult> StartAsync()
    {
        if (_connection.State == HubConnectionState.Connected)
            return new StartStopNotificationServiceResult { Success = true };
        try
        {
            await _connection.StartAsync();
            return new StartStopNotificationServiceResult { Success = true };
        }
        catch (Exception ex)
        {
            return new StartStopNotificationServiceResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<StartStopNotificationServiceResult> StopAsync()
    {
        if (_connection.State == HubConnectionState.Disconnected)
            return new StartStopNotificationServiceResult { Success = true };
        try
        {
            await _connection.StopAsync();
            return new StartStopNotificationServiceResult { Success = true };
        }
        catch (Exception ex)
        {
            return new StartStopNotificationServiceResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<GetNotificationsResult> GetNotificationsAsync()
    {
        try
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            if (principal is null || (!principal.Identity?.IsAuthenticated ?? true))
                return new GetNotificationsResult { Success = false, ErrorMessage = "Chưa đăng nhập" };
            var userIdClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return new GetNotificationsResult { Success = false, ErrorMessage = "Chưa đăng nhập" };
            var roleClaim = principal.FindFirstValue(ClaimTypes.Role);
            if (string.IsNullOrWhiteSpace(roleClaim))
                return new GetNotificationsResult { Success = false, ErrorMessage = "Chưa đăng nhập" };
            var notifications = await _dataAccess.GetUserNotificationsAsync(userId, roleClaim);
            return new GetNotificationsResult { Success = true, Notifications = [.. notifications] };
        }
        catch (Exception ex)
        {
            return new GetNotificationsResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<AddNotificationResult> AddNotificationAsync(Notification notification)
    {
        try
        {
            await _connection.InvokeAsync("AddNotification", NotificationDto.FromDomain(notification));
            return new AddNotificationResult { Success = true };
        }
        catch (Exception ex)
        {
            return new AddNotificationResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<MarkAsReadResult> MarkAsReadAsync(int notificationId)
    {
        try
        {
            await _connection.InvokeAsync("MarkAsRead", notificationId);
            return new MarkAsReadResult { Success = true };
        }
        catch (Exception ex)
        {
            return new MarkAsReadResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public ValueTask DisposeAsync() => _connection.DisposeAsync();
}