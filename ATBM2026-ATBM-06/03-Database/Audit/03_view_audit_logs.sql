-- ============================================================================
-- SCRIPT 03: XEM CÁC AUDIT LOGS
-- Hiển thị các records từ Standard Audit và Fine-Grained Audit (FGA)
-- Chạy bằng quyền ADMIN hoặc SYS
-- ============================================================================

SET PAGESIZE 100
SET LINESIZE 200
COLUMN sessionid FORMAT 9999
COLUMN username FORMAT A15
COLUMN owner FORMAT A15
COLUMN obj_name FORMAT A20
COLUMN action_name FORMAT A15
COLUMN timestamp FORMAT A25
COLUMN sql_text FORMAT A50

-- ============================================================================
-- 1. HIỂN THỊ STANDARD AUDIT LOGS
-- ============================================================================

PROMPT ======================================================================
PROMPT 1. STANDARD AUDIT LOGS (DBA_AUDIT_TRAIL)
PROMPT ======================================================================

-- 1a. SELECT trên bảng HSBA
PROMPT
PROMPT 1a. Audit SELECT trên HSBA:
SELECT 
    sessionid,
    username,
    owner,
    obj_name,
    action_name,
    TO_CHAR(timestamp, 'YYYY-MM-DD HH24:MI:SS') AS timestamp,
    SUBSTR(sql_text, 1, 50) AS sql_text
FROM dba_audit_trail
WHERE obj_name = 'HSBA' 
  AND action_name = 'SELECT'
ORDER BY timestamp DESC
FETCH FIRST 10 ROWS ONLY;

-- 1b. UPDATE thất bại trên DON_THUOC
PROMPT
PROMPT 1b. Audit UPDATE thất bại trên DON_THUOC:
SELECT 
    sessionid,
    username,
    owner,
    obj_name,
    action_name,
    returncode,
    TO_CHAR(timestamp, 'YYYY-MM-DD HH24:MI:SS') AS timestamp,
    sql_text
FROM dba_audit_trail
WHERE obj_name = 'DON_THUOC' 
  AND action_name = 'UPDATE'
  AND returncode != 0
ORDER BY timestamp DESC
FETCH FIRST 10 ROWS ONLY;

-- 1c. DELETE thành công trên HSBA_DV
PROMPT
PROMPT 1c. Audit DELETE thành công trên HSBA_DV:
SELECT 
    sessionid,
    username,
    owner,
    obj_name,
    action_name,
    TO_CHAR(timestamp, 'YYYY-MM-DD HH24:MI:SS') AS timestamp
FROM dba_audit_trail
WHERE obj_name = 'HSBA_DV' 
  AND action_name = 'DELETE'
  AND returncode = 0
ORDER BY timestamp DESC
FETCH FIRST 10 ROWS ONLY;

-- 1d. INSERT thành công trên BENH_NHAN
PROMPT
PROMPT 1d. Audit INSERT thành công trên BENH_NHAN:
SELECT 
    sessionid,
    username,
    owner,
    obj_name,
    action_name,
    TO_CHAR(timestamp, 'YYYY-MM-DD HH24:MI:SS') AS timestamp,
    SUBSTR(sql_text, 1, 60) AS sql_text
FROM dba_audit_trail
WHERE obj_name = 'BENH_NHAN' 
  AND action_name = 'INSERT'
  AND returncode = 0
ORDER BY timestamp DESC
FETCH FIRST 10 ROWS ONLY;

-- ============================================================================
-- 2. HIỂN THỊ FINE-GRAINED AUDIT (FGA) LOGS
-- ============================================================================

PROMPT
PROMPT ======================================================================
PROMPT 2. FINE-GRAINED AUDIT LOGS (DBA_FGA_AUDIT_TRAIL)
PROMPT ======================================================================

-- 2a. FGA trên DON_THUOC (UPDATE)
PROMPT
PROMPT 2a. FGA UPDATE trên DON_THUOC:
SELECT 
    db_user,
    object_schema,
    object_name,
    policy_name,
    statement_type,
    TO_CHAR(timestamp, 'YYYY-MM-DD HH24:MI:SS') AS timestamp,
    SUBSTR(sql_text, 1, 60) AS sql_text
FROM dba_fga_audit_trail
WHERE policy_name = 'FGA_AUDIT_UPDATE_DONTHUOC'
ORDER BY timestamp DESC
FETCH FIRST 10 ROWS ONLY;

