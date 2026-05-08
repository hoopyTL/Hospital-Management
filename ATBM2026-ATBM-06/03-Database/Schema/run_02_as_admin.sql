-- =====================================================
-- BƯỚC 2: Chạy file này bằng tài khoản ADMIN
-- TK: ADMIN / MK: 12345 / PDB: XEPDB1
--
-- File này orchestrate toàn bộ cài đặt theo thứ tự:
--   1. Insert data mẫu
--   2. Tạo user Oracle (100K+)
--   3. RBAC : create roles / grants / assign
--   4. VPD  : fn_get_role / policy cơ bản / policy mở rộng
--   5. Audit: trigger ghi vết KET_QUA
--
-- ⚠️ Mất ~15–30 phút vì tạo 100K user + assign role.
-- Khi chạy trên SQL Developer, dùng F5 (Run Script).
-- =====================================================

SET SERVEROUTPUT ON SIZE UNLIMITED;
SET TIMING ON;

PROMPT ==========================================
PROMPT BƯỚC 1/14: Insert dữ liệu mẫu...
PROMPT ==========================================
@@insertData.sql

PROMPT ==========================================
PROMPT BƯỚC 2/14: Tạo 100K+ user Oracle... (LÂU)
PROMPT ==========================================
@@createUser.sql

PROMPT ==========================================
PROMPT BƯỚC 3/14: RBAC - Create Roles
PROMPT ==========================================
@@../RBAC/01_create_roles.sql

PROMPT ==========================================
PROMPT BƯỚC 4/14: RBAC - Grant KTV + BN
PROMPT ==========================================
@@../RBAC/02_grant_ktv_bn.sql

PROMPT ==========================================
PROMPT BƯỚC 5/14: RBAC - Grant DPV + BS (+ MV)
PROMPT ==========================================
@@../RBAC/03_grant_dpv_bs.sql

PROMPT ==========================================
PROMPT BƯỚC 6/14: RBAC - Assign KTV + BN (LÂU)
PROMPT ==========================================
@@../RBAC/04_assign_ktv_bn.sql

PROMPT ==========================================
PROMPT BƯỚC 7/14: RBAC - Assign DPV + BS
PROMPT ==========================================
@@../RBAC/05_assign_dpv_bs.sql

PROMPT ==========================================
PROMPT BƯỚC 8/14: VPD - fn_get_role
PROMPT ==========================================
@@../VPD/01_fn_get_role.sql

PROMPT ==========================================
PROMPT BƯỚC 9/14: VPD - Policies cơ bản
PROMPT ==========================================
@@../VPD/02_vpd_basic.sql

PROMPT ==========================================
PROMPT BƯỚC 10/14: VPD - Policies mở rộng (DPV, BS)
PROMPT ==========================================
@@../VPD/03_vpd_dpv_bs.sql

PROMPT ==========================================
PROMPT BƯỚC 11/14: Audit - Trigger KET_QUA
PROMPT ==========================================
@@../Audit/01_audit_ketqua.sql

PROMPT ==========================================
PROMPT BƯỚC 12/14: OLS - Tạo bảng THONGBAO và Policy
PROMPT ==========================================
@@../OLS/02_admin_table_policy.sql

PROMPT ==========================================
PROMPT Đang Reconnect lại ADMIN để Role OLS có hiệu lực...
PROMPT ==========================================
-- Lưu ý: Chỉnh sửa lại chuỗi kết nối này nếu môi trường của bạn khác localhost/XEPDB1
CONNECT ADMIN/12345@localhost:1521/XEPDB1;

PROMPT ==========================================
PROMPT BƯỚC 13/14: OLS - Tạo Components (Levels, Compartments, Groups)
PROMPT ==========================================
@@../OLS/03_admin_policy_components.sql

PROMPT ==========================================
PROMPT BƯỚC 14/14: OLS - Apply Policy, Insert Data và Gán nhãn cho User
PROMPT ==========================================
@@../OLS/04_admin_data_apply.sql

PROMPT ==========================================
PROMPT  HOÀN THÀNH TẤT CẢ!
PROMPT Tài khoản test:
PROMPT   - DPV : NV0001   / 123  -> DieuPhoiVienForm
PROMPT   - BS  : NV0050   / 123  -> BacSiForm
PROMPT   - KTV : NV0121   / 123  -> KyThuatVienForm
PROMPT   - BN  : BN000001 / 123  -> BenhNhanForm
PROMPT   - OLS Test : NV0001 -> NV0123 / 123 -> Test bảng ADMIN.THONGBAO
PROMPT ==========================================

SET TIMING OFF;
