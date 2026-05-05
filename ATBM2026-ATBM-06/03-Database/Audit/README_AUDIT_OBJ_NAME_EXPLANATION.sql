-- ============================================================================
-- GIẢI THÍCH: Tại sao obj_name có thể khác với bảng thực tế bị lỗi?
-- ============================================================================

-- RỐI LOẠN CHÍNH:
-- ============================================================================
-- Khi sử dụng Standard Audit trong Oracle, cột obj_name trong DBA_AUDIT_TRAIL
-- ghi lại bảng được định nghĩa trong AUDIT clause, KHÔNG phải bảng thực tế
-- gây ra lỗi. Đây là thiết kế của Oracle.
--
-- Ví dụ:
-- 1. Ta định nghĩa: AUDIT SELECT ON admin.hsba BY ACCESS;
-- 2. User thực thi: SELECT * FROM benh_nhan WHERE ma_bn IN (SELECT ma_bn FROM hsba);
-- 3. Nếu có lỗi xảy ra, DBA_AUDIT_TRAIL.obj_name = 'HSBA'
--    Nhưng lỗi thực tế có thể từ BENH_NHAN hoặc ở điều kiện WHERE!

-- GIẢI PHÁP:
-- ============================================================================
-- 1. Luôn kiểm tra cột SQL_TEXT để biết câu lệnh thực tế được thực thi
-- 2. Kiểm tra returncode để xác định lỗi
-- 3. Sử dụng error_message nếu có thông tin chi tiết
--
-- SQL_TEXT sẽ cho thấy:
-- - Bảng nào thực sự được truy cập
-- - Điều kiện WHERE ra sao
-- - JOIN với bảng nào
-- - Lỗi từ đâu có thể là do ràng buộc, quyền truy cập, v.v...

-- EXAMPLE:
-- ============================================================================

-- Giả sử có audit record như vậy:
-- obj_name    = 'HSBA'
-- action_name = 'SELECT'
-- returncode  = -2291 (Child record found)
-- sql_text    = 'DELETE FROM benh_nhan WHERE ma_bn = :b1'
--
-- Phân tích:
-- - obj_name='HSBA' là bảng được audit
-- - Nhưng sql_text='DELETE FROM benh_nhan' => lỗi từ BENH_NHAN
-- - returncode=-2291 => Có child record tồn tại (Foreign Key violation)
-- - Lỗi xảy ra vì BENH_NHAN có FK tham chiếu đến HSBA
--
-- Kết luận: Không nên tin cậy obj_name một mình. Phải xem SQL_TEXT!

-- CỘT QUAN TRỌNG TRONG DBA_AUDIT_TRAIL:
-- ============================================================================
-- sessionid      - Session ID của user
-- username       - Tên user thực hiện hành động
-- owner          - Schema chủ sở hữu object
-- obj_name       - TÊN BẢNG TRONG AUDIT CLAUSE (không phải bảng bị lỗi)
-- action_name    - Loại hành động (SELECT, UPDATE, DELETE, INSERT, EXECUTE)
-- returncode     - Mã lỗi Oracle (0 = thành công, khác 0 = thất bại)
-- timestamp      - Thời gian thực hiện
-- sql_text       - ĐÂY LÀ CẦU LỆNH THỰC TẾ (LƯU Ý!)
-- priv_used      - Quyền được sử dụng
--
-- NHỮNG TRƯỜNG HỢP CẦU LỆNH KHÁC VỚI obj_name:
-- ============================================================================
-- 1. JOIN với nhiều bảng
--    AUDIT SELECT ON admin.hsba BY ACCESS;
--    SELECT h.*, b.ten_bn FROM hsba h JOIN benh_nhan b ON h.ma_bn = b.ma_bn;
--    => obj_name = 'HSBA', nhưng sql_text sẽ hiển thị JOIN

-- 2. Stored Procedure gọi nhiều bảng
--    AUDIT EXECUTE ON admin.sp_process BY ACCESS;
--    CALL sp_process(); -- procedure này truy cập nhiều bảng
--    => obj_name = 'SP_PROCESS', nhưng sql_text = 'BEGIN sp_process(); END;'
--    => Phải kiểm tra code của procedure

-- 3. Trigger gây lỗi
--    AUDIT UPDATE ON admin.hsba BY ACCESS;
--    UPDATE hsba SET ... ;
--    -- Trigger trên bảng khác gây lỗi
--    => obj_name = 'HSBA', nhưng lỗi từ trigger trên bảng khác

-- 4. Foreign Key Constraint
--    AUDIT DELETE ON admin.benh_nhan BY ACCESS;
--    DELETE FROM benh_nhan WHERE ma_bn = 'BN001';
--    -- HSBA còn có FK tham chiếu BN001
--    => obj_name = 'BENH_NHAN', returncode = -2292 (Parent key not found)

-- CÁCH PHÂN TÍCH LỖI ĐÚNG ĐẮN:
-- ============================================================================
SELECT 
    sessionid,
    username,
    obj_name              AS "Bảng AUDIT",
    action_name,
    returncode,
    CASE returncode
        WHEN 0 THEN 'SUCCESS'
        WHEN -1 THEN 'NOT EXECUTED'
        WHEN -2 THEN 'PARSE ERROR'
        WHEN -904 THEN 'OBJECT NOT EXIST'
        WHEN -1017 THEN 'LOGIN DENIED'
        WHEN -1031 THEN 'INSUFFICIENT PRIVILEGE'
        WHEN -2291 THEN 'CHILD RECORD FOUND (FK)'
        WHEN -2292 THEN 'PARENT KEY NOT FOUND'
        WHEN -1400 THEN 'CANNOT INSERT NULL'
        WHEN -6550 THEN 'PLSQL COMPILATION ERROR'
        ELSE 'ERROR CODE: ' || returncode
    END AS "Nguyên Nhân Lỗi",
    sql_text              AS "Câu Lệnh Thực Tế",
    TO_CHAR(timestamp, 'YYYY-MM-DD HH24:MI:SS') AS timestamp
FROM dba_audit_trail
WHERE returncode != 0  -- Chỉ lỗi
ORDER BY timestamp DESC
FETCH FIRST 20 ROWS ONLY;

-- KẾT LUẬN:
-- ============================================================================
-- Luôn kiểm tra SQL_TEXT để hiểu rõ lỗi xảy ra ở đâu!
-- obj_name chỉ cho biết bảng nào được audit, không phải bảng bị lỗi.
