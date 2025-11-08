# Kế hoạch triển khai Notification System cho Production Order

## 1. Tổng quan
Hệ thống thông báo sẽ tự động gửi thông báo cho các user liên quan khi có các hành động quan trọng trong luồng production order.

## 2. Database Schema

### 2.1. Bảng notifications (nếu chưa có)
```sql
CREATE TABLE IF NOT EXISTS notifications (
    id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL REFERENCES users(id),
    message TEXT NOT NULL,
    url VARCHAR(500), -- Đường dẫn khi click vào notification
    notification_type VARCHAR(50) NOT NULL, -- 'ProductionOrder', 'Stage', etc.
    related_id INTEGER, -- ID của đối tượng liên quan (order_id, stage_id, etc.)
    is_read BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT NOW(),
    CONSTRAINT fk_notifications_user FOREIGN KEY (user_id) REFERENCES users(id)
);

CREATE INDEX idx_notifications_user_read ON notifications(user_id, is_read);
CREATE INDEX idx_notifications_created_at ON notifications(created_at DESC);
```

## 3. Các hành động cần gửi notification

### 3.1. Production Order Level
1. **Order Created** (Tạo lệnh sản xuất)
   - Gửi cho: Director, Production Manager
   - Message: "Lệnh sản xuất {code} đã được tạo"
   - URL: `/production/orders/{orderId}`

2. **Order Submitted** (Gửi duyệt)
   - Gửi cho: Director
   - Message: "Lệnh sản xuất {code} đang chờ bạn duyệt"
   - URL: `/production/orders/{orderId}`

3. **Director Approved** (Giám đốc duyệt)
   - Gửi cho: Production Manager, QA
   - Message: "Lệnh sản xuất {code} đã được giám đốc duyệt"
   - URL: `/production/orders/{orderId}`

4. **Director Rejected** (Giám đốc từ chối)
   - Gửi cho: Production Manager
   - Message: "Lệnh sản xuất {code} đã bị giám đốc từ chối"
   - URL: `/production/orders/{orderId}`

5. **QA Machines Checked** (QA kiểm tra máy)
   - Gửi cho: Production Manager, QA (các QA khác)
   - Message: "QA đã xác nhận kiểm tra máy cho lệnh {code}"
   - URL: `/production/orders/{orderId}`

6. **QA Material Checked** (QA kiểm tra vật tư)
   - Gửi cho: Production Manager
   - Message: "QA đã xác nhận kiểm tra vật tư cho lệnh {code}"
   - URL: `/production/orders/{orderId}`

7. **Order Started** (Bắt đầu sản xuất)
   - Gửi cho: Production Manager, QA
   - Message: "Lệnh sản xuất {code} đã bắt đầu"
   - URL: `/production/orders/{orderId}`

8. **Order Finished** (Hoàn tất)
   - Gửi cho: Director, Production Manager
   - Message: "Lệnh sản xuất {code} đã hoàn tất"
   - URL: `/production/orders/{orderId}`

9. **Order Cancelled** (Hủy lệnh)
   - Gửi cho: Director, Production Manager
   - Message: "Lệnh sản xuất {code} đã bị hủy"
   - URL: `/production/orders/{orderId}`

### 3.2. Stage Level
1. **Stage Assigned to QA** (Giao QA)
   - Gửi cho: QA được gán
   - Message: "Bạn đã được gán kiểm tra giai đoạn {stageName} của lệnh {orderCode}"
   - URL: `/production/orders/{orderId}`

2. **Stage Approved** (Duyệt giai đoạn)
   - Gửi cho: Production Manager
   - Message: "Giai đoạn {stageName} của lệnh {orderCode} đã được duyệt"
   - URL: `/production/orders/{orderId}`

3. **Stage Rejected** (Từ chối giai đoạn)
   - Gửi cho: Production Manager
   - Message: "Giai đoạn {stageName} của lệnh {orderCode} đã bị từ chối: {reason}"
   - URL: `/production/orders/{orderId}`

4. **Stage Completed by PM** (PM hoàn thành giai đoạn)
   - Gửi cho: QA (nếu có), Production Manager
   - Message: "Giai đoạn {stageName} của lệnh {orderCode} đã được hoàn thành bởi PM"
   - URL: `/production/orders/{orderId}`

5. **Item Completed** (Hoàn thành item)
   - Gửi cho: Production Manager
   - Message: "Sản phẩm {productName} trong lệnh {orderCode} đã hoàn thành"
   - URL: `/production/orders/{orderId}`

