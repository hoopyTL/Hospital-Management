# Audit – Ghi vết hành vi

Folder này chứa các script ghi vết (audit) hành vi của người dùng ở tầng database.

## Phạm vi hiện tại

- **TC#4(b)**: Mọi thao tác `UPDATE` trên cột `KET_QUA` của `HSBA_DV` đều được ghi vết (Phase 1).

Các yêu cầu ghi vết khác (UPDATE `CHUAN_DOAN/DIEU_TRI/KET_LUAN`, INSERT `DON_THUOC`, Standard Audit 5 ngữ cảnh, FGA 4 tình huống…) sẽ bổ sung trong Phase 3 (YC3).

## Phụ thuộc

Bắt buộc có role `RL_KYTHUATVIEN` (tạo bởi `Sql/RBAC/01_create_roles.sql`), vì script cấp `SELECT` cho role này để KTV có thể xem lịch sử ghi vết.

## Thứ tự chạy

Đăng nhập `ADMIN / 12345 @ XEPDB1`, F5:

| # | File | Mô tả |
| - | - | - |
| 1 | `01_audit_ketqua.sql` | Tạo `AUDIT_KETQUA` + trigger `TRG_AUDIT_KETQUA` (Phase 1 - TC#4b) |
| 2 | `02_audit.sql` | Cấu hình Standard Audit (5 contexts) + Fine-Grained Audit (4 FGA policies) |
| 3 | `03_view_audit_logs.sql` | Hiển thị các audit logs từ DBA_AUDIT_TRAIL và DBA_FGA_AUDIT_TRAIL |

## Rollback

`99_drop_audit.sql` – drop trigger + bảng.

## Xem log

### Custom Audit Tables (Phase 1 - TC#4b)
```sql
SELECT * FROM ADMIN.AUDIT_KETQUA
ORDER BY thoi_gian_cap_nhat DESC
FETCH FIRST 100 ROWS ONLY;
```

### Standard Audit & Fine-Grained Audit (Phase 2+)
**Chạy file** `03_view_audit_logs.sql` **để xem các audit logs:**

- **Standard Audit Logs** (DBA_AUDIT_TRAIL):
  - SELECT trên HSBA
  - UPDATE thất bại trên DON_THUOC
  - DELETE thành công trên HSBA_DV
  - INSERT thành công trên BENH_NHAN
  
- **Fine-Grained Audit Logs** (DBA_FGA_AUDIT_TRAIL):
  - FGA UPDATE trên DON_THUOC
  - FGA Illegal UPDATE trên HSBA
  - FGA DML ngoài giờ hành chính trên HSBA_DV

- **Audit Statistics**:
  - Tổng số audit events theo loại hành động
  - Tổng số FGA events theo policy
  - Top users thực hiện audit events

- **Current Audit Triggers**:
  - Standard Audit Options (từ DBA_OBJ_AUDIT_OPTS)
  - Fine-Grained Audit Policies (từ DBA_AUDIT_POLICIES)
