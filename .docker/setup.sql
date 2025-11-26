set client_encoding to 'utf8';

drop table if exists stock_import;
drop table if exists material_planning_details;
drop table if exists material_plannings;
drop table if exists machines;
drop table if exists machine_types;
drop table if exists cavity_boms;
drop table if exists cavities;
drop table if exists projects;
drop table if exists clients;
drop table if exists material_stock;
drop table if exists materials;
drop table if exists material_type;
drop table if exists users;
drop table if exists roles;

create table if not exists roles ( 
	"id" serial primary key,
	"name" varchar(250) not null unique
);

create table if not exists users ( 
	"id" serial primary key,
	"fullname" varchar(250) not null,
	"phone" varchar(20) not null unique,
	"password_hash" text not null,
	"created_at" timestamp null default now(),
	"need_change_password" boolean not null default true,
	"role_id" integer not null references roles("id"),
    "active" boolean not null default true,
    "email" varchar(250),
    "date_of_birth" date
);

create table if not exists material_type (
	"id" serial primary key,
	"name" varchar(250) not null unique
);

create table if not exists materials (
    "id" varchar(250) primary key,
    "name" varchar(250) not null,
    "type" integer references material_type("id"),
	"weight" numeric(10,3)
);

create table if not exists material_stock (
	"id" serial primary key,
	"material_id" varchar(250) not null references materials("id"),
	"length" numeric(10,3) not null default 0,
	"width" numeric(10,3) not null default 0,
	"stock" int not null default 0,
	"base_price" numeric(20,0) not null default 0
);

create table if not exists clients ( 
  "id" serial primary key,
  "name" varchar(250) not null,
  "address" varchar(250),
  "phone" varchar(250),
  "email" varchar(250),
  "sales_in_charge_id" integer references users("id")
);

create table if not exists projects (
  "id" serial primary key,
  "name" varchar(250) not null,
  "location" varchar(250) not null,
  "client_id" integer not null references clients("id"),
  "design_company" varchar(250),
  "completion_date" date not null,
  "created_at" timestamp default now(),
  "design_file_path" varchar(250),
  "status" varchar(250) not null default 'Planning' check ("status" in ('Planning', 'Production', 'Deploying', 'Completed', 'Cancelled')),
  "document_path" varchar(250)
);

create table if not exists cavities (
    "id" serial primary key,
    "code" varchar(250) not null,
    "project_id" integer not null references projects("id"),
    "location" varchar(250),
    "quantity" integer not null,
    "window_type" varchar(250),
    "description" varchar(250),
    "width" numeric(10,2) not null,
    "height" numeric(10,2) not null
);

create table if not exists cavity_boms (
    "id" serial primary key,
    "cavity_id" integer not null references cavities("id"),
    "material_id" varchar(250) not null references materials("id"),
    "quantity" integer not null,
    "length" numeric(10,2) not null default 0, -- lưu độ dài cắt nhôm, độ dài gioăng, độ dài cắt kính
    "width" numeric(10,2) not null default 0, -- lưu độ rộng cắt kính
    "color" varchar(250),
    "unit" varchar(250)
);

create table if not exists machine_types (
  "id" serial primary key,
  "name" varchar(250) not null unique
);

create table if not exists machines (
  "id" serial primary key,
  "name" varchar(250) not null,
  "machine_type_id" integer not null references machine_types("id"),
  "status" varchar(50) not null default 'Operational' check ("status" in ('Operational', 'NeedsMaintenance', 'Broken')),
  "entry_date" date not null,
  "last_maintenance_date" date null,
  "expected_completion_date" date null,
  "capacity_value" numeric(10,2), -- <<  công suất 
  "capacity_unit" varchar(50) -- <<  đơn vị (sản phẩm/phút, kg/giờ, mm/phút)
);

create table if not exists material_plannings (
	"id" serial primary key,
	"made_by" integer not null references users("id"),
	"project_id" integer not null references projects("id"),
	"status" varchar(250) not null,
	"created_at" timestamp null default now()
);

create table if not exists material_planning_details (
	"id" serial primary key,
	"planning_id" integer not null references material_plannings("id"),
	"material_id" varchar(250) not null references materials("id"),
	"quantity" integer not null,
	"unit" varchar(250),
	"note" text
);

