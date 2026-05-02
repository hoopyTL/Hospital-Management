# Hướng dẫn Audit Log trong Hospital Management

## Tổng Quan

Hệ thống Audit Log được thiết kế để theo dõi tất cả các thay đổi, triển khai và hành vi của người dùng trong ứng dụng Hospital Management. Điều này giúp:

1. **Audit Security**: Theo dõi ai đã làm gì, khi nào, và cách thế nào
2. **Compliance**: Tuân thủ các yêu cầu pháp luật về ghi nhận dữ liệu
3. **Deployment Tracking**: Ghi lại tất cả các thay đổi khi triển khai phiên bản mới
4. **Troubleshooting**: Giúp xác định nguyên nhân các sự cố

## Các Thành Phần

### 1. Model: AuditLog.cs
- Lớp C# đại diện cho một bản ghi audit
- Chứa 20+ fields để ghi lại đủ thông tin

**Trường chính:**
- `AuditId`: ID duy nhất
- `Username`: Tên người dùng
- `ActionType`: Loại hành động (INSERT, UPDATE, DELETE, SELECT, LOGIN, etc.)
- `ObjectName`: Đối tượng bị tác động (tên bảng, view, procedure)
- `OldValue/NewValue`: Giá trị cũ và mới (để tracking thay đổi)
- `Result`: SUCCESS hoặc FAILED
- `DeploymentType`: Loại triển khai (INITIAL_SETUP, PATCH, HOTFIX, FEATURE)

### 2. Database: 04_audit_log_deployment.sql

**Bảng:**
- `AUDIT_LOG`: Lưu trữ tất cả các bản ghi audit

**Stored Procedure:**
- `sp_log_audit`: Ghi một sự kiện vào audit log

**Views:**
- `v_audit_log_today`: Audit log hôm nay
- `v_audit_log_data_changes`: Các thay đổi dữ liệu
- `v_audit_log_errors`: Các lỗi đã xảy ra
- `v_audit_log_deployment`: Audit log triển khai
- `v_audit_log_summary`: Thống kê

### 3. Service: AuditLogService.cs
- Lớp C# để giao tiếp với database
- Các hàm tiện lợi để ghi và đọc audit logs

## Cách Sử Dụng

### Bước 1: Chạy script SQL

```sql
-- Đăng nhập bằng ADMIN
sqlplus admin/password@XEPDB1

-- Chạy script để tạo bảng, procedure, views
@04_audit_log_deployment.sql
```

### Bước 2: Sử dụng trong C#

#### Ghi một hành động login
```csharp
// Tạo service
var auditService = new AuditLogService(connectionFactory);

// Ghi audit log cho login
auditService.LogLogin("NV_01", "Nguyễn Văn A", "192.168.1.100", true);
```

#### Ghi một hành động INSERT
```csharp
auditService.LogInsert(
    username: "NV_01",
    fullName: "Nguyễn Văn A",
    objectName: "HSBA",
    recordId: "HS001",
    newValue: "Chẩn đoán: Cảm cúm"
);
```

#### Ghi một hành động UPDATE
```csharp
auditService.LogUpdate(
    username: "BAC_SI",
    fullName: "Trần Bác Sĩ",
    objectName: "HSBA",
    recordId: "HS001",
    oldValue: "Chẩn đoán: Cảm cúm",
    newValue: "Chẩn đoán: Cảm cúm, sốt cao"
);
```

#### Ghi một hành động DELETE
```csharp
auditService.LogDelete(
    username: "NV_01",
    fullName: "Nguyễn Văn A",
    objectName: "DON_THUOC",
    recordId: "DT001",
    oldValue: "Aspirin 500mg"
);
```

#### Ghi audit log triển khai
```csharp
auditService.LogDeployment(
    deploymentType: "HOTFIX",
    applicationVersion: "1.2.1",
    description: "Fix bug xóa đơn thuốc",
    username: "ADMIN",
    fullName: "Administrator"
);
```

#### Lấy audit log hôm nay
```csharp
var todayLogs = auditService.GetTodayAuditLogs();
foreach (var log in todayLogs)
{
    Console.WriteLine($"{log.ActionTimestamp} - {log.Username} - {log.ActionType} - {log.ObjectName}");
}
```

#### Lấy các thay đổi dữ liệu
```csharp
var changes = auditService.GetDataChanges(limit: 50);
```

#### Lấy các lỗi
```csharp
var errors = auditService.GetErrors();
foreach (var error in errors)
{
    Console.WriteLine($"{error.Username} - {error.ErrorMessage}");
}
```

#### Lấy audit log của một user
```csharp
var userLogs = auditService.GetAuditLogByUser("NV_01", limit: 20);
```

#### Lấy lịch sử đăng nhập
```csharp
var loginHistory = auditService.GetLoginHistory(limit: 50);
foreach (var login in loginHistory)
{
    Console.WriteLine($"{login.ActionTimestamp} - {login.Username} - {login.Result} - {login.IpAddress}");
}
```

#### Lấy thông tin triển khai
```csharp
var deploymentInfo = auditService.GetDeploymentInfo();
// deploymentInfo là DataTable chứa thông tin triển khai
```

