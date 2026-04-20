-- =====================================================
-- 02_grant_ktv_bn.sql
-- Cấp quyền object/column-level cho 2 role:
--   RL_KYTHUATVIEN  (TC#4 + TC#5)
--   RL_BENHNHAN     (TC#5)
-- Chạy bằng tài khoản ADMIN (DBA)
-- =====================================================

SET SERVEROUTPUT ON;

-- =====================================================
-- ROLE: RL_KYTHUATVIEN
-- TC#4: KTV thực hiện dịch vụ, ghi kết quả HSBA_DV
--       Chỉ xem dòng mình được phân công (VPD sẽ xử lý)
--       Chỉ cập nhật trường KET_QUA
-- TC#5: Xem + sửa thông tin cá nhân trên NHAN_VIEN
-- =====================================================

GRANT SELECT ON ADMIN.HSBA_DV TO RL_KYTHUATVIEN;
GRANT UPDATE (KET_QUA) ON ADMIN.HSBA_DV TO RL_KYTHUATVIEN;

GRANT SELECT ON ADMIN.NHAN_VIEN TO RL_KYTHUATVIEN;
GRANT UPDATE (QUE_QUAN, SDT) ON ADMIN.NHAN_VIEN TO RL_KYTHUATVIEN;

-- =====================================================
-- ROLE: RL_BENHNHAN
-- TC#5: Xem + sửa thông tin cá nhân trên BENH_NHAN
-- =====================================================

GRANT SELECT ON ADMIN.BENH_NHAN TO RL_BENHNHAN;

GRANT UPDATE (SO_NHA, TEN_DUONG, QUAN_HUYEN, TINH_TP,
              TIEN_SU_BENH, TIEN_SU_BENH_GD, DI_UNG_THUOC)
    ON ADMIN.BENH_NHAN TO RL_BENHNHAN;

BEGIN
    DBMS_OUTPUT.PUT_LINE('Đã cấp quyền cho RL_KYTHUATVIEN.');
    DBMS_OUTPUT.PUT_LINE('Đã cấp quyền cho RL_BENHNHAN.');
    DBMS_OUTPUT.PUT_LINE('=== Hoàn thành cấp quyền KTV + BN ===');
END;
/
