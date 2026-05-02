-- ==========================================================================
-- DEMO LITE - BƯỚC 2: Chạy bằng ADMIN / 12345 trên PDB XEPDB1
-- Thời gian: ~2–3 phút
-- Gộp: insertData + RBAC + VPD + Audit + OLS
-- ==========================================================================

SET SERVEROUTPUT ON;
ALTER SESSION SET NLS_NUMERIC_CHARACTERS = '.,';

-- ╔════════════════════════════════════════════════════════════════╗
-- ║  PHẦN 1: INSERT DỮ LIỆU (vừa đủ để demo)                    ║
-- ╚════════════════════════════════════════════════════════════════╝

-- 1a. Nhân viên (chỉ tạo đúng các user demo)
BEGIN
    -- 3 Điều phối viên
    INSERT INTO nhan_vien VALUES ('NV0001', N'Nguyễn Văn An',    N'Nam', DATE '1985-03-15', '100000000001', N'Hà Nội',    '0900000001', N'Điều phối viên', NULL);
    INSERT INTO nhan_vien VALUES ('NV0002', N'Trần Thị Bình',    N'Nữ',  DATE '1987-07-20', '100000000002', N'TP HCM',    '0900000002', N'Điều phối viên', NULL);
    INSERT INTO nhan_vien VALUES ('NV0003', N'Lê Minh Cường',    N'Nam', DATE '1983-11-10', '100000000003', N'Đà Nẵng',   '0900000003', N'Điều phối viên', NULL);

    -- 3 Bác sĩ
    INSERT INTO nhan_vien VALUES ('NV0021', N'Phạm Đức Hải',     N'Nam', DATE '1978-05-12', '200000000021', N'TP HCM',    '0910000021', N'Bác sĩ/Y sĩ', N'Tim mạch');
    INSERT INTO nhan_vien VALUES ('NV0022', N'Hoàng Thị Lan',    N'Nữ',  DATE '1980-09-25', '200000000022', N'Hà Nội',    '0910000022', N'Bác sĩ/Y sĩ', N'Thần kinh');
    INSERT INTO nhan_vien VALUES ('NV0050', N'Võ Thanh Sơn',     N'Nam', DATE '1975-01-30', '200000000050', N'TP HCM',    '0910000050', N'Bác sĩ/Y sĩ', N'Nội tổng quát');

    -- 3 Kỹ thuật viên
    INSERT INTO nhan_vien VALUES ('NV0121', N'Đặng Quốc Tuấn',   N'Nam', DATE '1992-04-18', '300000000121', N'Đà Nẵng',   '0920000121', N'Kỹ thuật viên', N'Xét nghiệm');
    INSERT INTO nhan_vien VALUES ('NV0122', N'Ngô Thị Mai',      N'Nữ',  DATE '1994-08-22', '300000000122', N'TP HCM',    '0920000122', N'Kỹ thuật viên', N'Chẩn đoán hình ảnh');
    INSERT INTO nhan_vien VALUES ('NV0123', N'Bùi Văn Đạt',      N'Nam', DATE '1991-12-05', '300000000123', N'Hà Nội',    '0920000123', N'Kỹ thuật viên', N'Xét nghiệm');

    COMMIT;
    DBMS_OUTPUT.PUT_LINE('[OK] Đã chèn 9 nhân viên.');
END;
/

-- 1b. Bệnh nhân (5 người)
BEGIN
    INSERT INTO benh_nhan VALUES ('BN000001', N'Trương Văn Khoa',   N'Nam', DATE '1965-02-14', '000000000001', N'12',  N'Nguyễn Huệ',   N'Quận 1',   N'TP HCM',  N'Cao huyết áp',      N'Tiểu đường',   N'Penicillin');
    INSERT INTO benh_nhan VALUES ('BN000002', N'Lý Thị Hồng',      N'Nữ',  DATE '1972-06-30', '000000000002', N'45',  N'Lê Lợi',       N'Quận 3',   N'TP HCM',  N'Viêm dạ dày',       N'Không',        N'Không');
    INSERT INTO benh_nhan VALUES ('BN000003', N'Phạm Minh Tuấn',   N'Nam', DATE '1988-11-20', '000000000003', N'78',  N'Trần Phú',     N'Quận 5',   N'TP HCM',  N'Không',             N'Hen suyễn',    N'Aspirin');
    INSERT INTO benh_nhan VALUES ('BN000004', N'Nguyễn Thị Dung',  N'Nữ',  DATE '1955-01-01', '000000000004', N'100', N'Hai Bà Trưng', N'Hoàn Kiếm', N'Hà Nội', N'Tim mạch',          N'Cao huyết áp', N'Không');
    INSERT INTO benh_nhan VALUES ('BN000005', N'Đỗ Quang Vinh',    N'Nam', DATE '1995-08-15', '000000000005', N'22',  N'Bạch Đằng',    N'Hải Châu',  N'Đà Nẵng', N'Không',            N'Không',        N'Không');

    COMMIT;
    DBMS_OUTPUT.PUT_LINE('[OK] Đã chèn 5 bệnh nhân.');
