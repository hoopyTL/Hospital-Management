-- =====================================================
-- 01_create_roles.sql
-- Tạo tất cả các Role cho hệ thống RBAC:
--   RL_KYTHUATVIEN  - TC#4
--   RL_BENHNHAN     - TC#5
--   RL_DIEUPHOIVIEN - TC#2
--   RL_BACSI        - TC#3
-- Chạy bằng tài khoản ADMIN (DBA)
-- =====================================================

SET SERVEROUTPUT ON;

DECLARE
    v_count NUMBER;
    PROCEDURE create_role_if_not_exists(p_role IN VARCHAR2) IS
    BEGIN
        SELECT COUNT(*) INTO v_count FROM dba_roles WHERE role = p_role;
        IF v_count = 0 THEN
            EXECUTE IMMEDIATE 'CREATE ROLE ' || p_role;
            DBMS_OUTPUT.PUT_LINE('Đã tạo role ' || p_role || '.');
        ELSE
            DBMS_OUTPUT.PUT_LINE('Role ' || p_role || ' đã tồn tại.');
        END IF;
    END;
BEGIN
    create_role_if_not_exists('RL_KYTHUATVIEN');
    create_role_if_not_exists('RL_BENHNHAN');
    create_role_if_not_exists('RL_DIEUPHOIVIEN');
    create_role_if_not_exists('RL_BACSI');

    DBMS_OUTPUT.PUT_LINE('=== Hoàn thành tạo Roles ===');
END;
/
