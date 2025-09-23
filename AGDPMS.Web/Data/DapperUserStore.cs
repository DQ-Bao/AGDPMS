using Dapper;
using Microsoft.AspNetCore.Identity;
using System.Data;

namespace AGDPMS.Web.Data;

public class DapperUserStore(IDbConnection conn)
    : IUserStore<AppUser>, IUserPasswordStore<AppUser>, IUserEmailStore<AppUser>
{
    private readonly IDbConnection _conn = conn;

    public async Task<IdentityResult> CreateAsync(AppUser user, CancellationToken cancellationToken)
    {
        var sql = @"insert into users(username, email, password_hash)
                    values (@UserName, @Email, @PasswordHash)
                    returning id";
        user.Id = await _conn.ExecuteScalarAsync<int>(sql, user);
        return IdentityResult.Success;
    }

    public async Task<AppUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        var sql = @"select id as Id, username as UserName, email as Email, password_hash as PasswordHash
                    from users where upper(username) = @Name";
        return await _conn.QueryFirstOrDefaultAsync<AppUser>(sql, new { Name = normalizedUserName });
    }

    public async Task<IdentityResult> DeleteAsync(AppUser user, CancellationToken cancellationToken)
    {
        var sql = @"delete from users where id = @Id";
        await _conn.ExecuteAsync(sql, new { user.Id });
        return IdentityResult.Success;
    }

    public async Task<AppUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        var sql = @"select id as Id, username as UserName, email as Email, password_hash as PasswordHash 
                    from users where id = @Id";
        return await _conn.QueryFirstOrDefaultAsync<AppUser>(sql, new { Id = userId });
    }

    public Task<string?> GetNormalizedUserNameAsync(AppUser user, CancellationToken cancellationToken) => Task.FromResult(user.UserName?.ToUpperInvariant());

    public Task<string?> GetPasswordHashAsync(AppUser user, CancellationToken cancellationToken) => Task.FromResult(user.PasswordHash);

    public Task<string> GetUserIdAsync(AppUser user, CancellationToken cancellationToken) => Task.FromResult(user.Id.ToString());

    public Task<string?> GetUserNameAsync(AppUser user, CancellationToken cancellationToken) => Task.FromResult(user.UserName);

    public Task<bool> HasPasswordAsync(AppUser user, CancellationToken cancellationToken) => Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));

    public Task SetNormalizedUserNameAsync(AppUser user, string? normalizedName, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task SetPasswordHashAsync(AppUser user, string? passwordHash, CancellationToken cancellationToken)
    {
        user.PasswordHash = passwordHash;
        return Task.CompletedTask;
    }

    public Task SetUserNameAsync(AppUser user, string? userName, CancellationToken cancellationToken)
    {
        user.UserName = userName;
        return Task.CompletedTask;
    }

    public async Task<IdentityResult> UpdateAsync(AppUser user, CancellationToken cancellationToken)
    {
        var sql = @"update users 
                    set username = @UserName, email = @Email, password_hash = @PasswordHash 
                    where id = @Id";
        await _conn.ExecuteAsync(sql, user);
        return IdentityResult.Success;
    }

    public Task SetEmailAsync(AppUser user, string? email, CancellationToken cancellationToken)
    {
        user.Email = email;
        return Task.CompletedTask;
    }

    public Task<string?> GetEmailAsync(AppUser user, CancellationToken cancellationToken) => Task.FromResult(user.Email);

    public Task<bool> GetEmailConfirmedAsync(AppUser user, CancellationToken cancellationToken) => Task.FromResult(true);

    public Task SetEmailConfirmedAsync(AppUser user, bool confirmed, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task<AppUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        var sql = @"select id as Id, username as UserName, email as Email, password_hash as PasswordHash 
                    from users where upper(email) = @Email";
        return _conn.QueryFirstOrDefaultAsync<AppUser>(sql, new { Email = normalizedEmail });
    }

    public Task<string?> GetNormalizedEmailAsync(AppUser user, CancellationToken cancellationToken) => Task.FromResult(user.Email?.ToUpperInvariant());

    public Task SetNormalizedEmailAsync(AppUser user, string? normalizedEmail, CancellationToken cancellationToken) => Task.CompletedTask;
    public void Dispose()
    {
    }
}
