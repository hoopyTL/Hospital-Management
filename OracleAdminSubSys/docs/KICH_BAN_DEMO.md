# Kịch bản demo đề xuất

## Kịch bản 1: dựng môi trường
- Chạy `00_bootstrap_demo.sql`
- Mở app
- Đăng nhập `ATBM_ADMIN`

## Kịch bản 2: user management
- Tạo user `TEST_U1`
- Đổi mật khẩu `TEST_U1`
- Lock `TEST_U1`
- Unlock `TEST_U1`

## Kịch bản 3: role management
- Tạo role `TEST_R1`
- Đặt password cho role `TEST_R1`
- Drop `TEST_R1`

## Kịch bản 4: grant system privilege
- Grant `CREATE SESSION` cho `DEV_A`
- Sang `Tra cứu quyền` kiểm tra `DBA_SYS_PRIVS`

## Kịch bản 5: grant role cho user
- Grant `RL_READONLY` cho `DEV_A`
- Tích checkbox cho phép cấp tiếp
- Sang `Tra cứu quyền` kiểm tra `DBA_ROLE_PRIVS`

## Kịch bản 6: grant object privilege mức bảng
- Grant `SELECT` trên `LAB_OWNER.VW_EMP_SUMMARY` cho `DEV_B`
- Kiểm tra `DBA_TAB_PRIVS`

## Kịch bản 7: grant object privilege mức cột
- Grant `SELECT(FULL_NAME, EMAIL, SALARY)` trên `LAB_OWNER.EMPLOYEES` cho `APP_USER2`
- Kiểm tra `DBA_COL_PRIVS`

## Kịch bản 8: grant execute
- Grant `EXECUTE` trên `LAB_OWNER.PR_RAISE_SALARY` cho `APP_USER1`
- Grant `EXECUTE` trên `LAB_OWNER.FN_EMP_COUNT` cho `APP_USER1`

## Kịch bản 9: revoke
- Revoke role khỏi user
- Revoke quyền object
- Revoke system privilege
- Mở tab `Tra cứu quyền` kiểm tra lại

## Kịch bản 10: câu hỏi phản biện hay gặp
### Vì sao SELECT/UPDATE hỗ trợ mức cột còn INSERT/DELETE thì không?
Vì đề yêu cầu như vậy và cũng phù hợp cú pháp Oracle object privilege.

### Vì sao app cần tài khoản DBA?
Vì phân hệ 1 là ứng dụng quản trị Oracle, cần thao tác trên user/role và đọc metadata từ `DBA_*`.

### Vì sao dùng `DBA_COL_PRIVS` riêng?
Vì quyền mức cột không hiển thị đủ trong `DBA_TAB_PRIVS`.

### Vì sao checkbox “cấp tiếp” nhưng Oracle có hai từ khóa?
Vì ở tầng UX gom thành một thao tác. Bên trong:
- role / system privilege -> `WITH ADMIN OPTION`
- object privilege -> `WITH GRANT OPTION`
