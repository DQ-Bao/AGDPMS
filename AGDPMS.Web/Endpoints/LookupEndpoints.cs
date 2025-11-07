using AGDPMS.Web.Data;

namespace AGDPMS.Web.Endpoints;

public static class LookupEndpoints
{
    public static IEndpointRouteBuilder MapLookup(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/lookup");

        group.MapGet("/projects", async (string? q, ProjectDataAccess access) =>
        {
            var list = await access.SearchAsync(q);
            return Results.Ok(list.Select(t => new { id = t.Id, name = t.Name }));
        });

        group.MapGet("/products", async (int projectId, string? q, ProductDataAccess access) =>
        {
            var list = await access.SearchByProjectAsync(projectId, q);
            return Results.Ok(list.Select(t => new { id = t.Id, name = t.Name }));
        });

        group.MapGet("/qa-users", async (string? q, UserDataAccess access) =>
        {
            // naive search by fullname/phone for users with QA role
            var users = await access.GetAllAsync();
            var qa = users.Where(u => string.Equals(u.Role.Name, "QA", StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(q))
            {
                qa = qa.Where(u => (u.FullName?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false)
                                 || (u.PhoneNumber?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false));
            }
            return Results.Ok(qa.Select(u => new { id = u.Id, name = u.FullName, phone = u.PhoneNumber }));
        });

        return app;
    }
}


