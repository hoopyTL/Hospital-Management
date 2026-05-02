# Hướng Dẫn Tắt Audit - Oracle SQL

> **Lưu ý:** Tất cả các script trong hệ thống đã được tối ưu hóa cho Oracle Database

## Cách Tắt Audit

### 1. Tắt bằng SQL Script

```sql
-- Chạy script tắt audit (đã tối ưu cho Oracle)
sqlplus admin/password@XEPDB1
@05_disable_audit.sql
```

### 2. Tắt bằng C# Code

#### Tắt Standard Audit trên một bảng
```csharp
var auditService = new AuditLogService(connectionFactory);

// Tắt SELECT trên bảng hsba
auditService.DisableStandardAudit("HSBA", "SELECT");

// Tắt UPDATE trên bảng don_thuoc
auditService.DisableStandardAudit("DON_THUOC", "UPDATE");

// Tắt DELETE trên bảng hsba_dv
auditService.DisableStandardAudit("HSBA_DV", "DELETE");
```

#### Tắt tất cả Standard Audit
```csharp
auditService.DisableAllStandardAudit();
```

#### Tắt FGA Policy cụ thể
```csharp
// Tắt policy trên don_thuoc
auditService.DisableFGAPolicy("FGA_AUDIT_UPDATE_DONTHUOC", "don_thuoc");

// Tắt policy trên hsba
auditService.DisableFGAPolicy("FGA_AUDIT_ILLEGAL_UPDATE_HSBA", "hsba");

// Tắt policy trên hsba_dv
auditService.DisableFGAPolicy("FGA_AUDIT_ILLEGAL_DML_HSBA_DV", "hsba_dv");
```

#### Tắt tất cả FGA Policies
```csharp
auditService.DisableAllFGAPolicies();
```

#### Tắt Unified Audit Policy
```csharp
auditService.DisableUnifiedAuditPolicy("AUDIT_HSBA_UPDATE_CHANDOAN");
```

#### Xóa Audit Log cũ
```csharp
// Xóa audit log cũ hơn 30 ngày
auditService.ClearAuditTrail(daysToKeep: 30);

// Xóa audit log cũ hơn 7 ngày
auditService.ClearAuditTrail(daysToKeep: 7);

// Xóa tất cả audit log
auditService.ClearAllAuditTrail();
```

#### Lấy trạng thái Audit
```csharp
var status = auditService.GetAuditStatus();
// status là DataTable chứa thông tin trạng thái audit trail
```

### 3. Các Lệnh SQL Thủ Công (Oracle SQL)

#### Tắt Standard Audit
```sql
-- Tắt SELECT trên bảng hsba
NOAUDIT SELECT ON admin.hsba BY NV0021;

-- Tắt UPDATE trên bảng don_thuoc
NOAUDIT UPDATE ON admin.don_thuoc;

-- Tắt tất cả audit
NOAUDIT ALL;
```

#### Tắt FGA Policy (PL/SQL)
```sql
BEGIN
  DBMS_FGA.DROP_POLICY(
    object_schema => 'admin',
    object_name   => 'don_thuoc',
    policy_name   => 'FGA_AUDIT_UPDATE_DONTHUOC'
  );
END;
/
```

#### Tắt Unified Audit Policy
```sql
-- Sử dụng EXECUTE IMMEDIATE để tắt
BEGIN
  EXECUTE IMMEDIATE 'NOAUDIT POLICY AUDIT_HSBA_UPDATE_CHANDOAN';
  EXECUTE IMMEDIATE 'DROP AUDIT POLICY AUDIT_HSBA_UPDATE_CHANDOAN';
EXCEPTION
  WHEN OTHERS THEN
    DBMS_OUTPUT.PUT_LINE('Policy không tồn tại: ' || SQLERRM);
END;
/
```

#### Xóa Audit Log Records
```sql
-- Xóa audit log cũ hơn 30 ngày
DECLARE
  v_rows_deleted NUMBER;
BEGIN
  DELETE FROM admin.AUDIT_LOG WHERE action_timestamp < SYSDATE - 30;
  v_rows_deleted := SQL%ROWCOUNT;
  COMMIT;
  DBMS_OUTPUT.PUT_LINE(v_rows_deleted || ' bản ghi được xóa');
END;
/

-- Xóa tất cả audit log
DELETE FROM admin.AUDIT_LOG;
COMMIT;
```

## Các Tùy Chọn Tắt Audit

| Loại Audit | Cách Tắt | Ảnh Hưởng |
|-----------|---------|----------|
| Standard Audit | `NOAUDIT` | Không ghi nhận hành động nữa |
| FGA Policy | `DBMS_FGA.DROP_POLICY` | Không kiểm soát truy cập chi tiết nữa |
| Unified Audit | `EXECUTE IMMEDIATE 'NOAUDIT POLICY'` | Không ghi nhận các hành động được định nghĩa |
| Audit Trail | `DELETE FROM AUD$` | Xóa dữ liệu lịch sử |
| Audit Log | `DELETE FROM AUDIT_LOG` | Xóa dữ liệu lịch sử của ứng dụng |

## Kiểm Tra Trạng Thái

### Xem audit_trail parameter
```sql
SELECT name, value FROM v$parameter WHERE name = 'audit_trail';
```

