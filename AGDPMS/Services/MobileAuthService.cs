using AGDPMS.Shared.Models;
using AGDPMS.Shared.Services;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace AGDPMS.Services;

internal class MobileAuthService(HttpClient http, CustomAuthenticationStateProvider authenticationStateProvider) : IAuthService
{
    public async Task<LoginResult> LoginAsync(string phoneNumber, string password)
    {
        try
        {
            var request = new LoginRequest { PhoneNumber = phoneNumber, Password = password };
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
                return new LoginResult { Success = false, ErrorMessage = "Unexpected response from server" };

            await authenticationStateProvider.Login(login.Token);
            var redirectUrl = login.NeedChangePassword ? "/need-change-password" : "/";
            return new LoginResult { Success = true, RedirectUrl = redirectUrl };
        }
        catch (Exception ex)
        {
            return new LoginResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<BaseResult> ResetCurrentUserPasswordAsync(string password)
    {
        try
        {
            var request = new ResetCurrentUserPasswordRequest { Password = password };
            var response = await http.PostAsJsonAsync("users/me/password", request);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return new BaseResult { Success = false, ErrorMessage = error };
            }
            return new BaseResult { Success = true };
        }
        catch (Exception ex)
        {
            return new BaseResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<ForgotPasswordResult> SendForgotPasswordCodeAsync(string phoneNumber)
    {
        try
        {
            var request = new ForgotPasswordRequest { Phone = phoneNumber };
            var response = await http.PostAsJsonAsync("auth/forgot-password", request);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return new ForgotPasswordResult { Success = false, ErrorMessage = error };
            }

            var result = await response.Content.ReadFromJsonAsync<ForgotPasswordResponse>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (result is null)
                return new ForgotPasswordResult { Success = false, ErrorMessage = "Unexpected response from server" };

            return new ForgotPasswordResult { Success = true, UserId = result.UserId };
        }
        catch (Exception ex)
        {
            return new ForgotPasswordResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<VerifyOtpResult> VerifyOtpAsync(int userId, string otp)
    {
        try
        {
            var request = new VerifyOtpRequest { UserId = userId, Otp = otp };
            var response = await http.PostAsJsonAsync("auth/verify-otp", request);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return new VerifyOtpResult { Success = false, ErrorMessage = error };
            }
            var result = await response.Content.ReadFromJsonAsync<VerifyOtpResponse>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (result is null || string.IsNullOrEmpty(result.ResetToken))
                return new VerifyOtpResult { Success = false, ErrorMessage = "Unexpected response from server" };
            return new VerifyOtpResult { Success = true, ResetToken = result.ResetToken };
        }
        catch (Exception ex)
        {
            return new VerifyOtpResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<BaseResult> ResetPasswordWithTokenAsync(string token, string password)
    {
        try
        {
            var request = new ResetPasswordWithTokenRequest { Token = token, Password = password };
            var response = await http.PostAsJsonAsync("auth/reset-password", request);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return new BaseResult { Success = false, ErrorMessage = error };
            }
            return new BaseResult { Success = true };
        }
        catch (Exception ex)
        {
            return new BaseResult { Success = false, ErrorMessage = ex.Message };
        }
    }
}
