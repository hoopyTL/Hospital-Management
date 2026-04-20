# Hướng dẫn chạy

## A. Chuẩn bị môi trường

### Bước 1. Cài Oracle Database
Bạn có thể dùng:
- Oracle Database XE trên máy cá nhân
- hoặc Oracle instance do phòng lab cung cấp

### Bước 2. Cài công cụ chạy script SQL
Chọn một trong hai:
- Oracle SQL Developer
- Oracle SQLcl

### Bước 3. Cài Visual Studio + .NET workload
Cần mở được project WinForms .NET 8.

### Bước 4. Mở project
Mở file:

```text
src/OracleAdminWinForms/OracleAdminWinForms.csproj
```

## B. Dựng dữ liệu demo

### Cách 1: chạy bằng SQL Developer

1. Đăng nhập bằng `SYS` hoặc user có quyền tương đương
2. Mở file:

```text
database/00_bootstrap_demo.sql
```

3. Run script toàn bộ

### Cách 2: chạy bằng SQLcl

```sql
sql sys/<your_password>@localhost:1521/XEPDB1 as sysdba
@database/00_bootstrap_demo.sql
```

### Bước bắt buộc: tạo package PKG_ADMIN

Sau khi bootstrap xong, kết nối bằng `ATBM_ADMIN` và chạy `PROCEDURE.sql`:

Bằng SQL Developer:
1. Đăng nhập bằng `ATBM_ADMIN / Admin#12345`
2. Mở file `database/PROCEDURE.sql`
3. Run script toàn bộ

Bằng SQLcl:

```sql
sql ATBM_ADMIN/"Admin#12345"@localhost:1521/XEPDB1
@database/PROCEDURE.sql
```

Nếu thành công, bạn sẽ thấy: `PKG_ADMIN da duoc tao thanh cong.`

## C. Chạy ứng dụng

### Cách 1: Chạy trực tiếp bằng file .exe

File thực thi nằm tại:

```text
publish/OracleAdminWinForms.exe
```

Yêu cầu: máy đã cài **.NET 8 Desktop Runtime** (tải tại https://dotnet.microsoft.com/download/dotnet/8.0).

### Cách 2: Build từ source bằng Visual Studio

1. Mở file `ATBM-2026-Subsystem1-OracleAdmin.sln`
2. Restore NuGet packages
3. Build solution (Ctrl+Shift+B)
4. Run (F5)

### Cách 3: Build từ terminal

```bash
dotnet restore
dotnet build
dotnet run --project src/OracleAdminWinForms
```

### Tự tạo lại file .exe

```bash
dotnet publish src/OracleAdminWinForms/OracleAdminWinForms.csproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o publish
```

## D. Thông tin đăng nhập app

- Host: `localhost`
- Port: `1521`
- Service name: `XEPDB1`
- Username: `ATBM_ADMIN`
- Password: `Admin#12345`

Nếu máy bạn dùng service name khác, đổi lại ở màn hình login.

## E. Kiểm tra nhanh sau bootstrap

Chạy:

```text
database/10_verify_demo.sql
```

Nếu đúng, bạn sẽ thấy:
- các user demo đã có
- các role demo đã có
- schema `LAB_OWNER` có table, view, procedure, function
- một số grant nền đã tồn tại

## F. Chức năng nào map với đề bài?

### 1. Tạo / xóa / sửa user hoặc role
- Tab `Users`
- Tab `Roles`

### 2. Xem danh sách user / role
- Tab `Users`
- Tab `Roles`

### 3. Cấp quyền
- Tab `Grant`
- 3 mode:
  - system privilege
  - object privilege
  - role

### 4. Thu hồi quyền
- Tab `Revoke`

### 5. Xem thông tin quyền của mỗi user / role
- Tab `Tra cứu quyền`

## G. Tình huống demo đẹp

### Demo 1: tạo user và role mới
- tạo `TEST_USER1`
- tạo `TEST_ROLE1`

### Demo 2: grant role cho user
- grant `TEST_ROLE1` cho `TEST_USER1`

### Demo 3: grant object privilege mức bảng
- grant `SELECT` trên `LAB_OWNER.VW_EMP_SUMMARY` cho `TEST_USER1`

### Demo 4: grant object privilege mức cột
- grant `SELECT(FULL_NAME, SALARY)` trên `LAB_OWNER.EMPLOYEES` cho `TEST_USER1`

### Demo 5: grant execute
- grant `EXECUTE` trên `LAB_OWNER.PR_RAISE_SALARY` cho `TEST_USER1`

### Demo 6: revoke và kiểm tra lại
- revoke role
- revoke object privilege
- sang tab `Tra cứu quyền` để chứng minh

## H. Lỗi thường gặp

### ORA-01017
Sai username / password hoặc service name.

### ORA-01031
Tài khoản đang đăng nhập app không đủ quyền DBA / dictionary.

### ORA-01950
User chưa có quota trên tablespace.

### ORA-00942
Object owner / object name nhập sai hoặc object chưa tồn tại.

### PLS-00201 hoặc ORA-04063
Package `PKG_ADMIN` chưa được tạo hoặc tạo không thành công.
Chạy lại `PROCEDURE.sql` bằng tài khoản `ATBM_ADMIN`.

### Không thấy danh sách user / role / privilege
Tài khoản đang dùng app không có quyền xem `DBA_*` views.

## I. Dọn môi trường demo

Khi cần làm sạch:

```text
database/99_cleanup_demo.sql
```
