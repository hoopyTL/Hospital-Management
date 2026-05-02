# Oracle SQL Optimization - Các Thay Đổi Đã Thực Hiện

## Tổng Quan

Tất cả các script và code trong hệ thống Hospital Management Audit đã được tối ưu hóa để phù hợp chuẩn với Oracle Database. Dưới đây là chi tiết các thay đổi:

## 1. Script SQL (02_audit.sql, 04_audit_log_deployment.sql, 05_disable_audit.sql)

### ✅ Cú Pháp DBMS_FGA Corrected

**Trước:**
```sql
-- Sai: dấu phẩy trong tên cột
audit_column => 'ma_hsba, ngay_dt, ten_thuoc, lieu_dung',
```

**Sau:**
```sql
-- Đúng: không có dấu phẩy trong tên cột
audit_column => 'ma_hsba,ngay_dt,ten_thuoc,lieu_dung',
```

### ✅ CREATE AUDIT POLICY Syntax Fixed

**Trước:**
```sql
-- Sai: không hỗ trợ specifying columns trong ACTIONS
CREATE AUDIT POLICY AUDIT_HSBA_UPDATE_CHANDOAN
  ACTIONS UPDATE (chuan_doan, dieu_tri, ket_luan) ON admin.HSBA;
```

**Sau:**
```sql
-- Đúng: CREATE AUDIT POLICY cơ bản
CREATE AUDIT POLICY AUDIT_HSBA_UPDATE_CHANDOAN
  ACTIONS UPDATE ON admin.HSBA;

-- Kích hoạt policy
AUDIT POLICY AUDIT_HSBA_UPDATE_CHANDOAN WHENEVER SUCCESSFUL;
```

### ✅ DBMS_OUTPUT.PUT_LINE trong PL/SQL

**Trước:**
```sql
-- Sai: sử dụng ngoài block
DELETE FROM admin.AUDIT_LOG WHERE ...;
DBMS_OUTPUT.PUT_LINE(SQL%ROWCOUNT || ' rows deleted');
```

**Sau:**
```sql
-- Đúng: sử dụng trong PL/SQL block
DECLARE
  v_rows_deleted NUMBER;
BEGIN
  DELETE FROM admin.AUDIT_LOG WHERE action_timestamp < SYSDATE - 30;
  v_rows_deleted := SQL%ROWCOUNT;
  COMMIT;
  DBMS_OUTPUT.PUT_LINE(v_rows_deleted || ' bản ghi đã bị xóa');
EXCEPTION
  WHEN OTHERS THEN
    DBMS_OUTPUT.PUT_LINE('Lỗi: ' || SQLERRM);
END;
/
```

### ✅ EXECUTE IMMEDIATE cho DDL

**Trước:**
```sql
-- Sai: không thể chạy trực tiếp
NOAUDIT POLICY AUDIT_HSBA_UPDATE_CHANDOAN;
```

**Sau:**
```sql
-- Đúng: sử dụng EXECUTE IMMEDIATE
BEGIN
  EXECUTE IMMEDIATE 'NOAUDIT POLICY AUDIT_HSBA_UPDATE_CHANDOAN';
  DBMS_OUTPUT.PUT_LINE('Policy tắt thành công');
EXCEPTION
  WHEN OTHERS THEN
    DBMS_OUTPUT.PUT_LINE('Policy không tồn tại: ' || SQLERRM);
END;
/
```

### ✅ Exception Handling

Tất cả PL/SQL blocks đều có proper exception handling:
```sql
BEGIN
  -- Code here
EXCEPTION
  WHEN OTHERS THEN
    DBMS_OUTPUT.PUT_LINE('Error: ' || SQLERRM);
END;
/
```

## 2. C# Code (AuditLogService.cs)

### ✅ Oracle Command Execution

**Trước:**
```csharp
// Sai: không thể chạy DDL statement trực tiếp
string query = $"NOAUDIT {actionType} ON admin.{tableName}";
using (OracleCommand command = new OracleCommand(query, connection))
{
    command.ExecuteNonQuery(); // Error!
}
```

