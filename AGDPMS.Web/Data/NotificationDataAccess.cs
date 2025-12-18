using AGDPMS.Shared.Models;
using Dapper;
using System.Data;

namespace AGDPMS.Web.Data;

public class NotificationDataAccess(IDbConnection conn)
{
    public async Task<Notification> CreateAsync(Notification notification, IDbTransaction? tran = null)
    {
        var (userId, role) = notification.Target.ToValues();
        var id = await conn.ExecuteScalarAsync<int>(@"
            insert into notifications(message, url, user_id, role_id)
            values (@Message, @Url, @UserId, @RoleId)
            returning id",
            new
            {
                notification.Message,
                notification.Url,
                UserId = userId,
                RoleId = role?.Id
            }, tran);
        notification.Id = id;
        return notification;
    }

    public Task<IEnumerable<Notification>> GetUserNotificationsAsync(int userId, string roleName, IDbTransaction? tran = null) =>
        conn.QueryAsync<Notification, int?, AppRole?, Notification>(@"
            select n.id as Id, n.message as Message, n.url as Url, n.created_at as Timestamp, n.is_read as IsRead,
                   n.user_id as UserId,
                   r.id as Id, r.name as Name
            from notifications n
            left join roles r ON n.role_id = r.id
            where n.user_id = @UserId or r.name = @RoleName or (n.user_id is null and n.role_id is null)
            order by n.created_at desc",
            (notification, targetUserId, role) =>
            {
                notification.Target = targetUserId.HasValue
                    ? NotificationTarget.User(targetUserId.Value)
                    : role?.Id is not null ? NotificationTarget.ForRole(role) : NotificationTarget.All();
                return notification;
            }, new { UserId = userId, RoleName = roleName }, tran, splitOn: "Id,UserId,Id");

    public Task UpdateAsync(Notification notification, IDbTransaction? tran = null) =>
        conn.ExecuteAsync(@"
            update notifications
            set message = @Message,
                url = @Url,
                is_read = @IsRead
            where id = @Id",
            new
            {
                notification.Message,
                notification.Url,
                notification.IsRead,
                notification.Id
            }, tran);
}