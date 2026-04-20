# Phân hệ 1 - Ứng dụng quản trị CSDL Oracle

Bộ này được đóng gói để phục vụ riêng cho **Phân hệ 1** trong đề đồ án ATBM HTTT:

- quản lý user
- quản lý role
- grant / revoke
- xem quyền của user / role trên các đối tượng dữ liệu Oracle
- hỗ trợ grant mức object và mức cột cho `SELECT`, `UPDATE`

## 1. Cấu trúc thư mục

```text
ATBM-2026-Subsystem1-OracleAdmin/
│
├─ src/
│  └─ OracleAdminWinForms/       # source code WinForms C#
│
├─ database/
│  ├─ 00_bootstrap_demo.sql      # script dựng môi trường demo đầy đủ
│  ├─ PROCEDURE.sql              # PKG_ADMIN - toàn bộ logic nghiệp vụ PL/SQL
│  ├─ 10_verify_demo.sql         # kiểm tra dữ liệu / object / privilege
│  ├─ 20_required_grants_if_not_using_DBA.sql
│  ├─ 30_demo_scenarios.sql      # các lệnh demo nhanh
│  └─ 99_cleanup_demo.sql        # dọn môi trường demo
│
├─ docs/
│  ├─ HUONG_DAN_CHAY.md
│  ├─ KICH_BAN_DEMO.md
│  └─ KIEN_TRUC_HE_THONG.md
│
└─ sample-data/
   ├─ departments.csv
   └─ employees.csv
```

## 2. Công nghệ được dùng

- WinForms C# (.NET 8, `net8.0-windows`) — chỉ làm giao diện (thin client)
- Oracle Managed Data Provider for .NET (`Oracle.ManagedDataAccess.Core`)
- **PL/SQL package `PKG_ADMIN`** (`database/PROCEDURE.sql`) — toàn bộ logic nghiệp vụ
- Oracle Database (khuyến nghị Oracle XE hoặc Oracle lab instance)

## 3. Tài khoản demo mặc định sau khi chạy bootstrap

- App admin: `ATBM_ADMIN / Admin#12345`
- Object owner: `LAB_OWNER / Lab#12345`

Các tài khoản test thêm:

- `DEV_A / Dev#12345`
- `DEV_B / Dev#12345`
- `APP_USER1 / App#12345`
- `APP_USER2 / App#12345`

Roles test:

- `RL_READONLY`
- `RL_ANALYST`
- `RL_PROGRAM`

## 4. Tính năng đã có trong app

### Tab Tổng quan

- kiểm tra user phiên làm việc
- DB name
- thời gian server
- banner version

### Tab Users

- xem danh sách user
- tìm kiếm user
- tạo user
- đổi mật khẩu user
- khóa / mở khóa user
- drop user

### Tab Roles

- xem danh sách role
- tìm kiếm role
- tạo role
- sửa role theo hướng thay / bỏ password
- drop role

### Tab Objects demo

- xem danh sách table / view / procedure / function
- lọc theo owner

### Tab Grant

- grant system privilege
- grant object privilege
- grant role
- chọn `WITH ADMIN OPTION` / `WITH GRANT OPTION`
- grant mức cột cho `SELECT`, `UPDATE`

### Tab Revoke

- revoke system privilege
- revoke object privilege
- revoke role
- thu hồi mức cột cho `SELECT`, `UPDATE`

### Tab Tra cứu quyền

- xem `DBA_SYS_PRIVS`
- xem `DBA_ROLE_PRIVS`
- xem `DBA_TAB_PRIVS`
- xem `DBA_COL_PRIVS`
- hiển thị cả quyền cấp trực tiếp (`DIRECT`) và quyền nhận qua role (`VIA_ROLE`)

## 5. Lưu ý kỹ thuật

1. App giả định người đăng nhập có quyền quản trị đủ mạnh trên Oracle.
2. App đang ưu tiên **Oracle identifier không dùng dấu nháy kép**, tức là:
   - username dạng `DEV_A`
   - role dạng `RL_READONLY`
   - object dạng `LAB_OWNER.EMPLOYEES`
3. Mật khẩu được giới hạn ở bộ ký tự an toàn để tránh lỗi DDL.
4. Với Oracle, cơ chế “cho phép cấp tiếp” không hoàn toàn cùng một cú pháp:
   - system privilege / role grant dùng bản chất `WITH ADMIN OPTION`
   - object privilege dùng `WITH GRANT OPTION`
     Trong app, checkbox đã gom chung để thao tác tiện hơn.

## 6. Thứ tự chạy khuyến nghị

1. Cài Oracle DB
2. Chạy `database/00_bootstrap_demo.sql` bằng `SYS` hoặc tài khoản tương đương
3. Kết nối bằng `ATBM_ADMIN / Admin#12345` và chạy `database/PROCEDURE.sql`
4. Mở solution trong Visual Studio
5. Restore NuGet
6. Build và Run
7. Đăng nhập bằng `ATBM_ADMIN / Admin#12345`

## 7. Phân hệ chủ yếu focus vào 4 nhóm chức năng

- **Quản trị định danh**: user / role
- **Cấp phát quyền**: system, object, role
- **Thu hồi quyền**
- **Tra cứu và kiểm chứng quyền**

## 8. Flow demo

- tạo mới 1 user
- tạo mới 1 role
- grant role cho user
- grant `SELECT` mức cột trên `LAB_OWNER.EMPLOYEES`
- grant `EXECUTE` cho procedure / function
- revoke lại
- mở tab tra cứu quyền để chứng minh kết quả
