-- Xem toàn bộ dữ liệu Standard Audit liên quan đến các bảng và user trong script
SELECT 
    username AS "Tài khoản",
    timestamp AS "Thời gian",
    obj_name AS "Tên bảng",
    action_name AS "Hành động",
    returncode AS "Mã kết quả (0=Thành công)",
    sql_text AS "Câu lệnh SQL" -- (Nếu có bật audit_trail = db, extended)
FROM 
    dba_audit_trail
WHERE 
    -- 1. SELECT trên hsba (Tất cả)
    (obj_name = 'HSBA' AND action_name = 'SELECT')
    
    -- 2. UPDATE trên don_thuoc (Chỉ khi thất bại: returncode != 0)
    OR (obj_name = 'DON_THUOC' AND action_name = 'UPDATE' AND returncode != 0)
    
    -- 3. DELETE trên hsba_dv (Chỉ khi thành công: returncode = 0)
    OR (obj_name = 'HSBA_DV' AND action_name = 'DELETE' AND returncode = 0)
    
    -- 4. Audit phiên làm việc của user NV0051
    OR (username = 'NV0051' AND action_name IN ('LOGON', 'LOGOFF'))
    
    -- 5. INSERT trên benh_nhan (Chỉ khi thành công: returncode = 0)
    OR (obj_name = 'BENH_NHAN' AND action_name = 'INSERT' AND returncode = 0)
ORDER BY 
    timestamp DESC;

-- Xem dữ liệu từ các Unified Audit Policies đã tạo
SELECT 
    dbusername AS "Tài khoản",
    event_timestamp AS "Thời gian",
    action_name AS "Hành động",
    object_schema AS "Schema",
    object_name AS "Tên bảng",
    unified_audit_policies AS "Tên Policy vi phạm",
    sql_text AS "Câu lệnh SQL đã chạy",
    client_program_name AS "Phần mềm kết nối"
FROM 
    unified_audit_trail
WHERE 
    unified_audit_policies LIKE '%UNIFIED_AUDIT_UPDATE_DONTHUOC%'
    OR unified_audit_policies LIKE '%UNIFIED_AUDIT_ILLEGAL_UPDATE_HSBA%'
    OR unified_audit_policies LIKE '%UNIFIED_AUDIT_ILLEGAL_DML_HSBA_DV%'
ORDER BY 
    event_timestamp DESC;