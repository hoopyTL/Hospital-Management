-- =====================================================================
-- Script: Populate Sample Audit Logs for Testing
-- Purpose: Insert sample audit records to verify audit log display
-- =====================================================================

SET SERVEROUTPUT ON;

DECLARE
    v_audit_id VARCHAR2(50);
    v_count NUMBER := 0;
BEGIN
    DBMS_OUTPUT.PUT_LINE('=== Bắt đầu chèn sample audit logs ===');
    
    -- Sample 1: LOGIN
    EXECUTE IMMEDIATE 'INSERT INTO admin.AUDIT_LOG (username, full_name, action_type, object_name, result, action_timestamp, ip_address, machine_name) ' ||
                      'VALUES (''NV0021'', ''Nguyễn Văn A'', ''LOGIN'', ''LOGIN_SYSTEM'', ''SUCCESS'', SYSTIMESTAMP - 5/24, ''192.168.1.100'', ''PC001'')';
    v_count := v_count + 1;
    
    -- Sample 2: SELECT HSBA
    EXECUTE IMMEDIATE 'INSERT INTO admin.AUDIT_LOG (username, full_name, action_type, object_name, result, action_timestamp, record_id, notes) ' ||
                      'VALUES (''NV0021'', ''Nguyễn Văn A'', ''SELECT'', ''HSBA'', ''SUCCESS'', SYSTIMESTAMP - 4/24, ''HS001'', ''Xem hồ sơ bệnh án'')';
    v_count := v_count + 1;
    
    -- Sample 3: UPDATE DON_THUOC
    EXECUTE IMMEDIATE 'INSERT INTO admin.AUDIT_LOG (username, full_name, action_type, object_name, result, action_timestamp, record_id, old_value, new_value) ' ||
                      'VALUES (''BAC_SI'', ''Trần Bác Sĩ'', ''UPDATE'', ''DON_THUOC'', ''SUCCESS'', SYSTIMESTAMP - 3/24, ''DT001'', ''Aspirin 500mg'', ''Aspirin 500mg x2'')';
    v_count := v_count + 1;
    
    -- Sample 4: UPDATE HSBA with chẩn đoán
    EXECUTE IMMEDIATE 'INSERT INTO admin.AUDIT_LOG (username, full_name, action_type, object_name, result, action_timestamp, record_id, old_value, new_value) ' ||
                      'VALUES (''BAC_SI'', ''Trần Bác Sĩ'', ''UPDATE'', ''HSBA'', ''SUCCESS'', SYSTIMESTAMP - 2/24, ''HS001'', ''Chẩn đoán: Cảm cúm'', ''Chẩn đoán: Cảm cúm, sốt cao'')';
    v_count := v_count + 1;
    
    -- Sample 5: INSERT BENH_NHAN
    EXECUTE IMMEDIATE 'INSERT INTO admin.AUDIT_LOG (username, full_name, action_type, object_name, result, action_timestamp, record_id, new_value) ' ||
                      'VALUES (''NV_DPV'', ''Lê Điều Phối'', ''INSERT'', ''BENH_NHAN'', ''SUCCESS'', SYSTIMESTAMP - 1/24, ''BN001'', ''Bệnh nhân mới: Phạm Thị B, 28 tuổi'')';
    v_count := v_count + 1;
    
    -- Sample 6: Failed UPDATE attempt
    EXECUTE IMMEDIATE 'INSERT INTO admin.AUDIT_LOG (username, full_name, action_type, object_name, result, error_code, error_message, action_timestamp, record_id) ' ||
                      'VALUES (''NV0021'', ''Nguyễn Văn A'', ''UPDATE'', ''HSBA'', ''FAILED'', ''ORA-01031'', ''Insufficient privileges'', SYSTIMESTAMP - 1/48, ''HS002'')';
    v_count := v_count + 1;
    
    -- Sample 7: DELETE HSBA_DV
    EXECUTE IMMEDIATE 'INSERT INTO admin.AUDIT_LOG (username, full_name, action_type, object_name, result, action_timestamp, record_id, old_value) ' ||
                      'VALUES (''NV_DPV'', ''Lê Điều Phối'', ''DELETE'', ''HSBA_DV'', ''SUCCESS'', SYSTIMESTAMP - 0.5/24, ''DV001'', ''Xét nghiệm máu: 500,000đ'')';
    v_count := v_count + 1;
    
    -- Sample 8: LOGIN NV_DPV
    EXECUTE IMMEDIATE 'INSERT INTO admin.AUDIT_LOG (username, full_name, action_type, object_name, result, action_timestamp, ip_address) ' ||
                      'VALUES (''NV_DPV'', ''Lê Điều Phối'', ''LOGIN'', ''LOGIN_SYSTEM'', ''SUCCESS'', SYSTIMESTAMP, ''192.168.1.101'')';
    v_count := v_count + 1;
    
    -- Sample 9: Deployment
    EXECUTE IMMEDIATE 'INSERT INTO admin.AUDIT_LOG (username, full_name, action_type, object_name, deployment_type, application_version, deployment_description, result, action_timestamp) ' ||
                      'VALUES (''ADMIN'', ''Administrator'', ''DEPLOYMENT'', ''APPLICATION'', ''HOTFIX'', ''1.0.1'', ''Fix bug xóa đơn thuốc'', ''SUCCESS'', SYSDATE - 1)';
    v_count := v_count + 1;
    
    -- Sample 10: SELECT with RBAC check
    EXECUTE IMMEDIATE 'INSERT INTO admin.AUDIT_LOG (username, full_name, action_type, object_name, result, action_timestamp, record_id, notes) ' ||
                      'VALUES (''KTV001'', ''Kỹ Thuật Viên'', ''SELECT'', ''KET_QUA'', ''SUCCESS'', SYSTIMESTAMP, ''KQ001'', ''Xem kết quả xét nghiệm'')';
    v_count := v_count + 1;
    
    -- Sample 11: INSERT BENH_NHAN - Ngữ cảnh 5 (Theo dõi hành vi INSERT thành công trên bảng benhNhan)
    EXECUTE IMMEDIATE 'INSERT INTO admin.AUDIT_LOG (username, full_name, action_type, object_name, result, action_timestamp, record_id, new_value, notes) ' ||
                      'VALUES (''NV_DPV'', ''Lê Điều Phối'', ''INSERT'', ''BENH_NHAN'', ''SUCCESS'', SYSTIMESTAMP - 0.25/24, ''BN002'', ''Bệnh nhân: Võ Minh C, CMND: 123456789, Ngày sinh: 15/03/1990, Địa chỉ: 123 Nguyễn Huệ, TP.HCM'', ''Thêm bệnh nhân mới vào hệ thống'')';
    v_count := v_count + 1;
    
    -- Commit
    COMMIT;
    
    DBMS_OUTPUT.PUT_LINE('=== Hoàn thành chèn ' || v_count || ' bản ghi mẫu ===');
    DBMS_OUTPUT.PUT_LINE('');
    DBMS_OUTPUT.PUT_LINE('Xem kết quả:');
    DBMS_OUTPUT.PUT_LINE('  SELECT * FROM admin.v_audit_log_today;');
    DBMS_OUTPUT.PUT_LINE('  SELECT * FROM admin.v_audit_log_data_changes;');
    DBMS_OUTPUT.PUT_LINE('  SELECT * FROM admin.v_audit_log_errors;');
    
EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('Lỗi: ' || SQLERRM);
        ROLLBACK;
END;
/

-- Verify data
PROMPT === Kết quả audit log hôm nay ===
SELECT audit_id, username, full_name, action_type, object_name, result, action_timestamp
FROM admin.v_audit_log_today
ORDER BY action_timestamp DESC;

PROMPT === Tổng số bản ghi ===
SELECT COUNT(*) as tong_so_ban_ghi FROM admin.AUDIT_LOG;

PROMPT === Thống kê theo loại hành động ===
SELECT action_type, COUNT(*) as so_lan, 
       COUNT(CASE WHEN result = 'SUCCESS' THEN 1 END) as thanh_cong,
       COUNT(CASE WHEN result = 'FAILED' THEN 1 END) as that_bai
FROM admin.AUDIT_LOG
GROUP BY action_type
ORDER BY so_lan DESC;
