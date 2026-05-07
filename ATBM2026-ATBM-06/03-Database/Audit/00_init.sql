-- ==========================================
-- 1. Kích hoạt kiểm toán (Chạy bằng sysdba trên root container nếu hệ thống chưa bật)
ALTER SYSTEM SET audit_trail=db,extended SCOPE=SPFILE;
SHUTDOWN IMMEDIATE;
STARTUP;
GRANT AUDIT_ADMIN TO Admin;
GRANT AUDIT_VIEWER TO Admin;
GRANT AUDIT SYSTEM TO Admin;
GRANT AUDIT ANY TO Admin;