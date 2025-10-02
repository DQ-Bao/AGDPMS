namespace AGDPMS.Shared.Services;
public interface ISmsSender
{
    public Task SendAsync(string message, string[] phoneNumbers);
}
