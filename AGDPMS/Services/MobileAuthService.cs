using AGDPMS.Shared.Models;
using AGDPMS.Shared.Services;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace AGDPMS.Services;

internal class MobileAuthService(HttpClient http) : IAuthService
{
    public async Task<LoginResult> LoginAsync(string phoneNumber, string password)
    {
        try
        {
            var request = new LoginRequest
            {
                PhoneNumber = phoneNumber,
                Password = password
            };
            var response = await http.PostAsJsonAsync("auth/login", request);
            if (!response.IsSuccessStatusCode)
                return new LoginResult
                {
                    Success = false,
                    ErrorMessage = response.StatusCode == HttpStatusCode.Unauthorized
                        ? "Invalid phone number or password"
                        : "Login failed"
                };

            var login = await response.Content.ReadFromJsonAsync<LoginResponse>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (login is null || string.IsNullOrEmpty(login.Token))
                return new LoginResult
                {
                    Success = false,
                    ErrorMessage = "Unexpected response from server"
                };

            await SecureStorage.Default.SetAsync("auth_token", login.Token);
            var redirectUrl = login.NeedChangePassword ? "/need-change-password" : "/";
            return new LoginResult
            {
                Success = true,
                RedirectUrl = redirectUrl
            };
        }
        catch (Exception ex)
        {
            return new LoginResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}
