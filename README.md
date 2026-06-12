# 🏥 Hospital Management System

> Hệ thống Quản lý Bệnh viện – Đồ án An toàn & Bảo mật HTTT 2025-2026 (ATBM-06)

Ứng dụng quản lý bệnh viện desktop (.NET WinForms) tích hợp **bảo mật Oracle nâng cao**: RBAC, VPD (Row-Level Security), Audit, Oracle Label Security (OLS) và module quản trị CSDL Oracle chuyên biệt.

---

## ⚡ Quick Start

### Yêu cầu hệ thống

| Thành phần | Phiên bản |
|-----------|-----------|
| Oracle Database XE | 21c+ (PDB `XEPDB1`) |
| .NET SDK | 8.0+ |
| OS | Windows 10/11 |
| IDE (tuỳ chọn) | Visual Studio 2022 / SQL Developer |

### Cài đặt & Chạy (< 5 phút nếu bỏ qua seed data)

```bash
# 1. Clone repository
git clone https://github.com/hoopyTL/Hospital-Management.git
cd Hospital-Management

# 2. Cài đặt database (xem chi tiết tại 03-Database/README.md)
#    - Bước 1: Chạy initDB.sql bằng SYS AS SYSDBA
#    - Bước 2: Chạy run_02_as_admin.sql bằng ADMIN/12345

# 3. Build & chạy ứng dụng
cd ATBM2026-ATBM-06/01-ATBM-06-SourceCode
dotnet build HospitalManagement.sln
dotnet run --project HospitalManagement.App
```

> 💡 **Chạy nhanh không build**: Dùng file `.exe` có sẵn tại `ATBM2026-ATBM-06/02-Exe/`

### Tài khoản test

| User | Mật khẩu | Vai trò | Giao diện |
|------|----------|---------|-----------|
| `NV0001` | `123` | Điều phối viên | `DieuPhoiVienForm` |
| `NV0050` | `123` | Bác sĩ / Y sĩ | `BacSiForm` |
| `NV0121` | `123` | Kỹ thuật viên | `KyThuatVienForm` |
| `BN000001` | `123` | Bệnh nhân | `BenhNhanForm` |
| `ATBM_ADMIN` | `Admin#12345` | Quản trị CSDL | `AdminMainForm` |

---

## ✨ Features

### Phân hệ 1 – Quản trị CSDL Oracle (`ATBM_ADMIN`)

- 👥 **Quản lý User/Role** – Tạo, sửa, khoá, xoá user & role Oracle
- 🔑 **Grant/Revoke** – Cấp & thu hồi quyền hệ thống, quyền trên object, quyền mức cột
- 📊 **Dashboard** – Xem thông tin database, tablespaces, profiles
- 🛡️ **SQL Injection Prevention** – Mọi identifier qua `IdentifierValidator` trước khi thực thi

### Phân hệ 2 – Nghiệp vụ Bệnh viện

- 📋 **Điều phối viên (DPV)** – Quản lý nhân viên, bệnh nhân, hồ sơ bệnh án (HSBA), dịch vụ
- 🩺 **Bác sĩ (BS)** – Xem/cập nhật HSBA, kê đơn thuốc, xem thông báo khẩn
- 🔬 **Kỹ thuật viên (KTV)** – Cập nhật kết quả xét nghiệm
- 🏠 **Bệnh nhân (BN)** – Xem hồ sơ cá nhân, lịch sử khám

### Bảo mật Oracle tích hợp

| Tính năng | Mô tả |
|-----------|-------|
| **RBAC** | 4 role: `RL_DIEUPHOIVIEN`, `RL_BACSI`, `RL_KYTHUATVIEN`, `RL_BENHNHAN` |
| **VPD** | Row-Level Security – DPV/BS chỉ thấy dữ liệu thuộc phạm vi quản lý |
| **Audit** | Ghi vết cập nhật kết quả xét nghiệm |
| **OLS** | Oracle Label Security trên bảng `THONG_BAO` (thông báo khẩn) |

---

## 📁 Cấu trúc dự án

