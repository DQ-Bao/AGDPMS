using AGDPMS.Shared.Models.DTOs;

namespace AGDPMS.Shared.Services;

public interface INotificationClient
{
    Task ReceiveNotification(NotificationDto notification);
}