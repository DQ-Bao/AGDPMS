using Dapper;
using System.Data;

namespace AGDPMS.Web.Data;
public class UserDataAccess(IDbConnection conn)
{
    public Task<IEnumerable<AppUser>> GetAllAsync() => conn.QueryAsync<AppUser, AppRole, AppUser>(@"
        select
            u.id as Id, u.fullname as FullName,
            u.phone as PhoneNumber,
            u.password_hash as PasswordHash,
            u.need_change_password as NeedChangePassword,

            r.id as Id, r.name as Name
        from users u
        join roles r on u.role_id = r.id",
        (user, role) =>
        {
            user.Role = role;
            return user;
        });

    public async Task<AppUser?> GetByIdAsync(int id) => (await conn.QueryAsync<AppUser, AppRole, AppUser>(@"
        select
            u.id as Id, u.fullname as FullName,
            u.phone as PhoneNumber,
            u.password_hash as PasswordHash,
            u.need_change_password as NeedChangePassword,

            r.id as Id, r.name as Name
        from users u
        join roles r on u.role_id = r.id
        where u.id = @Id",
        (user, role) =>
        {
            user.Role = role;
            return user;
        },
        new { Id = id })).FirstOrDefault();

    public async Task<AppUser?> GetByPhoneNumberAsync(string phone) => (await conn.QueryAsync<AppUser, AppRole, AppUser>(@"
        select
            u.id as Id, u.fullname as FullName,
            u.phone as PhoneNumber,
            u.password_hash as PasswordHash,
            u.need_change_password as NeedChangePassword,

            r.id as Id, r.name as Name
        from users u
        join roles r on u.role_id = r.id
        where u.phone = @Phone",
        (user, role) =>
        {
            user.Role = role;
            return user;
        },
        new { Phone = phone })).FirstOrDefault();

    public async Task<AppUser> CreateAsync(AppUser user)
    {
        var roleId = await conn.ExecuteScalarAsync<int?>("select id from roles where name = @Name", new { user.Role.Name })
            ?? throw new InvalidOperationException($"Role '{user.Role.Name}' doesn't exist.");
        
        var id = await conn.ExecuteScalarAsync<int>(@"
            insert into users(fullname, phone, password_hash, role_id, need_change_password)
            values (@FullName, @PhoneNumber, @PasswordHash, @RoleId, @NeedChangePassword)
            returning id
        ", new
        {
            user.FullName,
            user.PhoneNumber,
            user.PasswordHash,
            RoleId = roleId,
            user.NeedChangePassword
        });
        user.Id = id;
        user.Role.Id = roleId;
        return user;
    }

    public Task SetPasswordHashAsync(int userId, string passwordHash, bool? needChange) =>
        conn.ExecuteAsync(@"
            update users
            set password_hash = @PasswordHash,
                need_change_password = @NeedChangePassword
            where id = @Id",
            new
            {
                PasswordHash = passwordHash,
                NeedChangePassword = needChange ?? false,
                Id = userId
            });
}
