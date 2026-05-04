# HƯỚNG DẪN DEMO TỔNG HỢP (PHÂN HỆ 1 & 2)

Tài liệu này hướng dẫn các bước thiết lập nhanh (Demo Lite) và kịch bản demo trên C# WinForms cho cả hai phân hệ của ứng dụng Quản lý Bệnh viện.

## Môi trường & Chuẩn bị
- **Cơ sở dữ liệu:** Oracle 21c/19c (PDB: `XEPDB1`, hoặc tùy chỉnh trong App.config).
- **Trạng thái Code:** Project đã được tích hợp combo box Phái, package `PKG_ADMIN` mới nhất, và `ThongBaoKhanForm` (cho OLS).
- Chạy ứng dụng từ Visual Studio (hoặc thư mục Build `02-Exe`).

---

## PHẦN A: SET UP DATABASE (CHỈ ~5 PHÚT)

### 1. Phân hệ 2 - Quản lý Bệnh viện & Bảo mật chuyên sâu (VPD, Audit, OLS)
Dùng **SQL Developer**, ưu tiên chạy bằng phím **F5 (Run Script)**:

1. **Đăng nhập `SYS AS SYSDBA` (vào CDB/PDB XEPDB1)**
   - Mở file: `03-Database/demo_lite_01_sysdba.sql`
   - Nhấn **F5**.
   - *Kết quả:* Chuyển session về PDB, tạo schema `ADMIN`, sinh 5 cấu trúc bảng cơ sở, tạo 14 user Oracle (DPV, BS, KTV, BN), và cấp quyền thực thi OLS cho ADMIN.

2. **Đăng nhập `ADMIN / 12345` (vào PDB XEPDB1)**
   - Mở file: `03-Database/demo_lite_02_admin.sql`
   - Nhấn **F5**.
   - *Kết quả:* Chèn dữ liệu mẫu (nhỏ gọn), tạo Roles, Views, cấp quyền RBAC, cài đặt VPD Policy chống rò rỉ (return NULL update patch), tạo trigger kiểm toán và apply chuẩn FGA/Unified. Bảng `THONGBAO` cũng được tạo.

3. **CẢNH BÁO BẮT BUỘC: Disconnect kết nối ADMIN & Connect lại**
   - Đóng kết nối `ADMIN` trong SQL Developer, sau đó mở lại kết nối `ADMIN`. *(Điều này để DB cấp lại context cho Oracle Label Security).*
   - Mở file: `03-Database/demo_lite_03_ols.sql`
   - Nhấn **F5**.
   - *Kết quả:* Chạy 1 phút hoàn tất OLS: tạo Level, Compartment, Group, sinh 7 Labels, áp nhãn cho dữ liệu mẫu và gán nhãn cho 8 User.

### 2. Phân hệ 1 - Hệ thống Quản trị Oracle (Automation Tools)
Tiếp tục bước Setup cho chức năng tab Admin trên App:

1. **Đăng nhập `SYS AS SYSDBA`**
   - Mở file: `03-Database/Admin/00_bootstrap_demo.sql` (F5)
   - *Kết quả:* Khởi tạo user `ATBM_ADMIN` (có DBA) để C# thao tác quản trị tài khoản chung.

2. **Đăng nhập `ATBM_ADMIN / Admin#12345`**
   - Mở file: `03-Database/Admin/01_pkg_admin.sql` (F5)
   - *Kết quả:* Compile cấu trúc bảng `APP_VPD_COL_GRANTS` (hỗ trợ phân quyền cấp cột trên app) và package gốc `PKG_ADMIN` chứa mọi procedures.

*(Setup Database Hoàn Thành!)*

---

## PHẦN B: KỊCH BẢN DEMO TRÊN ỨNG DỤNG WINFORMS

### MÀN 1: QUẢN TRỊ TÀI KHOẢN ORACLE (Phân hệ 1)
> *Chứng minh năng lực tương tác tự động hóa với hệ thống Oracle từ layer ứng dụng.*

1. **Đăng nhập App** bằng quyền: `ATBM_ADMIN` / Pass: `Admin#12345`.
2. Màn hình `AdminMainForm` hiển thị.
3. **Tab Users / Roles**:
   - Thử tính năng **Thêm User**: Điền thông tin tạo 1 user `TEST_USER` (VD: Pass 123, Tablespace USERS).
   - Thử tính năng **Thêm Role**: Tạo 1 role mới `ROLE_TEST_123`.
   - **Gán quyền System**: Cho `TEST_USER` quyền `CREATE SESSION`.
4. **Tab Object Privileges (Đỉnh cao của phần quản trị)**:
   - Hãy chọn schema: `ADMIN`, Table: `HSBA_DV`.
   - Cấp một quyền `SELECT` hoặc cấu hình **chỉ cho phép nhìn thấy một vài cột** (Nhập cột `MA_HSBA, LOAI_DV`).
   - *Giải thích cho Giảng viên:* Lúc này ngầm định C# đã gọi `PKG_ADMIN` để tự động build và add policy hàm VPD, chứ không chỉ đơn thuần là lỗi "Oracle thiếu hàm Cấp SELECT một cột".

---

### MÀN 2: CHỨC NĂNG BỆNH VIỆN - RBAC, CONSTRAINTS & VPD (Phân hệ 2)
> *Chứng minh Data Isolation an toàn giữa các phân hệ lâm sàng.*

