using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AGDPMS.Shared.Services;

public interface IApiClient
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<ForgotPasswordResponse?> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);
    Task<VerifyOtpResponse?> VerifyOtpAsync(VerifyOtpRequest request, CancellationToken cancellationToken = default);
    Task<ResetPasswordResponse?> ResetPasswordAsync(ResetPasswordWithTokenRequest request, CancellationToken cancellationToken = default);
    Task<ChangePasswordResponse?> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default);
    Task<AddAccountResponse?> AddAccountAsync(AddAccountRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<RoleDto>?> GetRolesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<UserDto>?> GetUsersAsync(CancellationToken cancellationToken = default);
    Task<DeleteUserResponse?> DeleteUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<bool> UpdateUserAsync(UserDto user, CancellationToken cancellationToken = default);
}