create table if not exists stock_import (
    "id" serial primary key,
    "material_id" varchar(250) not null references materials("id"),
    "quantity_change" int not null,
    "quantity_after" int not null,
    "price" numeric(20,0) not null,
    "date" date default now()
);

-- INSERT --
insert into roles ("name")
values ('Director'), ('Technician'), ('Sale'), ('InventoryManager'), ('QA');

insert into users ("fullname", "phone", "password_hash", "role_id")
values ('Doãn Quốc Bảo', '0382633428', 'AQAAAAIAAYagAAAAEC7iGEcwGcYC51eb2ijKCRyIa18U40iGykiY27MJ06+6UzKwx/heauSLbMSeFifZag==', 1);

insert into material_type ("name")
values ('Nhôm'), ('Kính'), ('Phụ kiện'), ('Roăng'), ('Vật tư phụ');

insert into materials ("id", "name", "weight", "type")
values
-- Cửa đi mở quay
('C3328', 'Khung cửa đi', 1.257, 1),
('C3303', 'Cánh cửa đi mở ngoài (có gân)', 1.441, 1),
('C18772', 'Cánh cửa đi mở ngoài (không gân)', 1.431, 1),
('C3332', 'Cánh cửa đi mở trong (có gân)', 1.442, 1),
('C18782', 'Cánh cửa đi mở trong (không gân)', 1.431, 1),

('C3322', 'Cánh cửa đi mở ngoài (có gân & bo cạnh)', 1.507, 1),
('C22912', 'Cánh cửa đi mở ngoài (không gân & bo cạnh)', 1.496, 1),
('C38032', 'Cánh cửa đi ngang dưới (có gân)', 2.260, 1),
('C3304', 'Cánh cửa đi ngang dưới (có gân)', 2.023, 1),
('C6614', 'Cánh cửa đi ngang dưới (không gân)', 2.014, 1),

('C3323', 'Đố động cửa đi', 1.086, 1),
('C22903', 'Đố động cửa đi và cửa sổ', 0.891, 1),
('C22900', 'Ốp đáy cánh cửa đi', 0.476, 1),
('C3329', 'Ốp đáy cánh cửa đi', 0.428, 1),
('C3319', 'Ngưỡng cửa đi', 0.689, 1),

('C3291', 'Nẹp kính', 0.206, 1),
('C3225', 'Nẹp kính', 0.211, 1),
('C3296', 'Nẹp kính', 0.237, 1),
('F347', 'Ke góc', 4.957, 5),

('C3246', 'Nẹp kính', 0.216, 1),
('C3286', 'Nẹp kính', 0.223, 1),
('C3236', 'Nẹp kính', 0.227, 1),
('C3206', 'Nẹp kính', 0.257, 1),
('C3295', 'Nẹp kính', 0.271, 1),
-- Cửa sổ mở quay
('C3318', 'Khung cửa sổ',  0.876, 1),
('C8092', 'Cánh cửa sổ mở ngoài (có gân)', 1.064, 1),
('C3202', 'Cánh cửa sổ mở ngoài (có gân)', 1.088, 1),
('C18762', 'Cánh cửa sổ mở ngoài (không gân)', 1.081, 1),
('C3312', 'Cánh cửa sổ mở ngoài (có gân & bo cạnh)', 1.159, 1),

('C22922', 'Cánh cửa sổ mở ngoài (không gân & bo cạnh', 1.118, 1),
('C3033', 'Đố động cửa sổ', 0.825, 1),
--('C22903', N'Đố động cửa đi và cửa sổ', 0.891, 1), --duplicate
('C3313', 'Đố cố định trên khung', 1.126, 1),
('C3209', 'Khung vách kính', 0.876, 1),

('C3203', 'Đố cố định (có lỗ vít)', 0.314, 1),
('F077', 'Pano', 0.664, 1),
('E1283', 'Khung lá sách', 0.290, 1),
('E192', 'Lá sách', 0.317, 1),
('B507', 'Nan dán trang trí', 0.150, 1),

