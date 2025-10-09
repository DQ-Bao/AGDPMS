using AGDPMS.Web.Data;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AGDPMS.Web.Services;

public class JwtOptions
{
    public string Key { get; set; } = string.Empty;
}

public class JwtService(IOptions<JwtOptions> opts)
{
    private readonly JwtOptions _opts = opts.Value;

    public string GenerateToken(AppUser user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Sub, user.PhoneNumber),
            new(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Name, user.FullName),
            new(JwtRegisteredClaimNames.PhoneNumber, user.PhoneNumber),
            new("role", user.Role.Name),
            new("NeedChangePassword", user.NeedChangePassword.ToString()),
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateJwtSecurityToken(
            subject: new ClaimsIdentity(claims),
            signingCredentials: new SigningCredentials(
                key: new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opts.Key)),
                algorithm: SecurityAlgorithms.HmacSha256Signature));
        return handler.WriteToken(token);
    }
}
