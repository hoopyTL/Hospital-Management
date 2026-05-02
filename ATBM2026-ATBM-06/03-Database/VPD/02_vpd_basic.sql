-- =====================================================
-- 02_vpd_basic.sql
-- VPD cơ bản chỉ giữ cho NHAN_VIEN:
--   - DPV/BS chỉ thấy chính mình trên NHAN_VIEN (TC#5)
--   - KTV không dùng VPD: query qua ADMIN.V_NHAN_VIEN_SELF
--   - BN không truy cập NHAN_VIEN
--
-- PHỤ THUỘC: 01_fn_get_role.sql phải chạy trước.
-- Chạy bằng tài khoản ADMIN (DBA)
-- =====================================================

SET SERVEROUTPUT ON;

-- =====================================================
-- 1. Drop các policy cũ từng dùng VPD cho KTV/BN
-- =====================================================
BEGIN DBMS_RLS.DROP_POLICY('ADMIN', 'HSBA_DV',   'POL_HSBA_DV_KTV');   EXCEPTION WHEN OTHERS THEN NULL; END;
/
BEGIN DBMS_RLS.DROP_POLICY('ADMIN', 'BENH_NHAN', 'POL_BENHNHAN_SELF'); EXCEPTION WHEN OTHERS THEN NULL; END;
/

-- =====================================================
-- 2. Policy Function cho NHAN_VIEN
--    DBA/KTV: không thêm predicate VPD.
--    DPV/BS : tự thấy chính mình.
--    Khác  : chặn.
-- =====================================================
CREATE OR REPLACE FUNCTION admin.fn_policy_nhanvien (
    p_schema IN VARCHAR2,
    p_table  IN VARCHAR2
) RETURN VARCHAR2
AS
    v_user VARCHAR2(30) := SYS_CONTEXT('USERENV', 'SESSION_USER');
    v_role VARCHAR2(20) := admin.fn_get_role;
BEGIN
    IF v_role IN ('DBA', 'KYTHUATVIEN') THEN
        RETURN NULL;
    ELSIF v_role IN ('DIEUPHOIVIEN', 'BACSI') THEN
        RETURN 'MA_NV = ''' || v_user || '''';
    ELSE
        RETURN '1=0';
    END IF;
END;
/

-- =====================================================
-- 3. Đăng ký VPD policy cho NHAN_VIEN
-- =====================================================
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
    DBMS_OUTPUT.PUT_LINE('Đã tạo VPD policy POL_NHANVIEN_SELF cho DPV/BS.');
EXCEPTION WHEN OTHERS THEN
    DBMS_OUTPUT.PUT_LINE('POL_NHANVIEN_SELF tồn tại / lỗi: ' || SQLERRM);
END;
/

BEGIN
    DBMS_OUTPUT.PUT_LINE('=== Hoàn thành VPD cơ bản: không dùng VPD cho KTV/BN ===');
END;
/
