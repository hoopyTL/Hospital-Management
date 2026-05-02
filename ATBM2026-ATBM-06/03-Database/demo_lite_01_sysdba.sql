-- ==========================================================================
-- DEMO LITE - BƯỚC 1: Chạy bằng SYS AS SYSDBA
-- Thời gian: ~1 phút
-- ==========================================================================

-- ========== 0. KÍCH HOẠT KIỂM TOÁN (chạy từ CDB root) ==========
-- Phải chạy TRƯỚC khi chuyển sang PDB
ALTER SYSTEM SET audit_trail=db,extended SCOPE=SPFILE;

-- Mở PDB và chuyển session
ALTER SESSION SET CONTAINER = XEPDB1;
ALTER SESSION SET "_ORACLE_SCRIPT"=TRUE;

-- ========== 1. TẠO USER ADMIN + BẢNG ==========

-- Drop ADMIN cũ nếu có
BEGIN EXECUTE IMMEDIATE 'DROP USER ADMIN CASCADE'; EXCEPTION WHEN OTHERS THEN NULL; END;
/

CREATE USER ADMIN IDENTIFIED BY 12345;
GRANT CONNECT, RESOURCE, DBA TO ADMIN;
GRANT UNLIMITED TABLESPACE TO ADMIN;

ALTER SESSION SET CURRENT_SCHEMA = ADMIN;

-- Tạo 5 bảng chính
CREATE TABLE benh_nhan (
   ma_bn           VARCHAR2(8) PRIMARY KEY,
   ten_bn          NVARCHAR2(100) NOT NULL,
   phai            NVARCHAR2(3) CHECK (phai IN (N'Nam', N'Nữ')),
   ngay_sinh       DATE NOT NULL,
   cccd            VARCHAR2(12) UNIQUE NOT NULL,
   so_nha          NVARCHAR2(20),
   ten_duong       NVARCHAR2(100),
   quan_huyen      NVARCHAR2(50),
   tinh_tp         NVARCHAR2(50),
   tien_su_benh    NVARCHAR2(1000),
   tien_su_benh_gd NVARCHAR2(1000),
   di_ung_thuoc    NVARCHAR2(1000)
);

CREATE TABLE nhan_vien (
   ma_nv       VARCHAR2(8) PRIMARY KEY,
   ho_ten      NVARCHAR2(100) NOT NULL,
   phai        NVARCHAR2(3) CHECK (phai IN (N'Nam', N'Nữ')),
   ngay_sinh   DATE NOT NULL,
   cmnd        VARCHAR2(12) UNIQUE NOT NULL,
   que_quan    NVARCHAR2(20),
   sdt         VARCHAR2(10) UNIQUE NOT NULL,
   vai_tro     NVARCHAR2(20) CHECK (vai_tro IN (N'Điều phối viên', N'Bác sĩ/Y sĩ', N'Kỹ thuật viên', N'Bệnh nhân')),
   chuyen_khoa NVARCHAR2(50)
);

CREATE TABLE hsba (
   ma_hsba    VARCHAR2(8) PRIMARY KEY,
   ma_bn      VARCHAR2(8),
   ngay       DATE,
   chuan_doan NVARCHAR2(1000),
   dieu_tri   NVARCHAR2(1000),
   ma_bs      VARCHAR2(8),
   ma_khoa    VARCHAR2(8),
   ket_luan   NVARCHAR2(1000),
   CONSTRAINT fk_hsba_bn FOREIGN KEY (ma_bn) REFERENCES benh_nhan(ma_bn),
   CONSTRAINT fk_hsba_bs FOREIGN KEY (ma_bs) REFERENCES nhan_vien(ma_nv)
);

CREATE TABLE hsba_dv (
   ma_hsba VARCHAR2(8),
   loai_dv NVARCHAR2(50),
   ngay_dv DATE,
   ma_ktv  VARCHAR2(8),
   ket_qua NVARCHAR2(50),
   CONSTRAINT pk_hsba_dv PRIMARY KEY (ma_hsba, loai_dv, ngay_dv),
   CONSTRAINT fk_hsba_dv_hsba FOREIGN KEY (ma_hsba) REFERENCES hsba(ma_hsba),
   CONSTRAINT fk_hsba_dv_nv FOREIGN KEY (ma_ktv) REFERENCES nhan_vien(ma_nv)
);

CREATE TABLE don_thuoc (
   ma_hsba   VARCHAR2(8),
   ngay_dt   DATE,
   ten_thuoc NVARCHAR2(50),
   lieu_dung NVARCHAR2(1000),
   CONSTRAINT pk_don_thuoc PRIMARY KEY (ma_hsba, ngay_dt, ten_thuoc),
   CONSTRAINT fk_don_thuoc_hsba FOREIGN KEY (ma_hsba) REFERENCES hsba(ma_hsba)
);

-- ========== 2. TẠO 15 ORACLE USER CHO DEMO ==========

-- DPV: NV0001, NV0002, NV0003
DECLARE
    PROCEDURE create_user_safe(p_user VARCHAR2) IS
    BEGIN
        EXECUTE IMMEDIATE 'DROP USER ' || p_user || ' CASCADE';
    EXCEPTION WHEN OTHERS THEN NULL;
    END;
