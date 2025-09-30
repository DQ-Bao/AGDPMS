namespace AGDPMS.Web.Services;
public interface ISmsSender
{
    Task SendAsync(string message, string[] phoneNumbers);
}
