# Yeu cau 4 - Sao luu va phuc hoi du lieu

Thu muc nay cai dat phan sao luu/phuc hoi cho Phan he 2 tren Oracle Database, dung schema nghiep vu `ADMIN`.

## 1. Cac phuong phap Oracle ho tro

| Phuong phap | Cong cu | Muc dich |
|---|---|---|
| Logical backup | Data Pump `expdp/impdp`, CTAS | Sao luu theo schema/bang, de chuyen doi va phuc hoi tung doi tuong |
| Physical backup | RMAN | Sao luu file vat ly cua database, phuc hoi sau su co lon, ho tro PITR |
| Flashback | Flashback Query/Table/Database | Phuc hoi nhanh ve thoi diem truoc loi thao tac |

Trong demo cua nhom:

- `CTAS` duoc cai dat thanh script SQL chay truc tiep trong SQL Developer.
- `DBMS_SCHEDULER` tao job tu dong backup hang ngay.
- `Data Pump` va `RMAN` duoc ghi thanh lenh OS vi khong the chay truc tiep bang F5 trong SQL Developer.
- `Flashback Query/Table` duoc dung ket hop audit log de xac dinh thoi diem can phuc hoi.

## 2. File trong thu muc

| File | Noi dung |
|---|---|
| `00_init.sql` | Tao `BACKUP_HISTORY`, package `PKG_BACKUP_RESTORE`, bat row movement cho Flashback Table |
| `01_backup.sql` | Backup chu dong cac bang chinh, tao job backup tu dong, kem lenh Data Pump/RMAN |
| `02_restore.sql` | Tra audit log, demo Flashback, restore tu backup CTAS, kem lenh `impdp`/RMAN |

## 3. Cach chay nhanh

Dang nhap `ADMIN/12345` vao `XEPDB1`, sau do chay bang F5:

```sql
@00_init.sql
@01_backup.sql
```

Sau khi tao su co demo, chay:

```sql
@02_restore.sql
```

`02_restore.sql` mac dinh chi hien thi audit log va tao procedure restore. Phan restore that su duoc dat trong block comment de tranh vo tinh ghi de du lieu. Khi demo, mo comment block can dung.

## 4. Backup chu dong

`01_backup.sql` goi:

```sql
BEGIN
    ADMIN.PKG_BACKUP_RESTORE.BACKUP_CORE_TABLES('MANUAL_CTAS');
END;
/
```

Lenh nay tao ban sao cho cac bang:

- `ADMIN.BENH_NHAN`
- `ADMIN.NHAN_VIEN`
- `ADMIN.HSBA`
- `ADMIN.HSBA_DV`
- `ADMIN.DON_THUOC`

Ten bang backup co dang:

```text
BKP_<TEN_BANG>_<YYYYMMDDHH24MISS>
```

Moi lan backup duoc ghi vao `ADMIN.BACKUP_HISTORY`, khong ghi de ban cu.

## 5. Backup tu dong

`01_backup.sql` tao job:

```text
ADMIN.JOB_AUTO_CTAS_BACKUP
```

Job chay hang ngay luc `02:00` va goi package backup cac bang nghiep vu. Kiem tra job:

```sql
SELECT JOB_NAME, ENABLED, STATE, REPEAT_INTERVAL, NEXT_RUN_DATE
FROM   USER_SCHEDULER_JOBS
WHERE  JOB_NAME = 'JOB_AUTO_CTAS_BACKUP';
```

Co the chay thu ngay:

```sql
BEGIN
    DBMS_SCHEDULER.RUN_JOB('ADMIN.JOB_AUTO_CTAS_BACKUP', USE_CURRENT_SESSION => TRUE);
END;
/
```

## 6. Data Pump

Tao thu muc tren may cai Oracle:

```cmd
mkdir C:\oracle_backup
```

Chay bang `SYSDBA`:

```sql
CREATE OR REPLACE DIRECTORY ATBM_BACKUP_DIR AS 'C:\oracle_backup';
GRANT READ, WRITE ON DIRECTORY ATBM_BACKUP_DIR TO ADMIN;
```

