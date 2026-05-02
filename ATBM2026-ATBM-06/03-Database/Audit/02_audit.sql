-- Yêu cầu 3:
-- 1. Kích hoạt kiểm toán
-- Đăng nhập bằng quyền SYSDBA
ALTER SYSTEM SET audit_trail=db,extended SCOPE=SPFILE;

-- 2. Thực hiện kiểm toán dùng Standart audit
-- Ngữ cảnh 1: Theo dõi hành vi SELECT trên bảng hsba của user NV0021 (cả thành công và thất bại), ghi nhận theo từng lần truy cập.
AUDIT SELECT ON admin.hsba BY NV0021 BY ACCESS;

-- Ngữ cảnh 2: Theo dõi hành vi UPDATE thất bại trên bảng don_thuoc của bất kỳ ai (phát hiện cố gắng sửa dữ liệu không có quyền).
AUDIT UPDATE ON admin.don_thuoc BY ACCESS WHENEVER NOT SUCCESSFUL;

-- Ngữ cảnh 3: Theo dõi hành vi DELETE thành công trên bảng hsba_dv (theo dõi ai đã thực sự xóa dịch vụ).
AUDIT DELETE ON admin.hsba_dv BY ACCESS WHENEVER SUCCESSFUL;

-- Ngữ cảnh 4: Theo dõi việc thực thi một Stored Procedure cụ thể (ví dụ SP_XOA_BENHAN) của user NV_01.
AUDIT EXECUTE ON admin.SP_XOA_BENHAN BY NV_01 BY ACCESS;

-- Ngữ cảnh 5: Theo dõi tất cả các hành vi trên một View (ví dụ V_THONGTIN_BENHNHAN) để kiểm soát quyền riêng tư.
AUDIT ALL ON admin.V_THONGTIN_BENHNHAN BY ACCESS;

-- 3a. Fine-Grained Audit Policy trên don_thuoc
BEGIN
  DBMS_FGA.ADD_POLICY(
   object_schema      => 'admin',
   object_name        => 'don_thuoc',
   policy_name        => 'FGA_AUDIT_UPDATE_DONTHUOC',
   audit_column       => 'ma_hsba,ngay_dt,ten_thuoc,lieu_dung',
   audit_condition    => NULL,
   statement_types    => 'UPDATE',
   audit_trail        => DBMS_FGA.DB + DBMS_FGA.EXTENDED,
   enable             => TRUE
  );
END;
/

-- 3b. Unified Audit Policy (Oracle 12c+)
CREATE AUDIT POLICY AUDIT_HSBA_UPDATE_CHANDOAN
  ACTIONS UPDATE ON admin.HSBA
  BY "BAC_SI";

AUDIT POLICY AUDIT_HSBA_UPDATE_CHANDOAN WHENEVER SUCCESSFUL;

-- 3c. Fine-Grained Audit Policy trên hsba - kiểm tra unauthorized update
BEGIN
  DBMS_FGA.ADD_POLICY(
   object_schema      => 'admin',
   object_name        => 'hsba',
   policy_name        => 'FGA_AUDIT_ILLEGAL_UPDATE_HSBA',
   audit_column       => 'chuan_doan,dieu_tri,ket_luan',
   audit_condition    => 'ma_bs != SYS_CONTEXT(''USERENV'',''SESSION_USER'')',
   statement_types    => 'UPDATE',
   audit_trail        => DBMS_FGA.DB + DBMS_FGA.EXTENDED,
   enable             => TRUE
  );
END;
/

-- 3d. Fine-Grained Audit Policy trên hsba_dv - kiểm tra DML ngoài giờ hành chính
BEGIN
  DBMS_FGA.ADD_POLICY(
   object_schema      => 'admin',
   object_name        => 'hsba_dv',
   policy_name        => 'FGA_AUDIT_ILLEGAL_DML_HSBA_DV',
   audit_condition    => 'SYS_CONTEXT(''USERENV'',''SESSION_USER'') != ''admin'' OR TO_CHAR(SYSDATE,''HH24'') NOT BETWEEN ''06'' AND ''18''',
   statement_types    => 'INSERT,UPDATE,DELETE',
   audit_trail        => DBMS_FGA.DB + DBMS_FGA.EXTENDED,
   enable             => TRUE
  );
END;
/

-- 4.
SELECT OS_USERNAME, USERNAME, OBJ_NAME, ACTION_NAME, TIMESTAMP, RETURNCODE
FROM DBA_AUDIT_TRAIL
WHERE OWNER = 'admin'
ORDER BY TIMESTAMP DESC;

SELECT DB_USER, OBJECT_NAME, POLICY_NAME, STATEMENT_TYPE, SQL_TEXT, TIMESTAMP 
FROM DBA_FGA_AUDIT_TRAIL
WHERE OBJECT_SCHEMA = 'admin'
ORDER BY TIMESTAMP DESC;

SELECT DBUSERNAME, ACTION_NAME, OBJECT_NAME, SQL_TEXT, EVENT_TIMESTAMP 
FROM UNIFIED_AUDIT_TRAIL 
WHERE UNIFIED_AUDIT_POLICIES LIKE '%AUDIT_HSBA_UPDATE_CHANDOAN%'
ORDER BY EVENT_TIMESTAMP DESC;