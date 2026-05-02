-- =====================================================
-- 99_drop_audit.sql
-- Rollback Audit: drop trigger + bảng audit.
-- Chạy bằng tài khoản ADMIN (DBA)
-- =====================================================

SET SERVEROUTPUT ON;

BEGIN EXECUTE IMMEDIATE 'DROP TRIGGER TRG_AUDIT_KETQUA'; EXCEPTION WHEN OTHERS THEN NULL; END;
/
BEGIN EXECUTE IMMEDIATE 'DROP TABLE AUDIT_KETQUA CASCADE CONSTRAINTS'; EXCEPTION WHEN OTHERS THEN NULL; END;
/

BEGIN
    DBMS_OUTPUT.PUT_LINE('=== Hoàn thành rollback Audit ===');
END;
/
