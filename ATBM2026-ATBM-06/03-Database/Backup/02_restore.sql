-- ============================================================
-- PHAN HE 2 - YEU CAU 4: PHUC HOI DU LIEU
-- 02_restore.sql
--
-- Bao gom:
--   1. Tra audit log de xac dinh su co.
--   2. Flashback Query/Table de xem va phuc hoi nhanh.
--   3. Restore bang tu ban CTAS moi nhat.
--   4. Lenh Import Data Pump va RMAN tham khao.
--
-- Cach chay:
--   - Chay 00_init.sql va 01_backup.sql truoc.
--   - Dang nhap ADMIN/12345 vao XEPDB1.
-- ============================================================

SET SERVEROUTPUT ON;

ALTER SESSION SET CURRENT_SCHEMA = ADMIN;

-- ============================================================
-- 1. XAC DINH SU CO TU NHAT KY KIEM TOAN
-- ============================================================
-- Yeu cau 3 dang cau hinh Standard Audit va Unified Audit.
-- Cac truy van DBA_/UNIFIED_ co the can quyen SELECT_CATALOG_ROLE/DBA.

PROMPT ==== Standard Audit gan nhat tren cac bang nghiep vu ====

SELECT username,
       timestamp,
       obj_name,
       action_name,
       returncode,
       sql_text
FROM   dba_audit_trail
WHERE  obj_name IN ('BENH_NHAN', 'NHAN_VIEN', 'HSBA', 'HSBA_DV', 'DON_THUOC')
ORDER  BY timestamp DESC
FETCH FIRST 20 ROWS ONLY;

PROMPT ==== Unified Audit gan nhat tren cac bang nghiep vu ====

SELECT dbusername,
       event_timestamp,
       action_name,
       object_schema,
       object_name,
       unified_audit_policies,
       sql_text
FROM   unified_audit_trail
WHERE  object_schema = 'ADMIN'
  AND  object_name IN ('BENH_NHAN', 'NHAN_VIEN', 'HSBA', 'HSBA_DV', 'DON_THUOC')
ORDER  BY event_timestamp DESC
FETCH FIRST 20 ROWS ONLY;

-- Neu chi demo audit custom cua Yeu cau 3 cho cap nhat KET_QUA:
/*
SELECT *
FROM   ADMIN.AUDIT_KETQUA
ORDER  BY THOI_GIAN_CAP_NHAT DESC
FETCH FIRST 20 ROWS ONLY;
*/

-- ============================================================
-- 2. FLASHBACK QUERY: XEM DU LIEU TRUOC SU CO
-- ============================================================
-- Thay timestamp ben duoi bang thoi diem lay tu audit log.

/*
SELECT *
FROM   ADMIN.DON_THUOC
AS OF TIMESTAMP TO_TIMESTAMP('2026-05-07 10:25:00', 'YYYY-MM-DD HH24:MI:SS')
WHERE  MA_HSBA = 'HS000001';

SELECT *
FROM   ADMIN.HSBA_DV
AS OF TIMESTAMP (SYSTIMESTAMP - INTERVAL '30' MINUTE)
WHERE  MA_HSBA = 'HS000001';
*/

-- ============================================================
-- 3. FLASHBACK TABLE: PHUC HOI NHANH THEO THOI DIEM
-- ============================================================
-- Chi dung khi su co nho va van con trong undo/flashback retention.
-- Nen chay rieng tung block sau khi da xac dinh thoi diem su co tu audit.

/*
FLASHBACK TABLE ADMIN.DON_THUOC
    TO TIMESTAMP TO_TIMESTAMP('2026-05-07 10:25:00', 'YYYY-MM-DD HH24:MI:SS');

FLASHBACK TABLE ADMIN.HSBA_DV
    TO TIMESTAMP (SYSTIMESTAMP - INTERVAL '30' MINUTE);
*/

-- ============================================================
-- 4. RESTORE TU BAN CTAS MOI NHAT
-- ============================================================
-- Kich ban demo: restore lai cac bang nghiep vu tu backup gan nhat.
-- Thu tu restore can ton trong FK:
--   child truoc khi truncate: DON_THUOC, HSBA_DV, HSBA, NHAN_VIEN, BENH_NHAN
--   parent truoc khi insert:  BENH_NHAN, NHAN_VIEN, HSBA, HSBA_DV, DON_THUOC

