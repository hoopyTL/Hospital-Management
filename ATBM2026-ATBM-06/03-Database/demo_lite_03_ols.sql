-- ==========================================================================
-- DEMO LITE - BƯỚC 3: Chạy bằng ADMIN / 12345 (SAU KHI DISCONNECT + RECONNECT)
-- Thời gian: ~1 phút
-- Tạo OLS components + insert dữ liệu THONGBAO + gán nhãn
-- ==========================================================================

SET SERVEROUTPUT ON;
ALTER SESSION SET NLS_NUMERIC_CHARACTERS = '.,';

-- ========== 1. TẠO LEVEL, COMPARTMENT, GROUP ==========
BEGIN
    SA_COMPONENTS.CREATE_LEVEL('REGION_POLICY', 1000, 'NV',  'Nhan vien');
    SA_COMPONENTS.CREATE_LEVEL('REGION_POLICY', 2000, 'LDK', 'Lanh dao khoa');
    SA_COMPONENTS.CREATE_LEVEL('REGION_POLICY', 3000, 'BGD', 'Ban giam doc');
END;
/

BEGIN
    SA_COMPONENTS.CREATE_COMPARTMENT('REGION_POLICY', 10, 'TH', 'Tieu hoa');
    SA_COMPONENTS.CREATE_COMPARTMENT('REGION_POLICY', 20, 'TK', 'Than kinh');
    SA_COMPONENTS.CREATE_COMPARTMENT('REGION_POLICY', 30, 'TM', 'Tim mach');
END;
/

BEGIN
    SA_COMPONENTS.CREATE_GROUP('REGION_POLICY', 100, 'HCM', 'Ho Chi Minh');
    SA_COMPONENTS.CREATE_GROUP('REGION_POLICY', 200, 'HP',  'Hai Phong');
    SA_COMPONENTS.CREATE_GROUP('REGION_POLICY', 300, 'HN',  'Ha Noi');
END;
/

-- ========== 2. TẠO LABELS ==========
BEGIN
    SA_LABEL_ADMIN.CREATE_LABEL('REGION_POLICY', 1001, 'NV');
    SA_LABEL_ADMIN.CREATE_LABEL('REGION_POLICY', 1002, 'BGD');
    SA_LABEL_ADMIN.CREATE_LABEL('REGION_POLICY', 1003, 'LDK');
    SA_LABEL_ADMIN.CREATE_LABEL('REGION_POLICY', 1004, 'LDK:TH');
    SA_LABEL_ADMIN.CREATE_LABEL('REGION_POLICY', 1005, 'NV:TH:HCM');
    SA_LABEL_ADMIN.CREATE_LABEL('REGION_POLICY', 1006, 'NV:TH:HN');
    SA_LABEL_ADMIN.CREATE_LABEL('REGION_POLICY', 1007, 'LDK:TH,TK:HP');
    DBMS_OUTPUT.PUT_LINE('[OK] Đã tạo 7 labels.');
END;
/

-- ========== 3. APPLY POLICY VỚI NO_CONTROL (để insert data) ==========
BEGIN
    SA_POLICY_ADMIN.APPLY_TABLE_POLICY(
        POLICY_NAME   => 'REGION_POLICY',
        SCHEMA_NAME   => 'ADMIN',
        TABLE_NAME    => 'THONGBAO',
        TABLE_OPTIONS => 'NO_CONTROL');
END;
/

-- ========== 4. INSERT 7 THÔNG BÁO ==========
INSERT INTO THONGBAO VALUES ('T1', N'Họp khẩn toàn thể nhân viên bệnh viện về quy trình ứng phó sự cố',
    TIMESTAMP '2025-05-01 08:00:00', N'Tất cả cơ sở', NULL);
INSERT INTO THONGBAO VALUES ('T2', N'Họp khẩn Ban giám đốc về kế hoạch điều hành bệnh viện',
    TIMESTAMP '2025-05-01 09:00:00', N'Phòng họp Ban giám đốc', NULL);
INSERT INTO THONGBAO VALUES ('T3', N'Họp khẩn dành cho lãnh đạo các khoa về phối hợp chuyên môn',
    TIMESTAMP '2025-05-01 10:00:00', N'Tất cả cơ sở', NULL);
INSERT INTO THONGBAO VALUES ('T4', N'Họp khẩn lãnh đạo Khoa Tiêu hóa về phân công nhân sự',
    TIMESTAMP '2025-05-01 11:00:00', N'Khoa Tiêu hóa', NULL);
INSERT INTO THONGBAO VALUES ('T5', N'Họp khẩn nhân viên Khoa Tiêu hóa tại cơ sở Hồ Chí Minh',
    TIMESTAMP '2025-05-01 12:00:00', N'Cơ sở Hồ Chí Minh', NULL);
INSERT INTO THONGBAO VALUES ('T6', N'Họp khẩn nhân viên Khoa Tiêu hóa tại cơ sở Hà Nội',
    TIMESTAMP '2025-05-01 13:00:00', N'Cơ sở Hà Nội', NULL);
