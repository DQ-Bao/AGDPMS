using AGDPMS.Shared.Models;

namespace AGDPMS.Shared.Services;

public interface IUserService
{
    Task<GetCurrentUserResult> GetCurrentUserAsync();
    Task<UpdateUserProfileResult> UpdateUserProfileAsync(AppUser user);
    Task<ChangePasswordResult> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
}

public sealed class GetCurrentUserResult
{
    public required bool Success { get; set; }
    public AppUser? User { get; set; }
    public string? ErrorMessage { get; set; }
}

public sealed class UpdateUserProfileResult
{
    public required bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

public sealed class ChangePasswordResult
{
    public required bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}
