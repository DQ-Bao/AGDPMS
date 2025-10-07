using System.Security.Claims;
using AGDPMS.Shared.Services;

namespace AGDPMS.Web.Services;

public sealed class WebUserSession(IHttpContextAccessor httpContextAccessor) : IUserSession
{
    public Task<UserProfileDto?> GetCurrentAsync()
    {
        var ctx = httpContextAccessor.HttpContext;
        if (ctx?.User?.Identity?.IsAuthenticated != true)
        {
            return Task.FromResult<UserProfileDto?>(null);
        }

        var id = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
        int? userId = int.TryParse(id, out var parsed) ? parsed : null;
        var phone = ctx.User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
        var fullName = ctx.User.FindFirst("FullName")?.Value ?? string.Empty;
        var role = ctx.User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        return Task.FromResult<UserProfileDto?>(new UserProfileDto
        {
            UserId = userId,
            PhoneNumber = phone,
            FullName = fullName,
            RoleName = role
        });
    }
}


