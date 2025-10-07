using System.Threading.Tasks;

namespace AGDPMS.Shared.Services;

public sealed class UserProfileDto
{
    public int? UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
}

public interface IUserSession
{
    Task<UserProfileDto?> GetCurrentAsync();
}


