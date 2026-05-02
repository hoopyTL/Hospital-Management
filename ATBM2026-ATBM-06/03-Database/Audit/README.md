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
| 1 | `01_audit_ketqua.sql` | Tạo `AUDIT_KETQUA` + trigger `TRG_AUDIT_KETQUA` |

## Rollback

`99_drop_audit.sql` – drop trigger + bảng.

## Xem log

```sql
SELECT * FROM ADMIN.AUDIT_KETQUA
ORDER  BY thoi_gian_cap_nhat DESC
FETCH FIRST 100 ROWS ONLY;
```
