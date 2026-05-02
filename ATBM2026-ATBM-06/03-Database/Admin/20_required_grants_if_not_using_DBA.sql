-- 20_required_grants_if_not_using_DBA.sql
-- Nếu bạn KHÔNG muốn dùng role DBA cho tài khoản chạy app,
-- hãy cấp tối thiểu các quyền gần đúng dưới đây cho tài khoản quản trị.
-- Lưu ý: tùy policy lab/giảng viên, dùng DBA cho demo sẽ đơn giản hơn.

-- Ví dụ tài khoản: APP_ADMIN_MIN

GRANT CREATE SESSION TO APP_ADMIN_MIN;
GRANT CREATE USER TO APP_ADMIN_MIN;
GRANT ALTER USER TO APP_ADMIN_MIN;
GRANT DROP USER TO APP_ADMIN_MIN;
GRANT CREATE ROLE TO APP_ADMIN_MIN;
GRANT DROP ANY ROLE TO APP_ADMIN_MIN;
GRANT GRANT ANY ROLE TO APP_ADMIN_MIN;
GRANT GRANT ANY PRIVILEGE TO APP_ADMIN_MIN;
GRANT GRANT ANY OBJECT PRIVILEGE TO APP_ADMIN_MIN;
GRANT SELECT ANY DICTIONARY TO APP_ADMIN_MIN;

-- Một số lab không khuyến nghị cấp quá nhiều ANY privilege.
-- Vì vậy, nếu môi trường của bạn bị siết chặt, nên dùng hẳn tài khoản DBA cho phân hệ 1.
