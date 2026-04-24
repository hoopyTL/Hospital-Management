-- =====================================================
-- 03_vpd_dpv_bs.sql
-- Mở rộng VPD để ép thỏa TC#2 (DPV) và TC#3 (BS).
-- KTV và BN không dùng VPD để lọc dòng; 2 nhóm này truy cập qua view
-- được tạo trong RBAC/02_grant_ktv_bn.sql.
--
-- Thay thế policy cũ nếu còn tồn tại từ bản script trước:
--   POL_BENHNHAN_SELF  -> POL_BN_RLS
--   POL_HSBA_DV_KTV    -> POL_HSBA_DV_RLS
-- Thêm mới:
--   POL_HSBA_RLS       (fn_policy_hsba)
--   POL_DON_THUOC_RLS  (fn_policy_don_thuoc)
-- Giữ nguyên:
--   POL_NHANVIEN_SELF  (fn_policy_nhanvien)
--
-- PHỤ THUỘC: 01_fn_get_role.sql phải chạy trước.
-- Chạy bằng tài khoản ADMIN (DBA)
-- =====================================================

SET SERVEROUTPUT ON;

-- =====================================================
-- 1. Drop 2 policy cũ trước khi thay function
-- =====================================================
BEGIN
    DBMS_RLS.DROP_POLICY('ADMIN', 'BENH_NHAN', 'POL_BENHNHAN_SELF');
    DBMS_OUTPUT.PUT_LINE('Đã xóa policy cũ POL_BENHNHAN_SELF.');
EXCEPTION WHEN OTHERS THEN
    DBMS_OUTPUT.PUT_LINE('POL_BENHNHAN_SELF không tồn tại hoặc đã xóa.');
END;
/

BEGIN
    DBMS_RLS.DROP_POLICY('ADMIN', 'HSBA_DV', 'POL_HSBA_DV_KTV');
    DBMS_OUTPUT.PUT_LINE('Đã xóa policy cũ POL_HSBA_DV_KTV.');
EXCEPTION WHEN OTHERS THEN
    DBMS_OUTPUT.PUT_LINE('POL_HSBA_DV_KTV không tồn tại hoặc đã xóa.');
END;
/

-- Drop luôn policy mới (nếu đã chạy script trước đó) để idempotent
BEGIN DBMS_RLS.DROP_POLICY('ADMIN', 'BENH_NHAN',  'POL_BN_RLS');        EXCEPTION WHEN OTHERS THEN NULL; END;
/
BEGIN DBMS_RLS.DROP_POLICY('ADMIN', 'HSBA_DV',    'POL_HSBA_DV_RLS');   EXCEPTION WHEN OTHERS THEN NULL; END;
/
BEGIN DBMS_RLS.DROP_POLICY('ADMIN', 'HSBA',       'POL_HSBA_RLS');      EXCEPTION WHEN OTHERS THEN NULL; END;
/
BEGIN DBMS_RLS.DROP_POLICY('ADMIN', 'DON_THUOC',  'POL_DON_THUOC_RLS'); EXCEPTION WHEN OTHERS THEN NULL; END;
/

