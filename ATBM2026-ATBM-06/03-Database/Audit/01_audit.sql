-- ==========================================
-- 2. THỰC HIỆN KIỂM TOÁN DÙNG STANDARD AUDIT
-- ==========================================
-- Xóa cài đặt cũ (nếu muốn reset)
NOAUDIT ALL ON admin.hsba;
NOAUDIT ALL ON admin.don_thuoc;
NOAUDIT ALL ON admin.hsba_dv;
NOAUDIT ALL ON admin.benh_nhan;
NOAUDIT SESSION BY NV0051;

AUDIT SELECT ON admin.hsba BY ACCESS;
AUDIT UPDATE ON admin.don_thuoc BY ACCESS WHENEVER NOT SUCCESSFUL;
AUDIT DELETE ON admin.hsba_dv BY ACCESS WHENEVER SUCCESSFUL;
AUDIT SESSION BY NV0051;
AUDIT INSERT ON admin.benh_nhan BY ACCESS WHENEVER SUCCESSFUL;

-- ==========================================
-- 3. THỰC HIỆN KIỂM TOÁN DÙNG UNIFIED AUDIT
-- ==========================================

-- 3a. Audit UPDATE trên các cột cụ thể của don_thuoc
CREATE AUDIT POLICY UNIFIED_AUDIT_UPDATE_DONTHUOC
  ACTIONS UPDATE ON admin.don_thuoc;

AUDIT POLICY UNIFIED_AUDIT_UPDATE_DONTHUOC;


-- 3b,3c. Audit cập nhật trên hsba
CREATE AUDIT POLICY UNIFIED_AUDIT_ILLEGAL_UPDATE_HSBA
  ACTIONS UPDATE ON admin.hsba;

AUDIT POLICY UNIFIED_AUDIT_ILLEGAL_UPDATE_HSBA;


-- 3d. Audit DML trên hsba_dv (Trừ user ADMIN)
CREATE AUDIT POLICY UNIFIED_AUDIT_ILLEGAL_DML_HSBA_DV
  ACTIONS INSERT ON admin.hsba_dv,
          UPDATE ON admin.hsba_dv,
          DELETE ON admin.hsba_dv
  WHEN 'SYS_CONTEXT(''USERENV'', ''SESSION_USER'') <> ''ADMIN'''
  EVALUATE PER STATEMENT;

AUDIT POLICY UNIFIED_AUDIT_ILLEGAL_DML_HSBA_DV;