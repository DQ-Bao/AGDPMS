using System.Security.Claims;

namespace AGDPMS.Web.Data;

public class AppUser
{
    public int Id { get; set; }
    public required string PhoneNumber { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public required string FullName { get; set; }
    public required AppRole Role { get; set; }
    public bool NeedChangePassword { get; set; } = true;

    public Claim[] ToClaims() =>
        [
            new(ClaimTypes.NameIdentifier, Id.ToString()),
            new(ClaimTypes.Name, PhoneNumber),
            new(ClaimTypes.Role, Role.Name),
            new("FullName", FullName),
            new("NeedPasswordChange", NeedChangePassword.ToString()),
        ];
}

public class AppRole
{
    public int Id { get; set; }
    public required string Name { get; set; }
}
