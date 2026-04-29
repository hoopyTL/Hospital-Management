-- =====================================================================
-- Script tạo bảng Audit Log để theo dõi các thay đổi và triển khai
-- =====================================================================
-- Đăng nhập bằng quyền ADMIN

-- Tạo sequence cho AuditId
CREATE SEQUENCE admin.seq_audit_log_id
  START WITH 1
  INCREMENT BY 1
  NOCYCLE;

-- Tạo bảng AUDIT_LOG
CREATE TABLE admin.AUDIT_LOG (
    audit_id         VARCHAR2(50) PRIMARY KEY,
    username         VARCHAR2(50) NOT NULL,
    full_name        VARCHAR2(100),
    action_type      VARCHAR2(30) NOT NULL,
    object_name      VARCHAR2(100),
    object_schema    VARCHAR2(30),
    old_value        CLOB,
    new_value        CLOB,
    sql_statement    CLOB,
    result           VARCHAR2(20) DEFAULT 'SUCCESS',
    error_code       VARCHAR2(50),
    error_message    VARCHAR2(500),
    ip_address       VARCHAR2(50),
    machine_name     VARCHAR2(100),
    action_timestamp TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
    notes            VARCHAR2(500),
    record_id        VARCHAR2(100),
    deployment_type  VARCHAR2(50),
    application_version VARCHAR2(20),
    deployment_description VARCHAR2(500),
    created_date     TIMESTAMP DEFAULT SYSTIMESTAMP
);

-- Tạo index cho tốc độ query
CREATE INDEX idx_audit_log_username ON admin.AUDIT_LOG(username);
CREATE INDEX idx_audit_log_action_type ON admin.AUDIT_LOG(action_type);
CREATE INDEX idx_audit_log_object_name ON admin.AUDIT_LOG(object_name);
CREATE INDEX idx_audit_log_timestamp ON admin.AUDIT_LOG(action_timestamp);
CREATE INDEX idx_audit_log_deployment_type ON admin.AUDIT_LOG(deployment_type);

-- Tạo trigger để tự động tạo audit_id
CREATE OR REPLACE TRIGGER admin.trg_audit_log_id
BEFORE INSERT ON admin.AUDIT_LOG
FOR EACH ROW
BEGIN
  IF :NEW.audit_id IS NULL THEN
    :NEW.audit_id := 'AUD' || TO_CHAR(SYSDATE, 'YYYYMMDD') || LPAD(seq_audit_log_id.NEXTVAL, 8, '0');
  END IF;
END;
/

-- =====================================================================
-- STORED PROCEDURE để ghi audit log
-- =====================================================================
CREATE OR REPLACE PROCEDURE admin.sp_log_audit (
    p_username              IN VARCHAR2,
    p_full_name             IN VARCHAR2,
    p_action_type           IN VARCHAR2,
    p_object_name           IN VARCHAR2,
    p_object_schema         IN VARCHAR2 DEFAULT 'ADMIN',
    p_old_value             IN CLOB DEFAULT NULL,
    p_new_value             IN CLOB DEFAULT NULL,
    p_sql_statement         IN CLOB DEFAULT NULL,
    p_result                IN VARCHAR2 DEFAULT 'SUCCESS',
    p_error_code            IN VARCHAR2 DEFAULT NULL,
    p_error_message         IN VARCHAR2 DEFAULT NULL,
    p_ip_address            IN VARCHAR2 DEFAULT NULL,
    p_machine_name          IN VARCHAR2 DEFAULT NULL,
    p_notes                 IN VARCHAR2 DEFAULT NULL,
    p_record_id             IN VARCHAR2 DEFAULT NULL,
    p_deployment_type       IN VARCHAR2 DEFAULT NULL,
    p_application_version   IN VARCHAR2 DEFAULT NULL,
    p_deployment_description IN VARCHAR2 DEFAULT NULL
)
IS
BEGIN
    INSERT INTO admin.AUDIT_LOG (
        username, full_name, action_type, object_name, object_schema,
        old_value, new_value, sql_statement, result, error_code, error_message,
        ip_address, machine_name, notes, record_id, deployment_type,
        application_version, deployment_description
    ) VALUES (
        p_username, p_full_name, p_action_type, p_object_name, p_object_schema,
        p_old_value, p_new_value, p_sql_statement, p_result, p_error_code, p_error_message,
        p_ip_address, p_machine_name, p_notes, p_record_id, p_deployment_type,
        p_application_version, p_deployment_description
    );
    COMMIT;
EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('Lỗi ghi audit log: ' || SQLERRM);
        ROLLBACK;
END sp_log_audit;
/

-- =====================================================================
-- VIEW để xem Audit Log theo các tiêu chí khác nhau
-- =====================================================================

-- View: Audit Log của hôm nay
CREATE OR REPLACE VIEW admin.v_audit_log_today AS
SELECT audit_id, username, full_name, action_type, object_name, 
       result, action_timestamp, notes