```
Hospital-Management/
├── README.md                          # ← Bạn đang đọc file này
│
├── ATBM2026-ATBM-06/                 # Thư mục nộp đồ án
│   ├── 01-ATBM-06-SourceCode/        # Source code .NET
│   │   ├── HospitalManagement.sln
│   │   └── HospitalManagement.App/
│   │       ├── Program.cs             # Entry point
│   │       ├── Forms/                 # UI (WinForms)
│   │       │   ├── LoginForm.cs       # Đăng nhập & routing theo role
│   │       │   ├── AdminMainForm.cs   # Quản trị CSDL (Phân hệ 1)
│   │       │   ├── DieuPhoiVienForm.cs
│   │       │   ├── BacSiForm.cs
│   │       │   ├── KyThuatVienForm.cs
│   │       │   ├── BenhNhanForm.cs
│   │       │   └── ...                # Dialog forms
│   │       ├── Models/                # Domain models
│   │       ├── Services/              # Business logic (OracleAdminService)
│   │       ├── DataAccess/            # OracleHelper (Singleton)
│   │       └── Helpers/               # UIHelper, IdentifierValidator
│   │
│   ├── 02-Exe/                        # Build Release + DLL
│   ├── 03-Database/                   # SQL scripts Oracle (PH1 & PH2)
│   │   ├── Schema/                    # Khởi tạo schema, data, user
│   │   ├── Admin/                     # Phân hệ 1: PKG_ADMIN
│   │   ├── RBAC/                      # Role-Based Access Control
│   │   ├── VPD/                       # Virtual Private Database
│   │   ├── Audit/                     # Audit triggers
│   │   └── OLS/                       # Oracle Label Security
│   │
│   ├── 04-Report/                     # Báo cáo (.docx/.pdf)
│   ├── 05-Demo/                       # Link video demo YouTube
│   └── 06-Guideline/                  # Hướng dẫn build & chạy
│
└── docs/                              # Tài liệu kỹ thuật
    ├── architecture.md                # Kiến trúc hệ thống
    └── api-reference.md               # API Reference (source code)
```

---

## 🔧 Cấu hình

### Kết nối Oracle

Ứng dụng kết nối trực tiếp bằng tài khoản Oracle user (không qua proxy) để VPD policies nhận đúng `SESSION_USER`.

| Biến | Mô tả | Mặc định |
|------|-------|----------|
| Host | Oracle DB host | `localhost` |
| Port | Oracle listener port | `1521` |
| ServiceName | PDB service name | `XEPDB1` |

> ⚠️ Hiện tại cấu hình hardcoded trong `LoginForm.cs`. Thay đổi host/port/service cần sửa trực tiếp trong code.

---

## 📖 Documentation

| Tài liệu | Mô tả |
|-----------|-------|
| [Architecture](./docs/architecture.md) | Kiến trúc hệ thống & luồng xác thực |
| [API Reference](./docs/api-reference.md) | Danh sách class, method, data model |
| [Database Scripts](./ATBM2026-ATBM-06/03-Database/README.md) | Hướng dẫn cài đặt CSDL chi tiết |
| [Admin Module](./ATBM2026-ATBM-06/03-Database/Admin/README.md) | Phân hệ 1 – Quản trị Oracle |
| [Build & Run Guide](./ATBM2026-ATBM-06/06-Guideline/) | PDF hướng dẫn build |

---

## 🛠️ Tech Stack

| Layer | Technology |
|-------|-----------|
| **Frontend** | .NET 8 WinForms (C#) |
| **Database** | Oracle Database XE 21c |
| **ORM/Data Access** | Oracle.ManagedDataAccess.Core 23.x |
| **Security** | Oracle RBAC, VPD, Audit, OLS |
| **UI Theme** | Custom dark theme (xem `UIHelper.cs`) |

---

## 👥 Contributors

| Tên | GitHub |
|-----|--------|
| Nguyen Van Hop | [@hoopyTL](https://github.com/hoopyTL) |
| Nguyen Quoc Bao | [@QUOCBAO1402](https://github.com/QUOCBAO1402) |
| Tin Nguyen | - |
| Ettesoc | - |

---

## 📄 License

Đồ án học thuật – ATBM HTTT 2025-2026.
