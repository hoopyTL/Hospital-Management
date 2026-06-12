# 📘 API Reference – Hospital Management

> Tài liệu tham chiếu chi tiết các class, method và data model trong source code.

---

## Mục lục

- [1. DataAccess Layer](#1-dataaccess-layer)
  - [OracleHelper](#oraclehelper)
  - [OracleConnectionFactory](#oracleconnectionfactory)
- [2. Services Layer](#2-services-layer)
  - [OracleAdminService](#oracleadminservice)
  - [AuditLogService](#auditlogservice)
- [3. Models](#3-models)
- [4. Helpers](#4-helpers)
  - [UIHelper](#uihelper)
  - [IdentifierValidator](#identifiervalidator)
- [5. Forms](#5-forms)

---

## 1. DataAccess Layer

### OracleHelper

> **File**: [`OracleHelper.cs`](../ATBM2026-ATBM-06/01-ATBM-06-SourceCode/HospitalManagement.App/DataAccess/OracleHelper.cs)
>
> **Pattern**: Singleton – `OracleHelper.Instance`
>
> **Mục đích**: Quản lý kết nối Oracle và cung cấp các phương thức thực thi SQL cho Phân hệ 2 (DPV, BS, KTV, BN).

#### `Initialize`

Khởi tạo singleton mới, dispose instance cũ nếu có.

```csharp
public static OracleHelper Initialize(
    string username,
    string password,
    string host = "localhost",
    int port = 1521,
    string serviceName = "XEPDB1"
)
```

| Tham số | Kiểu | Bắt buộc | Mô tả |
|---------|------|----------|-------|
| `username` | string | ✅ | Oracle username (sẽ uppercase) |
| `password` | string | ✅ | Oracle password |
| `host` | string | — | DB host | 
| `port` | int | — | Listener port |
| `serviceName` | string | — | PDB service name |

**Returns**: `OracleHelper` – instance mới

---

#### `GetConnection`

Mở connection nếu chưa mở, trả về connection hiện tại.

```csharp
public OracleConnection GetConnection()
```

**Returns**: `OracleConnection` – connection đang mở

---

#### `TestConnection`

Test kết nối Oracle. Tạo connection tạm, mở và đóng.

```csharp
public bool TestConnection()
```

**Returns**: `true` nếu kết nối thành công

---

#### `ExecuteQuery`

Thực thi SELECT query, trả về `DataTable`.

```csharp
public DataTable ExecuteQuery(string sql, params OracleParameter[] parameters)
```

| Tham số | Kiểu | Mô tả |
|---------|------|-------|
| `sql` | string | Câu lệnh SELECT |
| `parameters` | OracleParameter[] | Bind parameters |

**Returns**: `DataTable` – kết quả query

**Ví dụ**:
```csharp
var dt = OracleHelper.Instance.ExecuteQuery(
    "SELECT * FROM ADMIN.NHAN_VIEN WHERE MA_NV = :id",
    new OracleParameter("id", "NV0001")
);
```

---

#### `ExecuteNonQuery`

Thực thi INSERT / UPDATE / DELETE.

```csharp
public int ExecuteNonQuery(string sql, params OracleParameter[] parameters)
```

**Returns**: `int` – số dòng bị ảnh hưởng

---

#### `ExecuteScalar`

Trả về giá trị đơn (first row, first column).

```csharp
public object ExecuteScalar(string sql, params OracleParameter[] parameters)
```

---

#### `HasRole`

Kiểm tra user hiện tại có role cụ thể không.

```csharp
public bool HasRole(string roleName)
```

**Ví dụ**:
```csharp
if (OracleHelper.Instance.HasRole("RL_BACSI"))
{
    // Mở BacSiForm
}
```

---

#### `GetUserRoles`

Lấy toàn bộ roles được gán cho user hiện tại.

```csharp
public DataTable GetUserRoles()
```

---

#### `AllocNextMaBenhNhan`

Sinh mã bệnh nhân kế tiếp theo pattern `BN######`.

```csharp
public string AllocNextMaBenhNhan()
```

**Returns**: `string` – Ví dụ: `"BN000042"`

---

#### `ParamNvarchar2`

Tạo `OracleParameter` kiểu `NVarchar2` để giữ Unicode (dấu tiếng Việt).

```csharp
public static OracleParameter ParamNvarchar2(string name, object value)
```

> ⚠️ **Quan trọng**: Nếu dùng `OracleDbType.Varchar2` mặc định, ký tự Unicode có thể bị mất dấu → hiện `?`.

---

### OracleConnectionFactory

> **File**: [`OracleConnectionFactory.cs`](../ATBM2026-ATBM-06/01-ATBM-06-SourceCode/HospitalManagement.App/DataAccess/OracleConnectionFactory.cs)
>
> **Pattern**: Factory
>
> **Mục đích**: Tạo `OracleConnection` từ `DbConnectionSettings`. Dùng bởi `OracleAdminService` (Phân hệ 1).

```csharp
public OracleConnection CreateOpenConnection()
```

---

## 2. Services Layer

### OracleAdminService

> **File**: [`OracleAdminService.cs`](../ATBM2026-ATBM-06/01-ATBM-06-SourceCode/HospitalManagement.App/Services/OracleAdminService.cs)
>
> **Mục đích**: Proxy an toàn tới package `PKG_ADMIN` (stored procedures). Chỉ cho phép đăng nhập bằng `ATBM_ADMIN`.

#### Constructor

```csharp
public OracleAdminService(DbConnectionSettings settings)
```

> **Throws** `UnauthorizedAccessException` nếu `settings.Username != "ATBM_ADMIN"`.

---

#### Queries

| Method | Stored Procedure | Mô tả |
|--------|-----------------|-------|
| `GetDatabaseInfo()` | `PKG_ADMIN.SP_GET_DB_INFO` | Thông tin database (version, instance, etc.) |
| `GetUsers(keyword?)` | `PKG_ADMIN.SP_GET_USERS` | Danh sách user Oracle, tìm kiếm theo keyword |
| `GetRoles(keyword?)` | `PKG_ADMIN.SP_GET_ROLES` | Danh sách role Oracle |
| `GetUserNames()` | `PKG_ADMIN.SP_GET_USER_NAMES` | Danh sách tên user (dropdown) |
| `GetRoleNames()` | `PKG_ADMIN.SP_GET_ROLE_NAMES` | Danh sách tên role (dropdown) |
| `GetManagedObjects(owner?)` | `PKG_ADMIN.SP_GET_MANAGED_OBJECTS` | Danh sách object (table, view, procedure) |
| `GetColumns(owner, objectName)` | `PKG_ADMIN.SP_GET_COLUMNS` | Danh sách cột của table/view |
| `GetPrincipalSystemPrivileges(principal)` | `PKG_ADMIN.SP_GET_PRINCIPAL_SYS_PRIVS` | System privileges của user/role |
| `GetPrincipalRoleGrants(principal)` | `PKG_ADMIN.SP_GET_PRINCIPAL_ROLE_GRANTS` | Roles được gán cho user/role |
| `GetPrincipalObjectPrivileges(principal)` | `PKG_ADMIN.SP_GET_PRINCIPAL_OBJ_PRIVS` | Object-level privileges |
| `GetPrincipalColumnPrivileges(principal)` | `PKG_ADMIN.SP_GET_PRINCIPAL_COL_PRIVS` | Column-level privileges |
| `GetTablespaces()` | `PKG_ADMIN.SP_GET_TABLESPACES` | Danh sách tablespace |
| `GetProfiles()` | `PKG_ADMIN.SP_GET_PROFILES` | Danh sách profile |

---

#### User Management

| Method | Tham số | Stored Procedure |
|--------|---------|-----------------|
| `CreateUser(username, password, defaultTs?, tempTs?, quota?)` | username, password bắt buộc | `PKG_ADMIN.SP_CREATE_USER` |
| `AlterUserPassword(username, newPassword)` | | `PKG_ADMIN.SP_ALTER_USER_PASSWORD` |
| `AlterUserDefaultTablespace(username, defaultTs)` | | `PKG_ADMIN.SP_ALTER_USER_DEFAULT_TS` |
| `AlterUserTemporaryTablespace(username, tempTs)` | | `PKG_ADMIN.SP_ALTER_USER_TEMP_TS` |
| `AlterUserProfile(username, profile)` | | `PKG_ADMIN.SP_ALTER_USER_PROFILE` |
| `LockUser(username, lockUser)` | `lockUser`: true=lock, false=unlock | `PKG_ADMIN.SP_LOCK_USER` |
| `DropUser(username, cascade)` | `cascade`: true=CASCADE | `PKG_ADMIN.SP_DROP_USER` |

---

#### Role Management

| Method | Tham số | Stored Procedure |
|--------|---------|-----------------|
| `CreateRole(roleName, password?)` | password tuỳ chọn | `PKG_ADMIN.SP_CREATE_ROLE` |
| `AlterRolePassword(roleName, password?)` | null = NO AUTHENTICATION | `PKG_ADMIN.SP_ALTER_ROLE_PASSWORD` |
| `DropRole(roleName)` | | `PKG_ADMIN.SP_DROP_ROLE` |

---

#### Grant / Revoke

| Method | Mô tả |
|--------|-------|
| `GrantSystemPrivilege(principal, privilege, withAdminOption)` | Cấp quyền hệ thống |
| `GrantRole(roleName, principal, withAdminOption)` | Gán role cho user/role |
| `GrantObjectPrivilege(principal, owner, objectName, objectType, privilege, columns?, withGrantOption)` | Cấp quyền trên object, có thể giới hạn theo cột |
| `RevokeSystemPrivilege(principal, privilege)` | Thu hồi quyền hệ thống |
| `RevokeRole(roleName, principal)` | Bỏ role |
| `RevokeObjectPrivilege(principal, owner, objectName, objectType, privilege, columns?)` | Thu hồi quyền object |

**System Privileges hỗ trợ**:
```
CREATE SESSION, CREATE TABLE, CREATE VIEW, CREATE PROCEDURE,
CREATE ROLE, ALTER USER, DROP USER, SELECT ANY TABLE,
INSERT ANY TABLE, UPDATE ANY TABLE, DELETE ANY TABLE,
EXECUTE ANY PROCEDURE, UNLIMITED TABLESPACE
```

**Object Privileges**:
- Table/View: `SELECT`, `INSERT`, `UPDATE`, `DELETE`
- Procedure/Function/Package: `EXECUTE`

---

### AuditLogService

> **File**: [`AuditLogService.cs`](../ATBM2026-ATBM-06/01-ATBM-06-SourceCode/HospitalManagement.App/Services/AuditLogService.cs)
>
> **Mục đích**: Tra cứu audit log (kết quả xét nghiệm bị thay đổi).

---

## 3. Models

### NhanVien (Nhân viên)

> **File**: [`NhanVien.cs`](../ATBM2026-ATBM-06/01-ATBM-06-SourceCode/HospitalManagement.App/Models/NhanVien.cs)

| Property | Type | Mô tả |
|----------|------|-------|
| `MaNV` | string | Mã nhân viên (`NV0001`–`NV0170`) |
| `HoTen` | string | Họ và tên |
| `Phai` | string | Giới tính |
| `NgaySinh` | DateTime | Ngày sinh |
| `CMND` | string | Số CMND/CCCD |
| `QueQuan` | string | Quê quán |
| `SDT` | string | Số điện thoại |
| `VaiTro` | string | Vai trò: DPV / BS / KTV |
| `ChuyenKhoa` | string | Chuyên khoa (nếu BS) |

### BenhNhan (Bệnh nhân)

> **File**: [`BenhNhan.cs`](../ATBM2026-ATBM-06/01-ATBM-06-SourceCode/HospitalManagement.App/Models/BenhNhan.cs)

| Property | Type | Mô tả |
|----------|------|-------|
| `MaBN` | string | Mã bệnh nhân (`BN000001`–`BN100000`) |
| `TenBN` | string | Tên bệnh nhân |
| `Phai` | string | Giới tính |
| `NgaySinh` | DateTime | Ngày sinh |
| `CCCD` | string | Căn cước công dân |
| `SoNha` | string | Số nhà |
| `TenDuong` | string | Tên đường |
| `QuanHuyen` | string | Quận/Huyện |
| `TinhTP` | string | Tỉnh/Thành phố |
| `TienSuBenh` | string | Tiền sử bệnh |
| `TienSuBenhGD` | string | Tiền sử bệnh gia đình |
| `DiUngThuoc` | string | Dị ứng thuốc |

### Hsba (Hồ sơ bệnh án)

> **File**: [`Hsba.cs`](../ATBM2026-ATBM-06/01-ATBM-06-SourceCode/HospitalManagement.App/Models/Hsba.cs)

| Property | Type | Mô tả |
|----------|------|-------|
| `MaHsba` | string | Mã HSBA |
| `MaBN` | string | FK → BenhNhan |
| `Ngay` | DateTime? | Ngày tạo |
| `ChuanDoan` | string | Chẩn đoán |
| `DieuTri` | string | Phương pháp điều trị |
| `MaBS` | string | FK → NhanVien (bác sĩ) |
| `MaKhoa` | string | Mã khoa |
| `KetLuan` | string | Kết luận |

### HsbaDv (Dịch vụ HSBA)

> **File**: [`HsbaDv.cs`](../ATBM2026-ATBM-06/01-ATBM-06-SourceCode/HospitalManagement.App/Models/HsbaDv.cs)

Mapping dịch vụ khám/xét nghiệm cho từng HSBA.

### DonThuoc (Đơn thuốc)

> **File**: [`DonThuoc.cs`](../ATBM2026-ATBM-06/01-ATBM-06-SourceCode/HospitalManagement.App/Models/DonThuoc.cs)

Đơn thuốc liên kết với HSBA.

### DbConnectionSettings

> **File**: [`DbConnectionSettings.cs`](../ATBM2026-ATBM-06/01-ATBM-06-SourceCode/HospitalManagement.App/Models/DbConnectionSettings.cs)

| Property | Type | Default | Mô tả |
|----------|------|---------|-------|
| `Host` | string | `"localhost"` | Oracle host |
| `Port` | string | `"1521"` | Listener port |
| `ServiceName` | string | `"XEPDB1"` | PDB service |
| `Username` | string | `""` | Oracle username |
| `Password` | string | `""` | Oracle password |
| `UseSysDba` | bool | `false` | Kết nối bằng SYSDBA? |

**Methods**:
- `BuildConnectionString()` – Tạo connection string Oracle
- `ToString()` – Format: `USERNAME@host:port/service (SYSDBA)`

### AuditLog

> **File**: [`AuditLog.cs`](../ATBM2026-ATBM-06/01-ATBM-06-SourceCode/HospitalManagement.App/Models/AuditLog.cs)

Model cho dữ liệu audit trail.

---

## 4. Helpers

### UIHelper

> **File**: [`UIHelper.cs`](../ATBM2026-ATBM-06/01-ATBM-06-SourceCode/HospitalManagement.App/Helpers/UIHelper.cs)
>
> **Mục đích**: Design system – Color palette, fonts, factory methods cho UI components.

#### Factory Methods

| Method | Mô tả |
|--------|-------|
| `CreateButton(text, bgColor, width?, height?)` | Nút bấm modern với rounded corners, hover effect |
| `CreateTextBox(width?)` | TextBox dark theme |
| `CreateLabel(text, font?, color?)` | Label chuẩn |
| `CreateCard(width, height)` | Panel card container |
| `StyleDataGridView(dgv)` | Apply dark theme cho DataGridView |
| `StyleTabControl(tc)` | Owner-drawn TabControl |
| `StyleForm(form, title, width?, height?)` | Apply dark theme cho Form |
| `CreateRoundedRegion(size, radius)` | Tạo Region bo góc |

#### Dialogs

| Method | Mô tả |
|--------|-------|
| `ShowSuccess(message)` | MessageBox thành công ✅ |
| `ShowError(message)` | MessageBox lỗi ❌ |
| `ShowWarning(message)` | MessageBox cảnh báo ⚠️ |

---

### IdentifierValidator

> **File**: [`IdentifierValidator.cs`](../ATBM2026-ATBM-06/01-ATBM-06-SourceCode/HospitalManagement.App/Helpers/IdentifierValidator.cs)
>
> **Mục đích**: Chống SQL Injection – Normalize và validate các identifier trước khi đưa vào stored procedure.

| Method | Mô tả |
|--------|-------|
| `NormalizeSimpleIdentifier(value, label)` | Validate tên user/role/object (chỉ alphanumeric + underscore) |
| `NormalizePassword(value)` | Validate password format |
| `NormalizePrivilege(value)` | Validate privilege name |
| `NormalizeColumns(columns)` | Validate danh sách column names |

> ⚠️ Tất cả method đều **throw `ArgumentException`** nếu input không hợp lệ.

---

## 5. Forms

| Form | File | Vai trò | Kích thước |
|------|------|---------|-----------|
| `LoginForm` | [`LoginForm.cs`](../ATBM2026-ATBM-06/01-ATBM-06-SourceCode/HospitalManagement.App/Forms/LoginForm.cs) | Entry point, xác thực & routing | 520×620 |
| `AdminMainForm` | [`AdminMainForm.cs`](../ATBM2026-ATBM-06/01-ATBM-06-SourceCode/HospitalManagement.App/Forms/AdminMainForm.cs) | Quản trị CSDL Oracle (Phân hệ 1) | Tabbed |
| `DieuPhoiVienForm` | [`DieuPhoiVienForm.cs`](../ATBM2026-ATBM-06/01-ATBM-06-SourceCode/HospitalManagement.App/Forms/DieuPhoiVienForm.cs) | CRUD NV, BN, HSBA, DV | Tabbed |
| `BacSiForm` | [`BacSiForm.cs`](../ATBM2026-ATBM-06/01-ATBM-06-SourceCode/HospitalManagement.App/Forms/BacSiForm.cs) | Xem/sửa HSBA, kê đơn | Tabbed |
| `KyThuatVienForm` | [`KyThuatVienForm.cs`](../ATBM2026-ATBM-06/01-ATBM-06-SourceCode/HospitalManagement.App/Forms/KyThuatVienForm.cs) | Cập nhật kết quả XN | – |
| `BenhNhanForm` | [`BenhNhanForm.cs`](../ATBM2026-ATBM-06/01-ATBM-06-SourceCode/HospitalManagement.App/Forms/BenhNhanForm.cs) | Xem hồ sơ cá nhân | – |
| `AuditLogForm` | [`AuditLogForm.cs`](../ATBM2026-ATBM-06/01-ATBM-06-SourceCode/HospitalManagement.App/Forms/AuditLogForm.cs) | Tra cứu audit log | – |
| `ThongBaoKhanForm` | [`ThongBaoKhanForm.cs`](../ATBM2026-ATBM-06/01-ATBM-06-SourceCode/HospitalManagement.App/Forms/ThongBaoKhanForm.cs) | Thông báo khẩn (OLS) | – |
| `BenhNhanEditDialog` | [`BenhNhanEditDialog.cs`](../ATBM2026-ATBM-06/01-ATBM-06-SourceCode/HospitalManagement.App/Forms/BenhNhanEditDialog.cs) | Dialog thêm/sửa BN | Dialog |
| `RoleDialogForm` | [`RoleDialogForm.cs`](../ATBM2026-ATBM-06/01-ATBM-06-SourceCode/HospitalManagement.App/Forms/RoleDialogForm.cs) | Dialog tạo/sửa role | Dialog |
| `UserDialogForm` | [`UserDialogForm.cs`](../ATBM2026-ATBM-06/01-ATBM-06-SourceCode/HospitalManagement.App/Forms/UserDialogForm.cs) | Dialog tạo/sửa user | Dialog |
