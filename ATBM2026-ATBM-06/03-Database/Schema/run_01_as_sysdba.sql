-- =====================================================
-- BƯỚC 1: Chạy file này bằng SYSDBA (trên PDB)
-- Ví dụ SQLPlus: sqlplus sys/password@localhost:1521/XEPDB1 as sysdba
-- File này sẽ tạo tài khoản ADMIN + tạo bảng
-- =====================================================

@@initDB.sql


-- Chạy script Reset OLS (nếu có rác từ trước)
@@../OLS/00_reset_ols.sql

-- Cấp các quyền cấu hình OLS cho ADMIN
@@../OLS/01_sys_setup.sql

-- Sau khi chạy xong file này, đăng nhập ADMIN và chạy file:
-- run_02_as_admin.sql
