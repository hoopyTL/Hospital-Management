# Hospital Management – SQL Scripts

Toàn bộ script cài đặt database cho đồ án ATBM HTTT 2025-2026.

## Cấu trúc thư mục

```
Script/
├── initDB.sql # Tạo ADMIN + tables (chạy bằng SYSDBA)
├── insertData.sql # Sinh 170 NV + 100K BN + HSBA/DV/DT
├── createUser.sql # Tạo 100K+ user Oracle
├── dropAllTable.sql / dropAllUser.sql / fix_cleanup_sys.sql
├── run_01_as_sysdba.sql # Orchestration bước 1 (SYSDBA)
├── run_02_as_admin.sql # Orchestration bước 2 (ADMIN) – gọi đủ 3 tầng bên dưới
│
├── Admin/ # Phân hệ 1 – Quản trị Oracle (ATBM_ADMIN)
│   ├── 00_bootstrap_demo.sql # tạo ATBM_ADMIN, LAB_OWNER, user/role demo
│   ├── 01_pkg_admin.sql # package PKG_ADMIN + bảng APP_VPD_COL_GRANTS
│   ├── 10_verify_demo.sql / 20_required_grants... / 30_demo_scenarios...
│   ├── 99_cleanup_demo.sql
│   └── README.md
│
├── RBAC/ # Quyền truy cập: roles, grants, assign
│   ├── 01_create_roles.sql
│   ├── 02_grant_ktv_bn.sql
│   ├── 03_grant_dpv_bs.sql # + tạo MV_BACSI_LIST, MV_KTV_LIST
│   ├── 04_assign_ktv_bn.sql
│   ├── 05_assign_dpv_bs.sql
│   ├── 99_drop_rbac.sql
│   └── README.md
│
├── VPD/ # Row-Level Security cho DPV + BS
│   ├── 01_fn_get_role.sql
│   ├── 02_vpd_basic.sql # NHAN_VIEN self cho DPV/BS; KTV dùng view
│   ├── 03_vpd_dpv_bs.sql # DPV + BS trên BENH_NHAN, HSBA, HSBA_DV, DON_THUOC
│   ├── 99_drop_vpd.sql
│   └── README.md
│
├── Audit/ # Ghi vết hành vi
│   ├── 01_audit_ketqua.sql
│   ├── 99_drop_audit.sql
│   └── README.md
│
└── OLS/ # Oracle Label Security cho THONG_BAO
    ├── 00_reset_ols.sql / 01_sys_setup.sql
    ├── 02_admin_policy_setup.sql / 03_admin_data_apply.sql
    ├── 04_ols_test.sql
    └── README.md
```

> **Lưu ý phạm vi**: Phân hệ 1 (`Admin/`) chạy **độc lập** với Phân hệ 2
> (`RBAC/`, `VPD/`, `Audit/`, `OLS/`). File `run_02_as_admin.sql` chỉ cài
> Phân hệ 2. Muốn cài Phân hệ 1 hãy theo hướng dẫn trong `Admin/README.md`.

## Cài đặt lần đầu

### Bước 1 – đăng nhập `SYS AS SYSDBA` vào PDB `XEPDB1`

F5 file `initDB.sql` (hoặc `run_01_as_sysdba.sql`).

### Bước 2 – đăng nhập `ADMIN / 12345` vào PDB `XEPDB1`

F5 file `run_02_as_admin.sql`. Script sẽ gọi lần lượt:

1. `insertData.sql` (vài phút)
2. `createUser.sql` (15–30 phút)
3. `RBAC/01_create_roles.sql`
4. `RBAC/02_grant_ktv_bn.sql`
5. `RBAC/03_grant_dpv_bs.sql`
6. `RBAC/04_assign_ktv_bn.sql` (10–20 phút)
7. `RBAC/05_assign_dpv_bs.sql`
8. `VPD/01_fn_get_role.sql`
9. `VPD/02_vpd_basic.sql`
10. `VPD/03_vpd_dpv_bs.sql`
11. `Audit/01_audit_ketqua.sql`

> Chạy trong **SQL Developer**: mở đúng file rồi **F5 (Run Script)**, không dùng F9. Nhớ bật *View → Dbms Output* để thấy log.

## Tài khoản test

| User | Mật khẩu | Vai trò | Form mở |
| - | - | - | - |
| `NV0001`   | `123` | Điều phối viên | `DieuPhoiVienForm` |
| `NV0050`   | `123` | Bác sĩ / Y sĩ  | `BacSiForm` |
| `NV0121`   | `123` | Kỹ thuật viên  | `KyThuatVienForm` |
| `BN000001` | `123` | Bệnh nhân      | `BenhNhanForm` |

## Rollback

Drop theo **thứ tự ngược lại** (Audit → VPD → RBAC):

```
Audit/99_drop_audit.sql
VPD/99_drop_vpd.sql
RBAC/99_drop_rbac.sql
dropAllUser.sql       (rất lâu - 100K user)
dropAllTable.sql
```
