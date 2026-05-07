SET SERVEROUTPUT ON;

DECLARE
    v_table_name VARCHAR2(30) := 'HSBA';
    v_owner      VARCHAR2(30) := 'ADMIN';
BEGIN
    -- 1. Tự động DISABLE tất cả các Foreign Keys đang trỏ vào bảng HSBA
    FOR r IN (SELECT table_name, constraint_name 
              FROM all_constraints 
              WHERE r_owner = v_owner 
                AND r_constraint_name IN (SELECT constraint_name 
                                          FROM all_constraints 
                                          WHERE constraint_type IN ('P', 'U') 
                                            AND table_name = v_table_name 
                                            AND owner = v_owner))
    LOOP
        EXECUTE IMMEDIATE 'ALTER TABLE ' || v_owner || '.' || r.table_name || 
                          ' DISABLE CONSTRAINT ' || r.constraint_name;
    END LOOP;
    DBMS_OUTPUT.PUT_LINE('1. Đã tạm dừng các ràng buộc khóa ngoại liên quan.');

    -- 2. Xóa dữ liệu cũ trong bảng HSBA
    EXECUTE IMMEDIATE 'TRUNCATE TABLE ' || v_owner || '.' || v_table_name;
    DBMS_OUTPUT.PUT_LINE('2. Đã làm sạch bảng HSBA.');

    -- 3. Chuẩn bị dữ liệu từ bảng Backup (Xóa cột phụ BACKUP_AT nếu có)
    BEGIN
        EXECUTE IMMEDIATE 'ALTER TABLE ' || v_owner || '.HSBA_BACKUP DROP COLUMN BACKUP_AT';
    EXCEPTION WHEN OTHERS THEN NULL; 
    END;

    -- 4. Restore dữ liệu
    EXECUTE IMMEDIATE 'INSERT INTO ' || v_owner || '.' || v_table_name || 
                      ' SELECT * FROM ' || v_owner || '.HSBA_BACKUP';
    DBMS_OUTPUT.PUT_LINE('3. Đã khôi phục dữ liệu từ bản backup.');

    -- 5. Kích hoạt lại (ENABLE) các Foreign Keys
    FOR r IN (SELECT table_name, constraint_name 
              FROM all_constraints 
              WHERE r_owner = v_owner 
                AND r_constraint_name IN (SELECT constraint_name 
                                          FROM all_constraints 
                                          WHERE constraint_type IN ('P', 'U') 
                                            AND table_name = v_table_name 
                                            AND owner = v_owner))
    LOOP
        EXECUTE IMMEDIATE 'ALTER TABLE ' || v_owner || '.' || r.table_name || 
                          ' ENABLE CONSTRAINT ' || r.constraint_name;
    END LOOP;
    DBMS_OUTPUT.PUT_LINE('4. Các ràng buộc khóa ngoại đã hoạt động trở lại.');

    -- 6. Dọn dẹp
    EXECUTE IMMEDIATE 'DROP TABLE ' || v_owner || '.HSBA_BACKUP';
    
    COMMIT;
    DBMS_OUTPUT.PUT_LINE('===> RESTORE THÀNH CÔNG!');

EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('Lỗi: ' || SQLERRM);
        ROLLBACK;
END;
/