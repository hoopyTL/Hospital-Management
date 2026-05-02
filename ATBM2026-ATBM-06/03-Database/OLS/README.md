# Hướng dẫn chạy bộ script OLS

## 1. Danh sách file

- `00_reset_ols.sql`
- `01_sys_setup.sql`
- `02_admin_table_policy.sql`
- `03_admin_policy_components.sql`
- `04_admin_data_apply.sql`
- `05_ols_test.sql`

## 2. Mục đích từng file

### `00_reset_ols.sql`
Chạy bằng **SYS** trên `XEPDB1`.

### `01_sys_setup.sql`
Chạy bằng **SYS** trên `XEPDB1`.

### `02_admin_table_policy.sql`
Chạy bằng **ADMIN** trên `XEPDB1`.

Sau khi chạy file này phải:
DISCONNECT ADMIN → CONNECT lại ADMIN

### `03_admin_policy_components.sql`
Chạy bằng **ADMIN** trên `XEPDB1`.

### `04_admin_data_apply.sql`
Chạy bằng **ADMIN** trên `XEPDB1`.

### `05_ols_test.sql`
Chạy bằng từng user test `NV0001 -> NV0123` trên `XEPDB1`.

## 3. Thứ tự chạy đúng

### Bước 1: đăng nhập SYS
Chạy lần lượt:

1. `00_reset_ols.sql`
2. `01_sys_setup.sql`

### Bước 2: đăng nhập ADMIN
Chạy lần lượt:

3. `02_admin_table_policy.sql`

Sau đó:
DISCONNECT ADMIN  
CONNECT lại ADMIN  

Chạy tiếp:

4. `03_admin_policy_components.sql`
5. `04_admin_data_apply.sql`

### Bước 3: đăng nhập từng user test
Chạy:
6. `05_ols_test.sql`

## 4. Tài khoản dùng để test
Các user test:
- `NV0001`
- `NV0021`
- `NV0022`
- `NV0121`
- `NV0122`
- `NV0002`
- `NV0003`
- `NV0123`

Mật khẩu mặc định:
- `123`

Ví dụ đăng nhập test:

CONNECT NV0001/123;

Hoặc mở kết nối mới trong SQL Developer bằng user tương ứng.

---

## 5. Câu lệnh test chuẩn

SELECT MA_TB, NOI_DUNG, NGAY_GIO, DIA_DIEM
FROM ADMIN.THONGBAO
ORDER BY MA_TB;

SELECT COUNT(*) AS SO_DONG_NHIN_THAY
FROM ADMIN.THONGBAO;

---

## 6. Kết quả mong đợi khi test

- `NV0001` thấy: `T1 T2 T3 T4 T5 T6 T7` → `count = 7`
- `NV0021` thấy: `T1 T3` → `count = 2`
- `NV0022` thấy: `T1 T3` → `count = 2`
- `NV0121` thấy: `T1` → `count = 1`
- `NV0122` thấy: `T1` → `count = 1`
- `NV0002` thấy: `T1 T3` → `count = 2`
- `NV0003` thấy: `T1 T3 T4 T5 T6 T7` → `count = 6`
- `NV0123` thấy: `T1 T6` → `count = 2`

## 7. Lưu ý quan trọng

1. `00_reset_ols.sql` chỉ reset policy OLS, không xóa user `ADMIN`.
2. `01_sys_setup.sql` không tạo user `ADMIN`, chỉ cấp quyền cho `ADMIN`.
3. `02_admin_table_policy.sql`, `03_admin_policy_components.sql` và `04_admin_data_apply.sql` phải chạy bằng đúng user `ADMIN`.
4. Không dùng `ALTER SESSION SET CURRENT_SCHEMA = ADMIN` để thay cho đăng nhập ADMIN thật sự.
5. Nếu gặp lỗi `ORA-12407` hoặc `ORA-12446`, cần DISCONNECT → CONNECT lại ADMIN.
6. Nếu chạy lại nhiều lần, nên luôn chạy `00_reset_ols.sql` trước.