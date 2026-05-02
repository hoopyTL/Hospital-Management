-- Login bằng tài khoản ADMIN
-- TK: Admin
-- Mật khẩu: 12345
SET SERVEROUTPUT ON;

DECLARE
    v_i NUMBER;
BEGIN
    -- 1. Chèn 20 Điều phối viên (Mã từ NV0001 -> NV0020)
    FOR v_i IN 1..20 LOOP
        INSERT INTO nhan_vien (ma_nv, ho_ten, phai, ngay_sinh, cmnd, que_quan, sdt, vai_tro, chuyen_khoa)
        VALUES ('NV' || LPAD(v_i, 4, '0'), N'Điều Phối Viên ' || v_i, 
                CASE WHEN MOD(v_i, 2) = 0 THEN N'Nam' ELSE N'Nữ' END,
                TO_DATE('1980-01-01', 'YYYY-MM-DD') + v_i,
                '10000000' || LPAD(v_i, 4, '0'), N'Hà Nội', '090000' || LPAD(v_i, 4, '0'),
                N'Điều phối viên', NULL);
    END LOOP;

    -- 2. Chèn 100 Bác sĩ/Y sĩ (Mã từ NV0021 -> NV0120)
    FOR v_i IN 21..120 LOOP
        INSERT INTO nhan_vien (ma_nv, ho_ten, phai, ngay_sinh, cmnd, que_quan, sdt, vai_tro, chuyen_khoa)
        VALUES ('NV' || LPAD(v_i, 4, '0'), N'Bác Sĩ ' || v_i, 
                CASE WHEN MOD(v_i, 2) = 0 THEN N'Nam' ELSE N'Nữ' END,
                TO_DATE('1975-01-01', 'YYYY-MM-DD') + v_i,
                '20000000' || LPAD(v_i, 4, '0'), N'TP HCM', '091000' || LPAD(v_i, 4, '0'),
                N'Bác sĩ/Y sĩ', N'Nội tổng quát');
    END LOOP;

    -- 3. Chèn 50 Kỹ thuật viên (Mã từ NV0121 -> NV0170)
    FOR v_i IN 121..170 LOOP
        INSERT INTO nhan_vien (ma_nv, ho_ten, phai, ngay_sinh, cmnd, que_quan, sdt, vai_tro, chuyen_khoa)
        VALUES ('NV' || LPAD(v_i, 4, '0'), N'Kỹ Thuật Viên ' || v_i, 
                CASE WHEN MOD(v_i, 2) = 0 THEN N'Nam' ELSE N'Nữ' END,
                TO_DATE('1990-01-01', 'YYYY-MM-DD') + v_i,
                '30000000' || LPAD(v_i, 4, '0'), N'Đà Nẵng', '092000' || LPAD(v_i, 4, '0'),
                N'Kỹ thuật viên', N'Xét nghiệm');
    END LOOP;

    COMMIT;
    DBMS_OUTPUT.PUT_LINE('Hoàn thành chèn 170 nhân viên.');
END;
/

DECLARE
    v_i NUMBER;
BEGIN
    FOR v_i IN 1..100000 LOOP
        INSERT INTO benh_nhan (
            ma_bn, ten_bn, phai, ngay_sinh, cccd, so_nha, ten_duong, 
            quan_huyen, tinh_tp, tien_su_benh, tien_su_benh_gd, di_ung_thuoc
        )
        VALUES (
            'BN' || LPAD(v_i, 6, '0'), -- Mã BN từ BN000001 đến BN100000
            N'Bệnh Nhân ' || v_i,
            CASE WHEN MOD(v_i, 2) = 0 THEN N'Nam' ELSE N'Nữ' END,
            TO_DATE('1960-01-01', 'YYYY-MM-DD') + MOD(v_i, 20000), -- Ngày sinh rải rác
            LPAD(v_i, 12, '0'), -- CCCD duy nhất 12 số
            TO_NCHAR(MOD(v_i, 1000)), N'Đường ' || v_i,
            N'Quận ' || MOD(v_i, 20), N'TP HCM',
            N'Tiền sử bệnh ' || v_i, N'Không', N'Không'
        );

        -- Commit mỗi 10.000 dòng để tối ưu hiệu năng
        IF MOD(v_i, 10000) = 0 THEN
            COMMIT;
        END IF;
    END LOOP;
    
    COMMIT;
    DBMS_OUTPUT.PUT_LINE('Hoàn thành chèn 100.000 bệnh nhân.');
END;
/
DECLARE
    v_i NUMBER;
    v_ma_hsba VARCHAR2(8);
    v_ma_bn   VARCHAR2(8);
    v_ma_bs   VARCHAR2(8);
    v_ma_ktv  VARCHAR2(8);
    v_ngay    DATE;
BEGIN
    FOR v_i IN 1..100000 LOOP
        -- 1. Chuẩn bị dữ liệu liên kết
        v_ma_hsba := 'HS' || LPAD(v_i, 6, '0');
        v_ma_bn   := 'BN' || LPAD(v_i, 6, '0');
        
        -- Chọn xoay vòng Bác sĩ từ NV0021 đến NV0120 (100 người)
        v_ma_bs   := 'NV' || LPAD(21 + MOD(v_i, 100), 4, '0');
        
        -- Chọn xoay vòng Kỹ thuật viên từ NV0121 đến NV0170 (50 người)
        v_ma_ktv  := 'NV' || LPAD(121 + MOD(v_i, 50), 4, '0');
        
        v_ngay    := SYSDATE - MOD(v_i, 365); -- Ngày khám trong vòng 1 năm qua

        -- 2. Chèn vào bảng HSBA
        INSERT INTO hsba (ma_hsba, ma_bn, ngay, chuan_doan, dieu_tri, ma_bs, ma_khoa, ket_luan)
        VALUES (
            v_ma_hsba, 
            v_ma_bn, 
            v_ngay, 
            N'Chẩn đoán bệnh lý số ' || v_i, 
            N'Phác đồ điều trị số ' || v_i, 
            v_ma_bs, 
            'K01', -- Giả định mã khoa
            N'Bệnh nhân cần theo dõi thêm'
        );

        -- 3. Chèn vào bảng HSBA_DV (Dịch vụ hỗ trợ)
        INSERT INTO hsba_dv (ma_hsba, loai_dv, ngay_dv, ma_ktv, ket_qua)
        VALUES (
            v_ma_hsba, 
            N'Xét nghiệm tổng quát ' || MOD(v_i, 5), -- 5 loại xét nghiệm giả định
            v_ngay, 
            v_ma_ktv, 
            N'Chỉ số bình thường'
        );

        -- 4. Chèn vào bảng DON_THUOC
        INSERT INTO don_thuoc (ma_hsba, ngay_dt, ten_thuoc, lieu_dung)
        VALUES (
            v_ma_hsba, 
            v_ngay, 
            N'Thuốc đặc trị ' || MOD(v_i, 10), 
            N'Ngày uống 2 lần, mỗi lần 1 viên sau ăn'
        );

        -- Commit sau mỗi 10.000 bản ghi để tối ưu bộ nhớ
        IF MOD(v_i, 10000) = 0 THEN
            COMMIT;
            DBMS_OUTPUT.PUT_LINE('Đã chèn ' || v_i || ' bộ hồ sơ bệnh án...');
        END IF;
    END LOOP;

    COMMIT;
    DBMS_OUTPUT.PUT_LINE('Hoàn thành chèn 100.000 bộ dữ liệu HSBA, HSBA_DV và DON_THUOC.');
END;