**Kết quả có thể:**
- `DB, EXTENDED` - Audit đã bật
- `NONE` - Audit đã tắt

### Xem Standard Audit statements
```sql
SELECT owner, object_name, grantor, privilege 
FROM dba_audit_object 
WHERE owner = 'ADMIN';
```

### Xem FGA Policies
```sql
SELECT object_schema, object_name, policy_name, enabled 
FROM dba_audit_policies
WHERE object_schema = 'ADMIN';
```

### Xem Unified Audit Policies
```sql
SELECT policy_name, enabled FROM audit_unified_policies;
```

### Xem số bản ghi audit
```sql
SELECT COUNT(*) FROM admin.AUDIT_LOG;
SELECT COUNT(*) FROM DBA_AUDIT_TRAIL WHERE OWNER = 'ADMIN';
SELECT COUNT(*) FROM DBA_FGA_AUDIT_TRAIL WHERE OBJECT_SCHEMA = 'ADMIN';
```

## Oracle SQL - Lưu Ý Quan Trọng

### 1. COMMIT trong PL/SQL
```sql
-- COMMIT phải nằm trong PL/SQL block
BEGIN
  DELETE FROM admin.AUDIT_LOG WHERE ...;
  COMMIT;
END;
/
```

### 2. SQL%ROWCOUNT chỉ dùng trong PL/SQL
```sql
-- Đúng - sử dụng trong PL/SQL block
DECLARE
  v_rows NUMBER;
BEGIN
  DELETE FROM admin.AUDIT_LOG WHERE action_timestamp < SYSDATE - 30;
  v_rows := SQL%ROWCOUNT;
  DBMS_OUTPUT.PUT_LINE(v_rows || ' rows deleted');
END;
/

-- Sai - không thể dùng ngoài PL/SQL
DELETE FROM admin.AUDIT_LOG WHERE ...;
SELECT SQL%ROWCOUNT; -- LỖI!
```

### 3. DBMS_OUTPUT.PUT_LINE phải trong PL/SQL
```sql
-- Đúng
BEGIN
  DBMS_OUTPUT.PUT_LINE('Message');
END;
/

-- Sai - không thể dùng ngoài PL/SQL block
DBMS_OUTPUT.PUT_LINE('Message'); -- LỖI!
```

### 4. EXECUTE IMMEDIATE cho DDL
```sql
-- Sử dụng EXECUTE IMMEDIATE để chạy DDL statements động
BEGIN
  EXECUTE IMMEDIATE 'NOAUDIT POLICY AUDIT_HSBA_UPDATE_CHANDOAN';
  EXECUTE IMMEDIATE 'DROP AUDIT POLICY AUDIT_HSBA_UPDATE_CHANDOAN';
END;
/
```

### 5. Exception Handling
```sql
-- Luôn bao PL/SQL statement trong BEGIN...EXCEPTION...END
BEGIN
  DBMS_FGA.DROP_POLICY(
    object_schema => 'admin',
    object_name   => 'hsba',
    policy_name   => 'FGA_AUDIT_ILLEGAL_UPDATE_HSBA'
  );
EXCEPTION
  WHEN OTHERS THEN
    DBMS_OUTPUT.PUT_LINE('Lỗi: ' || SQLERRM);
END;
/
```

## Cảnh Báo Bảo Mật

⚠️ **Chú ý**: Tắt audit sẽ làm mất khả năng theo dõi các thay đổi và hành vi người dùng. Chỉ tắt audit khi thực sự cần thiết và hãy ghi lại lý do tắt.

## Quy Trình Tắt Audit An Toàn

1. **Ghi nhận**: Ghi lại lý do và thời gian tắt audit
2. **Backup**: Backup audit logs trước khi xóa
3. **Kiểm tra**: Xác nhận rằng đã tắt đúng audit cần tắt
4. **Thông báo**: Thông báo cho quản trị viên và bộ phận audit
5. **Ghi log**: Ghi lại việc tắt audit vào audit log nếu còn
6. **Khôi phục**: Chuẩn bị các bước khôi phục lại audit nếu cần

## Khôi Phục Audit

Để khôi phục audit, chạy script:
```sql
@02_audit.sql
@04_audit_log_deployment.sql
```

Hoặc sử dụng C#:
```csharp
// Chạy script SQL hoặc tạo script khôi phục riêng
```

## Troubleshooting

| Vấn Đề | Giải Pháp |
|-------|----------|
| ORA-01935: missing or invalid schema.object name | Kiểm tra tên schema và object có đúng không |
| ORA-28383: No more FGA policies for this table | Policy đã bị xóa hoặc không tồn tại |
| ORA-900: invalid SQL statement | Kiểm tra cú pháp SQL, đặc biệt DDL statements |
| PLS-00103: Encountered symbol when expecting | Thiếu `;` hoặc `/` ở cuối block |

## Tham Khảo

- [Oracle DBMS_FGA Documentation](https://docs.oracle.com/en/database/oracle/oracle-database/21/arpls/DBMS_FGA.htm)
- [Oracle Audit Trail Views](https://docs.oracle.com/en/database/oracle/oracle-database/21/refrn/audit-trail-views.html)
- [Oracle Unified Auditing](https://docs.oracle.com/en/database/oracle/oracle-database/21/dbseg/auditing-database-activity.html)
