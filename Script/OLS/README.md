# Hướng dẫn chạy bộ script OLS

## 1. Danh sách file

- `00_reset_ols.sql`
- `01_sys_setup.sql`
- `02_admin_policy_setup.sql`
- `03_admin_data_apply.sql`
- `04_ols_test.sql`

## 2. Mục đích từng file

### `00_reset_ols.sql`
Chạy bằng **SYS** trên `XEPDB1`.


### `01_sys_setup.sql`
Chạy bằng **SYS** trên `XEPDB1`.

### `02_admin_policy_setup.sql`
Chạy bằng **ADMIN** trên `XEPDB1`.

### `03_admin_data_apply.sql`
Chạy bằng **ADMIN** trên `XEPDB1`.

### `04_ols_test.sql`
Chạy bằng từng user test `U1 -> U8` trên `XEPDB1`.


## 3. Thứ tự chạy đúng

### Bước 1: đăng nhập SYS
Chạy lần lượt:

1. `00_reset_ols.sql`
2. `01_sys_setup.sql`

### Bước 2: đăng nhập ADMIN
Chạy lần lượt:

3. `02_admin_policy_setup.sql`
4. `03_admin_data_apply.sql`

### Bước 3: đăng nhập từng user test
Chạy:
5. `04_ols_test.sql`


## 4. Tài khoản dùng để test
Các user test:
- `U1`
- `U2`
- `U3`
- `U4`
- `U5`
- `U6`
- `U7`
- `U8`

Mật khẩu mặc định:
- `12345`

Ví dụ đăng nhập test:

```sql
CONNECT U1/12345;
```

Hoặc mở kết nối mới trong SQL Developer bằng user tương ứng.

---

## 5. Câu lệnh test chuẩn

```sql
SELECT MA_TB, NOI_DUNG, DIA_DIEM
FROM ADMIN.THONGBAO
ORDER BY MA_TB;

SELECT COUNT(*) AS SO_DONG_NHIN_THAY
FROM ADMIN.THONGBAO;
```

---

## 6. Kết quả mong đợi khi test

- `U1` thấy: `T1 T2 T3 T4 T5 T6 T7` → `count = 7`
- `U2` thấy: `T1 T3` → `count = 2`
- `U3` thấy: `T1 T3` → `count = 2`
- `U4` thấy: `T1` → `count = 1`
- `U5` thấy: `T1` → `count = 1`
- `U6` thấy: `T1 T3` → `count = 2`
- `U7` thấy: `T1 T3 T4 T5 T6 T7` → `count = 6`
- `U8` thấy: `T1 T6` → `count = 2`

## 7. Lưu ý quan trọng

1. `00_reset_ols.sql` chỉ reset policy OLS, không xóa user `ADMIN`.
2. `01_sys_setup.sql` không tạo user `ADMIN`, chỉ cấp quyền cho `ADMIN`.
3. `02_admin_policy_setup.sql` và `03_admin_data_apply.sql` phải chạy bằng đúng user `ADMIN`.
4. Không dùng `ALTER SESSION SET CURRENT_SCHEMA = ADMIN` để thay cho đăng nhập ADMIN thật sự.
5. Nếu gặp lỗi `ORA-12407`, thường là do policy được tạo bởi session khác user đang thao tác.
6. Nếu chạy lại nhiều lần, nên luôn chạy `00_reset_ols.sql` trước.