#### Lấy thống kê
```csharp
var statistics = auditService.GetAuditStatistics();
// statistics là DataTable chứa thống kê
```

## Các Loại ActionType

| ActionType | Mô Tả |
|-----------|-------|
| LOGIN | Đăng nhập vào ứng dụng |
| LOGOUT | Đăng xuất khỏi ứng dụng |
| INSERT | Thêm mới dữ liệu |
| UPDATE | Cập nhật dữ liệu |
| DELETE | Xóa dữ liệu |
| SELECT | Truy vấn dữ liệu |
| EXECUTE | Thực thi stored procedure |
| DEPLOYMENT | Triển khai phiên bản mới |

## Các Loại DeploymentType

| DeploymentType | Mô Tả |
|---|---|
| INITIAL_SETUP | Thiết lập ban đầu |
| PATCH | Bản vá lỗi nhỏ |
| HOTFIX | Bản sửa lỗi khẩn cấp |
| FEATURE | Tính năng mới |
| DATA_MIGRATION | Di chuyển dữ liệu |

## Queries SQL hữu ích

### 1. Xem toàn bộ audit log hôm nay
```sql
SELECT * FROM admin.v_audit_log_today;
```

### 2. Xem các thay đổi trên bảng HSBA
```sql
SELECT audit_id, username, action_type, record_id, old_value, new_value, action_timestamp
FROM admin.AUDIT_LOG
WHERE object_name = 'HSBA' AND action_type IN ('UPDATE', 'DELETE')
ORDER BY action_timestamp DESC;
```

### 3. Xem các lỗi
```sql
SELECT * FROM admin.v_audit_log_errors;
```

### 4. Xem hoạt động của user NV_01
```sql
SELECT audit_id, username, action_type, object_name, result, action_timestamp
FROM admin.AUDIT_LOG
WHERE username = 'NV_01'
ORDER BY action_timestamp DESC;
```

### 5. Xem lịch sử đăng nhập
```sql
SELECT audit_id, username, result, action_timestamp, ip_address
FROM admin.AUDIT_LOG
WHERE action_type = 'LOGIN'
ORDER BY action_timestamp DESC;
```

### 6. Xem timeline triển khai
```sql
SELECT deployment_type, application_version, deployment_description,
       COUNT(*) as so_thao_tac, MIN(action_timestamp) as bat_dau,
       MAX(action_timestamp) as ket_thuc
FROM admin.AUDIT_LOG
WHERE deployment_type IS NOT NULL
GROUP BY deployment_type, application_version, deployment_description
ORDER BY MIN(action_timestamp) DESC;
```

### 7. Thống kê theo ActionType
```sql
SELECT * FROM admin.v_audit_log_summary;
```

### 8. Xem các thay đổi trong 24 giờ qua
```sql
SELECT audit_id, username, action_type, object_name, result, action_timestamp
FROM admin.AUDIT_LOG
WHERE action_timestamp >= SYSDATE - 1
ORDER BY action_timestamp DESC;
```

## Tích Hợp vào Ứng Dụng

### Ở LoginForm.cs
```csharp
// Sau khi đăng nhập thành công
var auditService = new AuditLogService(connectionFactory);
auditService.LogLogin(username, fullName, GetClientIpAddress(), true);
```

### Ở BenhNhanForm.cs (khi thêm/sửa/xóa)
```csharp
// Khi thêm bệnh nhân
auditService.LogInsert(currentUser, currentUserName, "BENHNHAN", benhNhan.MaBN, benhNhan.ToString());

// Khi cập nhật bệnh nhân
auditService.LogUpdate(currentUser, currentUserName, "BENHNHAN", benhNhan.MaBN, oldValue, newValue);

// Khi xóa bệnh nhân
auditService.LogDelete(currentUser, currentUserName, "BENHNHAN", benhNhan.MaBN, oldValue);
```

### Ở AdminMainForm.cs (hiển thị audit dashboard)
```csharp
// Hiển thị audit log hôm nay
var auditLogs = auditService.GetTodayAuditLogs();
dataGridViewAudit.DataSource = auditLogs;

// Hiển thị thống kê
var stats = auditService.GetAuditStatistics();
dataGridViewStats.DataSource = stats;
```

## Bảo Mật

1. **Quyền truy cập**: Chỉ ADMIN hoặc các user có quyền mới được đọc audit logs
2. **Mã hóa**: Dữ liệu nhạy cảm trong OldValue/NewValue nên được mã hóa
3. **Archiving**: Nên archive audit logs cũ sau một khoảng thời gian

## Troubleshooting

### Lỗi: "Object does not exist"
- Chắc chắn rằng bạn đã chạy script 04_audit_log_deployment.sql
- Kiểm tra quyền của user ADMIN

### Lỗi: "Procedure does not exist"
- Chạy lại script để tạo stored procedure

### Performance chậm
- Xóa các audit logs cũ không cần thiết
- Thêm index trên các column thường xuyên query

## Liên Hệ

Nếu có câu hỏi hoặc vấn đề, vui lòng liên hệ với đội phát triển.
