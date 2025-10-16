using AGDPMS.Shared.Models;
using AGDPMS.Shared.Services;
using AGDPMS.Web.Data;
using Microsoft.AspNetCore.Identity;
using System.Data.Common;
using System.Security.Claims;

namespace AGDPMS.Web.Services;

internal class WebUserService(
    IHttpContextAccessor httpContextAccessor,
    UserDataAccess userDataAccess,
    IPasswordHasher<AppUser> passwordHasher) : IUserService
{
    public async Task<GetCurrentUserResult> GetCurrentUserAsync()
    {
        try
        {
            var principal = httpContextAccessor.HttpContext?.User;
            if (principal is null || (!principal.Identity?.IsAuthenticated ?? true))
                return new GetCurrentUserResult { Success = false, ErrorMessage = "Không tìm thấy thông tin hồ sơ." };
            var userIdClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return new GetCurrentUserResult { Success = false, ErrorMessage = "Không tìm thấy thông tin hồ sơ." };
            var user = await userDataAccess.GetByIdAsync(userId);
            return new GetCurrentUserResult { Success = true, User = user };
        }
        catch (DbException)
        {
            return new GetCurrentUserResult { Success = false, ErrorMessage = "Không thể tải hồ sơ lúc này. Vui lòng thử lại sau." };
        }
        catch (Exception ex)
        {
            return new GetCurrentUserResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<UpdateUserProfileResult> UpdateUserProfileAsync(AppUser user)
    {
        try
        {
            await userDataAccess.UpdateAsync(user);
            return new UpdateUserProfileResult { Success = true };
        }
        catch (Exception ex)
        {
            return new UpdateUserProfileResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<ChangePasswordResult> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        try
        {
            var user = await userDataAccess.GetByIdAsync(userId);
            if (user is null)
                return new ChangePasswordResult { Success = false, ErrorMessage = "Người dùng không tồn tại." };
            if (passwordHasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword) != PasswordVerificationResult.Success)
                return new ChangePasswordResult { Success = false, ErrorMessage = "Mật khẩu hiện tại không đúng." };
            var hash = passwordHasher.HashPassword(user, newPassword);
            await userDataAccess.SetPasswordHashAsync(user.Id, hash, needChange: false);
            return new ChangePasswordResult { Success = true };
        }
        catch (Exception ex)
        {
            return new ChangePasswordResult { Success = false, ErrorMessage = ex.Message };
        }
    }
}
