namespace AGDPMS.Shared.Services;

public sealed class LoginRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed class LoginResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public int? UserId { get; set; }
    public string? PhoneNumber { get; set; }
    public string? FullName { get; set; }
    public string? RoleName { get; set; }
    public bool NeedChangePassword { get; set; }
}

public sealed class ForgotPasswordRequest
{
    public string Phone { get; set; } = string.Empty;
}

public sealed class ForgotPasswordResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public int? UserId { get; set; }
}

public sealed class VerifyOtpRequest
{
    public int UserId { get; set; }
    public string Otp { get; set; } = string.Empty;
}

public sealed class ResetPasswordRequest
{
    public int UserId { get; set; }
    public string NewPassword { get; set; } = string.Empty;
}

public sealed class VerifyOtpResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? ResetToken { get; set; }
}

public sealed class ResetPasswordWithTokenRequest
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

public sealed class ResetPasswordResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}

public sealed class ChangePasswordRequest
{
    public int UserId { get; set; }
    public string NewPassword { get; set; } = string.Empty;
}

public sealed class ChangePasswordResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}

public sealed class AddAccountRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int RoleId { get; set; }
}

public sealed class AddAccountResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public int? UserId { get; set; }
}

public sealed class RoleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public sealed class UserDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public bool NeedChangePassword { get; set; }
    public int RoleId { get; set; }
}

public sealed class DeleteUserRequest
{
    public int UserId { get; set; }
}

public sealed class DeleteUserResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}

