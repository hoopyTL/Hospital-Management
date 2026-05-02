-- =====================================================
-- 05_assign_dpv_bs.sql
-- Gán role cho các user Oracle:
--   RL_DIEUPHOIVIEN : NV0001 -> NV0020 (20 DPV)
--   RL_BACSI        : NV0021 -> NV0120 (100 BS/Y sĩ)
-- Chạy bằng tài khoản ADMIN (DBA)
-- =====================================================

SET SERVEROUTPUT ON;

DECLARE
    v_count NUMBER := 0;
BEGIN
    -- 20 Điều phối viên
    FOR v_i IN 1..20 LOOP
        BEGIN
            EXECUTE IMMEDIATE 'GRANT RL_DIEUPHOIVIEN TO NV' || LPAD(v_i, 4, '0');
            v_count := v_count + 1;
        EXCEPTION WHEN OTHERS THEN
            DBMS_OUTPUT.PUT_LINE('Lỗi gán role cho NV' || LPAD(v_i, 4, '0') || ': ' || SQLERRM);
        END;
    END LOOP;
    DBMS_OUTPUT.PUT_LINE('Đã gán RL_DIEUPHOIVIEN cho ' || v_count || ' Điều phối viên.');

    -- 100 Bác sĩ / Y sĩ
    v_count := 0;
    FOR v_i IN 21..120 LOOP
        BEGIN
            EXECUTE IMMEDIATE 'GRANT RL_BACSI TO NV' || LPAD(v_i, 4, '0');
            v_count := v_count + 1;
        EXCEPTION WHEN OTHERS THEN
            DBMS_OUTPUT.PUT_LINE('Lỗi gán role cho NV' || LPAD(v_i, 4, '0') || ': ' || SQLERRM);
        END;
    END LOOP;
    DBMS_OUTPUT.PUT_LINE('Đã gán RL_BACSI cho ' || v_count || ' Bác sĩ / Y sĩ.');

    DBMS_OUTPUT.PUT_LINE('=== Hoàn thành gán role DPV + BS ===');
END;
/
