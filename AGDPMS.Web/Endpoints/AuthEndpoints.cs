using AGDPMS.Shared.Models;
using AGDPMS.Shared.Services;
using AGDPMS.Web.Data;
using AGDPMS.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace AGDPMS.Web.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/api/auth").WithTags("Auth");
        auth.MapPost("/login", async (
            [FromBody] LoginRequest request,
            UserDataAccess userDataAccess,
            IPasswordHasher<AppUser> passwordHasher,
            JwtService jwt) =>
        {
            var user = await userDataAccess.GetByPhoneNumberAsync(request.PhoneNumber);
            if (user is null) return Results.Unauthorized();
            
            var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (result != PasswordVerificationResult.Success) return Results.Unauthorized();

            var token = jwt.GenerateToken(user);

            return Results.Ok(new LoginResponse
            {
                Token = token,
                NeedChangePassword = user.NeedChangePassword
            });
        })
        .WithName("Login")
        .Produces<LoginResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);

        auth.MapPost("/forgot-password", async (
            [FromBody] ForgotPasswordRequest request,
            UserDataAccess userDataAccess,
            ISmsSender sms,
            IMemoryCache cache) =>
        {
            var user = await userDataAccess.GetByPhoneNumberAsync(request.Phone);
            if (user is null)
                return Results.BadRequest("Phone number not found");

            try
            {
                var otp = Random.Shared.Next(100000, 999999).ToString();
                cache.Set($"otp:{user.Id}", otp, TimeSpan.FromMinutes(5));
                await sms.SendAsync($"Your reset code is: {otp}", [user.PhoneNumber]);
                return Results.Ok(new ForgotPasswordResponse { UserId = user.Id });
            }
            catch (Exception ex)
            {
                return Results.InternalServerError(ex.Message);
            }
        })
        .WithName("ForgotPassword")
        .Produces<ForgotPasswordResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);

        auth.MapPost("/verify-otp", async (
            [FromBody] VerifyOtpRequest request,
            UserDataAccess userDataAccess,
            IMemoryCache cache) =>
        {
            var user = await userDataAccess.GetByIdAsync(request.UserId);
            if (user is null)
                return Results.BadRequest("Invalid request");

            var cacheKey = $"otp:{user.Id}";
            if (!cache.TryGetValue(cacheKey, out string? storedOtp))
                return Results.BadRequest("Code not found or expired");

            if (!string.Equals(request.Otp, storedOtp, StringComparison.Ordinal))
                return Results.BadRequest("Incorrect code");

            var token = Guid.NewGuid().ToString("N");
            cache.Set($"reset:{token}", user.Id, TimeSpan.FromDays(1));
            cache.Remove(cacheKey);
            return Results.Ok(new VerifyOtpResponse { ResetToken = token });
        })
        .WithName("VerifyOtp")
        .Produces<VerifyOtpResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        auth.MapPost("/reset-password", async (
            [FromBody] ResetPasswordWithTokenRequest request,
            UserDataAccess userDataAccess,
            IPasswordHasher<AppUser> passwordHasher,
            IMemoryCache cache) =>
        {
            if (!cache.TryGetValue($"reset:{request.Token}", out int? userId) || userId is null)
                return Results.BadRequest("Invalid or expired token");
            var user = await userDataAccess.GetByIdAsync(userId.Value);
            if (user is null)
                return Results.BadRequest("User not found");
            user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
            user.NeedChangePassword = false;
            await userDataAccess.SetPasswordHashAsync(user.Id, user.PasswordHash, needChange: false);
            cache.Remove($"reset:{request.Token}");
            return Results.Accepted();
        })
        .WithName("ResetPassword")
        .Produces<ResetPasswordWithTokenRequest>(StatusCodes.Status202Accepted)
        .Produces(StatusCodes.Status400BadRequest);
    }
}