INSERT INTO THONGBAO VALUES ('T7', N'Họp khẩn lãnh đạo Khoa Tiêu hóa và Khoa Thần kinh tại Hải Phòng',
    TIMESTAMP '2025-05-01 14:00:00', N'Cơ sở Hải Phòng', NULL);
COMMIT;

-- ========== 5. GÁN LABEL CHO TỪNG DÒNG DỮ LIỆU ==========
UPDATE THONGBAO SET REGION_LABEL = CHAR_TO_LABEL('REGION_POLICY', 'NV')          WHERE MA_TB = 'T1';
UPDATE THONGBAO SET REGION_LABEL = CHAR_TO_LABEL('REGION_POLICY', 'BGD')         WHERE MA_TB = 'T2';
UPDATE THONGBAO SET REGION_LABEL = CHAR_TO_LABEL('REGION_POLICY', 'LDK')         WHERE MA_TB = 'T3';
UPDATE THONGBAO SET REGION_LABEL = CHAR_TO_LABEL('REGION_POLICY', 'LDK:TH')     WHERE MA_TB = 'T4';
UPDATE THONGBAO SET REGION_LABEL = CHAR_TO_LABEL('REGION_POLICY', 'NV:TH:HCM')  WHERE MA_TB = 'T5';
UPDATE THONGBAO SET REGION_LABEL = CHAR_TO_LABEL('REGION_POLICY', 'NV:TH:HN')   WHERE MA_TB = 'T6';
UPDATE THONGBAO SET REGION_LABEL = CHAR_TO_LABEL('REGION_POLICY', 'LDK:TH,TK:HP') WHERE MA_TB = 'T7';
COMMIT;

-- ========== 6. CHUYỂN SANG READ_CONTROL ==========
BEGIN
    SA_POLICY_ADMIN.REMOVE_TABLE_POLICY('REGION_POLICY', 'ADMIN', 'THONGBAO');
END;
/

BEGIN
    SA_POLICY_ADMIN.APPLY_TABLE_POLICY(
        POLICY_NAME   => 'REGION_POLICY',
        SCHEMA_NAME   => 'ADMIN',
        TABLE_NAME    => 'THONGBAO',
        TABLE_OPTIONS => 'READ_CONTROL');
END;
/

-- Đảm bảo labels gắn đúng
UPDATE THONGBAO SET NOI_DUNG = NOI_DUNG;
COMMIT;

-- ========== 7. GÁN LABEL CHO 8 USER ==========
BEGIN
    SA_USER_ADMIN.SET_USER_LABELS('REGION_POLICY', 'NV0001', 'BGD:TH,TK,TM:HCM,HP,HN');   -- u1: Giám đốc
    SA_USER_ADMIN.SET_USER_LABELS('REGION_POLICY', 'NV0021', 'LDK:TM:HCM');                 -- u2: LĐ Khoa Tim mạch HCM
    SA_USER_ADMIN.SET_USER_LABELS('REGION_POLICY', 'NV0022', 'LDK:TK:HN');                  -- u3: LĐ Khoa Thần kinh HN
    SA_USER_ADMIN.SET_USER_LABELS('REGION_POLICY', 'NV0121', 'NV:TK:HCM');                  -- u4: NV Thần kinh HCM
    SA_USER_ADMIN.SET_USER_LABELS('REGION_POLICY', 'NV0122', 'NV:TM:HCM');                  -- u5: NV Tim mạch HCM
    SA_USER_ADMIN.SET_USER_LABELS('REGION_POLICY', 'NV0002', 'LDK:TM:HCM');                 -- u6: LĐ khoa Tim mạch HCM
    SA_USER_ADMIN.SET_USER_LABELS('REGION_POLICY', 'NV0003', 'LDK:TH,TK,TM:HCM,HP,HN');    -- u7: LĐ phòng, đọc tất cả
    SA_USER_ADMIN.SET_USER_LABELS('REGION_POLICY', 'NV0123', 'NV:TH:HN');                   -- u8: NV Tiêu hóa HN
    DBMS_OUTPUT.PUT_LINE('[OK] Đã gán nhãn OLS cho 8 user.');
END;
/

PROMPT ===================================================================
PROMPT   ✅ DEMO LITE SETUP HOÀN THÀNH!
PROMPT
PROMPT   Tài khoản test:
PROMPT     ATBM_ADMIN / Admin#12345  →  AdminMainForm (PH1)
PROMPT     NV0001     / 123          →  DieuPhoiVienForm
PROMPT     NV0050     / 123          →  BacSiForm
PROMPT     NV0121     / 123          →  KyThuatVienForm
PROMPT     BN000001   / 123          →  BenhNhanForm
PROMPT
PROMPT   Lưu ý: Phần PH1 cần chạy thêm Admin/00_bootstrap_demo.sql
PROMPT          + Admin/01_pkg_admin.sql nếu muốn demo AdminMainForm.
PROMPT ===================================================================
