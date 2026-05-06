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
-- 2. TAT UNIFIED AUDIT
-- Tắt Unified Audit Policy
-- =====================================================================
NOAUDIT POLICY UNIFIED_AUDIT_UPDATE_DONTHUOC;
NOAUDIT POLICY UNIFIED_AUDIT_ILLEGAL_UPDATE_HSBA;
NOAUDIT POLICY UNIFIED_AUDIT_ILLEGAL_DML_HSBA_DV;

DROP AUDIT POLICY UNIFIED_AUDIT_UPDATE_DONTHUOC;
DROP AUDIT POLICY UNIFIED_AUDIT_ILLEGAL_UPDATE_HSBA;
DROP AUDIT POLICY UNIFIED_AUDIT_ILLEGAL_DML_HSBA_DV;
COMMIT;