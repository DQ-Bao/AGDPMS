using AGDPMS.Web;
using Microsoft.AspNetCore.Authentication;

namespace Microsoft.AspNetCore.Routing;
internal static class IdentityComponentsEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapAdditionalIdentityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        endpoints.MapGet("/logout", async (HttpContext context) =>
        {
            await context.SignOutAsync(Constants.AuthScheme);
            context.Response.Redirect("/login");
        });
        return endpoints;
    }
}
