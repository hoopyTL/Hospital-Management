-- Script kiểm thử Audit hoạt động đúng không
-- Chạy bằng quyền ADMIN

-- ============================================================================
-- 1. KIỂM TRA TRẠNG THÁI AUDIT SYSTEM
-- ============================================================================
PROMPT === 1. KIỂM TRA CẤU HÌNH AUDIT TRAIL ===
SELECT name, value 
FROM v$parameter 
WHERE name = 'audit_trail';
-- ============================================================================
-- SCRIPT 03: KIỂM THỬ AUDIT HOẠT ĐỘNG
-- Chạy bằng quyền ADMIN
-- ============================================================================

SET SERVEROUTPUT ON;

PROMPT === 1. KIỂM TRA CẤU HÌNH AUDIT TRAIL HỆ THỐNG ===
SELECT name, value 
FROM v$parameter 
WHERE name = 'audit_trail';

PROMPT === 2. KIỂM TRA STANDARD AUDIT ĐANG ÁP DỤNG TRÊN BẢNG ===
BEGIN
  FOR rec IN (SELECT OBJECT_NAME, SEL, UPD, DEL 
              FROM dba_obj_audit_opts 
              WHERE owner = 'ADMIN' AND object_name IN ('HSBA', 'DON_THUOC', 'HSBA_DV')) 
  LOOP
    DBMS_OUTPUT.PUT_LINE('Bang: ' || rec.object_name || ' | Kiem toan SELECT: ' || rec.sel || ' | Kiem toan UPDATE: ' || rec.upd);
  END LOOP;
EXCEPTION WHEN OTHERS THEN
  DBMS_OUTPUT.PUT_LINE('Loi truy van dba_obj_audit_opts: ' || SQLERRM);
END;
/

PROMPT === 3. KIỂM TRA FGA POLICIES ĐÃ TẠO ===
SELECT object_schema, object_name, policy_name, enabled 
FROM dba_audit_policies
WHERE object_schema = 'ADMIN';

PROMPT === 4. KIỂM TRA UNIFIED AUDIT POLICIES ===
BEGIN
  FOR rec IN (SELECT DISTINCT policy_name FROM audit_unified_policies WHERE policy_name LIKE '%AUDIT%') LOOP
    DBMS_OUTPUT.PUT_LINE('Policy: ' || rec.policy_name);
  END LOOP;
EXCEPTION WHEN OTHERS THEN
  DBMS_OUTPUT.PUT_LINE('Chua co Unified Policy nao duoc tao hoac co loi: ' || SQLERRM);
END;
/

-- ============================================================================
-- THỰC HIỆN CÁC THAO TÁC ĐỂ KÍCH HOẠT LOG
-- ============================================================================

PROMPT === 5. KÍCH HOẠT STANDARD AUDIT LOG (SELECT) ===
SELECT COUNT(*) as tong_so_benh_nhan FROM admin.hsba;

PROMPT === 6. KÍCH HOẠT FGA AUDIT LOG (UPDATE) ===
BEGIN
  -- Cập nhật giả để ép FGA ghi log (Policy FGA_AUDIT_UPDATE_DONTHUOC sẽ bắt hành vi này)
  UPDATE admin.don_thuoc 
  SET lieu_dung = lieu_dung 
  WHERE ROWNUM = 1;
  COMMIT;
  DBMS_OUTPUT.PUT_LINE('UPDATE bang don_thuoc thanh cong. (Da kich hoat FGA Log)');
EXCEPTION 
  WHEN OTHERS THEN
    DBMS_OUTPUT.PUT_LINE('UPDATE that bai: ' || SQLERRM);
END;
/

PROMPT === DUMP BỘ NHỚ ĐỆM LOG XUỐNG ĐĨA (BẮT BUỘC ĐỂ XEM LOG NGAY) ===
EXEC DBMS_AUDIT_MGMT.FLUSH_UNIFIED_AUDIT_TRAIL;

-- ============================================================================
-- KIỂM TRA KẾT QUẢ
-- ============================================================================

PROMPT === 7. KẾT QUẢ STANDARD AUDIT TRAIL ===
SELECT USERNAME, OBJ_NAME, ACTION_NAME, TIMESTAMP, RETURNCODE
FROM DBA_AUDIT_TRAIL
WHERE OWNER = 'ADMIN' AND OBJ_NAME = 'HSBA'
ORDER BY TIMESTAMP DESC
FETCH FIRST 10 ROWS ONLY;

PROMPT === 8. KẾT QUẢ FINE-GRAINED AUDIT TRAIL ===
SELECT DB_USER, OBJECT_NAME, POLICY_NAME, STATEMENT_TYPE, SQL_TEXT
FROM DBA_FGA_AUDIT_TRAIL
WHERE OBJECT_SCHEMA = 'ADMIN'
ORDER BY TIMESTAMP DESC
FETCH FIRST 10 ROWS ONLY;

PROMPT === 9. KẾT QUẢ UNIFIED AUDIT TRAIL ===
SELECT DBUSERNAME, ACTION_NAME, OBJECT_NAME, SQL_TEXT, EVENT_TIMESTAMP 
FROM UNIFIED_AUDIT_TRAIL 
WHERE DBUSERNAME = 'ADMIN' AND OBJECT_NAME IN ('HSBA', 'DON_THUOC', 'HSBA_DV')
ORDER BY EVENT_TIMESTAMP DESC
FETCH FIRST 10 ROWS ONLY;

PROMPT === 10. THỐNG KÊ TỔNG QUAN ===
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
WHERE DBUSERNAME = 'ADMIN' AND OBJECT_NAME IN ('HSBA', 'DON_THUOC', 'HSBA_DV');

PROMPT ===== KIỂM THỬ HOÀN THÀNH =====
PROMPT === 2. KIỂM TRA STANDARD AUDIT STATEMENTS ===
-- Check if standard audit is enabled
SELECT * FROM dba_audit_object WHERE owner = 'ADMIN';

-- Fallback if no audit_object records
BEGIN
  FOR rec IN (SELECT * FROM dba_audit_object WHERE owner = 'ADMIN') LOOP
    DBMS_OUTPUT.PUT_LINE('Audit object found: ' || rec.object_name);
  END LOOP;
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE = -942 THEN
    DBMS_OUTPUT.PUT_LINE('Standard audit not yet active or no DML_AUDIT_OBJECT records');
  ELSE
    DBMS_OUTPUT.PUT_LINE('Error: ' || SQLERRM);
  END IF;
END;
/

-- ============================================================================
-- 2. KIỂM TRA FINE-GRAINED AUDIT (FGA) POLICIES
-- ============================================================================
PROMPT === 3. KIỂM TRA FGA POLICIES ===
SELECT object_schema, object_name, policy_name, enabled 
FROM dba_audit_policies
WHERE object_schema = 'ADMIN';

-- ============================================================================
-- 3. KIỂM TRA UNIFIED AUDIT POLICIES (if using unified audit)
-- ============================================================================
PROMPT === 4. KIỂM TRA UNIFIED AUDIT POLICIES ===
BEGIN
  FOR rec IN (SELECT * FROM dba_audit_policies WHERE policy_name LIKE '%AUDIT_HSBA%') LOOP
    DBMS_OUTPUT.PUT_LINE('Policy: ' || rec.policy_name);
  END LOOP;
EXCEPTION WHEN OTHERS THEN
  DBMS_OUTPUT.PUT_LINE('No unified audit policies found or error: ' || SQLERRM);
END;
/

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
