SET SERVEROUTPUT ON;

DECLARE
    v_count          NUMBER;
    v_table_main     VARCHAR2(30) := 'ADMIN.HSBA';
    v_table_backup   VARCHAR2(30) := 'ADMIN.HSBA_BACKUP'; -- Tên bảng backup cố định
BEGIN
    -- 1. Kiểm tra nếu bảng backup cũ đã tồn tại thì XÓA
    SELECT count(*) INTO v_count FROM all_tables 
    WHERE owner = 'ADMIN' AND table_name = 'HSBA_BACKUP';

    IF v_count > 0 THEN
        EXECUTE IMMEDIATE 'DROP TABLE ' || v_table_backup;
        DBMS_OUTPUT.PUT_LINE('Đã xóa bảng backup cũ.');
    END IF;

    -- 2. Tiến hành backup bằng lệnh Create Table As Select (CTAS)
    -- Lệnh này sẽ copy toàn bộ dữ liệu và cấu trúc bảng
    EXECUTE IMMEDIATE 'CREATE TABLE ' || v_table_backup || ' AS SELECT * FROM ' || v_table_main;

    -- 3. Thêm một cột để ghi lại thời gian backup (Tùy chọn)
    EXECUTE IMMEDIATE 'ALTER TABLE ' || v_table_backup || ' ADD (BACKUP_AT TIMESTAMP)';
    EXECUTE IMMEDIATE 'UPDATE ' || v_table_backup || ' SET BACKUP_AT = SYSTIMESTAMP';
    COMMIT;

    DBMS_OUTPUT.PUT_LINE('Backup thành công bảng ' || v_table_main || ' sang ' || v_table_backup);
    
EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('Lỗi trong quá trình backup: ' || SQLERRM);
END;
/