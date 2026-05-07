SET SERVEROUTPUT ON;
SET VERIFY OFF;

DECLARE
    v_handle      NUMBER;
    v_state       VARCHAR2(30);
    v_dump_file   VARCHAR2(200) := '&Nhap_Ten_File_Dump_Co_Duoi_dmp'; -- Ví dụ: HSBA_234716.dmp
BEGIN
    IF v_dump_file IS NULL OR trim(v_dump_file) = '' THEN
        DBMS_OUTPUT.PUT_LINE('LOI: Ban chua nhap ten file dmp!');
        RETURN;
    END IF;

    DBMS_OUTPUT.PUT_LINE('--- BAT DAU KHOI PHUC (RESTORE) ---');
    
    -- 1. Mở Job Data Pump ở chế độ IMPORT
    DBMS_OUTPUT.PUT_LINE('1. Mo Job Data Pump (IMPORT)...');
    v_handle := DBMS_DATAPUMP.OPEN('IMPORT', 'TABLE', NULL, NULL, 'COMPATIBLE');

    -- 2. Thêm file Log cho quá trình Import
    DBMS_OUTPUT.PUT_LINE('2. Them file Log...');
    DBMS_DATAPUMP.ADD_FILE(v_handle, 'IMPORT_' || v_dump_file || '.log', 'DATA_PUMP_DIR', NULL, DBMS_DATAPUMP.KU$_FILE_TYPE_LOG_FILE);

    -- 3. Chỉ định file Dump cần đọc để khôi phục
    DBMS_OUTPUT.PUT_LINE('3. Doc file Dump (' || v_dump_file || ')...');
    DBMS_DATAPUMP.ADD_FILE(v_handle, v_dump_file, 'DATA_PUMP_DIR');

    -- 4. Hành động nếu bảng đã tồn tại (REPLACE = Xóa bảng cũ, tạo lại từ file dmp)
    -- Các tùy chọn khác: TRUNCATE (chỉ xóa data), APPEND (thêm data vào data cũ), SKIP (bỏ qua)
    DBMS_OUTPUT.PUT_LINE('4. Cau hinh che do ghi de (REPLACE)...');
    DBMS_DATAPUMP.SET_PARAMETER(v_handle, 'TABLE_EXISTS_ACTION', 'REPLACE');

    -- 5. Khởi động tiến trình Import
    DBMS_OUTPUT.PUT_LINE('5. Khoi dong Job...');
    DBMS_DATAPUMP.START_JOB(v_handle);

    -- 6. Chờ tiến trình hoàn tất
    DBMS_OUTPUT.PUT_LINE('6. Cho Job hoan thanh...');
    DBMS_DATAPUMP.WAIT_FOR_JOB(v_handle, v_state);

    DBMS_OUTPUT.PUT_LINE('-----------------------------------');
    DBMS_OUTPUT.PUT_LINE('=> KET QUA KHOI PHUC: ' || v_state);
    DBMS_OUTPUT.PUT_LINE('-----------------------------------');

    -- Ngắt kết nối Job
    DBMS_DATAPUMP.DETACH(v_handle);

EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('!!! LOI KHI RESTORE !!!');
        DBMS_OUTPUT.PUT_LINE('Ma loi: ' || SQLERRM);
        IF v_handle IS NOT NULL THEN
            BEGIN DBMS_DATAPUMP.DETACH(v_handle); EXCEPTION WHEN OTHERS THEN NULL; END;
        END IF;
END;
/