END;
/

-- 1c. HSBA (phân bổ cho 3 BS, mỗi BS ~3-4 HSBA)
BEGIN
    -- BS NV0050 phụ trách
    INSERT INTO hsba VALUES ('HS000001', 'BN000001', SYSDATE - 30, N'Tăng huyết áp độ 2',       N'Thuốc hạ áp + chế độ ăn', 'NV0050', 'K01', N'Cần tái khám sau 2 tuần');
    INSERT INTO hsba VALUES ('HS000002', 'BN000002', SYSDATE - 20, N'Viêm dạ dày mãn tính',     N'Thuốc ức chế bơm proton', 'NV0050', 'K01', N'Đang theo dõi');
    INSERT INTO hsba VALUES ('HS000003', 'BN000003', SYSDATE - 10, N'Viêm họng cấp',            N'Kháng sinh + kháng viêm', 'NV0050', 'K01', NULL);

    -- BS NV0021 phụ trách
    INSERT INTO hsba VALUES ('HS000004', 'BN000004', SYSDATE - 25, N'Rối loạn nhịp tim',        N'Thuốc chống loạn nhịp',   'NV0021', 'K02', N'Cần siêu âm tim');
    INSERT INTO hsba VALUES ('HS000005', 'BN000001', SYSDATE - 5,  N'Đau thắt ngực',            N'Nghỉ ngơi + thuốc giãn mạch', 'NV0021', 'K02', NULL);

    -- BS NV0022 phụ trách
    INSERT INTO hsba VALUES ('HS000006', 'BN000005', SYSDATE - 15, N'Đau đầu migraine',         N'Thuốc giảm đau + phòng ngừa', 'NV0022', 'K03', N'Theo dõi định kỳ');
    INSERT INTO hsba VALUES ('HS000007', 'BN000003', SYSDATE - 3,  N'Chóng mặt do tiền đình',   N'Thuốc tuần hoàn não',     'NV0022', 'K03', NULL);

    COMMIT;
    DBMS_OUTPUT.PUT_LINE('[OK] Đã chèn 7 HSBA.');
END;
/

-- 1d. HSBA_DV (dịch vụ xét nghiệm, phân cho KTV)
BEGIN
    -- KTV NV0121 thực hiện
    INSERT INTO hsba_dv VALUES ('HS000001', N'Xét nghiệm máu',        SYSDATE - 29, 'NV0121', N'Chỉ số bình thường');
    INSERT INTO hsba_dv VALUES ('HS000002', N'Xét nghiệm nước tiểu',  SYSDATE - 19, 'NV0121', N'Bình thường');
    INSERT INTO hsba_dv VALUES ('HS000004', N'Điện tâm đồ',           SYSDATE - 24, 'NV0121', NULL);

    -- KTV NV0122 thực hiện
    INSERT INTO hsba_dv VALUES ('HS000003', N'Chụp X-quang ngực',     SYSDATE - 9,  'NV0122', NULL);
    INSERT INTO hsba_dv VALUES ('HS000006', N'Chụp MRI não',          SYSDATE - 14, 'NV0122', N'Không phát hiện bất thường');

    -- KTV NV0123 thực hiện
    INSERT INTO hsba_dv VALUES ('HS000005', N'Xét nghiệm men tim',    SYSDATE - 4,  'NV0123', NULL);
    INSERT INTO hsba_dv VALUES ('HS000007', N'Xét nghiệm máu',        SYSDATE - 2,  'NV0123', NULL);

    COMMIT;
    DBMS_OUTPUT.PUT_LINE('[OK] Đã chèn 7 HSBA_DV.');
END;
/

