namespace AGDPMS.Shared.Core.Services;
public interface ISmsSender
{
    Task SendAsync(string message, string[] phoneNumbers);
}


