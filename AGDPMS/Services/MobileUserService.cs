using AGDPMS.Shared.Models;
using AGDPMS.Shared.Services;
using System.Net.Http.Json;
using System.Text.Json;

namespace AGDPMS.Services;

public class MobileUserService(IHttpClientFactory httpFactory) : IUserService
{
    private readonly HttpClient _http = httpFactory.CreateClient("ApiClient");

    public async Task<GetCurrentUserResult> GetCurrentUserAsync()
    {
        try
        {
            var response = await _http.GetAsync("users/me");
            if (!response.IsSuccessStatusCode)
                return new GetCurrentUserResult
                {
                    Success = false,
                    ErrorMessage = response.StatusCode == System.Net.HttpStatusCode.Unauthorized
                        ? "Không tìm thấy thông tin hồ sơ."
                        : "Không thể tải hồ sơ lúc này. Vui lòng thử lại sau."
                };

            var dto = await response.Content.ReadFromJsonAsync<GetCurrentUserResponse>(new JsonSerializerOptions
            { 
                PropertyNameCaseInsensitive = true 
            });
            if (dto is null)
                return new GetCurrentUserResult { Success = false, ErrorMessage = "Không thể tải hồ sơ lúc này. Vui lòng thử lại sau." };

            return new GetCurrentUserResult { Success = true, User = dto.ToUser() };
        }
        catch (Exception ex)
        {
            return new GetCurrentUserResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<UpdateUserProfileResult> UpdateUserProfileAsync(AppUser user)
    {
        try
        {
            var request = new UpdateCurrentUserProfileRequest
            {
                FullName = user.FullName,
                Email = user.Email,
                DateOfBirth = user.DateOfBirth
            };
            var response = await _http.PutAsJsonAsync("users/me", request);
            if (!response.IsSuccessStatusCode)
                return new UpdateUserProfileResult
                {
                    Success = false,
                    ErrorMessage = response.StatusCode == System.Net.HttpStatusCode.Unauthorized
                        ? "Không tìm thấy thông tin hồ sơ."
                        : "Không thể cập nhật hồ sơ lúc này. Vui lòng thử lại sau."
                };
            return new UpdateUserProfileResult { Success = true };
        }
        catch (Exception ex)
        {
            return new UpdateUserProfileResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<ChangePasswordResult> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        try
        {
            var request = new ChangeCurrentUserPasswordRequest { CurrentPassword = currentPassword, NewPassword = newPassword };
            var response = await _http.PutAsJsonAsync("users/me/password", request);
            if (!response.IsSuccessStatusCode)
                return new ChangePasswordResult
                {
                    Success = false,
                    ErrorMessage = response.StatusCode switch
                    {
                        System.Net.HttpStatusCode.Unauthorized => "Không tìm thấy thông tin hồ sơ.",
                        System.Net.HttpStatusCode.BadRequest => "Mật khẩu hiện tại không đúng.",
                        _ => "Không thể đổi mật khẩu lúc này. Vui lòng thử lại sau."
                    }
                };
            return new ChangePasswordResult { Success = true };
        }
        catch (Exception ex)
        {
            return new ChangePasswordResult { Success = false, ErrorMessage = ex.Message };
        }
    }
}
