-- ============================================================
-- PHAN HE 2 - YEU CAU 4: SAO LUU DU LIEU
-- 01_backup.sql
--
-- Bao gom:
--   1. Backup chu dong bang CTAS trong Oracle.
--   2. Tao job backup tu dong bang DBMS_SCHEDULER.
--   3. Ghi chu lenh Data Pump va RMAN chay ngoai OS.
--
-- Cach chay:
--   - Chay 00_init.sql truoc.
--   - Dang nhap ADMIN/12345 vao XEPDB1.
--   - Mo file va bam F5 (Run Script).
-- ============================================================

SET SERVEROUTPUT ON;

ALTER SESSION SET CURRENT_SCHEMA = ADMIN;

-- ============================================================
-- 1. BACKUP CHU DONG CAC BANG NGHIEP VU
-- ============================================================

BEGIN
    ADMIN.PKG_BACKUP_RESTORE.BACKUP_CORE_TABLES('MANUAL_CTAS');
    DBMS_OUTPUT.PUT_LINE('Manual CTAS backup completed.');
END;
/

-- Kiem tra cac ban backup vua tao.
SELECT BACKUP_ID,
       BACKUP_NAME,
       BACKUP_TYPE,
       SOURCE_OBJECT,
       START_AT,
       END_AT,
       STATUS,
       MESSAGE
FROM   ADMIN.BACKUP_HISTORY
ORDER  BY BACKUP_ID DESC;

-- ============================================================
-- 2. BACKUP TU DONG BANG DBMS_SCHEDULER
-- ============================================================
-- Job nay chay PL/SQL trong database moi ngay luc 02:00.
-- Neu database dang tat vao thoi diem do, job se chay lai theo co che
-- scheduler cua Oracle khi database mo lai.

BEGIN
    BEGIN
        DBMS_SCHEDULER.DROP_JOB(
            job_name => 'ADMIN.JOB_AUTO_CTAS_BACKUP',
            force    => TRUE
        );
    EXCEPTION
        WHEN OTHERS THEN
            IF SQLCODE != -27475 THEN
                RAISE;
            END IF;
    END;

    DBMS_SCHEDULER.CREATE_JOB(
        job_name        => 'ADMIN.JOB_AUTO_CTAS_BACKUP',
        job_type        => 'PLSQL_BLOCK',
        job_action      => 'BEGIN ADMIN.PKG_BACKUP_RESTORE.BACKUP_CORE_TABLES(''AUTO_CTAS''); END;',
        start_date      => SYSTIMESTAMP,
        repeat_interval => 'FREQ=DAILY;BYHOUR=2;BYMINUTE=0;BYSECOND=0',
        enabled         => TRUE,
        comments        => 'Automatic daily CTAS backup for core hospital tables.'
    );

    DBMS_OUTPUT.PUT_LINE('Created scheduler job ADMIN.JOB_AUTO_CTAS_BACKUP.');
END;
/

-- Kiem tra job tu dong.
SELECT OWNER,
       JOB_NAME,
       ENABLED,
       STATE,
       REPEAT_INTERVAL,
       NEXT_RUN_DATE
FROM   ALL_SCHEDULER_JOBS
WHERE  OWNER = 'ADMIN'
  AND  JOB_NAME = 'JOB_AUTO_CTAS_BACKUP';

-- Co the chay thu job ngay lap tuc neu can demo:
/*
BEGIN
    DBMS_SCHEDULER.RUN_JOB('ADMIN.JOB_AUTO_CTAS_BACKUP', USE_CURRENT_SESSION => TRUE);
END;
/
*/