Export schema:

```cmd
expdp admin/12345@localhost:1521/XEPDB1 schemas=ADMIN directory=ATBM_BACKUP_DIR dumpfile=admin_backup_%U.dmp logfile=admin_backup.log compression=all
```

Import schema:

```cmd
impdp admin/12345@localhost:1521/XEPDB1 schemas=ADMIN directory=ATBM_BACKUP_DIR dumpfile=admin_backup_01.dmp logfile=admin_restore.log table_exists_action=replace
```

## 7. RMAN

RMAN dung cho backup vat ly va phuc hoi sau su co lon:

```cmd
rman target /
```

Lenh goi y:

```rman
CONFIGURE RETENTION POLICY TO RECOVERY WINDOW OF 7 DAYS;
CONFIGURE BACKUP OPTIMIZATION ON;
CONFIGURE CONTROLFILE AUTOBACKUP ON;
BACKUP DATABASE PLUS ARCHIVELOG;
```

Phuc hoi den thoi diem truoc su co:

```rman
SHUTDOWN ABORT;
STARTUP MOUNT;
SET UNTIL TIME "TO_DATE('2026-05-07 10:25:00', 'YYYY-MM-DD HH24:MI:SS')";
RESTORE DATABASE;
RECOVER DATABASE;
ALTER DATABASE OPEN RESETLOGS;
```

Thoi diem `2026-05-07 10:25:00` la vi du. Khi demo, lay thoi diem that tu audit log.

## 8. Phuc hoi dua vao audit log

Yeu cau 3 da cau hinh Standard Audit va Unified Audit. `02_restore.sql` doc:

- `DBA_AUDIT_TRAIL`
- `UNIFIED_AUDIT_TRAIL`
- `ADMIN.AUDIT_KETQUA` neu demo audit custom cua cap nhat `KET_QUA`

Quy trinh:

1. Xem audit log de tim user, thoi diem, bang va cau SQL gay su co.
2. Neu loi nho va con trong undo/flashback retention, dung Flashback Query/Table.
3. Neu can phuc hoi bang/schema, dung backup CTAS hoac `impdp`.
4. Neu su co nghiem trong o muc database, dung RMAN point-in-time recovery.

Vi du Flashback Query:

```sql
SELECT *
FROM   ADMIN.DON_THUOC
AS OF TIMESTAMP TO_TIMESTAMP('2026-05-07 10:25:00', 'YYYY-MM-DD HH24:MI:SS')
WHERE  MA_HSBA = 'HS000001';
```

Vi du restore tu CTAS backup moi nhat:

```sql
BEGIN
    ADMIN.PKG_BACKUP_RESTORE.RESTORE_TABLE_LATEST('DON_THUOC');
END;
/
```

## 9. Danh gia uu/nhuoc diem

| Phuong phap | Uu diem | Nhuoc diem |
|---|---|---|
| CTAS | Don gian, chay duoc bang SQL, phu hop demo tung bang | Ton dung luong trong database, khong copy day du index/constraint/trigger, khong phuc hoi duoc lien tuc theo thoi diem |
| Data Pump | Chuan Oracle, export/import linh hoat theo schema/bang, de luu file ngoai DB | Can chay OS, file DMP co the lon, restore ve thoi diem backup |
| RMAN | Bao ve toan database, ho tro incremental va PITR | Cau hinh phuc tap hon, can ARCHIVELOG cho online backup/PITR |
| Flashback | Phuc hoi nhanh sau loi thao tac, ket hop audit rat tot | Phu thuoc undo/flashback retention, khong thay the backup vat ly |

## 10. Ket luan

Phan cai dat dap ung yeu cau sao luu chu dong, sao luu tu dong va phuc hoi sau su co. Cach tot nhat la ket hop:

- CTAS/Data Pump cho logical backup muc schema/bang.
- Scheduler de tu dong hoa backup dinh ky.
- Flashback de sua loi thao tac nhanh dua tren audit log.
- RMAN cho su co lon va point-in-time recovery cua toan database.