('C3300', 'Nối khung', 0.347, 1),
('C3310', 'Nối khung', 1.308, 1),
('C3210', 'Nối khung 90 độ (bo cạnh)', 1.275, 1),
('C920', 'Nối khung 90 độ (vuông cạnh)', 1.126, 1),
('C910', 'Nối khung 135 độ', 0.916, 1),

('C459', 'Thanh truyền khóa', 0.139, 1),

('C3317', 'Pát liên kết (đố cố định với Fix)', 1.105, 1),
('C3207', 'Pát liên kết (đố cố định với Fix)', 1.154, 1),
('C1687', 'Ke góc', 3.134, 5),
('C4137', 'Ke góc', 1.879, 5),
('C1697', 'Ke góc', 2.436, 5),

--MẶT CẮT THANH NHÔM CỬA ĐI VÀ CỬA SỔ MỞ QUAY
('C38019', 'Khung cửa đi bản 100', 2.057, 1),
('C38038', 'Khung cửa sổ bản 100', 1.421, 1),
('C38039', 'Khung vách kính bản 100 (loại 1 nẹp)', 1.375, 1),
('C48949', 'Khung vách kính bản 100 (loại 2 nẹp)', 1.272, 1),

('C48954', 'Đố cố định bản 100 (loại 1 nẹp)', 1.521, 1),
('C48953', 'Đố cố định bản 100 (loại 2 nẹp)', 1.405, 1),
('C38010', 'Nối khung bản 100', 0.617, 1),
('C48980', 'Nối khung 90 độ bản 100', 2.090, 1),

('C48945', 'Nẹp phụ bản 100', 0.346, 1),
--('F347', 'Ke góc', 4.957, 1), --duplicate
--('C1687', 'Ke góc', 3.134, 1), --duplicate
--('C4137', 'Ke góc', 1.879, 1), --duplicate

--MẶT CẮT THANH NHÔM CỬA ĐI MỞ QUAY
('CX283', 'Khung cửa đi', 1.533, 1),
('CX281', 'Cánh cửa đi mở ngoài', 1.839, 1),
('CX282', 'Cánh ngang dưới cửa đi', 3.033, 1),
('CX568', 'Đố động cửa đi', 1.195, 1),
('CX309', 'Nối khung', 0.427, 1),

--('C22900', 'Ốp đáy cánh cửa đi', 0.476, 1), --duplicate
--('C3329', 'Ốp đáy cánh cửa đi', 0.428, 1), --duplicate
--('C3319', 'Ngưỡng cửa đi', 0.689, 1), --duplicate
--('C459', 'Thanh truyền khóa', 0.139, 1), --duplicate
--('B507', 'Nan dán trang trí', 0.150, 1), --duplicate

--('F347', 'Ke góc', 4.957, 1), --duplicate

--MẶT CẮT THANH NHÔM CỬA SỔ MỞ QUAY
('CX267', 'Khung cửa sổ, vách kính', 1.057, 1),
('CX264', 'Cánh cửa sổ mở ngoài', 1.419, 1),
('CX750', 'Đố động cửa sổ', 0.985, 1),
('CX266', 'Đố cố định (có lỗ vít)', 1.233, 1),
('CX265', 'Đố cố định (không lỗ vít)', 1.163, 1),

('C25899', 'Khung bao chuyển hướng', 0.727, 1),
('CX311', 'Nối khung vách kính', 1.461, 1),
('CX310', 'Thanh nối góc 90 độ', 1.614, 1),

--('C3246', 'Nẹp kính', 0.216, 1), --duplicate
--('C3286', 'Nẹp kính', 0.223, 1), --duplicate
--('C3236', 'Nẹp kính', 0.227, 1), --duplicate
--('C3206', 'Nẹp kính', 0.257, 1), --duplicate
--('C3295', 'Nẹp kính', 0.271, 1), --duplicate

--('C1687', 'Ke góc', 3.134, 1), --duplicate
--('C4137', 'Ke góc', 1.879, 1), --duplicate
--('C1697', 'Ke góc', 2.436, 1), --duplicate
('C1757', 'Ke góc', 2.167, 5),