-- ============================================================
-- 3. LOGICAL BACKUP CHUAN ORACLE BANG DATA PUMP
-- ============================================================
/*
  Cac lenh duoi day chay trong Command Prompt/PowerShell, khong chay F5.

  Buoc 1 - tao thu muc OS tren may cai Oracle:

    mkdir C:\oracle_backup

  Buoc 2 - tao Oracle DIRECTORY bang SYSDBA:

    CREATE OR REPLACE DIRECTORY ATBM_BACKUP_DIR AS 'C:\oracle_backup';
    GRANT READ, WRITE ON DIRECTORY ATBM_BACKUP_DIR TO ADMIN;

  Buoc 3 - export toan schema ADMIN:

    expdp admin/12345@localhost:1521/XEPDB1 ^
      schemas=ADMIN ^
      directory=ATBM_BACKUP_DIR ^
      dumpfile=admin_backup_%U.dmp ^
      logfile=admin_backup.log ^
      compression=all

  Buoc 4 - export mot so bang nghiep vu:

    expdp admin/12345@localhost:1521/XEPDB1 ^
      tables=ADMIN.BENH_NHAN,ADMIN.NHAN_VIEN,ADMIN.HSBA,ADMIN.HSBA_DV,ADMIN.DON_THUOC ^
      directory=ATBM_BACKUP_DIR ^
      dumpfile=admin_core_tables.dmp ^
      logfile=admin_core_tables.log ^
      compression=all
*/

-- ============================================================
-- 4. PHYSICAL BACKUP BANG RMAN
-- ============================================================
/*
  RMAN chay o OS voi quyen Oracle/SYSDBA:

    rman target /

  Cau hinh goi y:

    CONFIGURE RETENTION POLICY TO RECOVERY WINDOW OF 7 DAYS;
    CONFIGURE BACKUP OPTIMIZATION ON;
    CONFIGURE DEFAULT DEVICE TYPE TO DISK;
    CONFIGURE CONTROLFILE AUTOBACKUP ON;

  Neu muon online backup va point-in-time recovery, database nen bat ARCHIVELOG:

    SHUTDOWN IMMEDIATE;
    STARTUP MOUNT;
    ALTER DATABASE ARCHIVELOG;
    ALTER DATABASE OPEN;
    ARCHIVE LOG LIST;

  Full backup:

    BACKUP DATABASE PLUS ARCHIVELOG;

  Incremental backup:

    BACKUP INCREMENTAL LEVEL 0 DATABASE TAG 'ATBM_L0_WEEKLY';
    BACKUP INCREMENTAL LEVEL 1 DATABASE TAG 'ATBM_L1_DAILY';

  Neu muon tu dong hoa RMAN bang Scheduler, tao shell script tren OS,
  vi du C:\oracle_backup\rman_full_backup.bat:

    rman target / cmdfile=C:\oracle_backup\rman_full_backup.rcv log=C:\oracle_backup\rman_full_backup.log

  Noi dung rman_full_backup.rcv:

    RUN {
      BACKUP DATABASE PLUS ARCHIVELOG TAG 'ATBM_AUTO_FULL';
      DELETE NOPROMPT OBSOLETE;
    }

  Sau do co the tao job EXECUTABLE neu Oracle user co quyen chay external job:

    BEGIN
      DBMS_SCHEDULER.CREATE_JOB(
        job_name        => 'ADMIN.JOB_RMAN_FULL_BACKUP',
        job_type        => 'EXECUTABLE',
        job_action      => 'C:\oracle_backup\rman_full_backup.bat',
        repeat_interval => 'FREQ=WEEKLY;BYDAY=SUN;BYHOUR=1;BYMINUTE=0',
        enabled         => TRUE
      );
    END;
    /
*/

-- ============================================================
-- 5. TRUY VAN TRANG THAI BACKUP/RMAN/FLASHBACK
-- ============================================================
-- Cac view V$ thuong can quyen catalog/select dictionary.

/*
SELECT LOG_MODE, FLASHBACK_ON FROM V$DATABASE;

SELECT SESSION_KEY,
       INPUT_TYPE,
       STATUS,
       TO_CHAR(START_TIME, 'DD/MM/YYYY HH24:MI') AS START_TIME,
       TO_CHAR(END_TIME, 'DD/MM/YYYY HH24:MI') AS END_TIME,
       OUTPUT_MBYTES
FROM   V$RMAN_BACKUP_JOB_DETAILS
ORDER  BY START_TIME DESC;
*/

BEGIN
    DBMS_OUTPUT.PUT_LINE('01_backup.sql completed.');
END;
/