-- 1e. Đơn thuốc
BEGIN
    INSERT INTO don_thuoc VALUES ('HS000001', SYSDATE - 30, N'Amlodipine 5mg',    N'Ngày 1 viên, uống sáng');
    INSERT INTO don_thuoc VALUES ('HS000001', SYSDATE - 30, N'Losartan 50mg',     N'Ngày 1 viên, uống tối');
    INSERT INTO don_thuoc VALUES ('HS000002', SYSDATE - 20, N'Omeprazole 20mg',   N'Ngày 2 viên, trước ăn 30 phút');
    INSERT INTO don_thuoc VALUES ('HS000003', SYSDATE - 10, N'Amoxicillin 500mg', N'Ngày 3 lần, mỗi lần 1 viên');
    INSERT INTO don_thuoc VALUES ('HS000004', SYSDATE - 25, N'Amiodarone 200mg',  N'Ngày 1 viên');
    INSERT INTO don_thuoc VALUES ('HS000006', SYSDATE - 15, N'Sumatriptan 50mg',  N'Khi đau, tối đa 2 viên/ngày');
    INSERT INTO don_thuoc VALUES ('HS000005', SYSDATE - 5,  N'Nitroglycerin 0.5mg', N'Ngậm dưới lưỡi khi đau');

    COMMIT;
    DBMS_OUTPUT.PUT_LINE('[OK] Đã chèn 7 đơn thuốc.');
END;
/

-- ╔════════════════════════════════════════════════════════════════╗
-- ║  PHẦN 2: RBAC - Tạo Roles + Grants + Views + Assign          ║
-- ╚════════════════════════════════════════════════════════════════╝

-- 2a. Tạo 4 roles
DECLARE
    v_count NUMBER;
    PROCEDURE create_role_safe(p_role VARCHAR2) IS
    BEGIN
        SELECT COUNT(*) INTO v_count FROM dba_roles WHERE role = p_role;
        IF v_count = 0 THEN
            EXECUTE IMMEDIATE 'CREATE ROLE ' || p_role;
        END IF;
    END;
BEGIN
    create_role_safe('RL_KYTHUATVIEN');
    create_role_safe('RL_BENHNHAN');
    create_role_safe('RL_DIEUPHOIVIEN');
    create_role_safe('RL_BACSI');
    DBMS_OUTPUT.PUT_LINE('[OK] 4 roles đã tạo.');
END;
/

-- 2b. Views self-scope cho KTV + BN (RBAC, không dùng VPD)
CREATE OR REPLACE VIEW ADMIN.V_HSBA_DV_KTV AS
    SELECT MA_HSBA, LOAI_DV, NGAY_DV, MA_KTV, KET_QUA
    FROM   ADMIN.HSBA_DV
    WHERE  MA_KTV = SYS_CONTEXT('USERENV', 'SESSION_USER')
    WITH CHECK OPTION CONSTRAINT CK_V_HSBA_DV_KTV;

CREATE OR REPLACE VIEW ADMIN.V_NHAN_VIEN_SELF AS
    SELECT MA_NV, HO_TEN, PHAI, NGAY_SINH, CMND,
           QUE_QUAN, SDT, VAI_TRO, CHUYEN_KHOA
    FROM   ADMIN.NHAN_VIEN
    WHERE  MA_NV = SYS_CONTEXT('USERENV', 'SESSION_USER')
    WITH CHECK OPTION CONSTRAINT CK_V_NHAN_VIEN_SELF;

CREATE OR REPLACE VIEW ADMIN.V_BENH_NHAN_SELF AS
    SELECT MA_BN, TEN_BN, PHAI, NGAY_SINH, CCCD,
           SO_NHA, TEN_DUONG, QUAN_HUYEN, TINH_TP,
           TIEN_SU_BENH, TIEN_SU_BENH_GD, DI_UNG_THUOC
    FROM   ADMIN.BENH_NHAN
    WHERE  MA_BN = SYS_CONTEXT('USERENV', 'SESSION_USER')
    WITH CHECK OPTION CONSTRAINT CK_V_BENH_NHAN_SELF;

-- 2c. Grants cho KTV
GRANT SELECT ON ADMIN.V_HSBA_DV_KTV TO RL_KYTHUATVIEN;
GRANT UPDATE (KET_QUA) ON ADMIN.V_HSBA_DV_KTV TO RL_KYTHUATVIEN;
GRANT SELECT ON ADMIN.V_NHAN_VIEN_SELF TO RL_KYTHUATVIEN;
GRANT UPDATE (QUE_QUAN, SDT) ON ADMIN.V_NHAN_VIEN_SELF TO RL_KYTHUATVIEN;