## 4. Implementation Steps

### Step 1: Database & Models
- [ ] Tạo bảng `notifications` trong `table.sql`
- [ ] Tạo model `Notification` (nếu chưa có)
- [ ] Tạo `NotificationDataAccess` class

### Step 2: Notification Service
- [ ] Tạo `NotificationService` với các methods:
  - `CreateNotificationAsync(userId, message, url, type, relatedId)`
  - `CreateNotificationsForUsersAsync(userIds, message, url, type, relatedId)`
  - `GetUnreadCountAsync(userId)`
  - `MarkAsReadAsync(notificationId, userId)`
  - `MarkAllAsReadAsync(userId)`

### Step 3: Integration vào Production Order Flow
- [ ] Thêm notification vào `ProductionOrderService`:
  - Order Created
  - Order Submitted
  - Order Finished
  - Order Cancelled

- [ ] Thêm notification vào `ProductionOrdersEndpoints`:
  - Director Approve/Reject
  - QA Machines/Material Approve

### Step 4: Integration vào Stage Flow
- [ ] Thêm notification vào `StageService`:
  - Stage Assigned to QA
  - Stage Approved
  - Stage Rejected
  - Stage Completed by PM
  - Item Completed

### Step 5: Notification API Endpoints
- [ ] `GET /api/notifications` - Lấy danh sách notifications của user
- [ ] `GET /api/notifications/unread-count` - Lấy số lượng chưa đọc
- [ ] `PUT /api/notifications/{id}/read` - Đánh dấu đã đọc
- [ ] `PUT /api/notifications/read-all` - Đánh dấu tất cả đã đọc

### Step 6: UI Components
- [ ] Tạo `NotificationBell.razor` component (icon chuông với badge số lượng)
- [ ] Tạo `NotificationDropdown.razor` component (dropdown hiển thị notifications)
- [ ] Tạo `NotificationList.razor` page (trang xem tất cả notifications)
- [ ] Tích hợp vào `MainLayout.razor`

### Step 7: Real-time Updates (Optional - Future)
- [ ] Implement SignalR để push notifications real-time
- [ ] Update UI khi có notification mới

## 5. Code Structure

```
AGDPMS.Web/
  Data/
    NotificationDataAccess.cs
  Services/
    NotificationService.cs
  Endpoints/
    NotificationEndpoints.cs

AGDPMS.Shared/
  Models/
    Notification.cs (nếu chưa có)
  Components/
    Notifications/
      NotificationBell.razor
      NotificationDropdown.razor
      NotificationList.razor
```

## 6. Priority Implementation Order

### Phase 1 (High Priority)
1. Database schema
2. NotificationService cơ bản
3. Notification cho Order Submitted → Director
4. Notification cho Director Approved/Rejected → PM
5. Notification cho Stage Rejected → PM
6. UI Notification Bell và Dropdown

### Phase 2 (Medium Priority)
1. Notification cho tất cả các hành động còn lại
2. Notification List page
3. Mark as read functionality

### Phase 3 (Low Priority - Future)
1. Real-time notifications với SignalR
2. Email notifications
3. Notification preferences/settings

## 7. Example Code Snippets

### NotificationService.cs
```csharp
public class NotificationService(
    NotificationDataAccess notificationAccess,
    UserDataAccess userAccess)
{
    public async Task CreateNotificationAsync(int userId, string message, string? url, string type, int? relatedId)
    {
        await notificationAccess.CreateAsync(new Notification
        {
            UserId = userId,
            Message = message,
            Url = url ?? "#",
            NotificationType = type,
            RelatedId = relatedId,
            IsRead = false
        });
    }

    public async Task NotifyOrderSubmittedAsync(int orderId, string orderCode)
    {
        // Get Director users
        var directors = await userAccess.GetByRoleAsync("Director");
        foreach (var director in directors)
        {
            await CreateNotificationAsync(
                director.Id,
                $"Lệnh sản xuất {orderCode} đang chờ bạn duyệt",
                $"/production/orders/{orderId}",
                "ProductionOrder",
                orderId
            );
        }
    }
}
```

## 8. Testing Checklist
- [ ] Test tạo notification khi submit order
- [ ] Test notification hiển thị đúng cho đúng user
- [ ] Test mark as read
- [ ] Test unread count
- [ ] Test notification link navigation
- [ ] Test notification cho tất cả các hành động

