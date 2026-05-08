CREATE OR REPLACE PROCEDURE sp_backup_hsba_dv 
IS
    v_handle        NUMBER;
    v_state         VARCHAR2(30);
    v_table_name    VARCHAR2(30) := 'HSBA_DV'; -- Cố định tên bảng là HSBA_DV
    v_backup_table  VARCHAR2(30);
    v_dump_file     VARCHAR2(100);
    v_sql           VARCHAR2(500);
BEGIN
    DBMS_OUTPUT.PUT_LINE('--- BAT DAU QUY TRINH BACKUP BANG ' || v_table_name || ' ---');

    -- Thiết lập tên bảng backup: Tiền tố + NămThángNgày_GiờPhútGiây
    -- Ví dụ: HSBA_DV_BK_20231025_120000 (Tổng cộng 26 ký tự, an toàn cho giới hạn 30 ký tự của Oracle)
    v_backup_table := v_table_name || '_BK_' || TO_CHAR(SYSDATE, 'YYYYMMDD_HH24MI');
    v_dump_file    := v_backup_table || '.dmp';

    -- 1. Tạo bảng Backup nội bộ (CTAS)
    DBMS_OUTPUT.PUT_LINE('1. Dang tao bang backup: ' || v_backup_table);
    v_sql := 'CREATE TABLE ' || v_backup_table || ' AS SELECT * FROM ' || v_table_name;
    EXECUTE IMMEDIATE v_sql;

    -- 2. Cấu hình Data Pump để xuất bảng Backup vừa tạo
    DBMS_OUTPUT.PUT_LINE('2. Khoi tao Job Data Pump...');
    v_handle := DBMS_DATAPUMP.OPEN('EXPORT', 'TABLE', NULL, NULL, 'COMPATIBLE');

    -- Thêm file Log và Dump vào thư mục DATA_PUMP_DIR
    DBMS_DATAPUMP.ADD_FILE(v_handle, v_backup_table || '.log', 'DATA_PUMP_DIR', NULL, DBMS_DATAPUMP.KU$_FILE_TYPE_LOG_FILE);
    DBMS_DATAPUMP.ADD_FILE(v_handle, v_dump_file, 'DATA_PUMP_DIR');

    -- Lọc: Chỉ xuất đúng bảng backup vừa tạo
    DBMS_DATAPUMP.METADATA_FILTER(v_handle, 'NAME_EXPR', 'IN (''' || v_backup_table || ''')');

    -- 3. Chạy Job Export
    DBMS_OUTPUT.PUT_LINE('3. Dang xuat file .dmp...');
    DBMS_DATAPUMP.START_JOB(v_handle);
    DBMS_DATAPUMP.WAIT_FOR_JOB(v_handle, v_state);

    DBMS_OUTPUT.PUT_LINE('=> KET QUA EXPORT: ' || v_state);

    -- 4. Ghi lịch sử (Nếu bạn có bảng backup_history)
    BEGIN
        INSERT INTO backup_history (table_name, dump_file, status, backup_date)
        VALUES (v_table_name, v_dump_file, v_state, SYSDATE);
        COMMIT;
    EXCEPTION 
        WHEN OTHERS THEN 
            DBMS_OUTPUT.PUT_LINE('Luu y: Khong the ghi log vao bang backup_history. Loi: ' || SQLERRM);
    END;

    DBMS_DATAPUMP.DETACH(v_handle);
    DBMS_OUTPUT.PUT_LINE('--- HOAN THANH ---');

EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('!!! LOI HE THONG !!!');
        DBMS_OUTPUT.PUT_LINE('Chi tiet: ' || SQLERRM);
        -- Đảm bảo giải phóng handle Data Pump nếu có lỗi xảy ra
        IF v_handle IS NOT NULL THEN
            BEGIN 
                DBMS_DATAPUMP.DETACH(v_handle); 
            EXCEPTION 
                WHEN OTHERS THEN NULL; 
            END;
        END IF;
END sp_backup_hsba_dv;
/

BEGIN
    DBMS_SCHEDULER.DROP_JOB(job_name => 'JOB_BACKUP_HSBA_DV_12H', force => TRUE);
END;
/

BEGIN
  DBMS_SCHEDULER.CREATE_JOB (
   job_name           =>  'JOB_BACKUP_HSBA_DV_12H',
   job_type           =>  'PLSQL_BLOCK',
   job_action         =>  'BEGIN sp_backup_hsba_dv; END;',
   start_date         =>  SYSTIMESTAMP,
   repeat_interval    =>  'FREQ=MINUTELY; INTERVAL=1', -- Chạy lặp lại mỗi 1 phút
   enabled            =>  TRUE,
   comments           =>  'Tu dong backup bang HSBA_DV moi 12 tieng'
  );
END;
/