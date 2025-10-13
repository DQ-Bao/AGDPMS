using AGDPMS.Shared.Models;
using AGDPMS.Shared.Services;

namespace AGDPMS.Services;

public class MobileUserService : IUserService
{
    public Task<GetCurrentUserResult> GetCurrentUserAsync()
    {
        throw new NotImplementedException();
    }

    public Task<UpdateUserProfileResult> UpdateUserProfileAsync(AppUser user)
    {
        throw new NotImplementedException();
    }

    public Task<ChangePasswordResult> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        throw new NotImplementedException();
    }
}