-- =====================================================
-- 2. Policy function BENH_NHAN
--    DBA           : ''
--    DIEUPHOIVIEN  : ''                      (TC#2)
--    BACSI         : BN thuộc HSBA của mình  (TC#3d)
--    BENHNHAN      : ''                      (lọc bằng V_BENH_NHAN_SELF)
--    khác          : '1=0'
-- =====================================================
CREATE OR REPLACE FUNCTION admin.fn_policy_benhnhan (
    p_schema IN VARCHAR2,
    p_table  IN VARCHAR2
) RETURN VARCHAR2
AS
    v_user VARCHAR2(30) := SYS_CONTEXT('USERENV', 'SESSION_USER');
    v_role VARCHAR2(20) := admin.fn_get_role;
BEGIN
    IF v_role = 'DBA' THEN
        RETURN '';
    ELSIF v_role = 'DIEUPHOIVIEN' THEN
        RETURN '';
    ELSIF v_role = 'BACSI' THEN
        RETURN 'MA_BN IN (SELECT MA_BN FROM ADMIN.HSBA WHERE MA_BS = ''' || v_user || ''')';
    ELSIF v_role = 'BENHNHAN' THEN
        RETURN '';
    ELSE
        RETURN '1=0';
    END IF;
END;
/

-- =====================================================
-- 3. Policy function HSBA_DV
--    DBA           : ''
--    DIEUPHOIVIEN  : ''                         (TC#2)
--    BACSI         : HSBA do mình phụ trách      (TC#3)
--    KYTHUATVIEN   : ''                         (lọc bằng V_HSBA_DV_KTV)
--    khác          : '1=0'
-- =====================================================
CREATE OR REPLACE FUNCTION admin.fn_policy_hsba_dv (
    p_schema IN VARCHAR2,
    p_table  IN VARCHAR2
) RETURN VARCHAR2
AS
    v_user VARCHAR2(30) := SYS_CONTEXT('USERENV', 'SESSION_USER');
    v_role VARCHAR2(20) := admin.fn_get_role;
BEGIN
    IF v_role = 'DBA' THEN
        RETURN '';
    ELSIF v_role = 'DIEUPHOIVIEN' THEN
        RETURN '';
    ELSIF v_role = 'BACSI' THEN
        RETURN 'MA_HSBA IN (SELECT MA_HSBA FROM ADMIN.HSBA WHERE MA_BS = ''' || v_user || ''')';
    ELSIF v_role = 'KYTHUATVIEN' THEN
        RETURN '';
    ELSE
        RETURN '1=0';
    END IF;
END;
/

-- =====================================================
-- 4. Policy function HSBA (mới)
-- =====================================================
CREATE OR REPLACE FUNCTION admin.fn_policy_hsba (
    p_schema IN VARCHAR2,
    p_table  IN VARCHAR2
) RETURN VARCHAR2
AS
    v_user VARCHAR2(30) := SYS_CONTEXT('USERENV', 'SESSION_USER');
    v_role VARCHAR2(20) := admin.fn_get_role;
BEGIN
    IF v_role = 'DBA' THEN
        RETURN '';
    ELSIF v_role = 'DIEUPHOIVIEN' THEN
        RETURN '';
    ELSIF v_role = 'BACSI' THEN
        RETURN 'MA_BS = ''' || v_user || '''';
    ELSE
        RETURN '1=0';
    END IF;
END;
/

-- =====================================================
-- 5. Policy function DON_THUOC (mới)
-- =====================================================
CREATE OR REPLACE FUNCTION admin.fn_policy_don_thuoc (
    p_schema IN VARCHAR2,
    p_table  IN VARCHAR2
) RETURN VARCHAR2
AS
    v_user VARCHAR2(30) := SYS_CONTEXT('USERENV', 'SESSION_USER');
    v_role VARCHAR2(20) := admin.fn_get_role;
BEGIN
    IF v_role = 'DBA' THEN
        RETURN '';
    ELSIF v_role = 'BACSI' THEN
        RETURN 'MA_HSBA IN (SELECT MA_HSBA FROM ADMIN.HSBA WHERE MA_BS = ''' || v_user || ''')';
    ELSE
        RETURN '1=0';
    END IF;
END;
/

-- =====================================================
-- 6. Đăng ký các policy
-- =====================================================

BEGIN
    DBMS_RLS.ADD_POLICY(
        object_schema   => 'ADMIN',
        object_name     => 'BENH_NHAN',
        policy_name     => 'POL_BN_RLS',
        function_schema => 'ADMIN',
        policy_function => 'FN_POLICY_BENHNHAN',
        statement_types => 'SELECT,INSERT,UPDATE,DELETE',
        update_check    => TRUE,
        enable          => TRUE
    );
    DBMS_OUTPUT.PUT_LINE('Đã thêm policy POL_BN_RLS.');
END;
/

BEGIN
    DBMS_RLS.ADD_POLICY(
        object_schema   => 'ADMIN',
        object_name     => 'HSBA_DV',
        policy_name     => 'POL_HSBA_DV_RLS',
        function_schema => 'ADMIN',
        policy_function => 'FN_POLICY_HSBA_DV',
        statement_types => 'SELECT,INSERT,UPDATE,DELETE',
        update_check    => TRUE,
        enable          => TRUE
    );
    DBMS_OUTPUT.PUT_LINE('Đã thêm policy POL_HSBA_DV_RLS.');
END;
/

BEGIN
    DBMS_RLS.ADD_POLICY(
        object_schema   => 'ADMIN',
        object_name     => 'HSBA',
        policy_name     => 'POL_HSBA_RLS',
        function_schema => 'ADMIN',
        policy_function => 'FN_POLICY_HSBA',
        statement_types => 'SELECT,INSERT,UPDATE,DELETE',
        update_check    => TRUE,
        enable          => TRUE
    );
    DBMS_OUTPUT.PUT_LINE('Đã thêm policy POL_HSBA_RLS.');
END;
/

BEGIN
    DBMS_RLS.ADD_POLICY(
        object_schema   => 'ADMIN',
        object_name     => 'DON_THUOC',
        policy_name     => 'POL_DON_THUOC_RLS',
        function_schema => 'ADMIN',
        policy_function => 'FN_POLICY_DON_THUOC',
        statement_types => 'SELECT,INSERT,UPDATE,DELETE',
        update_check    => TRUE,
        enable          => TRUE
    );
    DBMS_OUTPUT.PUT_LINE('Đã thêm policy POL_DON_THUOC_RLS.');
END;
/

BEGIN
    DBMS_OUTPUT.PUT_LINE('=== Hoàn thành cài đặt VPD mở rộng ===');
END;
/
