# Kiến trúc hệ thống

## 1. Mục tiêu

Phân hệ 1 tập trung vào quản trị bảo mật Oracle ở mức:
- account
- role
- privilege
- metadata kiểm chứng

## 2. Kiến trúc logic

```text
WinForms UI  (Forms/)
   |
   v
OracleAdminService  (Services/)    -- thin client, chỉ gọi stored procedure
   |
   v
OracleConnectionFactory  (Data/)
   |
   v
PKG_ADMIN  (database/PROCEDURE.sql)  -- toàn bộ logic nghiệp vụ PL/SQL
   |
   v
Oracle Database
```

Toàn bộ xử lý logic (validation, sinh DDL, truy vấn metadata) nằm trong
PL/SQL package `PKG_ADMIN` trên Oracle. Lớp C# (`OracleAdminService`) chỉ
đóng vai trò thin client: truyền tham số xuống stored procedure và nhận
kết quả (`SYS_REFCURSOR`) về để hiển thị trên UI.

## 3. Thành phần source code

### Program.cs
Điểm vào của ứng dụng.

### Forms/LoginForm.cs
Form đăng nhập Oracle theo:
- host
- port
- service name
- username
- password
- tùy chọn SYSDBA

### Forms/MainForm.cs
Form chính, gồm các tab:
- Tổng quan
- Users
- Roles
- Objects demo
- Grant
- Revoke
- Tra cứu quyền

### Forms/UserDialogForm.cs
Popup tạo user / đổi mật khẩu user.

### Forms/RoleDialogForm.cs
Popup tạo role / sửa password của role.

### Services/OracleAdminService.cs
Thin client gọi stored procedure `PKG_ADMIN.*`:
- truyền tham số đã validate sơ bộ
- nhận `SYS_REFCURSOR` hoặc thực thi DDL thông qua procedure

### Data/OracleConnectionFactory.cs
Sinh kết nối Oracle.

### Utils/IdentifierValidator.cs
Kiểm tra định danh Oracle và password trước khi truyền xuống stored procedure
(validation tầng UI, defense-in-depth).

### Utils/UiHelper.cs
Các helper hiển thị message box và dựng control nhanh.

### database/PROCEDURE.sql
PL/SQL package `PKG_ADMIN` chứa toàn bộ logic:
- validate_identifier / validate_password / validate_privilege (private)
- SP_CREATE_USER, SP_ALTER_USER_PASSWORD, SP_LOCK_USER, SP_DROP_USER
- SP_CREATE_ROLE, SP_ALTER_ROLE_PASSWORD, SP_DROP_ROLE
- SP_GRANT_SYSTEM_PRIV, SP_GRANT_ROLE, SP_GRANT_OBJECT_PRIV
- SP_REVOKE_SYSTEM_PRIV, SP_REVOKE_ROLE, SP_REVOKE_OBJECT_PRIV
- SP_GET_DB_INFO, SP_GET_USERS, SP_GET_ROLES, SP_GET_USER_NAMES, SP_GET_ROLE_NAMES
- SP_GET_MANAGED_OBJECTS, SP_GET_COLUMNS
- SP_GET_PRINCIPAL_SYS_PRIVS, SP_GET_PRINCIPAL_ROLE_GRANTS
- SP_GET_PRINCIPAL_OBJ_PRIVS, SP_GET_PRINCIPAL_COL_PRIVS

## 4. Vì sao dùng kiến trúc này

- Logic nằm hoàn toàn trong PL/SQL → thể hiện rõ kiến thức Oracle/PL/SQL
  đúng mục tiêu môn ATBM HTTT.
- WinForms chỉ làm giao diện và gọi stored procedure.
- Validation 2 tầng (C# + PL/SQL) theo defense-in-depth.
- Package gom gọn, dễ giải thích với giảng viên.

## 5. Phần Oracle metadata đang dùng

- `DBA_USERS`
- `DBA_ROLES`
- `DBA_OBJECTS`
- `ALL_TAB_COLUMNS`
- `DBA_SYS_PRIVS`
- `DBA_ROLE_PRIVS`
- `DBA_TAB_PRIVS`
- `DBA_COL_PRIVS`
- `DBMS_RLS` (VPD — phân quyền SELECT mức cột)
- `APP_VPD_COL_GRANTS` (bảng metadata tùy chỉnh ghi nhận VPD policy đã tạo)

## 6. Chính sách an toàn trong app

- chỉ hỗ trợ identifier không quote
- chuẩn hóa chữ hoa để khớp Oracle
- bó mật khẩu về tập ký tự an toàn
- chỉ cho phép grant mức cột với `SELECT`, `UPDATE`
- chỉ cho phép grant mức cột trên `TABLE`, `VIEW`
- SELECT mức cột dùng VPD (`DBMS_RLS.ADD_POLICY` + `sec_relevant_cols`):
  user truy vấn trực tiếp bảng gốc, các cột không được phép trả về NULL
- UPDATE mức cột dùng cú pháp Oracle gốc: `GRANT UPDATE(col) ON ...`
- validation ở cả C# (UX nhanh) và PL/SQL (defense-in-depth)

## 7. Những gì app chưa cố làm

- chưa hỗ trợ profile, quota chi tiết, tablespace editor nâng cao
- chưa hỗ trợ object type ngoài table/view/procedure/function
- chưa hỗ trợ quoted identifier phức tạp
- chưa hỗ trợ export report PDF/Excel

Lý do: ưu tiên bám đúng yêu cầu đề và giữ app gọn để dễ demo, dễ sửa tại chỗ.
