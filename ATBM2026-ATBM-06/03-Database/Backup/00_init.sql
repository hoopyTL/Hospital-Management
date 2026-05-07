-- ============================================================
-- PHAN HE 2 - YEU CAU 4: SAO LUU VA PHUC HOI DU LIEU
-- 00_init.sql
--
-- Muc dich:
--   1. Tao cac bang log/metadata phuc vu backup demo bang SQL.
--   2. Chuan bi row movement de co the demo Flashback Table.
--   3. Ghi chu cac buoc can chay bang SYSDBA/OS cho Data Pump, RMAN.
--
-- Cach chay:
--   - Dang nhap ADMIN/12345 vao XEPDB1.
--   - Mo file va bam F5 (Run Script).
-- ============================================================

SET SERVEROUTPUT ON;

ALTER SESSION SET CURRENT_SCHEMA = ADMIN;

-- ============================================================
-- 1. BANG LICH SU BACKUP
-- ============================================================

BEGIN
    EXECUTE IMMEDIATE '
        CREATE TABLE ADMIN.BACKUP_HISTORY (
            BACKUP_ID     NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
            BACKUP_NAME   VARCHAR2(64) NOT NULL,
            BACKUP_TYPE   VARCHAR2(30) NOT NULL,
            SOURCE_OBJECT VARCHAR2(128),
            START_AT      TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
            END_AT        TIMESTAMP,
            STATUS        VARCHAR2(20) DEFAULT ''RUNNING'' NOT NULL,
            MESSAGE       NVARCHAR2(1000)
        )';
    DBMS_OUTPUT.PUT_LINE('Created ADMIN.BACKUP_HISTORY.');
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE = -955 THEN
            DBMS_OUTPUT.PUT_LINE('ADMIN.BACKUP_HISTORY already exists.');
        ELSE
            RAISE;
        END IF;
END;
/

-- ============================================================
-- 2. PACKAGE DEMO BACKUP BANG CTAS
--    Logical backup bang bang shadow trong cung schema.
--    Cac backup table duoc luu theo timestamp de khong ghi de ban cu.
-- ============================================================

CREATE OR REPLACE PACKAGE ADMIN.PKG_BACKUP_RESTORE AS
    PROCEDURE BACKUP_TABLE(
        p_table_name  IN VARCHAR2,
        p_backup_type IN VARCHAR2 DEFAULT 'MANUAL_CTAS'
    );

    PROCEDURE BACKUP_CORE_TABLES(
        p_backup_type IN VARCHAR2 DEFAULT 'MANUAL_CTAS'
    );

    PROCEDURE RESTORE_TABLE_LATEST(
        p_table_name IN VARCHAR2
    );
END PKG_BACKUP_RESTORE;
/