BEGIN
    -- Điều phối viên
    FOR i IN 1..3 LOOP
        create_user_safe('NV' || LPAD(i, 4, '0'));
        EXECUTE IMMEDIATE 'CREATE USER NV' || LPAD(i, 4, '0') || ' IDENTIFIED BY "123"';
        EXECUTE IMMEDIATE 'GRANT CREATE SESSION TO NV' || LPAD(i, 4, '0');
        EXECUTE IMMEDIATE 'GRANT UNLIMITED TABLESPACE TO NV' || LPAD(i, 4, '0');
    END LOOP;

    -- Bác sĩ: NV0021, NV0022, NV0050
    FOR i IN 1..3 LOOP
        DECLARE v_id NUMBER;
        BEGIN
            v_id := CASE i WHEN 1 THEN 21 WHEN 2 THEN 22 WHEN 3 THEN 50 END;
            create_user_safe('NV' || LPAD(v_id, 4, '0'));
            EXECUTE IMMEDIATE 'CREATE USER NV' || LPAD(v_id, 4, '0') || ' IDENTIFIED BY "123"';
            EXECUTE IMMEDIATE 'GRANT CREATE SESSION TO NV' || LPAD(v_id, 4, '0');
            EXECUTE IMMEDIATE 'GRANT UNLIMITED TABLESPACE TO NV' || LPAD(v_id, 4, '0');
        END;
    END LOOP;

    -- Kỹ thuật viên: NV0121, NV0122, NV0123
    FOR i IN 121..123 LOOP
        create_user_safe('NV' || LPAD(i, 4, '0'));
        EXECUTE IMMEDIATE 'CREATE USER NV' || LPAD(i, 4, '0') || ' IDENTIFIED BY "123"';
        EXECUTE IMMEDIATE 'GRANT CREATE SESSION TO NV' || LPAD(i, 4, '0');
        EXECUTE IMMEDIATE 'GRANT UNLIMITED TABLESPACE TO NV' || LPAD(i, 4, '0');
    END LOOP;

    -- Bệnh nhân: BN000001 đến BN000005
    FOR i IN 1..5 LOOP
        create_user_safe('BN' || LPAD(i, 6, '0'));
        EXECUTE IMMEDIATE 'CREATE USER BN' || LPAD(i, 6, '0') || ' IDENTIFIED BY "123"';
        EXECUTE IMMEDIATE 'GRANT CREATE SESSION TO BN' || LPAD(i, 6, '0');
        EXECUTE IMMEDIATE 'GRANT UNLIMITED TABLESPACE TO BN' || LPAD(i, 6, '0');
    END LOOP;

    DBMS_OUTPUT.PUT_LINE('=== Đã tạo 14 Oracle user cho demo ===');
END;
/

-- ========== 3. CẤP QUYỀN OLS CHO ADMIN (nếu cần demo YC2) ==========
GRANT SELECT ANY DICTIONARY TO ADMIN;

BEGIN
    EXECUTE IMMEDIATE 'GRANT EXECUTE ON LBACSYS.SA_COMPONENTS TO ADMIN WITH GRANT OPTION';
    EXECUTE IMMEDIATE 'GRANT EXECUTE ON LBACSYS.SA_USER_ADMIN TO ADMIN WITH GRANT OPTION';
    EXECUTE IMMEDIATE 'GRANT EXECUTE ON LBACSYS.SA_LABEL_ADMIN TO ADMIN WITH GRANT OPTION';
    EXECUTE IMMEDIATE 'GRANT EXECUTE ON SA_POLICY_ADMIN TO ADMIN WITH GRANT OPTION';
    EXECUTE IMMEDIATE 'GRANT EXECUTE ON CHAR_TO_LABEL TO ADMIN WITH GRANT OPTION';
    EXECUTE IMMEDIATE 'GRANT LBAC_DBA TO ADMIN';
    EXECUTE IMMEDIATE 'GRANT EXECUTE ON SA_SYSDBA TO ADMIN';
    EXECUTE IMMEDIATE 'GRANT EXECUTE ON TO_LBAC_DATA_LABEL TO ADMIN';
    DBMS_OUTPUT.PUT_LINE('Đã cấp quyền OLS cho ADMIN.');
EXCEPTION WHEN OTHERS THEN
    DBMS_OUTPUT.PUT_LINE('OLS grants skipped (OLS chưa cài?): ' || SQLERRM);
END;
/

-- ========== 4. RESET OLS (nếu chạy lại) ==========
ALTER SESSION SET NLS_NUMERIC_CHARACTERS = '.,';

BEGIN
    SA_POLICY_ADMIN.REMOVE_TABLE_POLICY('REGION_POLICY', 'ADMIN', 'THONGBAO');
EXCEPTION WHEN OTHERS THEN NULL;
END;
/

BEGIN
    SA_SYSDBA.DISABLE_POLICY('REGION_POLICY');
EXCEPTION WHEN OTHERS THEN NULL;
END;
/

BEGIN
    SA_SYSDBA.DROP_POLICY('REGION_POLICY', DROP_COLUMN => TRUE);
EXCEPTION WHEN OTHERS THEN NULL;
END;
/

COMMIT;

PROMPT ===================================================================
PROMPT   DEMO LITE BƯỚC 1 HOÀN THÀNH!
PROMPT   Tiếp theo: Đăng nhập ADMIN / 12345, chạy demo_lite_02_admin.sql
PROMPT   LƯU Ý: Nếu audit_trail lần đầu thay đổi, RESTART DB trước bước 2!
PROMPT ===================================================================
