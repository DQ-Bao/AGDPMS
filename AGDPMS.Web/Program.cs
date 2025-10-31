using AGDPMS.Web;
using AGDPMS.Web.Components;
using Microsoft.AspNetCore.Identity;
using AGDPMS.Shared.Models;
using AGDPMS.Shared.Services;
using AGDPMS.Web.Services;
using AGDPMS.Web.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddDataAccesses(builder.Configuration.GetConnectionString("Default")!);
builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();

builder.Services.AddCookieAndJwtAuth(opts => opts.Key = builder.Configuration["Jwt:Key"]!);

builder.Services.AddSmsSender(opts =>
{
    opts.Username = "S1DWDC";
    opts.Password = "4algwipfnvcd0p";
});

builder.Services.AddScoped<IAuthService, WebAuthService>();
builder.Services.AddScoped<IUserService, WebUserService>();
builder.Services.AddScoped<WStarService>();
builder.Services.AddScoped<IProductService, WebProductService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapAuthEndpoints();
app.MapUserEndpoints();

//var api = app.MapGroup("/api");

// Change password (for authenticated users)
//api.MapPost("/auth/change-password", async (
//    ChangePasswordRequest request,
//    UserDataAccess userDataAccess,
//    IPasswordHasher<AppUser> passwordHasher
//) =>
//{
//    var user = await userDataAccess.GetByIdAsync(request.UserId);
//    if (user is null)
//    {
//        return Results.Ok(new { Success = false, Message = "User not found" });
//    }

//    var hash = passwordHasher.HashPassword(user, request.NewPassword);
//    await userDataAccess.SetPasswordHashAsync(user.Id, hash, needChange: false);

//    return Results.Ok(new { Success = true, Message = "Password changed successfully" });
//});

//// Add account
//api.MapPost("/auth/add-account", async (
//    AddAccountRequest request,
//    UserDataAccess userDataAccess,
//    RoleDataAccess roleDataAccess,
//    IPasswordHasher<AppUser> passwordHasher,
//    ISmsSender smsSender
//) =>
//{
//    var role = (await roleDataAccess.GetAllAsync()).FirstOrDefault(r => r.Id == request.RoleId);
//    if (role is null)
//    {
//        return Results.Ok(new AddAccountResponse { Success = false, Message = "Invalid role selected" });
//    }

//    var user = new AppUser
//    {
//        PhoneNumber = request.PhoneNumber,
//        FullName = request.FullName,
//        Role = role,
//    };

//    user.PasswordHash = passwordHasher.HashPassword(user, "abc123");
    
//    try
//    {
//        await userDataAccess.CreateAsync(user);
//        await smsSender.SendAsync("Account added. Default password: abc123", [user.PhoneNumber]);
//        return Results.Ok(new AddAccountResponse { Success = true, UserId = user.Id, Message = "Account created successfully" });
//    }
//    catch (Exception ex)
//    {
//        return Results.Ok(new AddAccountResponse { Success = false, Message = ex.Message });
//    }
//});

//// Get roles
//api.MapGet("/auth/roles", async (RoleDataAccess roleDataAccess) =>
//{
//    var roles = await roleDataAccess.GetAllAsync();
//    return Results.Ok(roles.Select(r => new RoleDto { Id = r.Id, Name = r.Name }));
//});

//// Get users/accounts
//api.MapGet("/auth/users", async (UserDataAccess userDataAccess) =>
//{
//    var users = await userDataAccess.GetAllAsync();
//    return Results.Ok(users.Select(u => new UserDto 
//    { 
//        Id = u.Id, 
//        FullName = u.FullName, 
//        PhoneNumber = u.PhoneNumber, 
//        RoleName = u.Role.Name,
//        NeedChangePassword = u.NeedChangePassword,
//        RoleId = u.Role.Id
//    }));
//});

//// Delete user
//api.MapDelete("/auth/users/{userId:int}", async (
//    int userId,
//    UserDataAccess userDataAccess
//) =>
//{
//    try
//    {
//        var user = await userDataAccess.GetByIdAsync(userId);
//        if (user is null)
//        {
//            return Results.Ok(new DeleteUserResponse { Success = false, Message = "User not found" });
//        }

//        await userDataAccess.DeleteAsync(userId);
//        return Results.Ok(new DeleteUserResponse { Success = true, Message = "User deleted successfully" });
//    }
//    catch (Exception ex)
//    {
//        return Results.Ok(new DeleteUserResponse { Success = false, Message = ex.Message });
//    }
//});

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(AGDPMS.Shared.Components._Imports).Assembly);

app.MapAdditionalIdentityEndpoints();

app.Run();
