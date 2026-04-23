-- =====================================================
-- 02_vpd_basic.sql
-- Virtual Private Database (Row-Level Security) cơ bản:
--   - HSBA_DV   : KTV chỉ thấy dòng MA_KTV = SESSION_USER  (TC#4)
--   - NHAN_VIEN : NV chỉ thấy chính mình                    (TC#5)
--   - BENH_NHAN : BN chỉ thấy chính mình                    (TC#5)
-- Chú ý: script này sẽ bị OVERRIDE bởi 03_vpd_dpv_bs.sql
-- cho bảng BENH_NHAN và HSBA_DV (đổi policy name +
-- function body để hỗ trợ thêm DPV & BS).
-- Chạy bằng tài khoản ADMIN (DBA)
-- =====================================================

SET SERVEROUTPUT ON;

-- =====================================================
-- 1. Policy Function cho HSBA_DV (cơ bản)
--    KTV chỉ thấy dòng mà MA_KTV = SESSION_USER
--    ADMIN (DBA) thấy tất cả
-- =====================================================
CREATE OR REPLACE FUNCTION fn_policy_hsba_dv (
    p_schema IN VARCHAR2,
    p_table  IN VARCHAR2
) RETURN VARCHAR2
AS
    v_user VARCHAR2(30) := SYS_CONTEXT('USERENV', 'SESSION_USER');
BEGIN
    IF v_user = 'ADMIN' THEN
        RETURN '';
    END IF;
    RETURN 'MA_KTV = ''' || v_user || '''';
END;
/

-- =====================================================
-- 2. Policy Function cho NHAN_VIEN
--    Mỗi NV chỉ thấy chính mình; ADMIN thấy tất cả
-- =====================================================
CREATE OR REPLACE FUNCTION fn_policy_nhanvien (
    p_schema IN VARCHAR2,
    p_table  IN VARCHAR2
) RETURN VARCHAR2
AS
    v_user VARCHAR2(30) := SYS_CONTEXT('USERENV', 'SESSION_USER');
BEGIN
    IF v_user = 'ADMIN' THEN
        RETURN '';
    END IF;
    RETURN 'MA_NV = ''' || v_user || '''';
END;
/

-- =====================================================
-- 3. Policy Function cho BENH_NHAN (cơ bản)
--    Mỗi BN chỉ thấy chính mình; ADMIN thấy tất cả
-- =====================================================
CREATE OR REPLACE FUNCTION fn_policy_benhnhan (
    p_schema IN VARCHAR2,
    p_table  IN VARCHAR2
) RETURN VARCHAR2
AS
    v_user VARCHAR2(30) := SYS_CONTEXT('USERENV', 'SESSION_USER');
BEGIN
    IF v_user = 'ADMIN' THEN
        RETURN '';
    END IF;
    RETURN 'MA_BN = ''' || v_user || '''';
END;
/

-- =====================================================
-- 4. Đăng ký VPD Policies
-- =====================================================

BEGIN
    DBMS_RLS.ADD_POLICY(
        object_schema   => 'ADMIN',
        object_name     => 'HSBA_DV',
        policy_name     => 'POL_HSBA_DV_KTV',
        function_schema => 'ADMIN',
        policy_function => 'FN_POLICY_HSBA_DV',
        statement_types => 'SELECT,UPDATE',
        update_check    => TRUE,
        enable          => TRUE
    );
    DBMS_OUTPUT.PUT_LINE('Đã tạo VPD policy POL_HSBA_DV_KTV.');
EXCEPTION WHEN OTHERS THEN
    DBMS_OUTPUT.PUT_LINE('POL_HSBA_DV_KTV tồn tại / lỗi: ' || SQLERRM);
END;
/

BEGIN
    DBMS_RLS.ADD_POLICY(
        object_schema   => 'ADMIN',
        object_name     => 'NHAN_VIEN',
        policy_name     => 'POL_NHANVIEN_SELF',
        function_schema => 'ADMIN',
        policy_function => 'FN_POLICY_NHANVIEN',
        statement_types => 'SELECT,UPDATE',
        update_check    => TRUE,
        enable          => TRUE
    );
    DBMS_OUTPUT.PUT_LINE('Đã tạo VPD policy POL_NHANVIEN_SELF.');
EXCEPTION WHEN OTHERS THEN
    DBMS_OUTPUT.PUT_LINE('POL_NHANVIEN_SELF tồn tại / lỗi: ' || SQLERRM);
END;
/

BEGIN
    DBMS_RLS.ADD_POLICY(
        object_schema   => 'ADMIN',
        object_name     => 'BENH_NHAN',
        policy_name     => 'POL_BENHNHAN_SELF',
        function_schema => 'ADMIN',
        policy_function => 'FN_POLICY_BENHNHAN',
        statement_types => 'SELECT,UPDATE',
        update_check    => TRUE,
        enable          => TRUE
    );
    DBMS_OUTPUT.PUT_LINE('Đã tạo VPD policy POL_BENHNHAN_SELF.');
EXCEPTION WHEN OTHERS THEN
    DBMS_OUTPUT.PUT_LINE('POL_BENHNHAN_SELF tồn tại / lỗi: ' || SQLERRM);
END;
/

BEGIN
    DBMS_OUTPUT.PUT_LINE('=== Hoàn thành cài đặt VPD cơ bản ===');
END;
/
