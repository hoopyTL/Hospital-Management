-- =====================================================================
-- Script tắt Audit
-- Chạy bằng quyền ADMIN hoặc SYSDBA
-- =====================================================================

-- =====================================================================
-- 1. TAT STANDARD AUDIT
-- =====================================================================

-- Tắt Standard Audit trên các bảng
NOAUDIT SELECT ON admin.hsba;
NOAUDIT UPDATE ON admin.don_thuoc;
NOAUDIT DELETE ON admin.hsba_dv;
NOAUDIt SESSION BY NV0051;
NOAUDIT ALL ON admin.benh_nhan;

-- Tắt tất cả Standard Audit
NOAUDIT ALL;

COMMIT;

-- =====================================================================
-- 2. TAT FINE-GRAINED AUDIT (FGA) POLICIES
-- =====================================================================

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