CREATE OR REPLACE PACKAGE BODY ADMIN.PKG_BACKUP_RESTORE AS
    FUNCTION sanitize_name(p_name IN VARCHAR2) RETURN VARCHAR2 IS
    BEGIN
        RETURN DBMS_ASSERT.SIMPLE_SQL_NAME(UPPER(TRIM(p_name)));
    END;

    PROCEDURE log_finish(
        p_backup_id IN NUMBER,
        p_status    IN VARCHAR2,
        p_message   IN NVARCHAR2
    ) IS
    BEGIN
        UPDATE ADMIN.BACKUP_HISTORY
        SET END_AT = SYSTIMESTAMP,
            STATUS = p_status,
            MESSAGE = p_message
        WHERE BACKUP_ID = p_backup_id;
    END;

    PROCEDURE BACKUP_TABLE(
        p_table_name  IN VARCHAR2,
        p_backup_type IN VARCHAR2 DEFAULT 'MANUAL_CTAS'
    ) IS
        v_table_name  VARCHAR2(30);
        v_backup_name VARCHAR2(64);
        v_backup_id   NUMBER;
    BEGIN
        v_table_name := sanitize_name(p_table_name);
        v_backup_name := 'BKP_' || v_table_name || '_' || TO_CHAR(SYSDATE, 'YYYYMMDDHH24MISS');

        INSERT INTO ADMIN.BACKUP_HISTORY(BACKUP_NAME, BACKUP_TYPE, SOURCE_OBJECT)
        VALUES (v_backup_name, p_backup_type, 'ADMIN.' || v_table_name)
        RETURNING BACKUP_ID INTO v_backup_id;

        EXECUTE IMMEDIATE
            'CREATE TABLE ADMIN.' || v_backup_name ||
            ' AS SELECT t.*, SYSTIMESTAMP AS BACKUP_AT FROM ADMIN.' || v_table_name || ' t';

        log_finish(v_backup_id, 'SUCCESS', 'Backup completed.');
        COMMIT;

        DBMS_OUTPUT.PUT_LINE('Backup ADMIN.' || v_table_name || ' -> ADMIN.' || v_backup_name);
    EXCEPTION
        WHEN OTHERS THEN
            IF v_backup_id IS NOT NULL THEN
                log_finish(v_backup_id, 'FAILED', SQLERRM);
                COMMIT;
            END IF;
            RAISE;
    END;

    PROCEDURE BACKUP_CORE_TABLES(
        p_backup_type IN VARCHAR2 DEFAULT 'MANUAL_CTAS'
    ) IS
    BEGIN
        BACKUP_TABLE('BENH_NHAN', p_backup_type);
        BACKUP_TABLE('NHAN_VIEN', p_backup_type);
        BACKUP_TABLE('HSBA', p_backup_type);
        BACKUP_TABLE('HSBA_DV', p_backup_type);
        BACKUP_TABLE('DON_THUOC', p_backup_type);
    END;

    PROCEDURE RESTORE_TABLE_LATEST(
        p_table_name IN VARCHAR2
    ) IS
        v_table_name  VARCHAR2(30);
        v_backup_name VARCHAR2(64);
        v_column_list CLOB;
    BEGIN
        v_table_name := sanitize_name(p_table_name);

        SELECT BACKUP_NAME
        INTO v_backup_name
        FROM (
            SELECT BACKUP_NAME
            FROM ADMIN.BACKUP_HISTORY
            WHERE SOURCE_OBJECT = 'ADMIN.' || v_table_name
              AND STATUS = 'SUCCESS'
            ORDER BY START_AT DESC
        )
        WHERE ROWNUM = 1;

        SELECT LISTAGG(COLUMN_NAME, ', ') WITHIN GROUP (ORDER BY COLUMN_ID)
        INTO v_column_list
        FROM ALL_TAB_COLUMNS
        WHERE OWNER = 'ADMIN'
          AND TABLE_NAME = v_table_name;

        EXECUTE IMMEDIATE 'TRUNCATE TABLE ADMIN.' || v_table_name;
        EXECUTE IMMEDIATE
            'INSERT INTO ADMIN.' || v_table_name || '(' || v_column_list || ') ' ||
            'SELECT ' || v_column_list || ' FROM ADMIN.' || v_backup_name;

        COMMIT;
        DBMS_OUTPUT.PUT_LINE('Restored ADMIN.' || v_table_name || ' from ADMIN.' || v_backup_name);
    END;
END PKG_BACKUP_RESTORE;
/

-- ============================================================
-- 3. BAT ROW MOVEMENT CHO FLASHBACK TABLE
-- ============================================================

ALTER TABLE ADMIN.BENH_NHAN ENABLE ROW MOVEMENT;
ALTER TABLE ADMIN.NHAN_VIEN ENABLE ROW MOVEMENT;
ALTER TABLE ADMIN.HSBA ENABLE ROW MOVEMENT;
ALTER TABLE ADMIN.HSBA_DV ENABLE ROW MOVEMENT;
ALTER TABLE ADMIN.DON_THUOC ENABLE ROW MOVEMENT;

-- ============================================================
-- 4. CAC BUOC NGOAI SCRIPT NAY
-- ============================================================
/*
  Data Pump directory - chay bang SYSDBA neu muon export/import file DMP:

    CREATE OR REPLACE DIRECTORY ATBM_BACKUP_DIR AS 'C:\oracle_backup';
    GRANT READ, WRITE ON DIRECTORY ATBM_BACKUP_DIR TO ADMIN;

  Flashback Database - chay bang SYSDBA, can cau hinh Fast Recovery Area:

    ALTER SYSTEM SET DB_FLASHBACK_RETENTION_TARGET = 1440;
    ALTER DATABASE FLASHBACK ON;

  RMAN - chay o OS/RMAN prompt, khong chay trong SQL Developer:

    rman target /
    RMAN> CONFIGURE RETENTION POLICY TO RECOVERY WINDOW OF 7 DAYS;
    RMAN> BACKUP DATABASE PLUS ARCHIVELOG;
*/

BEGIN
    DBMS_OUTPUT.PUT_LINE('00_init.sql completed.');
END;
/
