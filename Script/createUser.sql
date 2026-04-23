-- Login bằng tài khoản ADMIN
-- TK: Admin
-- Mật khẩu: 12345

-- Chạy file này sau khi đã chạy các file initDB, insertData.
SET SERVEROUTPUT ON;
DECLARE
    v_count NUMBER := 0;
BEGIN
    -- 1. Tạo tài khoản cho NHÂN VIÊN
    FOR r IN (SELECT ma_nv FROM nhan_vien) LOOP
        BEGIN
            EXECUTE IMMEDIATE 'CREATE USER ' || r.ma_nv || ' IDENTIFIED BY 123';
            EXECUTE IMMEDIATE 'GRANT CONNECT TO ' || r.ma_nv;
            v_count := v_count + 1;
        EXCEPTION WHEN OTHERS THEN NULL; END;
    END LOOP;
    DBMS_OUTPUT.PUT_LINE('Đã tạo xong tài khoản cho Nhân viên.');

    -- 2. Tạo tài khoản cho BỆNH NHÂN (100.000 người)
    FOR r IN (SELECT ma_bn FROM benh_nhan) LOOP
        BEGIN
            EXECUTE IMMEDIATE 'CREATE USER ' || r.ma_bn || ' IDENTIFIED BY 123';
            EXECUTE IMMEDIATE 'GRANT CONNECT TO ' || r.ma_bn;
            v_count := v_count + 1;
            
            -- Cứ 5000 user thì in ra tiến độ để bạn đỡ sốt ruột
            IF MOD(v_count, 5000) = 0 THEN
                DBMS_OUTPUT.PUT_LINE('Tiến độ: Đã tạo được ' || v_count || ' người dùng...');
            END IF;
        EXCEPTION WHEN OTHERS THEN NULL; END;
    END LOOP;
    
    DBMS_OUTPUT.PUT_LINE('TỔNG CỘNG: Đã tạo thành công ' || v_count || ' tài khoản.');
END;
/