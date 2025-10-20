namespace AGDPMS.Shared.Models;

public sealed class LoginRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed class LoginResponse
{
    public string? Token { get; set; }
    public bool NeedChangePassword { get; set; }
}

public sealed class ForgotPasswordRequest
{
    public string Phone { get; set; } = string.Empty;
}

public sealed class ForgotPasswordResponse
{
    public int UserId { get; set; }
}

public sealed class VerifyOtpRequest
{
    public int UserId { get; set; }
    public string Otp { get; set; } = string.Empty;
}

public sealed class VerifyOtpResponse
{
    public required string ResetToken { get; set; }
}

public sealed class ResetPasswordWithTokenRequest
{
    public required string Token { get; set; }
    public required string Password { get; set; }
}

public sealed record GetCurrentUserResponse(
    int Id,
    string FullName,
    string PhoneNumber,
    AppRole Role,
    bool IsActive,
    bool NeedChangePassword,
    string? Email,
    DateTime? DateOfBirth)
{
    public static GetCurrentUserResponse From(AppUser user) =>
        new(user.Id, user.FullName, user.PhoneNumber, user.Role, user.IsActive, user.NeedChangePassword, user.Email, user.DateOfBirth);
    public AppUser ToUser() => new()
        {
            Id = Id,
            FullName = FullName,
            PhoneNumber = PhoneNumber,
            Role = Role,
            IsActive = IsActive,
            NeedChangePassword = NeedChangePassword,
            Email = Email,
            DateOfBirth = DateOfBirth
        };
}

public sealed class ResetCurrentUserPasswordRequest
{
    public required string Password { get; set; }
}

public sealed class UpdateCurrentUserProfileRequest
{
    public required string FullName { get; set; }
    public string? Email { get; set; }
    public DateTime? DateOfBirth { get; set; }
}

public sealed class ChangeCurrentUserPasswordRequest
{
    public required string CurrentPassword { get; set; }
    public required string NewPassword { get; set; }
}
