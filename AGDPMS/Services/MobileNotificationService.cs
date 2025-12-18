using AGDPMS.Shared.Models;
using AGDPMS.Shared.Models.DTOs;
using AGDPMS.Shared.Services;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net.Http.Json;

namespace AGDPMS.Services;

public class MobileNotificationService : INotificationService
{
    public event Func<Notification, Task>? OnNotificationReceived;
    private readonly HubConnection _connection;
    private readonly HttpClient _http;

    public MobileNotificationService(IHttpClientFactory httpFactory)
    {
        _http = httpFactory.CreateClient("ApiClient");
        if (_connection is not null) return;
        var hubUrl = _http.BaseAddress!.ToString().Replace("/api/", "/hubs/notifications");
        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, opts =>
            {
                opts.AccessTokenProvider = async () => await SecureStorage.GetAsync("auth_token");
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
            var notifications = await _http.GetFromJsonAsync<List<Notification>>("notifications") ?? [];
            return new GetNotificationsResult { Success = true, Notifications = notifications };
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