FROM admin.AUDIT_LOG
WHERE TRUNC(action_timestamp) = TRUNC(SYSDATE)
ORDER BY action_timestamp DESC;

-- View: Audit Log các thay đổi dữ liệu (INSERT, UPDATE, DELETE)
CREATE OR REPLACE VIEW admin.v_audit_log_data_changes AS
SELECT audit_id, username, full_name, action_type, object_name, record_id,
       old_value, new_value, result, action_timestamp
FROM admin.AUDIT_LOG
WHERE action_type IN ('INSERT', 'UPDATE', 'DELETE')
ORDER BY action_timestamp DESC;

-- View: Audit Log các lỗi
CREATE OR REPLACE VIEW admin.v_audit_log_errors AS
SELECT audit_id, username, full_name, action_type, object_name,
       error_code, error_message, result, action_timestamp
FROM admin.AUDIT_LOG
WHERE result = 'FAILED'
ORDER BY action_timestamp DESC;

-- View: Audit Log theo triển khai
CREATE OR REPLACE VIEW admin.v_audit_log_deployment AS
SELECT audit_id, username, full_name, action_type, object_name,
       deployment_type, application_version, deployment_description,
       result, action_timestamp
FROM admin.AUDIT_LOG
WHERE deployment_type IS NOT NULL
ORDER BY deployment_type, action_timestamp DESC;

-- View: Tổng thống kê Audit Log
CREATE OR REPLACE VIEW admin.v_audit_log_summary AS
SELECT 
    action_type,
    COUNT(*) as so_lan_thuc_hien,
    COUNT(CASE WHEN result = 'SUCCESS' THEN 1 END) as thanh_cong,
    COUNT(CASE WHEN result = 'FAILED' THEN 1 END) as that_bai,
    COUNT(DISTINCT username) as so_user_thuc_hien
FROM admin.AUDIT_LOG
GROUP BY action_type;

-- =====================================================================
-- QUERIES để xem Audit Log triển khai
-- =====================================================================

-- 1. Xem toàn bộ Audit Log
PROMPT === 1. TOÀN BỘ AUDIT LOG ===
SELECT * FROM admin.v_audit_log_today;

-- 2. Xem Audit Log hôm nay
PROMPT === 2. AUDIT LOG HÔM NAY ===
SELECT * FROM admin.v_audit_log_today;

-- 3. Xem các thay đổi dữ liệu
PROMPT === 3. CÁC THAY ĐỔI DỮ LIỆU ===
SELECT * FROM admin.v_audit_log_data_changes WHERE ROWNUM <= 20;

-- 4. Xem các lỗi
PROMPT === 4. CÁC LỖI ===
SELECT * FROM admin.v_audit_log_errors;

-- 5. Xem theo triển khai
PROMPT === 5. AUDIT LOG THEO TRIỂN KHAI ===
SELECT * FROM admin.v_audit_log_deployment;

-- 6. Thống kê
PROMPT === 6. THỐNG KÊ AUDIT LOG ===
SELECT * FROM admin.v_audit_log_summary;

-- 7. Xem hoạt động của user cụ thể
PROMPT === 7. HOẠT ĐỘNG CỦA USER ===
SELECT audit_id, username, action_type, object_name, result, action_timestamp
FROM admin.AUDIT_LOG
WHERE username = 'NV_01'
ORDER BY action_timestamp DESC
FETCH FIRST 20 ROWS ONLY;

-- 8. Xem các lần đăng nhập
PROMPT === 8. LỊCH SỬ ĐĂNG NHẬP ===
SELECT audit_id, username, full_name, result, action_timestamp, ip_address
FROM admin.AUDIT_LOG
WHERE action_type = 'LOGIN'
ORDER BY action_timestamp DESC
FETCH FIRST 20 ROWS ONLY;

-- 9. Xem timeline triển khai
PROMPT === 9. TIMELINE TRIỂN KHAI ===
SELECT deployment_type, application_version, deployment_description,
       COUNT(*) as so_thao_tac, MIN(action_timestamp) as thoi_gian_bat_dau,
       MAX(action_timestamp) as thoi_gian_ket_thuc
FROM admin.AUDIT_LOG
WHERE deployment_type IS NOT NULL
GROUP BY deployment_type, application_version, deployment_description
ORDER BY MIN(action_timestamp) DESC;

-- 10. Xem các thay đổi trên bảng cụ thể
PROMPT === 10. THAY ĐỔI TRÊN BẢNG HSBA ===
SELECT audit_id, username, action_type, record_id, 
       old_value, new_value, result, action_timestamp
FROM admin.AUDIT_LOG
WHERE object_name = 'HSBA' AND action_type IN ('UPDATE', 'DELETE')
ORDER BY action_timestamp DESC
FETCH FIRST 20 ROWS ONLY;

COMMIT;
PROMPT ===== AUDIT LOG SYSTEM SETUP HOÀN THÀNH =====
