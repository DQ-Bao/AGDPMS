using Dapper;
using System.Data;
using AGDPMS.Shared.Models;

namespace AGDPMS.Web.Data;

public class UserDataAccess(IDbConnection conn)
{
    public Task<IEnumerable<AppUser>> GetAllAsync() => conn.QueryAsync<AppUser, AppRole, AppUser>(@"
        select
            u.id as Id, u.fullname as FullName,
            u.phone as PhoneNumber,
            u.password_hash as PasswordHash,
            u.need_change_password as NeedChangePassword,
            u.email as Email, u.date_of_birth as DateOfBirth,
            u.active as IsActive,

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
            u.email as Email, u.date_of_birth as DateOfBirth,
            u.active as IsActive,

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
            u.email as Email, u.date_of_birth as DateOfBirth,
            u.active as IsActive,

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
        var id = await conn.ExecuteScalarAsync<int>(@"
            insert into users(fullname, phone, password_hash, role_id, active, need_change_password)
            values (@FullName, @PhoneNumber, @PasswordHash, @RoleId, @IsActive, @NeedChangePassword)
            returning id
        ", new
        {
            user.FullName,
            user.PhoneNumber,
            user.PasswordHash,
            RoleId = user.Role.Id,
            user.IsActive,
            user.NeedChangePassword
        });
        user.Id = id;
        return user;
    }

    public Task UpdateAsync(AppUser user) =>
        conn.ExecuteAsync(@"
            update users
            set fullname = @FullName,
                phone = @PhoneNumber,
                role_id = @RoleId,
                active = @IsActive,
                need_change_password = @NeedChangePassword,
                email = @Email,
                date_of_birth = @DateOfBirth
            where id = @Id",
            new
            {
                user.FullName,
                user.PhoneNumber,
                RoleId = user.Role.Id,
                user.IsActive,
                user.NeedChangePassword,
                user.Email,
                user.DateOfBirth,
                user.Id
            });

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

    public Task DeleteAsync(int userId) =>
        conn.ExecuteAsync(@"
            delete from users
            where id = @Id",
            new { Id = userId });
}
