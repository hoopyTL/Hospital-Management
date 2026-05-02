-- =====================================================
-- 99_drop_rbac.sql
-- Rollback RBAC: revoke role khỏi users, drop role,
-- drop 3 view RBAC cho KTV/BN và 2 materialized view
-- MV_BACSI_LIST, MV_KTV_LIST.
-- Để rollback VPD / Audit, dùng:
--   Sql/VPD/99_drop_vpd.sql
--   Sql/Audit/99_drop_audit.sql
-- Chạy bằng tài khoản ADMIN (DBA)
-- =====================================================

SET SERVEROUTPUT ON;

-- =====================================================
-- 1. Revoke role khỏi users
-- =====================================================
DECLARE
BEGIN
    FOR v_i IN 121..170 LOOP
        BEGIN EXECUTE IMMEDIATE 'REVOKE RL_KYTHUATVIEN FROM NV' || LPAD(v_i,4,'0');
        EXCEPTION WHEN OTHERS THEN NULL; END;
    END LOOP;

    FOR v_i IN 21..120 LOOP
        BEGIN EXECUTE IMMEDIATE 'REVOKE RL_BACSI FROM NV' || LPAD(v_i,4,'0');
        EXCEPTION WHEN OTHERS THEN NULL; END;
    END LOOP;

    FOR v_i IN 1..20 LOOP
        BEGIN EXECUTE IMMEDIATE 'REVOKE RL_DIEUPHOIVIEN FROM NV' || LPAD(v_i,4,'0');
        EXCEPTION WHEN OTHERS THEN NULL; END;
    END LOOP;

    FOR v_i IN 1..100000 LOOP
        BEGIN EXECUTE IMMEDIATE 'REVOKE RL_BENHNHAN FROM BN' || LPAD(v_i,6,'0');
        EXCEPTION WHEN OTHERS THEN NULL; END;
    END LOOP;

    DBMS_OUTPUT.PUT_LINE('Đã revoke toàn bộ role khỏi users.');
END;
/

-- =====================================================
-- 2. Drop Views
-- =====================================================
BEGIN EXECUTE IMMEDIATE 'DROP VIEW ADMIN.V_HSBA_DV_KTV'; EXCEPTION WHEN OTHERS THEN NULL; END;
/
BEGIN EXECUTE IMMEDIATE 'DROP VIEW ADMIN.V_NHAN_VIEN_SELF'; EXCEPTION WHEN OTHERS THEN NULL; END;
/
BEGIN EXECUTE IMMEDIATE 'DROP VIEW ADMIN.V_BENH_NHAN_SELF'; EXCEPTION WHEN OTHERS THEN NULL; END;
/

-- =====================================================
-- 3. Drop Materialized Views
-- =====================================================
BEGIN EXECUTE IMMEDIATE 'DROP MATERIALIZED VIEW ADMIN.MV_BACSI_LIST'; EXCEPTION WHEN OTHERS THEN NULL; END;
/
BEGIN EXECUTE IMMEDIATE 'DROP MATERIALIZED VIEW ADMIN.MV_KTV_LIST'; EXCEPTION WHEN OTHERS THEN NULL; END;
/

-- =====================================================
-- 4. Drop Roles
-- =====================================================
BEGIN EXECUTE IMMEDIATE 'DROP ROLE RL_KYTHUATVIEN'; EXCEPTION WHEN OTHERS THEN NULL; END;
/
BEGIN EXECUTE IMMEDIATE 'DROP ROLE RL_BENHNHAN'; EXCEPTION WHEN OTHERS THEN NULL; END;
/
BEGIN EXECUTE IMMEDIATE 'DROP ROLE RL_DIEUPHOIVIEN'; EXCEPTION WHEN OTHERS THEN NULL; END;
/
BEGIN EXECUTE IMMEDIATE 'DROP ROLE RL_BACSI'; EXCEPTION WHEN OTHERS THEN NULL; END;
/

BEGIN
    DBMS_OUTPUT.PUT_LINE('=== Hoàn thành rollback RBAC ===');
END;
/
