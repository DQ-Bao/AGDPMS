using System.Net.Http;
using AGDPMS.Shared.Hubs;
using AGDPMS.Shared.Models;
using AGDPMS.Shared.Services;
using AGDPMS.Web;
using AGDPMS.Web.Components;
using AGDPMS.Web.Endpoints;
using AGDPMS.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddDataAccesses(builder.Configuration.GetConnectionString("Default")!);
builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();

// Configure HttpClient for Blazor Server with cookie forwarding
builder.Services.AddTransient<CookieForwardingHandler>();
builder
    .Services.AddHttpClient("BlazorServer")
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler())
    .AddHttpMessageHandler<CookieForwardingHandler>();

builder.Services.AddScoped(sp =>
{
    var nav = sp.GetRequiredService<NavigationManager>();
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var client = factory.CreateClient("BlazorServer");
    client.BaseAddress = new Uri(nav.BaseUri);
    return client;
});

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
builder.Services.AddScoped<ISaleServices, SaleService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IQAService, QAService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddSingleton<INotificationService, NotificationService>();
builder.Services.AddScoped<IReportService, ReportSerivce>();

// enable SignalR
builder.Services.AddSignalR();

builder.Services.AddSingleton<IWebHostEnvironment>(builder.Environment);
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
app.MapProductionOrders();
app.MapProductionStages();
app.MapStageTypes();
app.MapOrderSettings();
app.MapQr();
app.MapLookup();
app.MapHub<NotificationHub>("/hubs/notifications");

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(AGDPMS.Shared.Components._Imports).Assembly);

app.MapAdditionalIdentityEndpoints();

app.Run();

// Cookie forwarding handler for Blazor Server HttpClient
public class CookieForwardingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CookieForwardingHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Request.Headers.ContainsKey("Cookie") == true)
        {
            var cookieHeader = httpContext.Request.Headers["Cookie"].ToString();
            if (!string.IsNullOrEmpty(cookieHeader))
            {
                request.Headers.Add("Cookie", cookieHeader);
            }
        }
        return await base.SendAsync(request, cancellationToken);
    }
}
