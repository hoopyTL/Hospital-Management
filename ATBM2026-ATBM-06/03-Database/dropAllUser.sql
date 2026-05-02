-- Login bằng tài khoản ADMIN
-- TK: Admin
-- Mật khẩu: 12345

SET SERVEROUTPUT ON;

DECLARE
    -- Thay 'S_ADMIN' bằng tên chính xác tài khoản Admin của bạn (viết hoa)
    v_admin_name VARCHAR2(30) := 'ADMIN'; 
BEGIN
    -- Lặp qua danh sách user có tiền tố BN hoặc NV
    FOR r IN (
        SELECT username 
        FROM dba_users 
        WHERE (username LIKE 'BN%' OR username LIKE 'NV%')
          AND username <> v_admin_name
          AND oracle_maintained = 'N' -- Chỉ chọn user do người dùng tạo
    ) LOOP
        BEGIN
            -- Sử dụng CASCADE để xóa sạch schema và các đối tượng của user đó
            EXECUTE IMMEDIATE 'DROP USER "' || r.username || '" CASCADE';
            
        EXCEPTION
            WHEN OTHERS THEN
                DBMS_OUTPUT.PUT_LINE('Không thể xóa user ' || r.username || ': ' || SQLERRM);
        END;
    END LOOP;
    
    DBMS_OUTPUT.PUT_LINE('Đã hoàn thành việc dọn dẹp toàn bộ tài khoản người dùng.');
END;
/