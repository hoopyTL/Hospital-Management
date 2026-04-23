# Script/Admin - Phân hệ 1: Quản trị CSDL Oracle

Thư mục này chứa toàn bộ script PL/SQL cho **Phân hệ 1 - Ứng dụng quản trị
CSDL Oracle** (tương ứng module `Admin` trong ứng dụng `HospitalManagement.App`,
vào bằng tài khoản `ATBM_ADMIN`).

Nội dung được gộp từ `PhanHe1_src/database/` (bản standalone cũ) về đây để
toàn dự án dùng chung một cấu trúc `Script/` duy nhất, cùng cấp với
`Script/RBAC/`, `Script/VPD/`, `Script/Audit/`, `Script/OLS/` phục vụ Phân hệ 2.

## 1. Danh sách file

| File | Mô tả |
| --- | --- |
| `00_bootstrap_demo.sql` | Tạo `ATBM_ADMIN`, `LAB_OWNER`, các user / role demo. Chạy bằng **SYS AS SYSDBA** trong PDB (ví dụ `XEPDB1`). |
| `01_pkg_admin.sql` | Tạo bảng `APP_VPD_COL_GRANTS` và toàn bộ package `PKG_ADMIN` (stored procedure cho tab Users/Roles/Grant/Revoke). Chạy bằng **ATBM_ADMIN**. |
| `10_verify_demo.sql` | Kiểm tra dữ liệu/object/privilege sau bootstrap. |
| `20_required_grants_if_not_using_DBA.sql` | Các grant bổ sung khi không chạy bằng DBA. |
| `30_demo_scenarios.sql` | Các kịch bản demo nhanh: grant/revoke mức cột, role,... |
| `99_cleanup_demo.sql` | Xoá toàn bộ môi trường demo. |

## 2. Thứ tự chạy khuyến nghị

1. Kết nối PDB (vd. `XEPDB1`) với vai trò **SYS AS SYSDBA**.
2. Chạy `00_bootstrap_demo.sql`.
3. Kết nối lại bằng **ATBM_ADMIN / Admin#12345** (không SYSDBA).
4. Chạy `01_pkg_admin.sql` để tạo package `PKG_ADMIN`.
5. (Tuỳ chọn) Chạy `10_verify_demo.sql` để kiểm tra.
6. Chạy ứng dụng `HospitalManagement.App`, đăng nhập bằng `ATBM_ADMIN`.

## 3. Liên hệ Phân hệ 2

Phân hệ 2 (nghiệp vụ bệnh viện) có SQL riêng ở các thư mục:

- `Script/RBAC/` - roles `RL_DIEUPHOIVIEN`, `RL_BACSI`, `RL_KYTHUATVIEN`, `RL_BENHNHAN`
- `Script/VPD/` - chính sách VPD theo vai trò
- `Script/Audit/` - audit trigger cho kết quả xét nghiệm
- `Script/OLS/` - OLS cho bảng THONG_BAO

Xem `Script/README.md` để biết thứ tự chạy tổng thể cho Phân hệ 2
(`run_01_as_sysdba.sql` + `run_02_as_admin.sql`). Phân hệ 1 chạy độc lập
theo hướng dẫn ở mục 2 phía trên.