--MẶT CẮT THANH NHÔM CỬA ĐI VÀ CỬA SỔ MỞ QUAY
('C40988', 'Khung cửa đi và cửa sổ', 0.862, 1),
('C48952', 'Cánh cửa đi vát cạnh bằng', 0.991, 1),
('C40912', 'Cánh cửa đi vát cạnh lệch', 1.008, 1),
('C48942', 'Cánh cửa sổ vát cạnh bằng', 0.908, 1),
('C40902', 'Cánh cửa sổ vát cạnh lệch', 0.924, 1),

('C40983', 'Đố cố định vát cạnh bằng', 0.977, 1),
('C40984', 'Đố cố định vát cạnh lệch', 1.021, 1),
('C44249', 'Khung vách kính', 0.753, 1),
('C44234', 'Đố cố định (có lỗ vít)', 0.857, 1),
('C40869', 'Đố động cửa đi và cửa sổ', 0.701, 1),

('C40973', 'Đố cố định trên khung', 0.860, 1),
('C40978', 'Ốp đáy cánh cửa đi', 0.375, 1),
('E17523', 'Pano', 0.605, 1),
('C44226', 'Nẹp kính', 0.199, 1),
('C40979', 'Nẹp kính', 0.218, 1),

--MẶT CẮT THANH NHÔM CỬA ĐI XẾP TRƯỢT
('F605', 'Khung ngang trên', 3.107, 1),
('F606', 'Khung đứng', 1.027, 1),
('F4116', 'Khung đứng (khoá đa điểm)', 1.056, 1),
('F607', 'Khung ngang dưới (ray nổi)', 1.053, 1),
('F2435', 'Khung ngang dưới (ray âm)', 1.351, 1),

('F523', 'Cánh cửa không lỗ vít (khoá đa điểm)', 1.254, 1),
('F4117', 'Cánh cửa không lỗ vít (khoá đa điểm)', 1.269, 1),
('F5017', 'Cánh cửa không lỗ vít (khoá đa điểm)', 1.307, 1),
('F522', 'Cánh cửa có lỗ vít (khoá đơn điểm)', 1.336, 1),
('F560', 'Đố cố định trên cánh', 1.142, 1),

('F520', 'Ốp giữa 2 cánh mở', 0.241, 1),
('F519', 'Ốp che nước mưa', 0.177, 1),
('F6029', 'Nẹp kính', 0.276, 1),
('F521', 'Nẹp kính', 0.222, 1),

('F608', 'Ke liên kết khung đứng với ngang trên', 1.440, 5),
('F609', 'Ke liên kết khung đứng với ngang dưới', 1.377, 5),
('F417', 'Ke góc', 5.228, 5),
--('F347', 'Ke góc', 4.957, 1), --duplicate

--MẶT CẮT THANH NHÔM CỬA SỔ MỞ LÙA
('D23151', 'Khung cửa lùa', 0.949, 1),
('D45482', 'Khung cửa lùa 3 ray', 1.414, 1),
('D23156', 'Cánh cửa lùa', 0.936, 1),
('D23157', 'Ốp cánh cửa lùa', 0.365, 1),
('D23158', 'Nẹp đối đầu cửa 4 cánh', 0.229, 1),

('D23159', 'Ốp che nước mưa', 0.279, 1),

--MẶT CẮT THANH NHÔM CỬA SỔ MỞ LÙA
('D44329', 'Khung cửa lùa', 0.885, 1),
('D44035', 'Cánh cửa mở lùa', 0.827, 1),
('D44327', 'Ốp cánh cửa lùa', 0.315, 1),
('D44328', 'Nẹp đối đầu cửa 4 cánh', 0.396, 1),

--MẶT CẮT THANH NHÔM CỬA ĐI MỞ LÙA
('D47713', 'Khung cửa lùa', 1.223, 1),
('D45316', 'Cánh cửa mở lùa', 1.319, 1),
('D44564', 'Cánh cửa mở lùa', 1.201, 1),
('D47688', 'Nẹp đối đầu cửa 4 cánh', 0.545, 1),
('D46070', 'Ốp khóa đa điểm', 0.364, 1),

('D47679', 'Ốp đậy ray', 0.096, 1),
('D47678', 'Ốp đậy rãnh phụ kiện', 0.073, 1),
('D45478', 'Thanh ốp móc', 0.383, 1),
('D44569', 'Nẹp kính', 0.199, 1),