-- 2d. Grants cho BN
GRANT SELECT ON ADMIN.V_BENH_NHAN_SELF TO RL_BENHNHAN;
GRANT UPDATE (SO_NHA, TEN_DUONG, QUAN_HUYEN, TINH_TP,
              TIEN_SU_BENH, TIEN_SU_BENH_GD, DI_UNG_THUOC)
    ON ADMIN.V_BENH_NHAN_SELF TO RL_BENHNHAN;

-- 2e. MV cho dropdown DPV/BS
BEGIN EXECUTE IMMEDIATE 'DROP MATERIALIZED VIEW ADMIN.MV_BACSI_LIST'; EXCEPTION WHEN OTHERS THEN NULL; END;
/
BEGIN EXECUTE IMMEDIATE 'DROP MATERIALIZED VIEW ADMIN.MV_KTV_LIST'; EXCEPTION WHEN OTHERS THEN NULL; END;
/

CREATE MATERIALIZED VIEW ADMIN.MV_BACSI_LIST BUILD IMMEDIATE REFRESH COMPLETE ON DEMAND AS
    SELECT MA_NV, HO_TEN, CHUYEN_KHOA FROM ADMIN.NHAN_VIEN WHERE VAI_TRO = N'Bác sĩ/Y sĩ';

CREATE MATERIALIZED VIEW ADMIN.MV_KTV_LIST BUILD IMMEDIATE REFRESH COMPLETE ON DEMAND AS
    SELECT MA_NV, HO_TEN, CHUYEN_KHOA FROM ADMIN.NHAN_VIEN WHERE VAI_TRO = N'Kỹ thuật viên';

