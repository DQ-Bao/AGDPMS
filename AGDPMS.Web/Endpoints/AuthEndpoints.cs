using AGDPMS.Shared.Models;
using AGDPMS.Web.Data;
using AGDPMS.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
    }
}