--MẶT CẮT THANH NHÔM CỬA MỞ LÙA
('D1541A', 'Khung ngang trên', 1.459, 1),
('D1551A', 'Đố chia cửa lùa với vách kính trên', 2.164, 1),
('D17182', 'Khung ngang dưới (ray bằng)', 1.307, 1),
('D1942', 'Khung ngang dưới (ray lệch)', 1.561, 1),
('D1542A', 'Khung ngang dưới (ray lệch)', 1.706, 1),

('D1543A', 'Khung đứng', 1.134, 1),
('D3213', 'Khung đứng (3 ray)', 1.367, 1),
('D3211', 'Khung ngang trên (3 ray)', 1.959, 1),
('D3212', 'Khung ngang dưới (3 ray)', 2.295, 1),
('D1544A', 'Cánh ngang trên', 0.990, 1),

('D1545A', 'Cánh ngang dưới', 1.000, 1),
('D1546A', 'Cánh đứng trơn', 1.273, 1),
('D1547A', 'Cánh đứng móc', 1.098, 1),
('D28144', 'Cánh ngang trên', 1.115, 1),
('D1555A', 'Cánh ngang dưới', 1.243, 1),

('D26146', 'Cánh đứng trơn', 1.330, 1),
('D28127', 'Cánh đứng móc', 1.303, 1),
('D1559A', 'Khung đứng vách kính', 1.070, 1),
('D2618', 'Đố cố định trên vách kính', 1.546, 1),
('D1354', 'Đố cố định trên cánh', 0.696, 1),

('D1548A', 'Nẹp đối đầu cửa 4 cánh', 0.620, 1),
('D1549A', 'Ốp khung vách kính', 0.712, 1),
('D1578', 'Nối khung vách kính', 0.676, 1),
('D2420', 'Nối góc 90 độ trái', 2.292, 1),
('D2490', 'Nối góc 90 độ phải', 2.292, 1),

('D34608', 'Thanh chuyển kính hộp', 0.399, 1),
('D1779', 'Nẹp kính', 0.100, 1),
('D1298', 'Nẹp kính', 0.109, 1),
('D1168', 'Nẹp kính', 0.130, 1),
('C101', 'Nẹp kính', 0.133, 1),

--MẶT CẮT THANH NHÔM CỬA BẢN LỀ SÀN
('F631', 'Cánh đứng', 2.570, 1),
('F632', 'Cánh ngang trên', 2.382, 1),
('F633', 'Cánh ngang dưới', 2.382, 1),

('F2084', 'Đố tĩnh', 2.278, 1),
('F630', 'Nẹp kính', 0.173, 1),
('F949', 'Nẹp kính', 0.176, 1),

--MỘT SỐ MÃ PHỤ
('D47680', 'Ngưỡng nhôm', 0.408, 1),
('A1079', 'Nẹp lưới chống muỗi', 0.087, 1),
('A1080', 'Nẹp lưới chống muỗi', 0.087, 1),
('D47590', 'Ray nhôm cho cửa nhựa', 0.040, 1),

--MẶT CẮT THANH NHÔM MẶT DỰNG LỘ ĐỐ
('GK461', 'Thanh đố đứng', 2.138, 1),
('GK471', 'Thanh đố đứng', 2.281, 1),
('GK481', 'Thanh đố đứng', 2.424, 1),
('GK491', 'Thanh đố đứng', 2.567, 1),

('GK501', 'Thanh đố đứng', 2.711, 1),
('E21451', 'Thanh đố đứng', 2.347, 1),
('GK581', 'Thanh đố đứng (kính hộp)', 2.730, 1),
('GK993', 'Thanh đố ngang', 1.908, 1),

('GK2053', 'Thanh đố ngang', 1.863, 1),
('GK2467', 'Thanh nêm đố ngang', 0.304, 1),
('GK858', 'Pat liên kết thang ngang', 1.218, 1),
('GK1073', 'Nắp đậy đố ngang', 0.292, 1),

('GK015', 'Đế ốp mặt ngoài', 0.577, 1),
('GK066', 'Nắp đậy đế ốp', 0.404, 1),
('GK780', 'Nối góc 90 độ ngoài', 0.743, 1),
('GK1495', 'Đế ốp mặt ngoài góc 90 độ', 1.110, 1),

