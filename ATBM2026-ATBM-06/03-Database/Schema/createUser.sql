-- Login bằng tài khoản ADMIN
-- TK: Admin
-- Mật khẩu: 12345

-- Chạy file này sau khi đã chạy các file initDB, insertData.
   SET SERVEROUTPUT ON;
declare
   v_count number := 0;
begin
    -- 1. Tạo tài khoản cho NHÂN VIÊN
   for r in (
      select ma_nv
        from nhan_vien
   ) loop
      begin
         execute immediate 'CREATE USER '
                           || r.ma_nv
                           || ' IDENTIFIED BY 123';
         execute immediate 'GRANT CONNECT TO ' || r.ma_nv;
         v_count := v_count + 1;
      exception
         when others then
            null;
      end;
   end loop;
   dbms_output.put_line('Đã tạo xong tài khoản cho Nhân viên.');

    -- -- 2. Tạo tài khoản cho BỆNH NHÂN (100.000 người)
   for r in (
      select ma_bn
        from benh_nhan
   ) loop
      begin
         execute immediate 'CREATE USER '
                           || r.ma_bn
                           || ' IDENTIFIED BY 123';
         execute immediate 'GRANT CONNECT TO ' || r.ma_bn;
         v_count := v_count + 1;
            
            -- Cứ 5000 user thì in ra tiến độ để bạn đỡ sốt ruột
         if mod(
            v_count,
            5000
         ) = 0 then
            dbms_output.put_line('Tiến độ: Đã tạo được '
                                 || v_count || ' người dùng...');
         end if;
      exception
         when others then
            null;
      end;
   end loop;


    
    -- 2. Tạo tài khoản cho BỆNH NHÂN (chỉ 200 người)
        -- FOR r IN (
        --     SELECT ma_bn 
        --     FROM benh_nhan 
        --     WHERE ROWNUM <= 200
        -- ) LOOP
        --     BEGIN
        --         EXECUTE IMMEDIATE 'CREATE USER ' || r.ma_bn || ' IDENTIFIED BY 123';
        --         EXECUTE IMMEDIATE 'GRANT CONNECT TO ' || r.ma_bn;
        --         v_count := v_count + 1;
                
        --         IF MOD(v_count, 50) = 0 THEN
        --             DBMS_OUTPUT.PUT_LINE('Tiến độ: Đã tạo được ' || v_count || ' người dùng...');
        --         END IF;
        --     EXCEPTION 
        --         WHEN OTHERS THEN NULL; 
        --     END;
        -- END LOOP;

   dbms_output.put_line('TỔNG CỘNG: Đã tạo thành công '
                        || v_count || ' tài khoản.');
end;
/