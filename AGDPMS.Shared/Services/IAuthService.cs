namespace AGDPMS.Shared.Services;

public interface IAuthService
{
    Task<LoginResult> LoginAsync(string phoneNumber, string password);
    Task<ForgotPasswordResult> SendForgotPasswordCodeAsync(string phoneNumber);
    Task<VerifyOtpResult> VerifyOtpAsync(int userId, string otp);
    Task<BaseResult> ResetCurrentUserPasswordAsync(string password);
    Task<BaseResult> ResetPasswordWithTokenAsync(string token, string password);
}

public class BaseResult
{
    public required bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

public sealed class LoginResult : BaseResult
{
    public string? RedirectUrl { get; set; }
}

public sealed class ForgotPasswordResult : BaseResult
{
    public int? UserId { get; set; }
}

public sealed class VerifyOtpResult : BaseResult
{
    public string? ResetToken { get; set; }
}