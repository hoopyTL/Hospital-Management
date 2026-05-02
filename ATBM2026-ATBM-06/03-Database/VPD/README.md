# VPD – Virtual Private Database (Row-Level Security)

Folder này chứa tầng bảo mật **lọc dòng dữ liệu** bằng `DBMS_RLS` cho Điều phối viên và Bác sĩ/Y sĩ. Kỹ thuật viên và Bệnh nhân không dùng VPD để lọc dòng; hai nhóm này dùng RBAC + View trong `RBAC/02_grant_ktv_bn.sql`.

## Phụ thuộc

Bắt buộc phải chạy RBAC trước (ít nhất `Sql/RBAC/01_create_roles.sql` + `04_assign_ktv_bn.sql` + `05_assign_dpv_bs.sql`) để `FN_GET_ROLE` biết được user nằm trong nhóm nào qua mã user Oracle.

## Thứ tự chạy

Đăng nhập `ADMIN / 12345 @ XEPDB1`, F5 từng file:

| # | File | Mô tả |
| - | - | - |
| 1 | `01_fn_get_role.sql` | Helper function `ADMIN.FN_GET_ROLE` – suy ra role từ `SESSION_USER` (tránh đệ quy VPD) |
| 2 | `02_vpd_basic.sql`   | Policy cơ bản cho `NHAN_VIEN`: DPV/BS chỉ thấy chính mình; KTV đi qua view |
| 3 | `03_vpd_dpv_bs.sql`  | Policy cho `BENH_NHAN`, `HSBA_DV`, `HSBA`, `DON_THUOC` phục vụ DPV/BS |

## Rollback

`99_drop_vpd.sql` – drop toàn bộ policies + policy functions + `FN_GET_ROLE`.

## Ma trận chính sách VPD

| Bảng | Policy | Function | DBA | DPV | BS | KTV | BN |
| - | - | - | - | - | - | - | - |
| BENH_NHAN | `POL_BN_RLS`        | `fn_policy_benhnhan`  | all | all | HSBA của mình | không grant base table | no predicate; dùng `V_BENH_NHAN_SELF` |
| HSBA      | `POL_HSBA_RLS`      | `fn_policy_hsba`      | all | all | `MA_BS = user` | 1=0 | 1=0 |
| HSBA_DV   | `POL_HSBA_DV_RLS`   | `fn_policy_hsba_dv`   | all | all | HSBA của mình | no predicate; dùng `V_HSBA_DV_KTV` | 1=0 |
| DON_THUOC | `POL_DON_THUOC_RLS` | `fn_policy_don_thuoc` | all | 1=0 | HSBA của mình | 1=0 | 1=0 |
| NHAN_VIEN | `POL_NHANVIEN_SELF` | `fn_policy_nhanvien`  | all | self | self | no predicate; dùng `V_NHAN_VIEN_SELF` | 1=0 |

Các policy VPD cho DPV/BS có `update_check = TRUE` để đảm bảo người dùng không thể `UPDATE` / `INSERT` ra khỏi vùng dữ liệu được phép. KTV/BN dùng `WITH CHECK OPTION` ở view để đạt mục tiêu tương tự.

## Tại sao BS phải dùng `MV_BACSI_LIST` / `MV_KTV_LIST`?

Khi DPV/BS cần hiện **danh sách bác sĩ/KTV** lên dropdown (để điều phối / chọn KTV), nếu query trực tiếp `NHAN_VIEN` sẽ bị `POL_NHANVIEN_SELF` chặn (chỉ thấy chính mình).

Giải pháp: DBA tạo sẵn 2 materialized view trong `Sql/RBAC/03_grant_dpv_bs.sql`. MV không có VPD, nên DPV/BS `SELECT` thẳng ra đủ danh sách.

Nếu có nhân viên mới, DBA cần refresh:

```sql
EXEC DBMS_MVIEW.REFRESH('ADMIN.MV_BACSI_LIST');
EXEC DBMS_MVIEW.REFRESH('ADMIN.MV_KTV_LIST');
```

## Kiểm tra nhanh

```sql
-- Xem toàn bộ policy đang active
SELECT object_name, policy_name, enable, pf_owner, function
FROM   dba_policies
WHERE  object_owner = 'ADMIN'
ORDER  BY object_name, policy_name;

-- Thử log bằng BN000001: SELECT * FROM ADMIN.V_BENH_NHAN_SELF; -> chỉ ra 1 dòng
-- Thử log bằng NV0121  : SELECT * FROM ADMIN.V_HSBA_DV_KTV;    -> chỉ DV của mình
-- Thử log bằng NV0050  : SELECT COUNT(*) FROM ADMIN.HSBA;      -> chỉ HSBA của mình
```
