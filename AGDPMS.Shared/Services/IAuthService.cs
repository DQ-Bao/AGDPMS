namespace AGDPMS.Shared.Services;

public interface IAuthService
{
    Task<LoginResult> LoginAsync(string phoneNumber, string password);
}

public class LoginResult
{
    public bool Success { get; set; }
    public string? RedirectUrl { get; set; }
    public string? ErrorMessage { get; set; }
}