CREATE OR REPLACE PROCEDURE ADMIN.RESTORE_CORE_TABLES_LATEST AS
    PROCEDURE SET_CORE_FK_STATE(p_state IN VARCHAR2) IS
    BEGIN
        FOR r IN (
            SELECT table_name, constraint_name
            FROM   all_constraints
            WHERE  owner = 'ADMIN'
              AND  constraint_type = 'R'
              AND  table_name IN ('HSBA', 'HSBA_DV', 'DON_THUOC')
        ) LOOP
            EXECUTE IMMEDIATE 'ALTER TABLE ADMIN.' || r.table_name ||
                              ' ' || p_state || ' CONSTRAINT ' || r.constraint_name;
        END LOOP;
    END;
BEGIN
    -- Tat FK phu thuoc de truncate/insert theo lo backup.
    SET_CORE_FK_STATE('DISABLE');

    ADMIN.PKG_BACKUP_RESTORE.RESTORE_TABLE_LATEST('DON_THUOC');
    ADMIN.PKG_BACKUP_RESTORE.RESTORE_TABLE_LATEST('HSBA_DV');
    ADMIN.PKG_BACKUP_RESTORE.RESTORE_TABLE_LATEST('HSBA');
    ADMIN.PKG_BACKUP_RESTORE.RESTORE_TABLE_LATEST('NHAN_VIEN');
    ADMIN.PKG_BACKUP_RESTORE.RESTORE_TABLE_LATEST('BENH_NHAN');

    -- Sau khi cac bang da co du lieu, bat lai FK.
    SET_CORE_FK_STATE('ENABLE');

    DBMS_OUTPUT.PUT_LINE('Restore core tables from latest CTAS backups completed.');
EXCEPTION
    WHEN OTHERS THEN
        BEGIN
            SET_CORE_FK_STATE('ENABLE');
        EXCEPTION
            WHEN OTHERS THEN
                NULL;
        END;
        RAISE;
END;
/

-- Chay restore toan bo bang nghiep vu tu CTAS backup gan nhat:
/*
BEGIN
    ADMIN.RESTORE_CORE_TABLES_LATEST;
END;
/
*/

-- Chay restore mot bang cu the tu CTAS backup gan nhat:
/*
BEGIN
    ADMIN.PKG_BACKUP_RESTORE.RESTORE_TABLE_LATEST('DON_THUOC');
END;
/
*/

-- ============================================================
-- 5. IMPORT DATA PUMP
-- ============================================================
/*
  Chay trong Command Prompt/PowerShell, khong chay F5.

  Restore toan schema ADMIN:

    impdp admin/12345@localhost:1521/XEPDB1 ^
      schemas=ADMIN ^
      directory=ATBM_BACKUP_DIR ^
      dumpfile=admin_backup_01.dmp ^
      logfile=admin_restore.log ^
      table_exists_action=replace

  Restore an toan mot bang ra bang tam de so sanh truoc khi ghi de:

    impdp admin/12345@localhost:1521/XEPDB1 ^
      tables=ADMIN.DON_THUOC ^
      directory=ATBM_BACKUP_DIR ^
      dumpfile=admin_backup_01.dmp ^
      logfile=restore_don_thuoc_stage.log ^
      remap_table=DON_THUOC:DON_THUOC_RESTORE

  Sau do co the so sanh/merge thu cong:

    SELECT cur.MA_HSBA, cur.NGAY_DT, cur.TEN_THUOC,
           cur.LIEU_DUNG AS CURRENT_VALUE,
           bak.LIEU_DUNG AS BACKUP_VALUE
    FROM   ADMIN.DON_THUOC cur
    JOIN   ADMIN.DON_THUOC_RESTORE bak
           ON bak.MA_HSBA = cur.MA_HSBA
          AND bak.NGAY_DT = cur.NGAY_DT
          AND bak.TEN_THUOC = cur.TEN_THUOC;
*/

-- ============================================================
-- 6. RMAN RECOVERY
-- ============================================================
/*
  Phuc hoi toan database den hien tai:

    rman target /
    RMAN> SHUTDOWN ABORT;
    RMAN> STARTUP MOUNT;
    RMAN> RESTORE DATABASE;
    RMAN> RECOVER DATABASE;
    RMAN> ALTER DATABASE OPEN RESETLOGS;

  Point-in-time recovery dua vao thoi diem su co trong audit log:

    RMAN> SHUTDOWN ABORT;
    RMAN> STARTUP MOUNT;
    RMAN> SET UNTIL TIME "TO_DATE(''2026-05-07 10:25:00'', ''YYYY-MM-DD HH24:MI:SS'')";
    RMAN> RESTORE DATABASE;
    RMAN> RECOVER DATABASE;
    RMAN> ALTER DATABASE OPEN RESETLOGS;
*/

BEGIN
    DBMS_OUTPUT.PUT_LINE('02_restore.sql loaded. Review audit output, then run the chosen restore block.');
END;
/
