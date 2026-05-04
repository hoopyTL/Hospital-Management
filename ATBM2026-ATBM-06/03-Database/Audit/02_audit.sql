-- ============================================================================
-- SCRIPT 02: CÀI ĐẶT CẤU HÌNH AUDIT
-- Chạy bằng quyền ADMIN (Hoặc SYS)
-- ============================================================================

SET SERVEROUTPUT ON;

-- 1. Kích hoạt kiểm toán (Chạy bằng sysdba trên root container nếu hệ thống chưa bật)
ALTER SYSTEM SET audit_trail=db,extended SCOPE=SPFILE;
SHUTDOWN IMMEDIATE;
STARTUP;

-- ==========================================
-- 2. THỰC HIỆN KIỂM TOÁN DÙNG STANDARD AUDIT
-- ==========================================

-- Ngữ cảnh 1: Theo dõi hành vi SELECT trên bảng hsba (cả thành công và thất bại)
AUDIT SELECT ON admin.hsba BY ACCESS;

-- Ngữ cảnh 2: Theo dõi hành vi UPDATE thất bại trên bảng don_thuoc
AUDIT UPDATE ON admin.don_thuoc BY ACCESS WHENEVER NOT SUCCESSFUL;

-- Ngữ cảnh 3: Theo dõi hành vi DELETE thành công trên bảng hsba_dv
AUDIT DELETE ON admin.hsba_dv BY ACCESS WHENEVER SUCCESSFUL;

-- Ngữ cảnh 4: Theo dõi việc thực thi các Stored Procedure
AUDIT EXECUTE ON admin.sp_log_audit BY ACCESS;

-- Ngữ cảnh 5: Theo dõi hành vi INSERT thành công trên bảng benhNhan
AUDIT INSERT ON admin.benh_nhan BY ACCESS WHENEVER SUCCESSFUL;

-- ==========================================
-- 3. THỰC HIỆN KIỂM TOÁN DÙNG FGA (FINE-GRAINED AUDIT)
-- ==========================================

-- 3a. Fine-Grained Audit Policy trên don_thuoc
BEGIN
  -- Xóa policy cũ nếu tồn tại để tránh lỗi ORA-28101
  BEGIN
    DBMS_FGA.DROP_POLICY(object_schema => 'admin', object_name => 'don_thuoc', policy_name => 'FGA_AUDIT_UPDATE_DONTHUOC');
  EXCEPTION WHEN OTHERS THEN NULL;
  END;

  -- Tạo policy mới
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
  DBMS_OUTPUT.PUT_LINE('Created FGA policy: FGA_AUDIT_UPDATE_DONTHUOC');
END;
/

-- 3b. Fine-Grained Audit Policy trên hsba - kiểm tra unauthorized update
BEGIN
  BEGIN
    DBMS_FGA.DROP_POLICY(object_schema => 'admin', object_name => 'hsba', policy_name => 'FGA_AUDIT_ILLEGAL_UPDATE_HSBA');
  EXCEPTION WHEN OTHERS THEN NULL;
  END;

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
  DBMS_OUTPUT.PUT_LINE('Created FGA policy: FGA_AUDIT_ILLEGAL_UPDATE_HSBA');
END;
/

-- 3c. Fine-Grained Audit Policy trên hsba_dv - kiểm tra DML ngoài giờ hành chính
BEGIN
  BEGIN
    DBMS_FGA.DROP_POLICY(object_schema => 'admin', object_name => 'hsba_dv', policy_name => 'FGA_AUDIT_ILLEGAL_DML_HSBA_DV');
  EXCEPTION WHEN OTHERS THEN NULL;
  END;

  DBMS_FGA.ADD_POLICY(
   object_schema      => 'admin',
   object_name        => 'hsba_dv',
   policy_name        => 'FGA_AUDIT_ILLEGAL_DML_HSBA_DV',
   audit_condition    => 'SYS_CONTEXT(''USERENV'',''SESSION_USER'') != ''admin'' OR TO_CHAR(SYSDATE,''HH24'') NOT BETWEEN ''06'' AND ''18''',
   statement_types    => 'INSERT,UPDATE,DELETE',
   audit_trail        => DBMS_FGA.DB + DBMS_FGA.EXTENDED,
   enable             => TRUE
  );
  DBMS_OUTPUT.PUT_LINE('Created FGA policy: FGA_AUDIT_ILLEGAL_DML_HSBA_DV');
END;
/