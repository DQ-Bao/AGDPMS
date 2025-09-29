using AGDPMS.Web;
using AGDPMS.Web.Components;
using AGDPMS.Web.Components.Account;
using AGDPMS.Web.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddDataAccesses(builder.Configuration.GetConnectionString("Default")!);
builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
//builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider<AppUser>>();
//builder.Services.AddIdentityCore<AppUser>(opts =>
//{
//    opts.Password.RequireNonAlphanumeric = false;
//    opts.Password.RequireDigit = false;
//    opts.Password.RequiredUniqueChars = 0;
//    opts.Password.RequireUppercase = false;
//    //opts.SignIn.RequireConfirmedPhoneNumber = true;
//})
//    .AddSignInManager()
//    .AddDefaultTokenProviders();
builder.Services.AddAuthentication(Constants.AuthScheme)
    .AddCookie(Constants.AuthScheme, opts =>
    {
        opts.LoginPath = "/login";
        opts.AccessDeniedPath = "/access-denied";
        opts.LogoutPath = "/logout";

        opts.Cookie.Name = Constants.AuthCookie;
        opts.Cookie.HttpOnly = true;
        opts.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        opts.Cookie.SameSite = SameSiteMode.Strict;
        opts.ExpireTimeSpan = TimeSpan.FromDays(1);
        opts.SlidingExpiration = true;
    });
//builder.Services.AddAuthorizationCore();

builder.Services.AddEmailSmsSender(opts =>
{
    opts.From = "baodqhe180053@fpt.edu.vn";
    opts.UserName = "baodqhe180053@fpt.edu.vn";
    opts.Password = "cqua gnht xwzv zxsy";
});

builder.Services.AddMemoryCache();

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

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAdditionalIdentityEndpoints();

app.Run();
