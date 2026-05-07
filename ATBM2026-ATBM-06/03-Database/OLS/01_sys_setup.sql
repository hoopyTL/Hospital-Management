--ĐĂNG NHẬP SYS VỚI XEPDB1 CẤP QUYỀN 

-- Quyền OLS 
-- Cấp quyền tạo/quản lý các thành phần của nhãn (Levels, Compartments, Groups) và cho phép ADMIN ủy quyền lại.
GRANT EXECUTE ON LBACSYS.SA_COMPONENTS TO ADMIN WITH GRANT OPTION;

-- Cấp quyền gán nhãn/đặc quyền OLS cho user khác và cho phép ADMIN ủy quyền lại.
GRANT EXECUTE ON LBACSYS.SA_USER_ADMIN TO ADMIN WITH GRANT OPTION;

-- Cấp quyền áp dụng/gỡ bỏ Policy trên các bảng/schema và cho phép ADMIN ủy quyền lại.
GRANT EXECUTE ON SA_POLICY_ADMIN TO ADMIN WITH GRANT OPTION;

-- Cấp Role quản trị viên OLS (bao gồm sẵn quyền tạo Policy, tạo Label,...).
GRANT LBAC_DBA TO ADMIN;