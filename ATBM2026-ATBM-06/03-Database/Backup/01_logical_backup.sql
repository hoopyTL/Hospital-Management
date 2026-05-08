SET SERVEROUTPUT ON;
SET VERIFY OFF;

DECLARE
    v_handle        NUMBER;
    v_state         VARCHAR2(30);
    v_table_name    VARCHAR2(100) := UPPER('&Nhap_Ten_Bang');
    v_backup_table  VARCHAR2(130); -- Tên bảng tạm để backup
    v_dump_file     VARCHAR2(200);
    v_sql           VARCHAR2(500);
BEGIN
    -- 1. Kiểm tra đầu vào
    IF v_table_name IS NULL OR trim(v_table_name) = '' THEN
        DBMS_OUTPUT.PUT_LINE('LOI: Ban chưa nhập tên bảng!');
        RETURN;
    END IF;

    -- Thiết lập tên bảng backup (Ví dụ: EMP_BK_112233) và tên file dump
    v_backup_table := v_table_name || '_BK_' || TO_CHAR(SYSDATE, 'HH24MISS');
    v_dump_file    := v_backup_table || '.dmp';

    DBMS_OUTPUT.PUT_LINE('--- BAT DAU QUY TRINH ---');

    -- 2. Tạo bảng Backup nội bộ (CTAS)
    DBMS_OUTPUT.PUT_LINE('1. Đang tạo bảng backup nội bộ: ' || v_backup_table);
    v_sql := 'CREATE TABLE ' || v_backup_table || ' AS SELECT * FROM ' || v_table_name;
    EXECUTE IMMEDIATE v_sql;

    -- 3. Cấu hình Data Pump để xuất bảng Backup vừa tạo
    DBMS_OUTPUT.PUT_LINE('2. Mở Job Data Pump cho bảng ' || v_backup_table || '...');
    v_handle := DBMS_DATAPUMP.OPEN('EXPORT', 'TABLE', NULL, NULL, 'COMPATIBLE');

    -- Thêm file Log và Dump
    DBMS_DATAPUMP.ADD_FILE(v_handle, v_backup_table || '.log', 'DATA_PUMP_DIR', NULL, DBMS_DATAPUMP.KU$_FILE_TYPE_LOG_FILE);
    DBMS_DATAPUMP.ADD_FILE(v_handle, v_dump_file, 'DATA_PUMP_DIR');

    -- Lọc: Chỉ xuất bảng backup vừa tạo
    DBMS_DATAPUMP.METADATA_FILTER(v_handle, 'NAME_EXPR', 'IN (''' || v_backup_table || ''')');

    -- 4. Chạy Job
    DBMS_OUTPUT.PUT_LINE('3. Khởi động Export file .dmp...');
    DBMS_DATAPUMP.START_JOB(v_handle);
    DBMS_DATAPUMP.WAIT_FOR_JOB(v_handle, v_state);

    DBMS_OUTPUT.PUT_LINE('=> KẾT QUẢ EXPORT: ' || v_state);

    -- 5. Ghi lịch sử vào bảng quản lý
    -- Giả sử bảng backup_history của bạn đã có các cột này
    INSERT INTO backup_history (table_name, dump_file, status, backup_date)
    VALUES (v_table_name, v_dump_file, v_state, SYSDATE);
    COMMIT;

    DBMS_DATAPUMP.DETACH(v_handle);
    DBMS_OUTPUT.PUT_LINE('--- HOÀN THÀNH ---');

EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('!!! LỖI HỆ THỐNG !!!');
        DBMS_OUTPUT.PUT_LINE('Thông báo: ' || SQLERRM);
        -- Giải phóng handle nếu lỗi
        IF v_handle IS NOT NULL THEN
            BEGIN DBMS_DATAPUMP.DETACH(v_handle); EXCEPTION WHEN OTHERS THEN NULL; END;
        END IF;
END;
/
