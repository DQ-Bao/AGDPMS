using AGDPMS.Shared.Models;
using AGDPMS.Web.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AGDPMS.Web.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var users = app.MapGroup("/api/users").WithTags("Users");
        users.MapGet("/me", async (HttpContext httpContext, UserDataAccess userDataAccess) =>
        {
            var userIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Results.Unauthorized();

            var user = await userDataAccess.GetByIdAsync(userId);
            if (user is null) return Results.Unauthorized();

            return Results.Ok(GetCurrentUserResponse.From(user));
        })
        .RequireAuthorization(new AuthorizeAttribute { AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme })
        .WithName("GetCurrentUser")
        .Produces<GetCurrentUserResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);

        users.MapPut("/me", async (
            HttpContext httpContext,
            [FromBody] UpdateCurrentUserProfileRequest request,
            UserDataAccess userDataAccess) =>
        {
            var userIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Results.Unauthorized();

            var user = await userDataAccess.GetByIdAsync(userId);
            if (user is null) return Results.Unauthorized();

            user.FullName = request.FullName;
            user.Email = request.Email;
            user.DateOfBirth = request.DateOfBirth;
            await userDataAccess.UpdateAsync(user);

            return Results.Ok();
        })
        .RequireAuthorization(new AuthorizeAttribute { AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme })
        .WithName("UpdateUserProfile")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);

        users.MapPost("/me/password", async (
            HttpContext httpContext,
            [FromBody] ResetCurrentUserPasswordRequest request,
            UserDataAccess userDataAccess,
            IPasswordHasher<AppUser> passwordHasher) =>
        {
            var userIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Results.Unauthorized();

            var user = await userDataAccess.GetByIdAsync(userId);
            if (user is null) return Results.Unauthorized();

            var newHashedPassword = passwordHasher.HashPassword(user, request.Password);
            await userDataAccess.SetPasswordHashAsync(user.Id, newHashedPassword, needChange: false);

            return Results.Ok();
        })
        .RequireAuthorization(new AuthorizeAttribute { AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme })
        .WithName("ResetCurrentUserPassword")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);

        users.MapPut("/me/password", async (
            HttpContext httpContext,
            [FromBody] ChangeCurrentUserPasswordRequest request,
            UserDataAccess userDataAccess,
            IPasswordHasher<AppUser> passwordHasher) =>
        {
            var userIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Results.Unauthorized();

            var user = await userDataAccess.GetByIdAsync(userId);
            if (user is null) return Results.Unauthorized();

            if (passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword) != PasswordVerificationResult.Success)
                return Results.BadRequest();

            var newHashedPassword = passwordHasher.HashPassword(user, request.NewPassword);
            await userDataAccess.SetPasswordHashAsync(user.Id, newHashedPassword, needChange: false);

            return Results.Ok();
        })
        .RequireAuthorization(new AuthorizeAttribute { AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme })
        .WithName("ChangeCurrentUserPassword")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
