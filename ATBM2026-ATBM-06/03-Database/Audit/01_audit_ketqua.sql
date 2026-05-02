-- =====================================================
-- 01_audit_ketqua.sql
-- Ghi vết (audit) mọi thao tác UPDATE trên KET_QUA
-- TC#4: Các thao tác cập nhật trên trường KẾT QUẢ
--       đều được ghi vết.
-- Chạy bằng tài khoản ADMIN (DBA)
-- =====================================================

SET SERVEROUTPUT ON;

-- =====================================================
-- 1. Tạo bảng audit log
-- =====================================================
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

-- =====================================================
-- 2. Trigger ghi vết khi UPDATE cột KET_QUA trên HSBA_DV
-- =====================================================
CREATE OR REPLACE TRIGGER TRG_AUDIT_KETQUA
BEFORE UPDATE OF KET_QUA ON HSBA_DV
FOR EACH ROW
BEGIN
    INSERT INTO AUDIT_KETQUA (
        ma_hsba, loai_dv, ngay_dv,
        gia_tri_cu, gia_tri_moi,
        nguoi_cap_nhat, thoi_gian_cap_nhat
    ) VALUES (
        :OLD.MA_HSBA, :OLD.LOAI_DV, :OLD.NGAY_DV,
        :OLD.KET_QUA, :NEW.KET_QUA,
        SYS_CONTEXT('USERENV', 'SESSION_USER'),
        SYSTIMESTAMP
    );
END;
/

-- =====================================================
-- 3. Cấp quyền SELECT cho KTV để xem lịch sử của mình
-- =====================================================
GRANT SELECT ON ADMIN.AUDIT_KETQUA TO RL_KYTHUATVIEN;

BEGIN
    DBMS_OUTPUT.PUT_LINE('Đã tạo bảng AUDIT_KETQUA và trigger TRG_AUDIT_KETQUA.');
    DBMS_OUTPUT.PUT_LINE('=== Hoàn thành cài đặt Audit cho KET_QUA ===');
END;
/
