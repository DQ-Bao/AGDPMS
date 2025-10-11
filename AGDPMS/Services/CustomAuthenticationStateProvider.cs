using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AGDPMS.Services;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    public async Task Login(string token) 
    {
        await SecureStorage.Default.SetAsync("auth_token", token);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public void Logout() 
    {
        SecureStorage.Default.Remove("auth_token");
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await SecureStorage.GetAsync("auth_token");
            if (token is not null)
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);
                var identity = new ClaimsIdentity(jwt.Claims, "jwt");
                var user = new ClaimsPrincipal(identity);
                return new AuthenticationState(user);
            }
            var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
            return new AuthenticationState(anonymous);
        }
        catch (Exception)
        {
            var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
            return new AuthenticationState(anonymous);
        }
    }
}
