-- Chuyển sang PDB trước khi tạo user (Oracle Multitenant)
ALTER SESSION SET CONTAINER = XEPDB1;

-- Cho phép tạo user không cần prefix C## (Oracle 12c+ Multitenant)
ALTER SESSION SET "_ORACLE_SCRIPT"=TRUE;

-- Tạo 1 tài khoản ADMIN với quyền DBA
create user admin identified by 12345;

-- Cấp quyền kết nối và DBA
grant connect,resource,dba to admin;

-- Cho phép tạo bảng không giới hạn
grant unlimited tablespace to admin;

-- Tạo bảng trên schema của ADMIN
alter session set current_schema = admin;

create table benh_nhan (
   ma_bn           varchar2(8) primary key,
   ten_bn          nvarchar2(100) not null,
   phai            nvarchar2(3) check ( phai in ( N'Nam',
                                       N'Nữ' ) ),
   ngay_sinh       date not null,
   cccd            varchar2(12) unique not null,
   so_nha          nvarchar2(20),
   ten_duong       nvarchar2(100),
   quan_huyen      nvarchar2(50),
   tinh_tp         nvarchar2(50),
   tien_su_benh    nvarchar2(1000),
   tien_su_benh_gd nvarchar2(1000),
   di_ung_thuoc    nvarchar2(1000)
);

create table nhan_vien (
   ma_nv       varchar2(8) primary key,
   ho_ten      nvarchar2(100) not null,
   phai        nvarchar2(3) check ( phai in ( N'Nam',
                                       N'Nữ' ) ),
   ngay_sinh   date not null,
   cmnd        varchar2(12) unique not null,
   que_quan    nvarchar2(20),
   sdt         varchar2(10) unique not null,
   vai_tro     nvarchar2(20) check ( vai_tro in ( N'Điều phối viên',
                                              N'Bác sĩ/Y sĩ',
                                              N'Kỹ thuật viên',
                                              N'Bệnh nhân' ) ),
   chuyen_khoa nvarchar2(50)
);

create table hsba (
   ma_hsba    varchar2(8) primary key,
   ma_bn      varchar2(8),
   ngay       date,
   chuan_doan nvarchar2(1000),
   dieu_tri   nvarchar2(1000),
   ma_bs      varchar2(8),
   ma_khoa    varchar2(8),
   ket_luan   nvarchar2(1000),
   constraint fk_hsba_bn foreign key ( ma_bn )
      references benh_nhan ( ma_bn ),
   constraint fk_hsba_bs foreign key ( ma_bs )
      references nhan_vien ( ma_nv )
);

create table hsba_dv (
   ma_hsba varchar2(8),
   loai_dv nvarchar2(50),
   ngay_dv date,
   ma_ktv  varchar2(8),
   ket_qua nvarchar2(50),
   constraint pk_hsba_dv primary key ( ma_hsba,
                                       loai_dv,
                                       ngay_dv ),
   constraint fk_hsba_dv_hsba foreign key ( ma_hsba )
      references hsba ( ma_hsba ),
   constraint fk_hsba_dv_nv foreign key ( ma_ktv )
      references nhan_vien ( ma_nv )
);

create table don_thuoc (
   ma_hsba   varchar2(8),
   ngay_dt   date,
   ten_thuoc nvarchar2(50),
   lieu_dung nvarchar2(1000),
   constraint pk_don_thuoc primary key ( ma_hsba,
                                         ngay_dt,
                                         ten_thuoc ),
   constraint fk_don_thuoc_hsba foreign key ( ma_hsba )
      references hsba ( ma_hsba )
);