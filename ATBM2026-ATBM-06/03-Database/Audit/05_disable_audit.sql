-- =====================================================================
-- Script tắt Audit
-- Chạy bằng quyền ADMIN hoặc SYSDBA
-- =====================================================================

PROMPT ===== TAT AUDIT SYSTEM =====

-- =====================================================================
-- 1. TAT STANDARD AUDIT
-- =====================================================================

PROMPT === 1. TAT STANDARD AUDIT ===

-- Tắt Standard Audit trên các bảng
NOAUDIT SELECT ON admin.hsba BY NV0021;
NOAUDIT UPDATE ON admin.don_thuoc;
NOAUDIT DELETE ON admin.hsba_dv;
NOAUDIT EXECUTE ON admin.SP_XOA_BENHAN;
NOAUDIT ALL ON admin.V_THONGTIN_BENHNHAN;

-- Tắt tất cả Standard Audit
NOAUDIT ALL;

COMMIT;

-- =====================================================================
-- 2. TAT FINE-GRAINED AUDIT (FGA) POLICIES
-- =====================================================================

PROMPT === 2. TAT FGA POLICIES ===

-- Tắt FGA Policy trên don_thuoc
BEGIN
  DBMS_FGA.DROP_POLICY(
    object_schema      => 'admin',
    object_name        => 'don_thuoc',
    policy_name        => 'FGA_AUDIT_UPDATE_DONTHUOC'
  );
  DBMS_OUTPUT.PUT_LINE('Tắt FGA Policy FGA_AUDIT_UPDATE_DONTHUOC thành công');
EXCEPTION
  WHEN OTHERS THEN
    DBMS_OUTPUT.PUT_LINE('Policy FGA_AUDIT_UPDATE_DONTHUOC không tồn tại hoặc đã xóa');
END;
/

-- Tắt FGA Policy trên hsba
BEGIN
  DBMS_FGA.DROP_POLICY(
    object_schema      => 'admin',
    object_name        => 'hsba',
    policy_name        => 'FGA_AUDIT_ILLEGAL_UPDATE_HSBA'
  );
  DBMS_OUTPUT.PUT_LINE('Tắt FGA Policy FGA_AUDIT_ILLEGAL_UPDATE_HSBA thành công');
EXCEPTION
  WHEN OTHERS THEN
    DBMS_OUTPUT.PUT_LINE('Policy FGA_AUDIT_ILLEGAL_UPDATE_HSBA không tồn tại hoặc đã xóa');
END;
/

-- Tắt FGA Policy trên hsba_dv
BEGIN
  DBMS_FGA.DROP_POLICY(
    object_schema      => 'admin',
    object_name        => 'hsba_dv',
    policy_name        => 'FGA_AUDIT_ILLEGAL_DML_HSBA_DV'
  );
  DBMS_OUTPUT.PUT_LINE('Tắt FGA Policy FGA_AUDIT_ILLEGAL_DML_HSBA_DV thành công');
EXCEPTION
  WHEN OTHERS THEN
    DBMS_OUTPUT.PUT_LINE('Policy FGA_AUDIT_ILLEGAL_DML_HSBA_DV không tồn tại hoặc đã xóa');
END;
/

-- =====================================================================
-- 3. TAT UNIFIED AUDIT POLICIES
-- =====================================================================

PROMPT === 3. TAT UNIFIED AUDIT POLICIES ===

-- Tắt Unified Audit Policy
BEGIN
  EXECUTE IMMEDIATE 'NOAUDIT POLICY AUDIT_HSBA_UPDATE_CHANDOAN';
  DBMS_OUTPUT.PUT_LINE('Tắt Unified Audit Policy AUDIT_HSBA_UPDATE_CHANDOAN thành công');
EXCEPTION
  WHEN OTHERS THEN
    DBMS_OUTPUT.PUT_LINE('NOAUDIT POLICY không thành công: ' || SQLERRM);
END;
/

-- Drop Unified Audit Policy
BEGIN
  EXECUTE IMMEDIATE 'DROP AUDIT POLICY AUDIT_HSBA_UPDATE_CHANDOAN';
  DBMS_OUTPUT.PUT_LINE('Xóa Audit Policy AUDIT_HSBA_UPDATE_CHANDOAN thành công');
EXCEPTION
  WHEN OTHERS THEN
    DBMS_OUTPUT.PUT_LINE('Policy AUDIT_HSBA_UPDATE_CHANDOAN không tồn tại hoặc đã xóa: ' || SQLERRM);
END;
/

-- =====================================================================
-- 4. XOA DU LIEU AUDIT LOG
-- =====================================================================

PROMPT === 4. XOA AUDIT LOG CU ===

-- Xóa audit log cũ hơn 30 ngày
DECLARE
  v_rows_deleted NUMBER;
BEGIN
  DELETE FROM admin.AUDIT_LOG 
  WHERE action_timestamp < SYSDATE - 30;
  v_rows_deleted := SQL%ROWCOUNT;
  COMMIT;
  DBMS_OUTPUT.PUT_LINE(v_rows_deleted || ' bản ghi audit cũ hơn 30 ngày đã bị xóa');
EXCEPTION
  WHEN OTHERS THEN
    DBMS_OUTPUT.PUT_LINE('Lỗi xóa audit log cũ: ' || SQLERRM);
END;
/

-- Xóa tất cả audit log (nếu cần)
-- DELETE FROM admin.AUDIT_LOG;
-- COMMIT;

-- =====================================================================
-- 5. KIEM TRA TRANG THAI TAT AUDIT
-- =====================================================================

PROMPT === 5. KIEM TRA TRANG THAI AUDIT ===

PROMPT === Trang thai audit_trail:
SELECT name, value FROM v$parameter WHERE name = 'audit_trail';

PROMPT === Danh sach Standard Audit:
SELECT owner, object_name, grantor, privilege 
FROM dba_audit_object 
WHERE owner = 'ADMIN';

PROMPT === Danh sach FGA Policies:
SELECT object_schema, object_name, policy_name, enabled 
FROM dba_audit_policies
WHERE object_schema = 'ADMIN';

PROMPT === Danh sach Unified Audit Policies:
SELECT policy_name, enabled
FROM audit_unified_policies;

PROMPT === So ban ghi AUDIT_LOG:
SELECT COUNT(*) as tong_ban_ghi FROM admin.AUDIT_LOG;

PROMPT === So ban ghi Standard Audit Trail:
SELECT COUNT(*) as standard_audit FROM DBA_AUDIT_TRAIL WHERE OWNER = 'ADMIN';

PROMPT === So ban ghi FGA Audit Trail:
SELECT COUNT(*) as fga_audit FROM DBA_FGA_AUDIT_TRAIL WHERE OBJECT_SCHEMA = 'ADMIN';

PROMPT ===== TAT AUDIT SYSTEM HOAN THANH =====
