set client_encoding to 'utf8';

drop table if exists stage_review_criteria_results;
drop table if exists stage_reviews;
drop table if exists stage_criteria;
drop table if exists production_item_stages;
drop table if exists production_order_items;
drop table if exists production_orders;
drop table if exists stage_types;
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
    "status" varchar(250) not null default 'Pending' check ("status" in ('Pending', 'Cancelled', 'Completed')),
    "created_at" timestamp null default now()
);

create table if not exists material_planning_details (
    "id" serial primary key,
    "planning_id" integer not null references material_plannings("id"),
    "material_id" varchar(250) not null references materials("id"),
    "length" numeric(10,3) not null default 0,
    "width" numeric(10,3) not null default 0,
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

-- production orders
create table if not exists production_orders (
    "id" serial,
    "project_id" integer not null,
    "code" varchar(50),
    "status" smallint not null,
    "is_cancelled" boolean not null default false,
    "assigned_qa_user_id" integer null references users("id"),
    "planned_start_date" timestamp null,
    "planned_finish_date" timestamp null,
    "submitted_at" timestamp null,
    "director_decision_at" timestamp null,
    "qa_machines_checked_at" timestamp null,
    "qa_material_checked_at" timestamp null,
    "started_at" timestamp null,
    "finished_at" timestamp null,
    "created_at" timestamp null default now(),
    "created_by" integer null,
    "updated_at" timestamp null,
    "updated_by" integer null,
    constraint "pk_production_orders" primary key ("id"),
    constraint "fk_production_orders_project" foreign key ("project_id") references projects ("id"),
    constraint "uq_production_orders_code" unique ("code")
);

create index if not exists "ix_production_orders_project_status" on production_orders ("project_id", "status");

-- stage types (Vietnamese names for UI, English codes)
create table if not exists stage_types (
    "id" serial,
    "code" varchar(50) not null,
    "machine_id" integer null,
    "name" varchar(250) not null,
    "is_active" boolean not null default true,
    "is_default" boolean not null default false,
    "created_at" timestamp null default now(),
    "updated_at" timestamp null,
    constraint "pk_stage_types" primary key ("id"),
    constraint "fk_stage_types_machine" foreign key ("machine_id") references machines ("id"),
    constraint "uq_stage_types_code" unique ("code")
);

-- production order items
create table if not exists production_order_items (
    "id" serial,
    "production_order_id" integer not null,
    "cavity_id" integer not null,
    "code" varchar(4) not null,
    "line_no" integer not null,
    "status" smallint not null default 0,
    "planned_start_date" timestamp null,
    "planned_finish_date" timestamp null,
    "actual_start_date" timestamp null,
    "actual_finish_date" timestamp null,
    "qr_code" varchar(255) null,
    "qr_image" bytea null,
    "is_completed" boolean not null default false,
    "completed_at" timestamp null,
    "is_stored" boolean not null default false,
    "is_canceled" boolean not null default false,
    "created_at" timestamp null default now(),
    "updated_at" timestamp null,
    constraint "pk_production_order_items" primary key ("id"),
    constraint "fk_order_items_order" foreign key ("production_order_id") references production_orders ("id"),
    constraint "fk_order_items_cavity" foreign key ("cavity_id") references cavities ("id")
);

create unique index if not exists "uq_production_order_items_order_code"
    on production_order_items ("production_order_id", "code");

create unique index if not exists "uq_production_order_items_order_line_no"
    on production_order_items ("production_order_id", "line_no");

create index if not exists "ix_production_order_items_order"
    on production_order_items ("production_order_id");

-- production item stages
create table if not exists production_item_stages (
    "id" serial,
    "production_order_item_id" integer not null,
    "stage_type_id" integer not null,
    "assigned_qa_user_id" integer null,
    "planned_start_date" timestamp null,
    "planned_finish_date" timestamp null,
    "actual_start_date" timestamp null,
    "actual_finish_date" timestamp null,
    "planned_time_hours" numeric(10,2) null,
    "actual_time_hours" numeric(10,2) null,
    "planned_units" integer null,
    "actual_units" integer null,
    "is_completed" boolean not null default false,
    "completed_at" timestamp null,
    "note" text null,
    "created_at" timestamp null default now(),
    "updated_at" timestamp null,
    constraint "pk_production_item_stages" primary key ("id"),
    constraint "fk_item_stages_item" foreign key ("production_order_item_id") references production_order_items ("id"),
    constraint "fk_item_stages_stage_type" foreign key ("stage_type_id") references stage_types ("id"),
    constraint "uq_item_stage_per_type" unique ("production_order_item_id", "stage_type_id")
);

create index if not exists "ix_item_stages_item"
    on production_item_stages ("production_order_item_id");

-- stage criteria for QA review
create table if not exists stage_criteria (
    "id" serial,
    "stage_type_id" integer not null,
    "code" varchar(50) not null,
    "name" varchar(250) not null,
    "description" text null,
    "check_type" varchar(50) not null default 'boolean' check ("check_type" in ('boolean', 'numeric', 'text', 'select')),
    "required" boolean not null default true,
    "order_index" integer not null default 0,
    "is_active" boolean not null default true,
    "created_at" timestamp null default now(),
    "updated_at" timestamp null,
    constraint "pk_stage_criteria" primary key ("id"),
    constraint "fk_stage_criteria_stage_type" foreign key ("stage_type_id") references stage_types ("id"),
    constraint "uq_stage_criteria_code" unique ("stage_type_id", "code")
);

create index if not exists "ix_stage_criteria_stage_type"
    on stage_criteria ("stage_type_id", "order_index");

-- stage reviews for QA review process
create table if not exists stage_reviews (
    "id" serial,
    "production_item_stage_id" integer not null,
    "requested_by_user_id" integer not null,
    "reviewed_by_user_id" integer null,
    "status" varchar(50) not null default 'pending' check ("status" in ('pending', 'in_progress', 'passed', 'failed')),
    "notes" text null,
    "attachments" text null,
    "requested_at" timestamp null default now(),
    "reviewed_at" timestamp null,
    "created_at" timestamp null default now(),
    "updated_at" timestamp null,
    constraint "pk_stage_reviews" primary key ("id"),
    constraint "fk_stage_reviews_stage" foreign key ("production_item_stage_id") references production_item_stages ("id"),
    constraint "fk_stage_reviews_requested_by" foreign key ("requested_by_user_id") references users ("id"),
    constraint "fk_stage_reviews_reviewed_by" foreign key ("reviewed_by_user_id") references users ("id")
);

create index if not exists "ix_stage_reviews_stage"
    on stage_reviews ("production_item_stage_id", "created_at" desc);

-- stage review criteria results
create table if not exists stage_review_criteria_results (
    "id" serial,
    "stage_review_id" integer not null,
    "stage_criteria_id" integer not null,
    "is_passed" boolean null,
    "value" text null,
    "notes" text null,
    "severity" varchar(50) null check ("severity" in ('low', 'medium', 'high', 'critical')),
    "content" text null,
    "attachments" text null,
    "created_at" timestamp null default now(),
    "updated_at" timestamp null,
    constraint "pk_stage_review_criteria_results" primary key ("id"),
    constraint "fk_review_criteria_review" foreign key ("stage_review_id") references stage_reviews ("id") on delete cascade,
    constraint "fk_review_criteria_criteria" foreign key ("stage_criteria_id") references stage_criteria ("id"),
    constraint "uq_review_criteria" unique ("stage_review_id", "stage_criteria_id")
);

create index if not exists "ix_review_criteria_review"
    on stage_review_criteria_results ("stage_review_id");

-- Global stage time settings
create table if not exists global_stage_time_settings (
    "id" serial,
    "stage_type_id" integer not null unique,
    "setup_minutes" numeric(10,2) not null default 3.0,
    "finish_minutes" numeric(10,2) not null default 1.0,
    "cut_al_minutes_per_unit" numeric(10,2) null,
    "mill_lock_minutes_per_group" numeric(10,2) null,
    "corner_cut_minutes_per_corner" numeric(10,2) null,
    "assemble_frame_minutes_per_corner" numeric(10,2) null,
    "cut_glass_minutes_per_square_meter" numeric(10,2) null,
    "glass_install_minutes_per_unit" numeric(10,2) null,
    "gasket_minutes_per_unit" numeric(10,2) null,
    "accessory_minutes_per_unit" numeric(10,2) null,
    "cut_flush_minutes_per_unit" numeric(10,2) null,
    "silicon_minutes_per_meter" numeric(10,2) null,
    "updated_at" timestamp null default now(),
    constraint "pk_global_stage_time_settings" primary key ("id"),
    constraint "fk_global_stage_time_stage_type" foreign key ("stage_type_id") references stage_types ("id")
);

-- Global labor cost settings (unified rate for all stages)
-- Only one row should exist (enforced by application logic)
create table if not exists global_labor_cost_settings (
    "id" serial,
    "hourly_rate" numeric(10,2) not null default 100000,
    "updated_at" timestamp null default now(),
    constraint "pk_global_labor_cost_settings" primary key ("id")
);

-- Production order specific time settings
create table if not exists production_order_settings (
    "id" serial,
    "production_order_id" integer not null,
    "stage_type_id" integer not null,
    "setup_minutes" numeric(10,2) null,
    "finish_minutes" numeric(10,2) null,
    "cut_al_minutes_per_unit" numeric(10,2) null,
    "mill_lock_minutes_per_group" numeric(10,2) null,
    "corner_cut_minutes_per_corner" numeric(10,2) null,
    "assemble_frame_minutes_per_corner" numeric(10,2) null,
    "cut_glass_minutes_per_square_meter" numeric(10,2) null,
    "glass_install_minutes_per_unit" numeric(10,2) null,
    "gasket_minutes_per_unit" numeric(10,2) null,
    "accessory_minutes_per_unit" numeric(10,2) null,
    "cut_flush_minutes_per_unit" numeric(10,2) null,
    "silicon_minutes_per_meter" numeric(10,2) null,
    "created_at" timestamp null default now(),
    "updated_at" timestamp null,
    constraint "pk_production_order_settings" primary key ("id"),
    constraint "fk_order_settings_order" foreign key ("production_order_id") references production_orders ("id") on delete cascade,
    constraint "fk_order_settings_stage_type" foreign key ("stage_type_id") references stage_types ("id"),
    constraint "uq_order_settings" unique ("production_order_id", "stage_type_id")
);

create index if not exists "ix_order_settings_order"
    on production_order_settings ("production_order_id");

create index if not exists "ix_order_settings_stage_type"
    on production_order_settings ("stage_type_id");

-- INSERT --
insert into roles ("name")
values ('Director'), ('Technician'), ('Sale'), ('InventoryManager'), ('QA'), ('Production Manager');

insert into users ("fullname", "phone", "password_hash", "role_id", "need_change_password")
values 
('Doãn Quốc Bảo', '0382633428', 'AQAAAAIAAYagAAAAEC7iGEcwGcYC51eb2ijKCRyIa18U40iGykiY27MJ06+6UzKwx/heauSLbMSeFifZag==', 1, false),
('Nguyễn Bảo Khánh', '0966699704', 'AQAAAAIAAYagAAAAEJHkv+A5U/7Q7OnAKH+XwNUirexztf3hXIhV6er5Ec3xvpEXqBzqFostupbw+5H4Uw==', 1, false),
('Nguyễn Bảo Khánh', '0231232323', 'AQAAAAIAAYagAAAAEJHkv+A5U/7Q7OnAKH+XwNUirexztf3hXIhV6er5Ec3xvpEXqBzqFostupbw+5H4Uw==', 3, false),
('QA Tester 0', '0900000000', 'AQAAAAIAAYagAAAAEJHkv+A5U/7Q7OnAKH+XwNUirexztf3hXIhV6er5Ec3xvpEXqBzqFostupbw+5H4Uw==', 5, false),
('QA Tester 2', '0900000002', 'AQAAAAIAAYagAAAAEJHkv+A5U/7Q7OnAKH+XwNUirexztf3hXIhV6er5Ec3xvpEXqBzqFostupbw+5H4Uw==', 5, false),
('QA Tester 3', '0900000003', 'AQAAAAIAAYagAAAAEJHkv+A5U/7Q7OnAKH+XwNUirexztf3hXIhV6er5Ec3xvpEXqBzqFostupbw+5H4Uw==', 5, false),
('Production Manager', '0900000001', 'AQAAAAIAAYagAAAAEJHkv+A5U/7Q7OnAKH+XwNUirexztf3hXIhV6er5Ec3xvpEXqBzqFostupbw+5H4Uw==', 6, false);

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

-- Cavities dummy data
insert into cavities ("code", "project_id", "location", "quantity", "window_type", "description", "width", "height")
values
-- Project 1: Dự án Vinhome
('S0S1', 1, 'Tầng 1 - Phòng khách', 2, 'Cửa đi mở quay', 'Cửa đi chính phòng khách', 900, 2100),
('S2S3', 1, 'Tầng 1 - Phòng khách', 1, 'Cửa sổ mở quay', 'Cửa sổ phòng khách', 1200, 1500),
('S3S4', 1, 'Tầng 2 - Phòng ngủ 1', 1, 'Cửa đi mở quay', 'Cửa phòng ngủ chính', 800, 2100),
('S4S5', 1, 'Tầng 2 - Phòng ngủ 1', 2, 'Cửa sổ mở quay', 'Cửa sổ phòng ngủ', 1000, 1200),
('S5S6', 1, 'Tầng 2 - Phòng ngủ 2', 1, 'Cửa đi mở quay', 'Cửa phòng ngủ 2', 800, 2100),
-- Project 2: Dự án Ecopark
('E0E1', 2, 'Tầng 1 - Phòng khách', 1, 'Cửa đi mở quay', 'Cửa đi chính', 1000, 2200),
('E1E2', 2, 'Tầng 1 - Phòng khách', 3, 'Cửa sổ mở quay', 'Cửa sổ phòng khách', 1500, 1800),
('E2E3', 2, 'Tầng 2 - Phòng ngủ', 1, 'Cửa đi mở quay', 'Cửa phòng ngủ', 800, 2100),
-- Project 3: Dự án Biệt thự FLC
('F0F1', 3, 'Mặt tiền - Tầng 1', 2, 'Cửa đi mở quay', 'Cửa đi chính mặt tiền', 1200, 2400),
('F1F2', 3, 'Mặt tiền - Tầng 1', 4, 'Cửa sổ mở quay', 'Cửa sổ mặt tiền', 1800, 2000),
('F2F3', 3, 'Sau nhà - Tầng 1', 1, 'Cửa đi mở quay', 'Cửa đi sau nhà', 900, 2100),
-- Project 4: Khách sạn Imperial Huế
('I0I1', 4, 'Phòng 101', 1, 'Cửa đi mở quay', 'Cửa phòng 101', 900, 2100),
('I1I2', 4, 'Phòng 101', 1, 'Cửa sổ mở quay', 'Cửa sổ phòng 101', 1200, 1500),
('I2I3', 4, 'Phòng 102', 1, 'Cửa đi mở quay', 'Cửa phòng 102', 900, 2100),
('I3I4', 4, 'Phòng 102', 1, 'Cửa sổ mở quay', 'Cửa sổ phòng 102', 1200, 1500),
-- Project 5: Homestay Phố Cổ
('H0H1', 5, 'Tầng 1', 1, 'Cửa đi mở quay', 'Cửa đi chính', 800, 2000),
('H1H2', 5, 'Tầng 1', 2, 'Cửa sổ mở quay', 'Cửa sổ tầng 1', 1000, 1200);

-- Cavity BOMs dummy data
-- S0S1: Cửa đi chính phòng khách (Project 1)
insert into cavity_boms ("cavity_id", "material_id", "quantity", "length", "width", "color", "unit")
select c.id, 'C3328', 2, 900, 0, null, 'm' from cavities c where c.code = 'S0S1' and c.project_id = 1
union all
select c.id, 'C3303', 2, 2100, 0, null, 'm' from cavities c where c.code = 'S0S1' and c.project_id = 1
union all
select c.id, 'C3319', 1, 900, 0, null, 'm' from cavities c where c.code = 'S0S1' and c.project_id = 1
union all
select c.id, 'C3295', 1, (900 + 2100) * 2, 0, 'Trắng', 'm' from cavities c where c.code = 'S0S1' and c.project_id = 1;

-- S2S3: Cửa sổ phòng khách (Project 1)
insert into cavity_boms ("cavity_id", "material_id", "quantity", "length", "width", "color", "unit")
select c.id, 'C3318', 2, 1200, 0, null, 'm' from cavities c where c.code = 'S2S3' and c.project_id = 1
union all
select c.id, 'C3202', 2, 1500, 0, null, 'm' from cavities c where c.code = 'S2S3' and c.project_id = 1
union all
select c.id, 'C3033', 1, 1200, 0, null, 'm' from cavities c where c.code = 'S2S3' and c.project_id = 1
union all
select c.id, 'C3295', 1, (1200 + 1500) * 2, 0, 'Trắng', 'm' from cavities c where c.code = 'S2S3' and c.project_id = 1;

-- E0E1: Cửa đi chính (Project 2)
insert into cavity_boms ("cavity_id", "material_id", "quantity", "length", "width", "color", "unit")
select c.id, 'C3328', 2, 1000, 0, null, 'm' from cavities c where c.code = 'E0E1' and c.project_id = 2
union all
select c.id, 'C3303', 2, 2200, 0, null, 'm' from cavities c where c.code = 'E0E1' and c.project_id = 2
union all
select c.id, 'C3319', 1, 1000, 0, null, 'm' from cavities c where c.code = 'E0E1' and c.project_id = 2
union all
select c.id, 'C3295', 1, (1000 + 2200) * 2, 0, 'Trắng', 'm' from cavities c where c.code = 'E0E1' and c.project_id = 2;

-- E1E2: Cửa sổ phòng khách (Project 2)
insert into cavity_boms ("cavity_id", "material_id", "quantity", "length", "width", "color", "unit")
select c.id, 'C3318', 2, 1500, 0, null, 'm' from cavities c where c.code = 'E1E2' and c.project_id = 2
union all
select c.id, 'C3202', 2, 1800, 0, null, 'm' from cavities c where c.code = 'E1E2' and c.project_id = 2
union all
select c.id, 'C3033', 1, 1500, 0, null, 'm' from cavities c where c.code = 'E1E2' and c.project_id = 2
union all
select c.id, 'C3295', 1, (1500 + 1800) * 2, 0, 'Trắng', 'm' from cavities c where c.code = 'E1E2' and c.project_id = 2;

-- F0F1: Cửa đi chính mặt tiền (Project 3)
insert into cavity_boms ("cavity_id", "material_id", "quantity", "length", "width", "color", "unit")
select c.id, 'C3328', 2, 1200, 0, null, 'm' from cavities c where c.code = 'F0F1' and c.project_id = 3
union all
select c.id, 'C3303', 2, 2400, 0, null, 'm' from cavities c where c.code = 'F0F1' and c.project_id = 3
union all
select c.id, 'C3319', 1, 1200, 0, null, 'm' from cavities c where c.code = 'F0F1' and c.project_id = 3
union all
select c.id, 'C3295', 1, (1200 + 2400) * 2, 0, 'Nâu', 'm' from cavities c where c.code = 'F0F1' and c.project_id = 3;

-- F1F2: Cửa sổ mặt tiền (Project 3)
insert into cavity_boms ("cavity_id", "material_id", "quantity", "length", "width", "color", "unit")
select c.id, 'C3318', 2, 1800, 0, null, 'm' from cavities c where c.code = 'F1F2' and c.project_id = 3
union all
select c.id, 'C3202', 2, 2000, 0, null, 'm' from cavities c where c.code = 'F1F2' and c.project_id = 3
union all
select c.id, 'C3033', 1, 1800, 0, null, 'm' from cavities c where c.code = 'F1F2' and c.project_id = 3
union all
select c.id, 'C3295', 1, (1800 + 2000) * 2, 0, 'Nâu', 'm' from cavities c where c.code = 'F1F2' and c.project_id = 3;

-- I0I1: Cửa phòng 101 (Project 4)
insert into cavity_boms ("cavity_id", "material_id", "quantity", "length", "width", "color", "unit")
select c.id, 'C3328', 2, 900, 0, null, 'm' from cavities c where c.code = 'I0I1' and c.project_id = 4
union all
select c.id, 'C3303', 2, 2100, 0, null, 'm' from cavities c where c.code = 'I0I1' and c.project_id = 4
union all
select c.id, 'C3319', 1, 900, 0, null, 'm' from cavities c where c.code = 'I0I1' and c.project_id = 4
union all
select c.id, 'C3295', 1, (900 + 2100) * 2, 0, 'Trắng', 'm' from cavities c where c.code = 'I0I1' and c.project_id = 4;

-- I1I2: Cửa sổ phòng 101 (Project 4)
insert into cavity_boms ("cavity_id", "material_id", "quantity", "length", "width", "color", "unit")
select c.id, 'C3318', 2, 1200, 0, null, 'm' from cavities c where c.code = 'I1I2' and c.project_id = 4
union all
select c.id, 'C3202', 2, 1500, 0, null, 'm' from cavities c where c.code = 'I1I2' and c.project_id = 4
union all
select c.id, 'C3033', 1, 1200, 0, null, 'm' from cavities c where c.code = 'I1I2' and c.project_id = 4
union all
select c.id, 'C3295', 1, (1200 + 1500) * 2, 0, 'Trắng', 'm' from cavities c where c.code = 'I1I2' and c.project_id = 4;

-- H0H1: Cửa đi chính (Project 5)
insert into cavity_boms ("cavity_id", "material_id", "quantity", "length", "width", "color", "unit")
select c.id, 'C3328', 2, 800, 0, null, 'm' from cavities c where c.code = 'H0H1' and c.project_id = 5
union all
select c.id, 'C3303', 2, 2000, 0, null, 'm' from cavities c where c.code = 'H0H1' and c.project_id = 5
union all
select c.id, 'C3319', 1, 800, 0, null, 'm' from cavities c where c.code = 'H0H1' and c.project_id = 5
union all
select c.id, 'C3295', 1, (800 + 2000) * 2, 0, 'Trắng', 'm' from cavities c where c.code = 'H0H1' and c.project_id = 5;

-- Seed default stage types (if not exists)
insert into stage_types ("code", "name", "is_active", "is_default")
select v.code, v.name, true, true
from (
  values
      ('CUT_AL', 'Cắt nhôm'),
      ('MILL_LOCK', 'Phay ổ khóa'),
      ('DOOR_CORNER_CUT', 'Cắt góc cửa'),
      ('ASSEMBLE_FRAME', 'Ghép khung'),
      ('CUT_GLASS', 'Cắt kính'),
      ('GLASS_INSTALL', 'Lắp kính'),
      ('PRESS_GASKET', 'Ép gioăng'),
      ('INSTALL_ACCESSORIES', 'Bắn phụ kiện'),
      ('CUT_FLUSH', 'Cắt sập'),
      ('FINISH_SILICON', 'Hoàn thiện silicon')
) as v(code, name)
where not exists (select 1 from stage_types s where s."code" = v.code);

-- Orders
insert into production_orders ("project_id", "code", "status", "is_cancelled", "created_at")
select v.project_id, v.code, v.status, false, now()
from (
  values
      (1, 'PO-ALPHA-001', 0),
      (2, 'PO-BETA-001',  0)
) as v(project_id, code, status)
where not exists (select 1 from production_orders o where o."code" = v.code);

-- Items for PO-ALPHA-001 (using first cavity from project 1 if exists)
insert into production_order_items ("production_order_id", "cavity_id", "code", "line_no", "qr_code", "is_completed", "created_at")
select o.id, c.id, 'S001', 1, null, false, now()
from production_orders o
join cavities c on c.project_id = o.project_id
where o."code" = 'PO-ALPHA-001'
and c.id = (select min(id) from cavities where project_id = o.project_id)
and not exists (
  select 1 from production_order_items i where i.production_order_id = o.id and i.code = 'S001'
)
limit 1;

insert into production_order_items ("production_order_id", "cavity_id", "code", "line_no", "qr_code", "is_completed", "created_at")
select o.id, c.id, 'S002', 2, null, false, now()
from production_orders o
join cavities c on c.project_id = o.project_id
where o."code" = 'PO-ALPHA-001'
and c.id = (select min(id) from cavities where project_id = o.project_id)
and not exists (
  select 1 from production_order_items i where i.production_order_id = o.id and i.code = 'S002'
)
limit 1;

-- Items for PO-BETA-001 (using first cavity from project 2 if exists)
insert into production_order_items ("production_order_id", "cavity_id", "code", "line_no", "qr_code", "is_completed", "created_at")
select o.id, c.id, 'S001', 1, null, false, now()
from production_orders o
join cavities c on c.project_id = o.project_id
where o."code" = 'PO-BETA-001'
and c.id = (select min(id) from cavities where project_id = o.project_id)
and not exists (
  select 1 from production_order_items i where i.production_order_id = o.id and i.code = 'S001'
)
limit 1;

insert into production_order_items ("production_order_id", "cavity_id", "code", "line_no", "qr_code", "is_completed", "created_at")
select o.id, c.id, 'S002', 2, null, false, now()
from production_orders o
join cavities c on c.project_id = o.project_id
where o."code" = 'PO-BETA-001'
and c.id = (select min(id) from cavities where project_id = o.project_id)
and not exists (
  select 1 from production_order_items i where i.production_order_id = o.id and i.code = 'S002'
)
limit 1;

-- Create default stages for all items in these orders
insert into production_item_stages ("production_order_item_id", "stage_type_id", "is_completed", "created_at")
select i.id, st.id, false, now()
from production_order_items i
join production_orders o on o.id = i.production_order_id
join stage_types st on st.is_default = true
where o."code" in ('PO-ALPHA-001','PO-BETA-001')
and not exists (
  select 1 from production_item_stages s where s.production_order_item_id = i.id and s.stage_type_id = st.id
);

-- Update PO-ALPHA-001 to InProduction status with planned dates
update production_orders
set status = 5, -- InProduction
  planned_start_date = '2025-11-01 08:00:00',
  planned_finish_date = '2025-11-15 17:00:00',
  submitted_at = '2025-10-25 09:00:00',
  director_decision_at = '2025-10-26 10:00:00',
  qa_machines_checked_at = '2025-10-27 11:00:00',
  qa_material_checked_at = '2025-10-28 12:00:00',
  started_at = '2025-11-01 08:00:00',
  updated_at = now()
where code = 'PO-ALPHA-001';

-- Update items for PO-ALPHA-001 with planned dates
update production_order_items
set planned_start_date = '2025-11-01 08:00:00',
  planned_finish_date = '2025-11-15 17:00:00',
  updated_at = now()
where production_order_id = (select id from production_orders where code = 'PO-ALPHA-001');

-- Update stages for PO-ALPHA-001 items with planned dates and hours
-- Stage order: CUT_AL, MILL_LOCK, DOOR_CORNER_CUT, ASSEMBLE_FRAME, GLASS_INSTALL, PRESS_GASKET, INSTALL_ACCESSORIES, CUT_FLUSH, FINISH_SILICON
WITH src AS (
  SELECT 
      pis.id,
      i.planned_start_date,
      st.code
  FROM production_item_stages pis
  JOIN production_order_items i ON i.id = pis.production_order_item_id
  JOIN production_orders o ON o.id = i.production_order_id
  JOIN stage_types st ON st.id = pis.stage_type_id
  WHERE o.code = 'PO-ALPHA-001'
)
UPDATE production_item_stages AS pis
SET 
  planned_start_date = src.planned_start_date + (
      CASE 
          WHEN src.code = 'CUT_AL' THEN interval '0 days'
          WHEN src.code = 'MILL_LOCK' THEN interval '0 days'
          WHEN src.code = 'DOOR_CORNER_CUT' THEN interval '1 day'
          WHEN src.code = 'ASSEMBLE_FRAME' THEN interval '2 days'
          WHEN src.code = 'GLASS_INSTALL' THEN interval '3 days'
          WHEN src.code = 'PRESS_GASKET' THEN interval '3 days'
          WHEN src.code = 'INSTALL_ACCESSORIES' THEN interval '4 days'
          WHEN src.code = 'CUT_FLUSH' THEN interval '4 days'
          WHEN src.code = 'FINISH_SILICON' THEN interval '5 days'
          ELSE interval '0 days'
      END
  ),
  updated_at = now()
FROM src
WHERE pis.id = src.id;

INSERT INTO machine_types ("name") 
VALUES 
('Máy Cắt'),
('Máy Phay Ổ Khóa'),
('Máy Tiện Tự Động');

INSERT INTO machines 
("name", "machine_type_id", "status", "entry_date", "last_maintenance_date", "capacity_value", "capacity_unit", "expected_completion_date")
VALUES
('Máy Cắt CNC 01', 1, 'Operational', '2025-01-15', NULL, NULL,'mm/phút', NULL),
('Máy Cắt Góc', 1, 'Operational', '2025-02-20', NULL, 5, 'mm', NULL),
('Máy Phay Ổ Khóa', 2, 'Operational', '2025-03-10', NULL, 50, 'sản phẩm/giờ', NULL);

-- Stage Criteria for QA Review
-- =========================
-- CUT_AL – Cắt nhôm (1)
-- =========================
INSERT INTO stage_criteria ("stage_type_id", "code", "name", "description", "check_type", "required", "order_index")
SELECT st.id, 'CUT_AL_01', 'Kích thước tổng thể đúng theo bản vẽ', 'Chiều dài & chiều rộng sai số ≤ ±1mm.', 'numeric', true, 1
FROM stage_types st WHERE st.code = 'CUT_AL' ON CONFLICT DO NOTHING;

INSERT INTO stage_criteria ("stage_type_id", "code", "name", "description", "check_type", "required", "order_index")
SELECT st.id, 'CUT_AL_02', 'Dung sai chiều dài sau cắt', 'Sai số chiều dài ≤ ±1mm.', 'numeric', true, 2
FROM stage_types st WHERE st.code = 'CUT_AL' ON CONFLICT DO NOTHING;

INSERT INTO stage_criteria ("stage_type_id", "code", "name", "description", "check_type", "required", "order_index")
SELECT st.id, 'CUT_AL_03', 'Đầu cắt vuông góc', 'Độ lệch góc ≤ 1°.', 'numeric', true, 3
FROM stage_types st WHERE st.code = 'CUT_AL' ON CONFLICT DO NOTHING;

INSERT INTO stage_criteria ("stage_type_id", "code", "name", "description", "check_type", "required", "order_index")
SELECT st.id, 'CUT_AL_04', 'Bề mặt cắt sạch, không ba via', 'Không ba via, răng cưa hoặc cháy cạnh.', 'boolean', true, 4
FROM stage_types st WHERE st.code = 'CUT_AL' ON CONFLICT DO NOTHING;

INSERT INTO stage_criteria ("stage_type_id", "code", "name", "description", "check_type", "required", "order_index")
SELECT st.id, 'CUT_AL_05', 'Số lượng cắt đúng theo kế hoạch', 'Đủ số lượng theo lệnh sản xuất.', 'numeric', true, 5
FROM stage_types st WHERE st.code = 'CUT_AL' ON CONFLICT DO NOTHING;

-- =========================
-- MILL_LOCK – Phay ổ khóa (2)
-- =========================
INSERT INTO stage_criteria ("stage_type_id", "code", "name", "description", "check_type", "required", "order_index")
SELECT st.id, 'MILL_LOCK_01', 'Vị trí phay đúng bản vẽ', 'Sai lệch vị trí ≤ ±1mm.', 'numeric', true, 1
FROM stage_types st WHERE st.code = 'MILL_LOCK' ON CONFLICT DO NOTHING;

INSERT INTO stage_criteria ("stage_type_id", "code", "name", "description", "check_type", "required", "order_index")
SELECT st.id, 'MILL_LOCK_02', 'Kích thước hốc phay đúng', 'Chiều dài & chiều rộng sai số ≤ ±1mm.', 'numeric', true, 2
FROM stage_types st WHERE st.code = 'MILL_LOCK' ON CONFLICT DO NOTHING;

INSERT INTO stage_criteria ("stage_type_id", "code", "name", "description", "check_type", "required", "order_index")
SELECT st.id, 'MILL_LOCK_03', 'Độ sâu hốc phay đúng', 'Độ sâu phay sai số ≤ ±0.5mm.', 'numeric', true, 3
FROM stage_types st WHERE st.code = 'MILL_LOCK' ON CONFLICT DO NOTHING;

INSERT INTO stage_criteria ("stage_type_id", "code", "name", "description", "check_type", "required", "order_index")
SELECT st.id, 'MILL_LOCK_04', 'Không ba via, không sứt mẻ', 'Mép phay sạch, không gãy nhôm.', 'boolean', true, 4
FROM stage_types st WHERE st.code = 'MILL_LOCK' ON CONFLICT DO NOTHING;

-- =========================
-- DOOR_CORNER_CUT – Cắt góc cửa (3)
-- =========================
INSERT INTO stage_criteria ("stage_type_id", "code", "name", "description", "check_type", "required", "order_index")
SELECT st.id, 'DOOR_CORNER_01', 'Vị trí cắt góc', 'Sai lệch vị trí ≤ ±1mm.', 'numeric', true, 1
FROM stage_types st WHERE st.code = 'DOOR_CORNER_CUT' ON CONFLICT DO NOTHING;

INSERT INTO stage_criteria ("stage_type_id", "code", "name", "description", "check_type", "required", "order_index")
SELECT st.id, 'DOOR_CORNER_02', 'Góc cắt đúng thông số', 'Sai lệch góc ≤ 1°.', 'numeric', true, 2
FROM stage_types st WHERE st.code = 'DOOR_CORNER_CUT' ON CONFLICT DO NOTHING;

INSERT INTO stage_criteria ("stage_type_id", "code", "name", "description", "check_type", "required", "order_index")
SELECT st.id, 'DOOR_CORNER_03', 'Khu vực cắt không nứt vỡ', 'Không xuất hiện vết nứt hoặc sứt cạnh.', 'boolean', true, 3
FROM stage_types st WHERE st.code = 'DOOR_CORNER_CUT' ON CONFLICT DO NOTHING;

-- =========================
-- ASSEMBLE_FRAME – Ghép khung (4)
-- =========================
INSERT INTO stage_criteria ("stage_type_id", "code", "name", "description", "check_type", "required", "order_index")
SELECT st.id, 'ASSEMBLE_01', 'Lắp đúng mã chi tiết & đúng hướng', 'Theo đúng bản vẽ.', 'boolean', true, 1
FROM stage_types st WHERE st.code = 'ASSEMBLE_FRAME' ON CONFLICT DO NOTHING;

INSERT INTO stage_criteria ("stage_type_id", "code", "name", "description", "check_type", "required", "order_index")
SELECT st.id, 'ASSEMBLE_02', 'Lỗ bắt vít thẳng hàng', 'Sai lệch ≤ 1mm.', 'numeric', true, 2
FROM stage_types st WHERE st.code = 'ASSEMBLE_FRAME' ON CONFLICT DO NOTHING;

INSERT INTO stage_criteria ("stage_type_id", "code", "name", "description", "check_type", "required", "order_index")
SELECT st.id, 'ASSEMBLE_03', 'Khung không vênh trong quá trình ghép', 'Kiểm tra độ vênh thô.', 'boolean', true, 3
FROM stage_types st WHERE st.code = 'ASSEMBLE_FRAME' ON CONFLICT DO NOTHING;

INSERT INTO stage_criteria ("stage_type_id", "code", "name", "description", "check_type", "required", "order_index")
SELECT st.id, 'ASSEMBLE_04', 'Khe hở giữa các góc ≤ 0.5mm', 'Không có khe hở lớn gây mất thẩm mỹ.', 'numeric', true, 4
FROM stage_types st WHERE st.code = 'ASSEMBLE_FRAME' ON CONFLICT DO NOTHING;

INSERT INTO stage_criteria ("stage_type_id", "code", "name", "description", "check_type", "required", "order_index")
SELECT st.id, 'ASSEMBLE_05', 'Độ vênh tổng thể của khung < 1mm', 'Đo đường chéo đảm bảo khung không méo.', 'numeric', true, 5
FROM stage_types st WHERE st.code = 'ASSEMBLE_FRAME' ON CONFLICT DO NOTHING;

-- =========================
-- CUT_GLASS – Cắt kính (5)
-- =========================
INSERT INTO stage_criteria ("stage_type_id", "code", "name", "description", "check_type", "required", "order_index")
SELECT st.id, 'CUT_GLASS_01', 'Kích thước tổng thể đúng bản vẽ', 'Sai số ≤ ±1mm.', 'numeric', true, 1
FROM stage_types st WHERE st.code = 'CUT_GLASS' ON CONFLICT DO NOTHING;

INSERT INTO stage_criteria ("stage_type_id", "code", "name", "description", "check_type", "required", "order_index")
SELECT st.id, 'CUT_GLASS_02', 'Mặt cắt thẳng', 'Sai lệch đường cắt ≤ 1mm/m.', 'numeric', true, 2
FROM stage_types st WHERE st.code = 'CUT_GLASS' ON CONFLICT DO NOTHING;

INSERT INTO stage_criteria ("stage_type_id", "code", "name", "description", "check_type", "required", "order_index")
SELECT st.id, 'CUT_GLASS_03', 'Không mẻ cạnh, không nứt chân', 'Mép cắt đảm bảo an toàn.', 'boolean', true, 3
FROM stage_types st WHERE st.code = 'CUT_GLASS' ON CONFLICT DO NOTHING;

INSERT INTO stage_criteria ("stage_type_id", "code", "name", "description", "check_type", "required", "order_index")
SELECT st.id, 'CUT_GLASS_04', 'Không trầy xước bề mặt kính', 'Không có vết xước do cắt hoặc vận chuyển.', 'boolean', true, 4
FROM stage_types st WHERE st.code = 'CUT_GLASS' ON CONFLICT DO NOTHING;

-- =========================
-- GLASS_INSTALL – Lắp kính (6)
-- =========================
INSERT INTO stage_criteria ("stage_type_id", "code", "name", "description", "check_type", "required", "order_index")
SELECT st.id, 'GLASS_INSTALL_01', 'Kích thước kính đúng bản vẽ', 'Sai số ≤ ±1mm.', 'numeric', true, 1
FROM stage_types st WHERE st.code = 'GLASS_INSTALL' ON CONFLICT DO NOTHING;

INSERT INTO stage_criteria ("stage_type_id", "code", "name", "description", "check_type", "required", "order_index")
SELECT st.id, 'GLASS_INSTALL_02', 'Kính đặt đúng vị trí', 'Không lệch khỏi gờ đỡ.', 'boolean', true, 2
FROM stage_types st WHERE st.code = 'GLASS_INSTALL' ON CONFLICT DO NOTHING;

INSERT INTO stage_criteria ("stage_type_id", "code", "name", "description", "check_type", "required", "order_index")
SELECT st.id, 'GLASS_INSTALL_03', 'Kính cố định chắc chắn', 'Không rung lắc.', 'boolean', true, 3
FROM stage_types st WHERE st.code = 'GLASS_INSTALL' ON CONFLICT DO NOTHING;

INSERT INTO stage_criteria ("stage_type_id", "code", "name", "description", "check_type", "required", "order_index")
SELECT st.id, 'GLASS_INSTALL_04', 'Khe hở đều 2–3mm', 'Chuẩn để bơm keo và ép gioăng.', 'numeric', true, 4
FROM stage_types st WHERE st.code = 'GLASS_INSTALL' ON CONFLICT DO NOTHING;

-- =========================
-- PRESS_GASKET – Ép gioăng (7)
-- =========================
INSERT INTO stage_criteria ("stage_type_id", "code", "name", "description", "check_type", "required", "order_index")
SELECT st.id, 'GASKET_01', 'Gioăng ép đều', 'Gioăng ép liên tục, không hở.', 'boolean', true, 1
FROM stage_types st WHERE st.code = 'PRESS_GASKET' ON CONFLICT DO NOTHING;

INSERT INTO stage_criteria ("stage_type_id", "code", "name", "description", "check_type", "required", "order_index")
SELECT st.id, 'GASKET_02', 'Không lồi lõm hoặc bung', 'Đảm bảo tính kín khít.', 'boolean', true, 2
FROM stage_types st WHERE st.code = 'PRESS_GASKET' ON CONFLICT DO NOTHING;

INSERT INTO stage_criteria ("stage_type_id", "code", "name", "description", "check_type", "required", "order_index")
SELECT st.id, 'GASKET_03', 'Gioăng khít với kính & khung', 'Không hở khe.', 'boolean', true, 3
FROM stage_types st WHERE st.code = 'PRESS_GASKET' ON CONFLICT DO NOTHING;

INSERT INTO stage_criteria ("stage_type_id", "code", "name", "description", "check_type", "required", "order_index")
SELECT st.id, 'GASKET_04', 'Đúng loại & màu gioăng', 'Theo BOM.', 'select', true, 4
FROM stage_types st WHERE st.code = 'PRESS_GASKET' ON CONFLICT DO NOTHING;

-- =========================
-- INSTALL_ACCESSORIES – Bắn phụ kiện (8)
-- =========================
INSERT INTO stage_criteria ("stage_type_id", "code", "name", "description", "check_type", "required", "order_index")
SELECT st.id, 'ACCESSORIES_01', 'Vị trí lắp đúng theo bản vẽ', 'Lắp đặt đúng điểm định vị.', 'boolean', true, 1
FROM stage_types st WHERE st.code = 'INSTALL_ACCESSORIES' ON CONFLICT DO NOTHING;

INSERT INTO stage_criteria ("stage_type_id", "code", "name", "description", "check_type", "required", "order_index")
SELECT st.id, 'ACCESSORIES_02', 'Vít bắt chặt, không rơ lỏng', 'Đảm bảo an toàn.', 'boolean', true, 2
FROM stage_types st WHERE st.code = 'INSTALL_ACCESSORIES' ON CONFLICT DO NOTHING;

INSERT INTO stage_criteria ("stage_type_id", "code", "name", "description", "check_type", "required", "order_index")
SELECT st.id, 'ACCESSORIES_03', 'Không trầy xước khung', 'Không gây hư hại bề mặt.', 'boolean', true, 3
FROM stage_types st WHERE st.code = 'INSTALL_ACCESSORIES' ON CONFLICT DO NOTHING;

INSERT INTO stage_criteria ("stage_type_id", "code", "name", "description", "check_type", "required", "order_index")
SELECT st.id, 'ACCESSORIES_04', 'Đúng chủng loại phụ kiện', 'Theo BOM.', 'select', true, 4
FROM stage_types st WHERE st.code = 'INSTALL_ACCESSORIES' ON CONFLICT DO NOTHING;

-- =========================
-- CUT_FLUSH – Cắt sập (9)
-- =========================
INSERT INTO stage_criteria ("stage_type_id", "code", "name", "description", "check_type", "required", "order_index")
SELECT st.id, 'FLUSH_01', 'Kích thước đúng thông số', 'Sai số ≤ ±1mm.', 'numeric', true, 1
FROM stage_types st WHERE st.code = 'CUT_FLUSH' ON CONFLICT DO NOTHING;

INSERT INTO stage_criteria ("stage_type_id", "code", "name", "description", "check_type", "required", "order_index")
SELECT st.id, 'FLUSH_02', 'Vị trí cắt chính xác', 'Theo đúng bản vẽ.', 'numeric', true, 2
FROM stage_types st WHERE st.code = 'CUT_FLUSH' ON CONFLICT DO NOTHING;

INSERT INTO stage_criteria ("stage_type_id", "code", "name", "description", "check_type", "required", "order_index")
SELECT st.id, 'FLUSH_03', 'Bề mặt cắt nhẵn', 'Không ba via, không mẻ.', 'boolean', true, 3
FROM stage_types st WHERE st.code = 'CUT_FLUSH' ON CONFLICT DO NOTHING;

-- =========================
-- FINISH_SILICON – Hoàn thiện silicon (10)
-- =========================
INSERT INTO stage_criteria ("stage_type_id", "code", "name", "description", "check_type", "required", "order_index")
SELECT st.id, 'SILICON_01', 'Đường keo thẳng & đều', 'Không phình, không lõm.', 'boolean', true, 1
FROM stage_types st WHERE st.code = 'FINISH_SILICON' ON CONFLICT DO NOTHING;

INSERT INTO stage_criteria ("stage_type_id", "code", "name", "description", "check_type", "required", "order_index")
SELECT st.id, 'SILICON_02', 'Không hở keo', 'Đảm bảo độ kín.', 'boolean', true, 2
FROM stage_types st WHERE st.code = 'FINISH_SILICON' ON CONFLICT DO NOTHING;

INSERT INTO stage_criteria ("stage_type_id", "code", "name", "description", "check_type", "required", "order_index")
SELECT st.id, 'SILICON_03', 'Giáp nối mượt', 'Không gợn hoặc ngắt quãng.', 'boolean', true, 3
FROM stage_types st WHERE st.code = 'FINISH_SILICON' ON CONFLICT DO NOTHING;

-- Default global stage time settings
INSERT INTO global_stage_time_settings ("stage_type_id", "setup_minutes", "finish_minutes", "cut_al_minutes_per_unit", "mill_lock_minutes_per_group", "corner_cut_minutes_per_corner", "assemble_frame_minutes_per_corner", "cut_glass_minutes_per_square_meter", "glass_install_minutes_per_unit", "gasket_minutes_per_unit", "accessory_minutes_per_unit", "cut_flush_minutes_per_unit", "silicon_minutes_per_meter")
SELECT st.id, 3.0, 1.0, 0.5, 2.0, 1.5, 2.0, 3.0, 2.0, 1.0, 1.5, 0.8, 0.5
FROM stage_types st
WHERE st.is_default = true
ON CONFLICT (stage_type_id) DO NOTHING;

-- Default global labor cost settings (unified rate for all stages)
INSERT INTO global_labor_cost_settings ("hourly_rate")
VALUES (100000)
ON CONFLICT DO NOTHING;