('GK806', 'Nắp đậy đế ốp góc 90 độ', 1.721, 1),
('GK1035', 'Đế ốp mặt ngoài góc 135 độ', 0.743, 1),
('GK606', 'Nắp đậy đế ốp góc 135 độ', 0.675, 1),
('GK294', 'Nắp đậy che rãnh', 0.138, 1),

('GK2464', 'Nắp đậy khe rãnh', 0.264, 1),
('GK1255', 'Khung trên cửa sổ (dạng móc treo)', 0.918, 1),
('GK1325', 'Cánh trên cửa sổ (dạng móc treo)', 0.791, 1),
('GK1295', 'Khung cửa sổ', 0.751, 1),

('GK1365', 'Cánh cửa sổ', 0.801, 1),
('GK505', 'Thanh đố kính cho cánh cửa sổ', 0.959, 1),
('GK1215', 'Ke cửa sổ', 0.959, 5),

--THANH NHÔM MẶT DỰNG GIẤU ĐỐ
('GK001', 'Thanh đố đứng', 1.923, 1),
('GK011', 'Thanh đố đứng', 2.211, 1),
('GK021', 'Thanh đố đứng', 2.495, 1),
('GK251', 'Thanh đố đứng', 2.638, 1),

('GK261', 'Thanh đố đứng', 3.051, 1),
('GK813', 'Thanh đố ngang', 1.733, 1),
('GK853', 'Thanh đố ngang', 1.757, 1),
('GK413', 'Nắp đậy thanh đố ngang', 0.217, 1),

('GK1745', 'Pat liên kết thanh đố ngang', 1.173, 1),
--('GK2467', 'Thanh nêm đố ngang', 0.304, 1), --duplicate
('GK228', 'Nẹp kính trái', 0.356, 1),
('GK238', 'Nẹp kính phải', 0.294, 1),

('GK218', 'Nẹp kính trên', 0.437, 1),
('GK208', 'Nẹp kính dưới', 0.383, 1),
('GK255', 'Thanh móc treo kính', 0.436, 1),
--('C459', 'Thanh truyền khóa', 0.139, 1), --duplicate

('GK275', 'Thanh đố kính', 0.245, 1),
('GK1064', 'Chống nhấc cánh', 0.257, 1),
--('GK1255', 'Khung trên cửa sổ (dạng móc treo)', 0.918, 1), --duplicate
--('GK1325', 'Cánh trên cửa sổ (dạng móc treo)', 0.791, 1), --duplicate

--('GK1295', 'Khung cửa sổ', 0.751, 1), --duplicate
--('GK1365', 'Cánh cửa sổ', 0.801, 1), --duplicate
('GK534', 'Thanh đỡ kính cho cánh cửa sổ', 0.195, 1),
('GK454', 'Máng che cánh cửa sổ', 0.288, 1),

--('GK1215', 'Ke cửa sổ', 0.959, 1), --duplicate

--MẶT CẮT PROFILE LAN CAN KÍNH
('E1214', 'Khung bao ngang trên', 1.795, 1),
('E1215', 'Khung bao dưới', 0.976, 1),
('E1216', 'Đố Lan Can', 1.131, 1),
('B1735', 'Đố Lan Can', 1.347, 1),

('E1217', 'Nối góc 90 độ', 1.453, 1),
('E1218', 'Nắp đậy che rãnh', 0.110, 1),

('B2831', 'Khung bao ngang trên', 1.402, 1),
('B2832', 'Nắp đậy che rãnh', 0.177, 1),
('B2846', 'Khung đứng', 1.081, 1),
('B2833', 'Đố Lan Can', 1.404, 1),

('B2834', 'Nối góc 90 độ', 1.617, 1),
('B2835', 'Nắp đậy rãnh khung đứng', 0.109, 1),

('B4425', 'Khung bao ngang trên', 1.453, 1),
('B4426', 'Nẹp kính', 0.155, 1),
('B4429', 'Khung bao đứng', 0.765, 1),
('B4428', 'Đố lan can', 0.932, 1),

('B4430', 'Nẹp kính', 0.153, 1),
('B4427', 'Nối góc 90 độ', 1.197, 1),