1. **Điều Phối Viên (`NV0001` / `123`)**
   - Đăng nhập App. DPV sẽ có quyền điều phối tổng thể qua `DieuPhoiVienForm`.
   - Chuyển sang **Tab Bệnh Nhân**: Chỉnh sửa dữ liệu, đổi "Phái" trên combobox `Nam/Nữ` rảng buộc (Test ORA-02290 Check Constraint đã xử lý thành công). Nhấn Lưu.
   - Chuyển sang **Tab Dịch Vụ**: Tiến hành phân công chọn 1 mã KTV từ dropdown cho 1 dịch vụ bất kỳ. Nhấn Lưu (Áp dụng RBAC mạnh mẽ từ `MV_KTV_LIST`).
   - Vào **Tab Thông tin cá nhân**: Chỉ được sửa Quê Quán & SĐT. *Giải thích thêm hàm VPD `POL_NHANVIEN_SELF` giới hạn DPV chỉ tự thấy record có `MA_NV` là của chính anh ta*.

2. **Bác Sĩ (`NV0021` / `123`)**
   - Logout DPV, Đăng nhập lại với BS. App điều hướng thẳng tới `BacSiForm`.
   - *[Điểm ăn tiền VPD]*: BS sẽ **CHỈ THẤY** HSBA và các bệnh nhân hiện diện trong danh sách được cấp phép quản lý (`MA_BS = 'NV0021'`). Các HSBA của BS khác sẽ bị cô lập hoàn toàn.
   - Thử kê **Đơn Thuốc**: Insert thuốc mới hoặc sửa liều dùng.

3. **Kỹ Thuật Viên (`NV0121` / `123`)**
   - Logout BS, Đăng nhập lại với KTV. Mở form `KyThuatVienForm`.
   - Thấy list hiển thị các dịch vụ mà `NV0121` được phân công.
   - Thử nhập liệu vào cột `KET_QUA`. Nhấn Lưu thông tin. Hành động này sẽ được ghi dấu vết cho màn Audit ở sau.

---

### MÀN 3: LABEL SECURITY COMPARTMENT DYNAMIC (OLS - YC2)
> *Chứng minh quản lý nhãn và ma trận quyền hạn dựa trên Sensitivity Data.*

1. Quay về đăng nhập Giám Đốc/Quản lý chung: **`NV0001` / `123`**.
2. Góc trên bên phải (Header), click vào icon **Chuông (Thông báo Khẩn) 🔔**.
3. Popup `ThongBaoKhanForm` hiện diện.
   - Vì Giám đốc `NV0001` được gán label `BGD` quét tất cả Khoa (`TH,TK,TM`) và vùng địa lý (`HCM,HP,HN`).
   - **Kết quả:** Thấy đủ **7 thông báo khẩn** (Từ T1 tới T7).
4. Đăng xuất, đăng nhập bằng nhân viên KTV bình thường cấp khoa: **`NV0123` / `123`** (Nhân viên: Khoa Tiêu Hóa ở Hà Nội - `NV:TH:HN`).
5. Click icon Chuông 🔔.
   - **Kết quả:** Filter bằng Security Clearances của Database. Hệ thống chỉ cho phép anh ta đọc đúng thông báo T1, T3 (Thông báo public), và **T6** (Bản tin mật của Khoa Tiêu hóa Hà Nội). Toàn bộ khu vực khác (Tim Mạch, Hải Phòng, HCM...) bị che lấp không tồn tại trong tập dữ liệu.

---

### MÀN 4: KIỂM TOÁN LỊCH SỬ THAO TÁC (AUDIT STRATEGIES)
> *Chứng minh Database đang hoạt động theo dõi và ghi vết ngầm (FGA & Unified & Trigger).*

Mở SQL Developer, kết nối bằng `ADMIN` (hoặc sys) và chạy các dòng kiểm chứng:

1. **Kiểm kê ghi vết bảng Trigger (`AUDIT_KETQUA`)**:
   ```sql
   SELECT NGUOI_CAP_NHAT, MA_HSBA, GIA_TRI_CU, GIA_TRI_MOI, THOI_GIAN_CAP_NHAT 
   FROM ADMIN.AUDIT_KETQUA ORDER BY THOI_GIAN_CAP_NHAT DESC;
   ```
   *(Trình bày)*: Show row vừa nãy KTV `NV0121` thực sự update giá trị Xét nghiệm, lưu giữ lại bằng Trigger Before Update.

2. **Kiểm kê FGA (Fine-Grained Audit)**:
   ```sql
   SELECT DB_USER, OBJECT_SCHEMA, OBJECT_NAME, POLICY_NAME, STATEMENT_TYPE, EXTENDED_TIMESTAMP
   FROM DBA_FGA_AUDIT_TRAIL ORDER BY EXTENDED_TIMESTAMP DESC;
   ```
   *(Trình bày)*: FGA sẽ theo sát lịch sử Bác sĩ sửa thông tin nhạy cảm của bảng `DON_THUOC` (`FGA_AUDIT_UPDATE_DONTHUOC`), và chặn/theo dõi điều chỉnh `HSBA` trái pháp luật.

3. **Kiểm tra Unified Auditing (Nếu có kích hoạt)**:
   ```sql
   SELECT DBUSERNAME, ACTION_NAME, OBJECT_SCHEMA, OBJECT_NAME, EVENT_TIMESTAMP 
   FROM UNIFIED_AUDIT_TRAIL 
   WHERE AUDIT_TYPE = 'Standard' AND OBJECT_NAME = 'HSBA'
   ORDER BY EVENT_TIMESTAMP DESC;
   ```
   *(Trình bày)*: Bằng chứng audit `AUDIT_HSBA_UPDATE_CHANDOAN` tuân thủ Audit hợp nhất của Oracle. Mọi hành vi cập nhật thành công đều không thoát khỏi record này.

--- 🎯 Hoàn tất quy trình Demo 🎯 ---
