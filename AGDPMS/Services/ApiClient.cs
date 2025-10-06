using System.Net.Http.Json;
using AGDPMS.Shared.Services;

namespace AGDPMS.Services;

public interface IApiClient
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<ForgotPasswordResponse?> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);
    Task<VerifyOtpResponse?> VerifyOtpAsync(VerifyOtpRequest request, CancellationToken cancellationToken = default);
    Task<ResetPasswordResponse?> ResetPasswordAsync(ResetPasswordWithTokenRequest request, CancellationToken cancellationToken = default);
    Task<dynamic?> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default);
    Task<AddAccountResponse?> AddAccountAsync(AddAccountRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<RoleDto>?> GetRolesAsync(CancellationToken cancellationToken = default);
}

public sealed class ApiClient(HttpClient httpClient) : IApiClient
{
    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("/api/auth/login", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: cancellationToken);
    }

    public async Task<ForgotPasswordResponse?> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("/api/auth/forgot-password", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ForgotPasswordResponse>(cancellationToken: cancellationToken);
    }

    public async Task<VerifyOtpResponse?> VerifyOtpAsync(VerifyOtpRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("/api/auth/verify-otp", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<VerifyOtpResponse>(cancellationToken: cancellationToken);
    }

    public async Task<ResetPasswordResponse?> ResetPasswordAsync(ResetPasswordWithTokenRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("/api/auth/reset-password", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ResetPasswordResponse>(cancellationToken: cancellationToken);
    }

    public async Task<dynamic?> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("/api/auth/change-password", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<dynamic>(cancellationToken: cancellationToken);
    }

    public async Task<AddAccountResponse?> AddAccountAsync(AddAccountRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("/api/auth/add-account", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AddAccountResponse>(cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<RoleDto>?> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync("/api/auth/roles", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<RoleDto>>(cancellationToken: cancellationToken);
    }
}

