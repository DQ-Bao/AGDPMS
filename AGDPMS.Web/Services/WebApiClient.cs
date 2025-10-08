using System.Net.Http.Json;
using AGDPMS.Shared.Services;

namespace AGDPMS.Web.Services;

public sealed class WebApiClient(IHttpContextAccessor httpContextAccessor) : IApiClient
{
    private HttpClient CreateHttpClient()
    {
        var ctx = httpContextAccessor.HttpContext ?? throw new InvalidOperationException("No HttpContext available");
        var req = ctx.Request;
        var baseUri = new Uri($"{req.Scheme}://{req.Host}");
        return new HttpClient { BaseAddress = baseUri };
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        using var http = CreateHttpClient();
        var response = await http.PostAsJsonAsync("/api/auth/login", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: cancellationToken);
    }

    public async Task<ForgotPasswordResponse?> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        using var http = CreateHttpClient();
        var response = await http.PostAsJsonAsync("/api/auth/forgot-password", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ForgotPasswordResponse>(cancellationToken: cancellationToken);
    }

    public async Task<VerifyOtpResponse?> VerifyOtpAsync(VerifyOtpRequest request, CancellationToken cancellationToken = default)
    {
        using var http = CreateHttpClient();
        var response = await http.PostAsJsonAsync("/api/auth/verify-otp", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<VerifyOtpResponse>(cancellationToken: cancellationToken);
    }

    public async Task<ResetPasswordResponse?> ResetPasswordAsync(ResetPasswordWithTokenRequest request, CancellationToken cancellationToken = default)
    {
        using var http = CreateHttpClient();
        var response = await http.PostAsJsonAsync("/api/auth/reset-password", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ResetPasswordResponse>(cancellationToken: cancellationToken);
    }

    public async Task<ChangePasswordResponse?> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        using var http = CreateHttpClient();
        var response = await http.PostAsJsonAsync("/api/auth/change-password", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ChangePasswordResponse>(cancellationToken: cancellationToken);
    }

    public async Task<AddAccountResponse?> AddAccountAsync(AddAccountRequest request, CancellationToken cancellationToken = default)
    {
        using var http = CreateHttpClient();
        var response = await http.PostAsJsonAsync("/api/auth/users", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AddAccountResponse>(cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<RoleDto>?> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        using var http = CreateHttpClient();
        var response = await http.GetAsync("/api/auth/roles", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<RoleDto>>(cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<UserDto>?> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        using var http = CreateHttpClient();
        var response = await http.GetAsync("/api/auth/users", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<UserDto>>(cancellationToken: cancellationToken);
    }

    public async Task<DeleteUserResponse?> DeleteUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        using var http = CreateHttpClient();
        var response = await http.DeleteAsync($"/api/auth/users/{userId}", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DeleteUserResponse>(cancellationToken: cancellationToken);
    }

    public async Task<bool> UpdateUserAsync(UserDto user, CancellationToken cancellationToken = default)
    {
        using var http = CreateHttpClient();
        var response = await http.PutAsJsonAsync($"/api/auth/users/{user.Id}", user, cancellationToken);
        response.EnsureSuccessStatusCode();
        return response.IsSuccessStatusCode;
    }
}