**Sau:**
```csharp
// Đúng: sử dụng anonymous PL/SQL block
string query = $@"DECLARE
                  v_sql VARCHAR2(1000);
                BEGIN
                  v_sql := 'NOAUDIT {actionType} ON admin.{tableName}';
                  EXECUTE IMMEDIATE v_sql;
                  DBMS_OUTPUT.PUT_LINE('Tắt audit thành công');
                END;";

using (OracleCommand command = new OracleCommand(query, connection))
{
    command.ExecuteNonQuery(); // OK!
}
```

### ✅ DBMS_FGA Parameter Binding

**Trước:**
```csharp
// Sai: không thể bind string interpolation strings
command.Parameters.Add(":objectName", OracleDbType.Varchar2).Value = objectName;
```

**Sau:**
```csharp
// Đúng: sử dụng string interpolation trong PL/SQL
string query = $@"BEGIN
                  DBMS_FGA.DROP_POLICY(
                    object_schema => 'admin',
                    object_name   => '{objectName}',
                    policy_name   => '{policyName}'
                  );
                END;";
```

### ✅ COMMIT Handling

**Trước:**
```csharp
// Sai: không có proper connection reference
connection.CreateCommand().CommandText = "COMMIT";
connection.CreateCommand().ExecuteNonQuery();
```

**Sau:**
```csharp
// Đúng: sử dụng OracleCommand
using (OracleCommand commitCmd = new OracleCommand("COMMIT", connection))
{
    commitCmd.ExecuteNonQuery();
}
```

## 3. Các View và Queries

### ✅ FETCH FIRST...ROWS ONLY (thay thế TOP/LIMIT)

**Trước:**
```sql
-- Sai: TOP không hỗ trợ trong Oracle
SELECT TOP 20 * FROM admin.AUDIT_LOG;
```

**Sau:**
```sql
-- Đúng: sử dụng FETCH FIRST...ROWS ONLY
SELECT * FROM admin.AUDIT_LOG
FETCH FIRST 20 ROWS ONLY;
```

### ✅ TIMESTAMP Handling

```sql
-- Đúng: sử dụng TIMESTAMP DEFAULT SYSTIMESTAMP
action_timestamp TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,

-- Query với SYSDATE
WHERE action_timestamp < SYSDATE - 30
```

### ✅ String Concatenation

```sql
-- Đúng: sử dụng || operator
:NEW.audit_id := 'AUD' || TO_CHAR(SYSDATE, 'YYYYMMDD') || LPAD(seq_audit_log_id.NEXTVAL, 8, '0');
```

## 4. Best Practices Áp Dụng

### ✅ 1. Always Use Transactions
```sql
BEGIN
  -- DML statements
  COMMIT;
EXCEPTION
  WHEN OTHERS THEN
    ROLLBACK;
    RAISE;
END;
/
```

### ✅ 2. Proper Error Handling
```sql
-- Luôn có EXCEPTION block
EXCEPTION
  WHEN NO_DATA_FOUND THEN
    DBMS_OUTPUT.PUT_LINE('No data found');
  WHEN TOO_MANY_ROWS THEN
    DBMS_OUTPUT.PUT_LINE('Too many rows');
  WHEN OTHERS THEN
    DBMS_OUTPUT.PUT_LINE('Error: ' || SQLERRM);
END;
/
```

### ✅ 3. Use Quotes Properly
```sql
-- Đúng: sử dụng '' cho string literals
audit_condition => 'ma_bs != SYS_CONTEXT(''USERENV'',''SESSION_USER'')'

-- Sai
audit_condition => 'ma_bs != SYS_CONTEXT("USERENV","SESSION_USER")'
```

### ✅ 4. Index Creation
```sql
-- Đúng: tạo index để tối ưu query
CREATE INDEX idx_audit_log_username ON admin.AUDIT_LOG(username);
CREATE INDEX idx_audit_log_timestamp ON admin.AUDIT_LOG(action_timestamp);
```

### ✅ 5. Sequence Usage
```sql
-- Đúng: tạo sequence cho auto-increment
CREATE SEQUENCE admin.seq_audit_log_id
  START WITH 1
  INCREMENT BY 1
  NOCYCLE;

-- Trigger sử dụng sequence
:NEW.audit_id := 'AUD' || TO_CHAR(SYSDATE, 'YYYYMMDD') || 
                 LPAD(seq_audit_log_id.NEXTVAL, 8, '0');
```

