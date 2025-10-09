using AGDPMS.Shared.Services;
using AGDPMS.Web.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace AGDPMS.Web.Services;

internal class WebAuthService(
    IHttpContextAccessor httpContextAccessor,
    UserDataAccess userDataAccess,
    IPasswordHasher<AppUser> passwordHasher) : IAuthService
{
    public async Task<LoginResult> LoginAsync(string phoneNumber, string password)
    {
        var httpContext = httpContextAccessor.HttpContext!;
        var user = await userDataAccess.GetByPhoneNumberAsync(phoneNumber);

        if (user is null || passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password) != PasswordVerificationResult.Success)
            return new LoginResult { Success = false, ErrorMessage = "Invalid phone number or password." };

        if (user.NeedChangePassword)
        {
            var limitClaims = new Claim[]
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.PhoneNumber),
                new("NeedPasswordChange", user.NeedChangePassword.ToString()),
            };
            var limitIdentity = new ClaimsIdentity(limitClaims, Constants.AuthScheme);
            var limitPrincipal = new ClaimsPrincipal(limitIdentity);
            await httpContext.SignInAsync(Constants.AuthScheme, limitPrincipal);
            return new LoginResult { Success = true, RedirectUrl = "/need-change-password" };
        }

        var claims = user.ToClaims();
        var identity = new ClaimsIdentity(claims, Constants.AuthScheme);
        var principal = new ClaimsPrincipal(identity);
        var authProps = new AuthenticationProperties
        {
            IsPersistent = true
        };

        await httpContext.SignInAsync(Constants.AuthScheme, principal, authProps);
        return new LoginResult { Success = true, RedirectUrl = "/" };
    }
}
