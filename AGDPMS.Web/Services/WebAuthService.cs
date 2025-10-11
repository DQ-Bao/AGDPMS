using AGDPMS.Shared.Services;
using AGDPMS.Web.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace AGDPMS.Web.Services;

internal class WebAuthService(
    IHttpContextAccessor httpContextAccessor,
    UserDataAccess userDataAccess,
    IPasswordHasher<AppUser> passwordHasher,
    IMemoryCache cache,
    ISmsSender sms) : IAuthService
{
    public async Task<LoginResult> LoginAsync(string phoneNumber, string password)
    {
        try
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
        catch (Exception e)
        {
            return new LoginResult { Success = false, ErrorMessage = e.Message };
        }
    }

    public async Task<BaseResult> ResetCurrentUserPasswordAsync(string password)
    {
        try
        {
            var httpContext = httpContextAccessor.HttpContext!;
            if (httpContext.User.Identity?.IsAuthenticated != true)
                return new BaseResult { Success = false, ErrorMessage = "User not authenticated" };
        
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim is null || !int.TryParse(userIdClaim.Value, out int userId))
                return new BaseResult { Success = false, ErrorMessage = "Unauthorized" };
        
            var user = await userDataAccess.GetByIdAsync(userId);
            if (user is null)
                return new BaseResult { Success = false, ErrorMessage = "User not found" };
        
            user.PasswordHash = passwordHasher.HashPassword(user, password);
            user.NeedChangePassword = false;
            await userDataAccess.SetPasswordHashAsync(user.Id, user.PasswordHash, needChange: false);
        
            var claims = user.ToClaims();
            var identity = new ClaimsIdentity(claims, Constants.AuthScheme);
            var principal = new ClaimsPrincipal(identity);
            var authProps = new AuthenticationProperties
            {
                IsPersistent = true
            };
            await httpContext.SignInAsync(Constants.AuthScheme, principal, authProps);
            return new BaseResult { Success = true };
        }
        catch (Exception e)
        {
            return new BaseResult { Success = false, ErrorMessage = e.Message };
        }
    }

    public async Task<ForgotPasswordResult> SendForgotPasswordCodeAsync(string phoneNumber)
    {
        try
        {
            var user = await userDataAccess.GetByPhoneNumberAsync(phoneNumber);
            if (user is null)
                return new ForgotPasswordResult { Success = false, ErrorMessage = "Phone number not found" };
            var otp = Random.Shared.Next(100000, 999999).ToString();
            cache.Set($"otp:{user.Id}", otp, TimeSpan.FromMinutes(5));
            await sms.SendAsync($"Your reset code is: {otp}", [user.PhoneNumber]);
            return new ForgotPasswordResult { Success = true, UserId = user.Id };
        }
        catch (Exception e)
        {
            return new ForgotPasswordResult { Success = false, ErrorMessage = e.Message };
        }
    }

    public async Task<VerifyOtpResult> VerifyOtpAsync(int userId, string otp)
    {
        try
        {
            var user = await userDataAccess.GetByIdAsync(userId);
            if (user is null)
                return new VerifyOtpResult { Success = false, ErrorMessage = "Invalid Request" };
        
            var cacheKey = $"otp:{user.Id}";
            if (!cache.TryGetValue(cacheKey, out string? storedOtp))
                return new VerifyOtpResult { Success = false, ErrorMessage = "Code not found or expired" };
        
            if (!string.Equals(otp, storedOtp, StringComparison.Ordinal))
                return new VerifyOtpResult { Success = false, ErrorMessage = "Incorrect Code" };

            var token = Guid.NewGuid().ToString("N");
            cache.Set($"reset:{token}", user.Id, TimeSpan.FromDays(1));
            cache.Remove(cacheKey);
            return new VerifyOtpResult { Success = true, ResetToken = token };
        }
        catch (Exception e)
        {
            return new VerifyOtpResult { Success = false, ErrorMessage = e.Message };
        }
    }

    public async Task<BaseResult> ResetPasswordWithTokenAsync(string token, string password)
    {
        try
        {
            if (!cache.TryGetValue($"reset:{token}", out int? userId) || userId is null)
                return new BaseResult { Success = false, ErrorMessage = "Invalid or expired token" };
            var user = await userDataAccess.GetByIdAsync(userId.Value);
            if (user is null)
                return new BaseResult { Success = false, ErrorMessage = "User not found" };
            user.PasswordHash = passwordHasher.HashPassword(user, password);
            user.NeedChangePassword = false;
            await userDataAccess.SetPasswordHashAsync(user.Id, user.PasswordHash, needChange: false);
            cache.Remove($"reset:{token}");
            return new BaseResult { Success = true };
        }
        catch (Exception e)
        {
            return new BaseResult { Success = false, ErrorMessage = e.Message };
        }
    }
}