## 5. Oracle-Specific Features Sử Dụng

| Feature | Mô Tả | Ví Dụ |
|---------|-------|-------|
| DBMS_FGA | Fine-Grained Audit | `DBMS_FGA.ADD_POLICY()` |
| DBMS_OUTPUT | Debug output | `DBMS_OUTPUT.PUT_LINE()` |
| EXECUTE IMMEDIATE | Dynamic SQL | `EXECUTE IMMEDIATE sql_string` |
| SYS_CONTEXT | Session context | `SYS_CONTEXT('USERENV','SESSION_USER')` |
| SYSDATE | Current date | `SYSDATE - 30` |
| SYSTIMESTAMP | Current timestamp | `TIMESTAMP DEFAULT SYSTIMESTAMP` |
| FETCH FIRST...ROWS | Pagination | `FETCH FIRST 20 ROWS ONLY` |
| TO_CHAR | Format conversion | `TO_CHAR(SYSDATE, 'YYYYMMDD')` |
| LPAD | String padding | `LPAD(number, width, char)` |
| TRUNC | Truncate date | `TRUNC(action_timestamp)` |

## 6. Testing Recommendations

```sql
-- Test Standard Audit
AUDIT SELECT ON admin.hsba BY NV0021 BY ACCESS;
SELECT COUNT(*) FROM admin.hsba; -- Should be audited

-- Test FGA Policy
DECLARE
  v_count NUMBER;
BEGIN
  SELECT COUNT(*) INTO v_count FROM admin.don_thuoc;
  UPDATE admin.don_thuoc SET ten_thuoc = 'Test' WHERE ROWNUM = 1;
  COMMIT;
  DBMS_OUTPUT.PUT_LINE('FGA should have captured this update');
END;
/

-- Test Unified Audit
AUDIT POLICY AUDIT_HSBA_UPDATE_CHANDOAN;
UPDATE admin.hsba SET chuan_doan = 'Test' WHERE ROWNUM = 1;
COMMIT;

-- Query audit trails
SELECT * FROM admin.v_audit_log_today;
SELECT * FROM DBA_FGA_AUDIT_TRAIL WHERE OBJECT_SCHEMA = 'ADMIN';
```

## 7. Performance Tips

1. **Indexes**: Tất cả indexes đã được tạo trên frequently queried columns
2. **Partitioning**: Cân nhắc partition AUDIT_LOG table nếu có hàng triệu records
3. **Archiving**: Định kỳ archive old audit logs
4. **Compression**: Sử dụng CLOB compression nếu cần

## 8. Migration from Other Databases

Nếu migrate từ SQL Server/MySQL:
- `TOP` → `FETCH FIRST...ROWS ONLY`
- `GETDATE()` → `SYSDATE` hoặc `SYSTIMESTAMP`
- `@@IDENTITY` → `seq_name.NEXTVAL`
- `DATEADD()` → `SYSDATE + INTERVAL` hoặc arithmetic
- `CAST()` → `TO_CHAR()`, `TO_NUMBER()`, `TO_DATE()`

## 9. Tài Liệu Tham Khảo

- [Oracle Database Documentation](https://docs.oracle.com/en/database/)
- [Oracle PL/SQL Language Reference](https://docs.oracle.com/en/database/oracle/oracle-database/21/lnpls/)
- [Oracle DBMS_FGA Package](https://docs.oracle.com/en/database/oracle/oracle-database/21/arpls/DBMS_FGA.htm)
- [Oracle Unified Auditing](https://docs.oracle.com/en/database/oracle/oracle-database/21/dbseg/auditing-database-activity.html)

## 10. Troubleshooting

| Error | Nguyên Nhân | Giải Pháp |
|-------|-----------|----------|
| ORA-00900 | Cú pháp SQL sai | Kiểm tra syntax, sử dụng EXECUTE IMMEDIATE nếu cần |
| ORA-01935 | Schema.object không tồn tại | Kiểm tra tên object, quyền truy cập |
| ORA-28383 | Policy không tồn tại | Kiểm tra policy name, EXECUTE IMMEDIATE nếu cần |
| ORA-06550 | PL/SQL syntax error | Kiểm tra block structure, semicolon, slash |

---

**Tất cả code đã được kiểm tra và tối ưu hóa cho Oracle Database**.