('B3730', 'Khung bao ngang trên', 1.128, 1),
('B3731', 'Đố đứng', 0.920, 1),
('B3732', 'Khung đứng', 0.689, 1),
('B3733', 'Nẹp kính', 0.136, 1);

insert into material_stock("material_id", "length", "base_price")
values
('C3209', 6000, 88000),
('C3295', 6000, 88000),
('C3313', 6000, 88000),
('C3328', 6000, 88000),
('C22903', 6000, 88000),
('C3303', 6000, 88000),
('C3329', 6000, 88000),
('C3296', 6000, 88000),
('C3203', 6000, 88000),
('C3318', 6000, 88000),
('C3202', 6000, 88000),
('C3300', 6000, 88000),
('C3304', 6000, 88000),
--('C3208', 6000, 88000),
('D1543A', 6000, 88000),
('D1555A', 6000, 88000),
('D1544A', 6000, 88000),
('D1546A', 6000, 88000),
('D1547A', 6000, 88000),
('D1942', 6000, 88000),
('D1541A', 6000, 88000),
('D23156', 6000, 88000),
('D23157', 6000, 88000),
('D23151', 6000, 88000),
('C3236', 6000, 88000),
('F606', 6000, 88000),
('F605', 6000, 88000),
('F523', 6000, 88000),
('F560', 6000, 88000),
('F607', 6000, 88000),
('F2435', 6000, 88000),
('F520', 6000, 88000),
('F521', 6000, 88000);
--('F431', 6000, 88000);

insert into clients ("name", "address", "phone", "email", "sales_in_charge_id")
values
('Albert Cook', '123 Đường ABC, Hà Nội', '090-123-4567', 'albert.cook@example.com', null),
('Barry Hunter', '456 Đường XYZ, TP. HCM', '091-234-5678', 'barry.hunter@example.com', null),
('Trevor Baker', '789 Đường QWE, Đà Nẵng', '092-345-6789', 'trevor.baker@example.com', null),
('Nguyễn Văn An', '101 Đường Hùng Vương, Huế', '098-888-9999', 'an.nguyen@company.vn', null),
('Trần Thị Bích', '22 Phố Cổ, Hà Nội', '097-777-6666', 'bich.tran@startup.com', null);

insert into projects ("name", "location", "client_id", "design_company", "completion_date", "created_at", "design_file_path", "status", "document_path")
values
('Dự án Vinhome', 'Hà Nội', 1, 'Design Firm X', '2025-12-31', '2025-10-01 09:00:00', 'path/A.pdf', 'Production', 'doc/A.docx'),
('Dự án Ecopark', 'Hưng Yên', 2, 'Design Firm Y', '2024-10-20', '2025-10-05 10:00:00', 'path/B.pdf', 'Completed', 'doc/B.docx'),
('Dự án Biệt thự FLC', 'Quy Nhơn', 1, 'Design Firm X', '2026-06-15', '2025-10-10 11:00:00', 'path/C.pdf', 'Planning', 'doc/C.docx'),
('Khách sạn Imperial Huế', 'Huế', 4, 'Kiến trúc Sông Hương', '2026-03-01', '2025-10-15 14:00:00', 'designs/hue_imperial.pdf', 'Deploying', 'rfq/imperial_docs.docx'),
('Homestay Phố Cổ', 'Hà Nội', 5, null, '2025-11-30', '2025-10-20 16:30:00', null, 'Planning', 'rfq/homestay_hanoi.docx');

insert into machine_types ("name") 
values  ('Máy Cắt'), ('Máy Phay Ổ Khóa'), ('Máy Tiện Tự Động');

insert into machines ("name", "machine_type_id", "status", "entry_date", "last_maintenance_date", "capacity_value", "capacity_unit", "expected_completion_date")
values
('Máy Cắt CNC 01', 1, 'Operational', '2025-01-15', NULL, 5, 'mm/phút', NULL),
('Máy Cắt Góc', 1, 'Operational', '2025-02-20', NULL, 5, 'mm', NULL),
('Máy Phay Ổ Khóa', 2, 'Operational', '2025-03-10', NULL, 50, 'sản phẩm/giờ', NULL);
