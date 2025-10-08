using AGDPMS.Shared.Services;

namespace AGDPMS.Services;

public sealed class MauiUserSession : IUserSession
{
    public async Task<UserProfileDto?> GetCurrentAsync()
    {
        try
        {
            var remembered = await Microsoft.Maui.Storage.SecureStorage.Default.GetAsync("remembered");
            if (!string.Equals(remembered, "true", StringComparison.Ordinal))
            {
                return null;
            }

            var userIdStr = await Microsoft.Maui.Storage.SecureStorage.Default.GetAsync("rememberedUserId");
            int? userId = int.TryParse(userIdStr, out var parsedId) ? parsedId : null;
            var phone = await Microsoft.Maui.Storage.SecureStorage.Default.GetAsync("rememberedPhone") ?? string.Empty;
            var fullName = await Microsoft.Maui.Storage.SecureStorage.Default.GetAsync("rememberedFullName") ?? string.Empty;
            var role = await Microsoft.Maui.Storage.SecureStorage.Default.GetAsync("rememberedRole") ?? string.Empty;

            return new UserProfileDto
            {
                UserId = userId,
                PhoneNumber = phone,
                FullName = fullName,
                RoleName = role
            };
        }
        catch
        {
            return null;
        }
    }
}