-- 2b. FGA trên HSBA (kiểm tra unauthorized UPDATE)
PROMPT
PROMPT 2b. FGA Illegal UPDATE trên HSBA:
SELECT 
    db_user,
    object_schema,
    object_name,
    policy_name,
    statement_type,
    TO_CHAR(timestamp, 'YYYY-MM-DD HH24:MI:SS') AS timestamp,
    SUBSTR(sql_text, 1, 60) AS sql_text
FROM dba_fga_audit_trail
WHERE policy_name = 'FGA_AUDIT_ILLEGAL_UPDATE_HSBA'
ORDER BY timestamp DESC
FETCH FIRST 10 ROWS ONLY;

-- 2c. FGA trên HSBA_DV (DML ngoài giờ hành chính)
PROMPT
PROMPT 2c. FGA DML ngoài giờ hành chính trên HSBA_DV:
SELECT 
    db_user,
    object_schema,
    object_name,
    policy_name,
    statement_type,
    TO_CHAR(timestamp, 'YYYY-MM-DD HH24:MI:SS') AS timestamp,
    SUBSTR(sql_text, 1, 60) AS sql_text
FROM dba_fga_audit_trail
WHERE policy_name = 'FGA_AUDIT_ILLEGAL_DML_HSBA_DV'
ORDER BY timestamp DESC
FETCH FIRST 10 ROWS ONLY;

-- ============================================================================
-- 3. TÓMLƯỢC AUDIT STATISTICS
-- ============================================================================

PROMPT
PROMPT ======================================================================
PROMPT 3. TÓMLƯỢC AUDIT STATISTICS
PROMPT ======================================================================

PROMPT
PROMPT 3a. Số lượng audit events theo loại hành động (Standard Audit):
SELECT 
    action_name,
    COUNT(*) AS so_lan_thuc_hien,
    SUM(CASE WHEN returncode = 0 THEN 1 ELSE 0 END) AS thanh_cong,
    SUM(CASE WHEN returncode != 0 THEN 1 ELSE 0 END) AS that_bai
FROM dba_audit_trail
WHERE obj_name IN ('HSBA', 'DON_THUOC', 'HSBA_DV', 'BENH_NHAN')
GROUP BY action_name
ORDER BY so_lan_thuc_hien DESC;

PROMPT
PROMPT 3b. Số lượng FGA events theo policy:
SELECT 
    policy_name,
    COUNT(*) AS so_lan_ghi_vay
FROM dba_fga_audit_trail
GROUP BY policy_name
ORDER BY so_lan_ghi_vay DESC;

PROMPT
PROMPT 3c. Top 10 users thực hiện audit events nhiều nhất:
SELECT 
    username,
    COUNT(*) AS so_lan_thuc_hien
FROM dba_audit_trail
WHERE obj_name IN ('HSBA', 'DON_THUOC', 'HSBA_DV', 'BENH_NHAN')
GROUP BY username
ORDER BY so_lan_thuc_hien DESC
FETCH FIRST 10 ROWS ONLY;

-- ============================================================================
-- 4. HIỂN THỊ CÁC TRÌNH KÍCH HOẠT AUDIT HIỆN TẠI
-- ============================================================================

PROMPT
PROMPT ======================================================================
PROMPT 4. CÁC TRÌNH KÍCH HOẠT AUDIT HIỆN TẠI
PROMPT ======================================================================

PROMPT
PROMPT 4a. Standard Audit Options (Tất cả các audit trên bảng):
SELECT 
    owner,
    object_name,
    object_type,
    audit_option,
    success,
    failure
FROM dba_obj_audit_opts
WHERE object_name IN ('HSBA', 'DON_THUOC', 'HSBA_DV', 'BENH_NHAN')
  AND owner = 'ADMIN'
ORDER BY object_name, audit_option;

PROMPT
PROMPT 4b. Fine-Grained Audit Policies (Tất cả các FGA policies):
SELECT 
    object_schema,
    object_name,
    policy_name,
    policy_text,
    enabled
FROM dba_audit_policies
WHERE object_name IN ('HSB', 'DON_THUOC', 'HSBA_DV')
  OR policy_name LIKE 'FGA_%'
ORDER BY object_name, policy_name;
