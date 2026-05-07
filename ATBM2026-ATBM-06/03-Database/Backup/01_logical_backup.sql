SET SERVEROUTPUT ON;
SET VERIFY OFF;

DECLARE
    v_handle      NUMBER;
    v_state       VARCHAR2(30);
    v_table_name  VARCHAR2(100) := UPPER('&Nhap_Ten_Bang');
    v_dump_file   VARCHAR2(200);
BEGIN
    IF v_table_name IS NULL OR trim(v_table_name) = '' THEN
        DBMS_OUTPUT.PUT_LINE('LOI: Ban chua nhap ten bang!');
        RETURN;
    END IF;

    v_dump_file := v_table_name || '_' || TO_CHAR(SYSDATE, 'HH24MISS') || '.dmp';
    
    DBMS_OUTPUT.PUT_LINE('--- BAT DAU BACKUP ---');
    
    DBMS_OUTPUT.PUT_LINE('1. Mo Job Data Pump...');
    v_handle := DBMS_DATAPUMP.OPEN('EXPORT', 'TABLE', NULL, NULL, 'COMPATIBLE');

    -- Chú ý: Dùng DATA_PUMP_DIR (Mặc định của hệ thống)
    DBMS_OUTPUT.PUT_LINE('2. Them file Log...');
    DBMS_DATAPUMP.ADD_FILE(v_handle, v_table_name || '.log', 'DATA_PUMP_DIR', NULL, DBMS_DATAPUMP.KU$_FILE_TYPE_LOG_FILE);

    DBMS_OUTPUT.PUT_LINE('3. Them file Dump (' || v_dump_file || ')...');
    DBMS_DATAPUMP.ADD_FILE(v_handle, v_dump_file, 'DATA_PUMP_DIR');

    DBMS_OUTPUT.PUT_LINE('4. Thiet lap bo loc...');
    DBMS_DATAPUMP.METADATA_FILTER(v_handle, 'NAME_EXPR', 'IN (''' || v_table_name || ''')');

    DBMS_OUTPUT.PUT_LINE('5. Khoi dong Job...');
    DBMS_DATAPUMP.START_JOB(v_handle);

    DBMS_OUTPUT.PUT_LINE('6. Cho Job hoan thanh...');
    DBMS_DATAPUMP.WAIT_FOR_JOB(v_handle, v_state);

    DBMS_OUTPUT.PUT_LINE('=> KET QUA CUOI CUNG: ' || v_state);
    
    -- Ghi Log
    INSERT INTO backup_history (table_name, dump_file, status)
    VALUES (v_table_name, v_dump_file, v_state);
    COMMIT;

    DBMS_DATAPUMP.DETACH(v_handle);

EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('!!! LOI ROI !!!');
        DBMS_OUTPUT.PUT_LINE('Ma loi: ' || SQLERRM);
        IF v_handle IS NOT NULL THEN
            BEGIN DBMS_DATAPUMP.DETACH(v_handle); EXCEPTION WHEN OTHERS THEN NULL; END;
        END IF;
END;
/
SELECT table_name FROM user_tables;