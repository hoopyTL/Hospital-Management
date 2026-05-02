# RBAC – Role-Based Access Control

Folder này CHỈ chứa các script liên quan **quyền truy cập** (roles, grants, assign).
Các cơ chế khác đã được tách riêng:

- **VPD** (Row-Level Security) → `Sql/VPD/`
- **Audit** (ghi vết) → `Sql/Audit/`

## Thứ tự chạy

Đăng nhập `ADMIN / 12345 @ XEPDB1`, mở lần lượt từng file bằng SQL Developer và nhấn **F5 (Run Script)**:

| # | File | Mô tả |
| - | - | - |
| 1 | `01_create_roles.sql` | Tạo 4 role: `RL_KYTHUATVIEN`, `RL_BENHNHAN`, `RL_DIEUPHOIVIEN`, `RL_BACSI` |
| 2 | `02_grant_ktv_bn.sql`  | Tạo view self-scope + GRANT object/column cho KTV + BN (TC#4, TC#5), không grant trực tiếp base table |
| 3 | `03_grant_dpv_bs.sql`  | GRANT object/column cho DPV + BS (TC#2, TC#3) + tạo `MV_BACSI_LIST`, `MV_KTV_LIST` |
| 4 | `04_assign_ktv_bn.sql` | GRANT role cho 50 KTV + 100K BN (mất nhiều thời gian) |
| 5 | `05_assign_dpv_bs.sql` | GRANT role cho NV0001-NV0020 (DPV) + NV0021-NV0120 (BS) |

## Rollback

`99_drop_rbac.sql` – revoke + drop role + drop view RBAC + drop MV.
**Lưu ý**: chạy `VPD/99_drop_vpd.sql` và `Audit/99_drop_audit.sql` TRƯỚC nếu muốn dọn sạch hoàn toàn.

## Danh sách quyền tóm tắt

### `RL_KYTHUATVIEN` (TC#4 + TC#5)

| Object | SELECT | INSERT | UPDATE | DELETE |
| - | - | - | - | - |
| `V_HSBA_DV_KTV`    | ✅ | – | ✅ (chỉ `KET_QUA`) | – |
| `V_NHAN_VIEN_SELF` | ✅ | – | ✅ (`QUE_QUAN`, `SDT`) | – |

### `RL_BENHNHAN` (TC#5)

| Object | SELECT | INSERT | UPDATE | DELETE |
| - | - | - | - | - |
| `V_BENH_NHAN_SELF` | ✅ | – | ✅ (7 trường địa chỉ + tiền sử + dị ứng) | – |

### `RL_DIEUPHOIVIEN` (TC#2 + TC#5)

| Bảng | SELECT | INSERT | UPDATE | DELETE |
| - | - | - | - | - |
| BENH_NHAN | ✅ | ✅ | ✅ (11 trường) | – |
| HSBA      | ✅ | ✅ | ✅ (`MA_KHOA`, `MA_BS`) | – |
| HSBA_DV   | ✅ | ✅ | ✅ (`MA_KTV`) | – |
| NHAN_VIEN | ✅ | – | ✅ (`QUE_QUAN`, `SDT`) | – |
| MV_BACSI_LIST / MV_KTV_LIST | ✅ | – | – | – |

### `RL_BACSI` (TC#3 + TC#5)

| Bảng | SELECT | INSERT | UPDATE | DELETE |
| - | - | - | - | - |
| HSBA      | ✅ | – | ✅ (`CHUAN_DOAN`, `DIEU_TRI`, `KET_LUAN`) | – |
| HSBA_DV   | ✅ | ✅ | – | ✅ |
| BENH_NHAN | ✅ | – | ✅ (`TIEN_SU_BENH`, `TIEN_SU_BENH_GD`, `DI_UNG_THUOC`) | – |
| DON_THUOC | ✅ | ✅ | ✅ (`TEN_THUOC`, `LIEU_DUNG`) | ✅ |
| NHAN_VIEN | ✅ | – | ✅ (`QUE_QUAN`, `SDT`) | – |
| MV_KTV_LIST | ✅ | – | – | – |

Với `RL_KYTHUATVIEN` và `RL_BENHNHAN`, tính **ép thỏa** nằm ở tầng **RBAC + View**:

- Role chỉ được grant trên view, không grant trực tiếp trên base table.
- View lọc theo `SYS_CONTEXT('USERENV', 'SESSION_USER')`.
- `WITH CHECK OPTION` chặn thao tác làm dòng thoát khỏi phạm vi view.

Với `RL_DIEUPHOIVIEN` và `RL_BACSI`, RBAC quyết định quyền object/column, còn VPD lọc dòng trên base table.
