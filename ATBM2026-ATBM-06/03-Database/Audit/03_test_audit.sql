-- Script kiểm thử Audit hoạt động đúng không
-- Chạy bằng quyền ADMIN

-- ============================================================================
-- 1. KIỂM TRA TRẠNG THÁI AUDIT SYSTEM
-- ============================================================================
PROMPT === 1. KIỂM TRA CẤU HÌNH AUDIT TRAIL ===
SELECT name, value 
FROM v$parameter 
WHERE name = 'audit_trail';

PROMPT === 2. KIỂM TRA STANDARD AUDIT STATEMENTS ===
SELECT owner, object_name, object_type, grantor, privilege, grantee 
FROM dba_audit_object 
WHERE owner = 'ADMIN';

-- ============================================================================
-- 2. KIỂM TRA FINE-GRAINED AUDIT (FGA) POLICIES
-- ============================================================================
PROMPT === 3. KIỂM TRA FGA POLICIES ===
SELECT object_schema, object_name, policy_name, enabled 
FROM dba_audit_policies
WHERE object_schema = 'ADMIN';

-- ============================================================================
-- 3. KIỂM TRA UNIFIED AUDIT POLICIES
-- ============================================================================
PROMPT === 4. KIỂM TRA UNIFIED AUDIT POLICIES ===
SELECT policy_name, enabled
FROM audit_unified_policies
WHERE policy_name LIKE '%AUDIT_HSBA%';

-- ============================================================================
-- 4. THỰC HIỆN CÁC THAO TÁC ĐỂ KÍCH HOẠT AUDIT
-- ============================================================================
PROMPT === 5. THỰC HIỆN THAO TÁC SELECT (kiểm tra audit) ===
SELECT COUNT(*) as tong_so_benh_nhan FROM admin.hsba;

PROMPT === 6. THỰC HIỆN THAO TÁC UPDATE (kiểm tra FGA) ===
-- Thử cập nhật một record (có thể fail nếu không có quyền)
BEGIN
  UPDATE admin.don_thuoc 
  SET ten_thuoc = 'Test Update' 
  WHERE ROWNUM = 1;
  COMMIT;
  DBMS_OUTPUT.PUT_LINE('UPDATE thành công');
EXCEPTION 
  WHEN OTHERS THEN
    DBMS_OUTPUT.PUT_LINE('UPDATE thất bại: ' || SQLERRM);
END;
/

-- ============================================================================
-- 5. KIỂM TRA BẢN GHI AUDIT ĐÃ GHI
-- ============================================================================
PROMPT === 7. KIỂM TRA STANDARD AUDIT TRAIL ===
SELECT OS_USERNAME, USERNAME, OBJ_NAME, ACTION_NAME, TIMESTAMP, RETURNCODE
FROM DBA_AUDIT_TRAIL
WHERE OWNER = 'ADMIN' 
ORDER BY TIMESTAMP DESC
FETCH FIRST 10 ROWS ONLY;

PROMPT === 8. KIỂM TRA FINE-GRAINED AUDIT TRAIL ===
SELECT DB_USER, OBJECT_NAME, POLICY_NAME, STATEMENT_TYPE, SQL_TEXT, TIMESTAMP 
FROM DBA_FGA_AUDIT_TRAIL
WHERE OBJECT_SCHEMA = 'ADMIN'
ORDER BY TIMESTAMP DESC
FETCH FIRST 10 ROWS ONLY;

PROMPT === 9. KIỂM TRA UNIFIED AUDIT TRAIL ===
SELECT DBUSERNAME, ACTION_NAME, OBJECT_NAME, SQL_TEXT, EVENT_TIMESTAMP 
FROM UNIFIED_AUDIT_TRAIL 
WHERE DBUSERNAME IN ('ADMIN', 'BAC_SI', 'NV0021', 'NV_01')
ORDER BY EVENT_TIMESTAMP DESC
FETCH FIRST 10 ROWS ONLY;

-- ============================================================================
-- 6. THỐNG KÊ AUDIT
-- ============================================================================
PROMPT === 10. THỐNG KÊ SỐ BẢN GHI AUDIT ===
SELECT 'Standard Audit' as audit_type, COUNT(*) as so_ban_ghi 
FROM DBA_AUDIT_TRAIL 
WHERE OWNER = 'ADMIN'
UNION ALL
SELECT 'FGA Audit' as audit_type, COUNT(*) as so_ban_ghi
FROM DBA_FGA_AUDIT_TRAIL 
WHERE OBJECT_SCHEMA = 'ADMIN'
UNION ALL
SELECT 'Unified Audit' as audit_type, COUNT(*) as so_ban_ghi
FROM UNIFIED_AUDIT_TRAIL 
WHERE DBUSERNAME IN ('ADMIN', 'BAC_SI', 'NV0021', 'NV_01');

PROMPT ===== KIỂM THỬ HOÀN THÀNH =====
