-- =====================================================
-- 01_fn_get_role.sql
-- Helper function xác định vai trò của user đang đăng nhập.
-- Dùng trong các VPD policy function để tránh đệ quy
-- (không query trực tiếp bảng NHAN_VIEN vốn đã có VPD).
-- Chạy bằng tài khoản ADMIN (DBA)
-- =====================================================

SET SERVEROUTPUT ON;

CREATE OR REPLACE FUNCTION admin.fn_get_role RETURN VARCHAR2
    DETERMINISTIC
AS
    v_user VARCHAR2(30) := SYS_CONTEXT('USERENV', 'SESSION_USER');
    v_num  NUMBER;
BEGIN
    IF v_user = 'ADMIN' THEN
        RETURN 'DBA';
    END IF;

    -- Bệnh nhân: mã dạng BNxxxxxx
    IF v_user LIKE 'BN%' THEN
        RETURN 'BENHNHAN';
    END IF;

    -- Nhân viên: mã NVxxxx
    --   NV0001-NV0020  : Điều phối viên
    --   NV0021-NV0120  : Bác sĩ/Y sĩ
    --   NV0121-NV0170  : Kỹ thuật viên
    IF v_user LIKE 'NV%' THEN
        BEGIN
            v_num := TO_NUMBER(SUBSTR(v_user, 3));
            IF v_num BETWEEN 1 AND 20 THEN
                RETURN 'DIEUPHOIVIEN';
            ELSIF v_num BETWEEN 21 AND 120 THEN
                RETURN 'BACSI';
            ELSIF v_num BETWEEN 121 AND 170 THEN
                RETURN 'KYTHUATVIEN';
            END IF;
        EXCEPTION
            WHEN OTHERS THEN NULL;
        END;
    END IF;

    RETURN 'UNKNOWN';
END;
/

BEGIN
    DBMS_OUTPUT.PUT_LINE('Đã tạo function FN_GET_ROLE.');
END;
/