-- 2f. Grants cho DPV (TC#2)
GRANT SELECT, INSERT ON ADMIN.BENH_NHAN TO RL_DIEUPHOIVIEN;
GRANT UPDATE (TEN_BN, PHAI, NGAY_SINH, CCCD, SO_NHA, TEN_DUONG, QUAN_HUYEN, TINH_TP,
              TIEN_SU_BENH, TIEN_SU_BENH_GD, DI_UNG_THUOC) ON ADMIN.BENH_NHAN TO RL_DIEUPHOIVIEN;
GRANT SELECT, INSERT ON ADMIN.HSBA TO RL_DIEUPHOIVIEN;
GRANT UPDATE (MA_KHOA, MA_BS) ON ADMIN.HSBA TO RL_DIEUPHOIVIEN;
GRANT SELECT, INSERT ON ADMIN.HSBA_DV TO RL_DIEUPHOIVIEN;
GRANT UPDATE (MA_KTV) ON ADMIN.HSBA_DV TO RL_DIEUPHOIVIEN;
GRANT SELECT ON ADMIN.NHAN_VIEN TO RL_DIEUPHOIVIEN;
GRANT UPDATE (QUE_QUAN, SDT) ON ADMIN.NHAN_VIEN TO RL_DIEUPHOIVIEN;
GRANT SELECT ON ADMIN.MV_BACSI_LIST TO RL_DIEUPHOIVIEN;
GRANT SELECT ON ADMIN.MV_KTV_LIST   TO RL_DIEUPHOIVIEN;

-- 2g. Grants cho BS (TC#3)
GRANT SELECT ON ADMIN.HSBA TO RL_BACSI;
GRANT UPDATE (CHUAN_DOAN, DIEU_TRI, KET_LUAN) ON ADMIN.HSBA TO RL_BACSI;
GRANT SELECT, INSERT, DELETE ON ADMIN.HSBA_DV TO RL_BACSI;
GRANT SELECT ON ADMIN.BENH_NHAN TO RL_BACSI;
GRANT UPDATE (TIEN_SU_BENH, TIEN_SU_BENH_GD, DI_UNG_THUOC) ON ADMIN.BENH_NHAN TO RL_BACSI;
GRANT SELECT, INSERT, DELETE ON ADMIN.DON_THUOC TO RL_BACSI;
GRANT UPDATE (TEN_THUOC, LIEU_DUNG) ON ADMIN.DON_THUOC TO RL_BACSI;
GRANT SELECT ON ADMIN.NHAN_VIEN TO RL_BACSI;
GRANT UPDATE (QUE_QUAN, SDT) ON ADMIN.NHAN_VIEN TO RL_BACSI;
GRANT SELECT ON ADMIN.MV_KTV_LIST TO RL_BACSI;

-- 2h. Assign roles cho demo users
BEGIN
    -- DPV
    EXECUTE IMMEDIATE 'GRANT RL_DIEUPHOIVIEN TO NV0001';
    EXECUTE IMMEDIATE 'GRANT RL_DIEUPHOIVIEN TO NV0002';
    EXECUTE IMMEDIATE 'GRANT RL_DIEUPHOIVIEN TO NV0003';
    -- BS
    EXECUTE IMMEDIATE 'GRANT RL_BACSI TO NV0021';
    EXECUTE IMMEDIATE 'GRANT RL_BACSI TO NV0022';
    EXECUTE IMMEDIATE 'GRANT RL_BACSI TO NV0050';
    -- KTV
    EXECUTE IMMEDIATE 'GRANT RL_KYTHUATVIEN TO NV0121';
    EXECUTE IMMEDIATE 'GRANT RL_KYTHUATVIEN TO NV0122';
    EXECUTE IMMEDIATE 'GRANT RL_KYTHUATVIEN TO NV0123';
    -- BN
    FOR i IN 1..5 LOOP
        EXECUTE IMMEDIATE 'GRANT RL_BENHNHAN TO BN' || LPAD(i, 6, '0');
    END LOOP;

    DBMS_OUTPUT.PUT_LINE('[OK] RBAC hoàn thành: 4 roles + grants + assign.');
END;
/

-- ╔════════════════════════════════════════════════════════════════╗
-- ║  PHẦN 3: VPD - Virtual Private Database                      ║
-- ╚════════════════════════════════════════════════════════════════╝

-- 3a. FN_GET_ROLE helper
CREATE OR REPLACE FUNCTION admin.fn_get_role RETURN VARCHAR2
    DETERMINISTIC
AS
    v_user VARCHAR2(30) := SYS_CONTEXT('USERENV', 'SESSION_USER');
    v_num  NUMBER;
BEGIN
    IF v_user = 'ADMIN' THEN RETURN 'DBA'; END IF;
    IF v_user LIKE 'BN%' THEN RETURN 'BENHNHAN'; END IF;
    IF v_user LIKE 'NV%' THEN
        BEGIN
            v_num := TO_NUMBER(SUBSTR(v_user, 3));
            IF v_num BETWEEN 1 AND 20 THEN RETURN 'DIEUPHOIVIEN';
            ELSIF v_num BETWEEN 21 AND 120 THEN RETURN 'BACSI';
            ELSIF v_num BETWEEN 121 AND 170 THEN RETURN 'KYTHUATVIEN';
            END IF;
        EXCEPTION WHEN OTHERS THEN NULL;
        END;
    END IF;
    RETURN 'UNKNOWN';
END;
/

-- 3b. Policy functions
CREATE OR REPLACE FUNCTION admin.fn_policy_nhanvien (p_schema VARCHAR2, p_table VARCHAR2) RETURN VARCHAR2 AS
    v_role VARCHAR2(20) := admin.fn_get_role;
BEGIN
    IF v_role IN ('DBA', 'KYTHUATVIEN') THEN RETURN NULL;
    ELSIF v_role IN ('DIEUPHOIVIEN', 'BACSI') THEN RETURN 'MA_NV = ''' || SYS_CONTEXT('USERENV','SESSION_USER') || '''';
    ELSE RETURN '1=0'; END IF;
END;
/

CREATE OR REPLACE FUNCTION admin.fn_policy_benhnhan (p_schema VARCHAR2, p_table VARCHAR2) RETURN VARCHAR2 AS
    v_user VARCHAR2(30) := SYS_CONTEXT('USERENV','SESSION_USER');
    v_role VARCHAR2(20) := admin.fn_get_role;
BEGIN
    IF v_role IN ('DBA', 'DIEUPHOIVIEN', 'BENHNHAN') THEN RETURN NULL;
    ELSIF v_role = 'BACSI' THEN RETURN 'MA_BN IN (SELECT MA_BN FROM ADMIN.HSBA WHERE MA_BS = ''' || v_user || ''')';
    ELSE RETURN '1=0'; END IF;
END;
/

CREATE OR REPLACE FUNCTION admin.fn_policy_hsba (p_schema VARCHAR2, p_table VARCHAR2) RETURN VARCHAR2 AS
    v_user VARCHAR2(30) := SYS_CONTEXT('USERENV','SESSION_USER');
    v_role VARCHAR2(20) := admin.fn_get_role;
BEGIN
    IF v_role IN ('DBA', 'DIEUPHOIVIEN') THEN RETURN NULL;
    ELSIF v_role = 'BACSI' THEN RETURN 'MA_BS = ''' || v_user || '''';
    ELSE RETURN '1=0'; END IF;
END;
/

CREATE OR REPLACE FUNCTION admin.fn_policy_hsba_dv (p_schema VARCHAR2, p_table VARCHAR2) RETURN VARCHAR2 AS
    v_user VARCHAR2(30) := SYS_CONTEXT('USERENV','SESSION_USER');
    v_role VARCHAR2(20) := admin.fn_get_role;
BEGIN
    IF v_role IN ('DBA', 'DIEUPHOIVIEN', 'KYTHUATVIEN') THEN RETURN NULL;
    ELSIF v_role = 'BACSI' THEN RETURN 'MA_HSBA IN (SELECT MA_HSBA FROM ADMIN.HSBA WHERE MA_BS = ''' || v_user || ''')';
    ELSE RETURN '1=0'; END IF;
END;
/

CREATE OR REPLACE FUNCTION admin.fn_policy_don_thuoc (p_schema VARCHAR2, p_table VARCHAR2) RETURN VARCHAR2 AS
    v_user VARCHAR2(30) := SYS_CONTEXT('USERENV','SESSION_USER');
    v_role VARCHAR2(20) := admin.fn_get_role;
BEGIN
    IF v_role = 'DBA' THEN RETURN NULL;
    ELSIF v_role = 'BACSI' THEN RETURN 'MA_HSBA IN (SELECT MA_HSBA FROM ADMIN.HSBA WHERE MA_BS = ''' || v_user || ''')';
    ELSE RETURN '1=0'; END IF;
END;
/

-- 3c. Drop policies cũ (idempotent)
BEGIN DBMS_RLS.DROP_POLICY('ADMIN','NHAN_VIEN','POL_NHANVIEN_SELF'); EXCEPTION WHEN OTHERS THEN NULL; END;
/
BEGIN DBMS_RLS.DROP_POLICY('ADMIN','BENH_NHAN','POL_BN_RLS');        EXCEPTION WHEN OTHERS THEN NULL; END;
/
BEGIN DBMS_RLS.DROP_POLICY('ADMIN','HSBA',     'POL_HSBA_RLS');      EXCEPTION WHEN OTHERS THEN NULL; END;
/
BEGIN DBMS_RLS.DROP_POLICY('ADMIN','HSBA_DV',  'POL_HSBA_DV_RLS');   EXCEPTION WHEN OTHERS THEN NULL; END;
/
BEGIN DBMS_RLS.DROP_POLICY('ADMIN','DON_THUOC','POL_DON_THUOC_RLS'); EXCEPTION WHEN OTHERS THEN NULL; END;
/

-- 3d. Add 5 VPD policies
BEGIN
    DBMS_RLS.ADD_POLICY('ADMIN','NHAN_VIEN','POL_NHANVIEN_SELF','ADMIN','FN_POLICY_NHANVIEN','SELECT,UPDATE',update_check=>TRUE,enable=>TRUE);
    DBMS_OUTPUT.PUT_LINE('[OK] VPD: POL_NHANVIEN_SELF');
END;
/
BEGIN
    DBMS_RLS.ADD_POLICY('ADMIN','BENH_NHAN','POL_BN_RLS','ADMIN','FN_POLICY_BENHNHAN','SELECT,INSERT,UPDATE,DELETE',update_check=>TRUE,enable=>TRUE);
    DBMS_OUTPUT.PUT_LINE('[OK] VPD: POL_BN_RLS');
END;
/
BEGIN
    DBMS_RLS.ADD_POLICY('ADMIN','HSBA','POL_HSBA_RLS','ADMIN','FN_POLICY_HSBA','SELECT,INSERT,UPDATE,DELETE',update_check=>TRUE,enable=>TRUE);
    DBMS_OUTPUT.PUT_LINE('[OK] VPD: POL_HSBA_RLS');
END;
/
BEGIN
    DBMS_RLS.ADD_POLICY('ADMIN','HSBA_DV','POL_HSBA_DV_RLS','ADMIN','FN_POLICY_HSBA_DV','SELECT,INSERT,UPDATE,DELETE',update_check=>TRUE,enable=>TRUE);
    DBMS_OUTPUT.PUT_LINE('[OK] VPD: POL_HSBA_DV_RLS');
END;
/
BEGIN
    DBMS_RLS.ADD_POLICY('ADMIN','DON_THUOC','POL_DON_THUOC_RLS','ADMIN','FN_POLICY_DON_THUOC','SELECT,INSERT,UPDATE,DELETE',update_check=>TRUE,enable=>TRUE);
    DBMS_OUTPUT.PUT_LINE('[OK] VPD: POL_DON_THUOC_RLS');
END;
/

-- ╔════════════════════════════════════════════════════════════════╗
-- ║  PHẦN 4: AUDIT - Trigger + Standard + FGA/Unified             ║
-- ╚════════════════════════════════════════════════════════════════╝

-- 4a. Bảng + Trigger ghi vết KET_QUA (TC#4)
BEGIN EXECUTE IMMEDIATE 'DROP TABLE AUDIT_KETQUA CASCADE CONSTRAINTS'; EXCEPTION WHEN OTHERS THEN NULL; END;
/

CREATE TABLE AUDIT_KETQUA (
    audit_id            NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    ma_hsba             VARCHAR2(8),
    loai_dv             NVARCHAR2(50),
    ngay_dv             DATE,
    gia_tri_cu          NVARCHAR2(50),
    gia_tri_moi         NVARCHAR2(50),
    nguoi_cap_nhat      VARCHAR2(30),
    thoi_gian_cap_nhat  TIMESTAMP DEFAULT SYSTIMESTAMP
);

CREATE OR REPLACE TRIGGER TRG_AUDIT_KETQUA
BEFORE UPDATE OF KET_QUA ON HSBA_DV
FOR EACH ROW
BEGIN
    INSERT INTO AUDIT_KETQUA (ma_hsba, loai_dv, ngay_dv, gia_tri_cu, gia_tri_moi, nguoi_cap_nhat, thoi_gian_cap_nhat)
    VALUES (:OLD.MA_HSBA, :OLD.LOAI_DV, :OLD.NGAY_DV, :OLD.KET_QUA, :NEW.KET_QUA,
            SYS_CONTEXT('USERENV','SESSION_USER'), SYSTIMESTAMP);
END;
/

GRANT SELECT ON ADMIN.AUDIT_KETQUA TO RL_KYTHUATVIEN;

-- 4b. Standard Audit (5 ngữ cảnh) - cần chạy bằng SYSDBA?
-- Các lệnh AUDIT chỉ hoạt động nếu user ADMIN có DBA role
BEGIN
    EXECUTE IMMEDIATE 'AUDIT SELECT ON admin.hsba BY NV0021 BY ACCESS';
    EXECUTE IMMEDIATE 'AUDIT UPDATE ON admin.don_thuoc BY ACCESS WHENEVER NOT SUCCESSFUL';
    EXECUTE IMMEDIATE 'AUDIT DELETE ON admin.hsba_dv BY ACCESS WHENEVER SUCCESSFUL';
    -- NC4 + NC5: cần SP_XOA_BENHAN và V_THONGTIN_BENHNHAN tồn tại
    DBMS_OUTPUT.PUT_LINE('[OK] Standard Audit: 3 ngữ cảnh (NC1-3).');
EXCEPTION WHEN OTHERS THEN
    DBMS_OUTPUT.PUT_LINE('[WARN] Standard Audit có thể cần SYSDBA: ' || SQLERRM);
END;
/

-- 4c. FGA policies (YC3: 3a, 3c, 3d)
BEGIN
    DBMS_FGA.DROP_POLICY('ADMIN','DON_THUOC','FGA_AUDIT_UPDATE_DONTHUOC');
EXCEPTION WHEN OTHERS THEN NULL;
END;
/
BEGIN
    DBMS_FGA.ADD_POLICY(
        object_schema => 'ADMIN', object_name => 'DON_THUOC',
        policy_name => 'FGA_AUDIT_UPDATE_DONTHUOC',
        audit_column => 'MA_HSBA,NGAY_DT,TEN_THUOC,LIEU_DUNG',
        statement_types => 'UPDATE',
        audit_trail => DBMS_FGA.DB + DBMS_FGA.EXTENDED, enable => TRUE);
    DBMS_OUTPUT.PUT_LINE('[OK] FGA: FGA_AUDIT_UPDATE_DONTHUOC (YC3-3a)');
END;
/

BEGIN
    DBMS_FGA.DROP_POLICY('ADMIN','HSBA','FGA_AUDIT_ILLEGAL_UPDATE_HSBA');
EXCEPTION WHEN OTHERS THEN NULL;
END;
/
BEGIN
    DBMS_FGA.ADD_POLICY(
        object_schema => 'ADMIN', object_name => 'HSBA',
        policy_name => 'FGA_AUDIT_ILLEGAL_UPDATE_HSBA',
        audit_column => 'CHUAN_DOAN,DIEU_TRI,KET_LUAN',
        audit_condition => 'MA_BS != SYS_CONTEXT(''USERENV'',''SESSION_USER'')',
        statement_types => 'UPDATE',
        audit_trail => DBMS_FGA.DB + DBMS_FGA.EXTENDED, enable => TRUE);
    DBMS_OUTPUT.PUT_LINE('[OK] FGA: FGA_AUDIT_ILLEGAL_UPDATE_HSBA (YC3-3c)');
END;
/

BEGIN
    DBMS_FGA.DROP_POLICY('ADMIN','HSBA_DV','FGA_AUDIT_ILLEGAL_DML_HSBA_DV');
EXCEPTION WHEN OTHERS THEN NULL;
END;
/
BEGIN
    DBMS_FGA.ADD_POLICY(
        object_schema => 'ADMIN', object_name => 'HSBA_DV',
        policy_name => 'FGA_AUDIT_ILLEGAL_DML_HSBA_DV',
        audit_condition => 'SYS_CONTEXT(''USERENV'',''SESSION_USER'') != ''ADMIN'' OR TO_CHAR(SYSDATE,''HH24'') NOT BETWEEN ''06'' AND ''18''',
        statement_types => 'INSERT,UPDATE,DELETE',
        audit_trail => DBMS_FGA.DB + DBMS_FGA.EXTENDED, enable => TRUE);
    DBMS_OUTPUT.PUT_LINE('[OK] FGA: FGA_AUDIT_ILLEGAL_DML_HSBA_DV (YC3-3d)');
END;
/

-- 4d. Unified Audit Policy (YC3-3b)
BEGIN EXECUTE IMMEDIATE 'DROP AUDIT POLICY AUDIT_HSBA_UPDATE_CHANDOAN'; EXCEPTION WHEN OTHERS THEN NULL; END;
/
BEGIN
    EXECUTE IMMEDIATE 'CREATE AUDIT POLICY AUDIT_HSBA_UPDATE_CHANDOAN ACTIONS UPDATE ON ADMIN.HSBA';
    EXECUTE IMMEDIATE 'AUDIT POLICY AUDIT_HSBA_UPDATE_CHANDOAN WHENEVER SUCCESSFUL';
    DBMS_OUTPUT.PUT_LINE('[OK] Unified: AUDIT_HSBA_UPDATE_CHANDOAN (YC3-3b)');
EXCEPTION WHEN OTHERS THEN
    DBMS_OUTPUT.PUT_LINE('[WARN] Unified Audit: ' || SQLERRM);
END;
/

-- ╔════════════════════════════════════════════════════════════════╗
-- ║  PHẦN 5: OLS - Oracle Label Security (YC2)                   ║
-- ╚════════════════════════════════════════════════════════════════╝

-- 5a. Tạo bảng THONGBAO
BEGIN EXECUTE IMMEDIATE 'DROP TABLE THONGBAO CASCADE CONSTRAINTS'; EXCEPTION WHEN OTHERS THEN NULL; END;
/

CREATE TABLE THONGBAO (
    MA_TB     VARCHAR2(10) PRIMARY KEY,
    NOI_DUNG  VARCHAR2(500) NOT NULL,
    NGAY_GIO  TIMESTAMP NOT NULL,
    DIA_DIEM  VARCHAR2(100) NOT NULL
);

-- Grant SELECT cho các user OLS test
GRANT SELECT ON THONGBAO TO NV0001;
GRANT SELECT ON THONGBAO TO NV0021;
GRANT SELECT ON THONGBAO TO NV0022;
GRANT SELECT ON THONGBAO TO NV0121;
GRANT SELECT ON THONGBAO TO NV0122;
GRANT SELECT ON THONGBAO TO NV0002;
GRANT SELECT ON THONGBAO TO NV0003;
GRANT SELECT ON THONGBAO TO NV0123;

-- 5b. Tạo OLS Policy
BEGIN
    SA_SYSDBA.CREATE_POLICY(POLICY_NAME => 'REGION_POLICY', COLUMN_NAME => 'REGION_LABEL');
    DBMS_OUTPUT.PUT_LINE('[OK] OLS Policy REGION_POLICY đã tạo.');
EXCEPTION WHEN OTHERS THEN
    DBMS_OUTPUT.PUT_LINE('[WARN] OLS: ' || SQLERRM);
END;
/

EXEC SA_SYSDBA.ENABLE_POLICY('REGION_POLICY');

PROMPT ===================================================================
PROMPT  ⚠️ CẦN DISCONNECT rồi CONNECT lại ADMIN trước khi tiếp tục!
PROMPT  Sau khi reconnect, chạy tiếp file demo_lite_03_ols.sql
PROMPT ===================================================================
