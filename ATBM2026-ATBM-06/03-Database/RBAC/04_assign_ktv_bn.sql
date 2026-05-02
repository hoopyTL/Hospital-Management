-- =====================================================
-- 04_assign_ktv_bn.sql
-- Gán role cho các user Oracle:
--   RL_KYTHUATVIEN : NV0121 -> NV0170 (50 KTV)
--   RL_BENHNHAN    : BN000001 -> BN100000 (100.000 BN)
-- Chạy bằng tài khoản ADMIN (DBA)
-- =====================================================

SET SERVEROUTPUT ON;

DECLARE
    v_count NUMBER := 0;
BEGIN
    -- 50 Kỹ thuật viên
    FOR v_i IN 121..170 LOOP
        BEGIN
            EXECUTE IMMEDIATE 'GRANT RL_KYTHUATVIEN TO NV' || LPAD(v_i, 4, '0');
            v_count := v_count + 1;
        EXCEPTION
            WHEN OTHERS THEN
                DBMS_OUTPUT.PUT_LINE('Lỗi gán role cho NV' || LPAD(v_i, 4, '0') || ': ' || SQLERRM);
        END;
    END LOOP;
    DBMS_OUTPUT.PUT_LINE('Đã gán RL_KYTHUATVIEN cho ' || v_count || ' Kỹ thuật viên.');

    -- 100.000 Bệnh nhân
    v_count := 0;
    FOR v_i IN 1..100000 LOOP
        BEGIN
            EXECUTE IMMEDIATE 'GRANT RL_BENHNHAN TO BN' || LPAD(v_i, 6, '0');
            v_count := v_count + 1;

            IF MOD(v_count, 10000) = 0 THEN
                DBMS_OUTPUT.PUT_LINE('Tiến độ: Đã gán role cho ' || v_count || ' bệnh nhân...');
            END IF;
        EXCEPTION
            WHEN OTHERS THEN NULL;
        END;
    END LOOP;
    DBMS_OUTPUT.PUT_LINE('Đã gán RL_BENHNHAN cho ' || v_count || ' Bệnh nhân.');

    DBMS_OUTPUT.PUT_LINE('=== Hoàn thành gán role KTV + BN ===');
END;
/
