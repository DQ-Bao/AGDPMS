set client_encoding to 'utf8';

drop table if exists quotation_settings;
drop table if exists notifications;
drop table if exists global_stage_time_settings;
drop table if exists global_labor_cost_settings;
drop table if exists production_order_settings;
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
    "stock_id" integer not null references material_stock("id"),
	"voucher_code" varchar(250) not null,
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

create table if not exists notifications (
    "id" serial primary key,
    "message" varchar(512) not null,
    "url" varchar(512) not null,
    "created_at" timestamp default now(),
    "is_read" boolean not null default false,
    "user_id" int references users ("id"),
    "role_id" int references roles ("id")
);

create table if not exists quotation_settings (
    "id" serial primary key,
    "project_id" int not null unique references projects ("id"),
    "labor_cost" int not null default 0,
    "profit_percentage" numeric(10,3) not null default 0,
    "tax_percentage" numeric(10,3) not null default 0,
    "transport_cost" int not null default 0,
    "contingency" int not null default 0
);

-- INSERT --
insert into roles ("name")
values ('Director'), ('Technician'), ('Sale'), ('InventoryManager'), ('QA'), ('Production Manager');

insert into users ("fullname", "phone", "password_hash", "role_id", "need_change_password")
values 
('Doãn Quốc Bảo', '0382633428', 'AQAAAAIAAYagAAAAEC7iGEcwGcYC51eb2ijKCRyIa18U40iGykiY27MJ06+6UzKwx/heauSLbMSeFifZag==', 1, false), -- abcd1234
('Tech 1', '0765432198', 'AQAAAAIAAYagAAAAEHoCG5WPIdq51mQm0+bDxfE4iUfQaADce3Jb3bsSAOU6AaJ2V52pl/g0ZbYX4bh9WQ==', 2, false), -- abcd1234
('Nguyễn Bảo Khánh', '0966699704', 'AQAAAAIAAYagAAAAEJHkv+A5U/7Q7OnAKH+XwNUirexztf3hXIhV6er5Ec3xvpEXqBzqFostupbw+5H4Uw==', 6, false), -- 123456
('Inventory 1', '0654321987', 'AQAAAAIAAYagAAAAEL9X4EICrXS/tsMWnuTa93rauL1TLY1K2312Ob2Pi/a0SDuIUF0hEAA/RN5m2U5noA==', 4, false), -- abcd1234
('QA Tester 0', '0900000000', 'AQAAAAIAAYagAAAAEJHkv+A5U/7Q7OnAKH+XwNUirexztf3hXIhV6er5Ec3xvpEXqBzqFostupbw+5H4Uw==', 5, false), -- 123456
('QA Tester 1', '0876543219', 'AQAAAAIAAYagAAAAEF38pt9JBNPP8NtWZNmAKVd0Lbo/Gtxw9qcMc4Eekf7uJBl8DpyQZNNS8190RJcKbg==', 5, false), -- 123456
('QA Tester 2', '0900000002', 'AQAAAAIAAYagAAAAEJHkv+A5U/7Q7OnAKH+XwNUirexztf3hXIhV6er5Ec3xvpEXqBzqFostupbw+5H4Uw==', 5, false), -- 123456
('QA Tester 3', '0900000003', 'AQAAAAIAAYagAAAAEJHkv+A5U/7Q7OnAKH+XwNUirexztf3hXIhV6er5Ec3xvpEXqBzqFostupbw+5H4Uw==', 5, false), -- 123456
('Sale 1', '0123456789', 'AQAAAAIAAYagAAAAEBuje+go7HqKeZ1XDzGMSqfAjT97yu012mQ7rSw5QNPcF8ftrCM5yFB9SxgF/l1LbQ==', 3, false), -- abcd1234
('Sale', '0231232323', 'AQAAAAIAAYagAAAAEJHkv+A5U/7Q7OnAKH+XwNUirexztf3hXIhV6er5Ec3xvpEXqBzqFostupbw+5H4Uw==', 3, false); -- 123456

insert into material_type ("name")
values ('Nhôm'), ('Kính'), ('Phụ kiện'), ('Roăng'), ('Vật tư phụ');

INSERT INTO materials (id, name, type, weight) VALUES ('C3328', 'Khung cửa đi', 1, '1.257');
INSERT INTO materials (id, name, type, weight) VALUES ('C3303', 'Cánh cửa đi mở ngoài (có gân)', 1, '1.441');
INSERT INTO materials (id, name, type, weight) VALUES ('C18772', 'Cánh cửa đi mở ngoài (không gân)', 1, '1.431');
INSERT INTO materials (id, name, type, weight) VALUES ('C3332', 'Cánh cửa đi mở trong (có gân)', 1, '1.442');
INSERT INTO materials (id, name, type, weight) VALUES ('C18782', 'Cánh cửa đi mở trong (không gân)', 1, '1.431');
INSERT INTO materials (id, name, type, weight) VALUES ('C3322', 'Cánh cửa đi mở ngoài (có gân & bo cạnh)', 1, '1.507');
INSERT INTO materials (id, name, type, weight) VALUES ('C22912', 'Cánh cửa đi mở ngoài (không gân & bo cạnh)', 1, '1.496');
INSERT INTO materials (id, name, type, weight) VALUES ('C38032', 'Cánh cửa đi ngang dưới (có gân)', 1, '2.260');
INSERT INTO materials (id, name, type, weight) VALUES ('C3304', 'Cánh cửa đi ngang dưới (có gân)', 1, '2.023');
INSERT INTO materials (id, name, type, weight) VALUES ('C6614', 'Cánh cửa đi ngang dưới (không gân)', 1, '2.014');
INSERT INTO materials (id, name, type, weight) VALUES ('C3323', 'Đố động cửa đi', 1, '1.086');
INSERT INTO materials (id, name, type, weight) VALUES ('C22903', 'Đố động cửa đi và cửa sổ', 1, '0.891');
INSERT INTO materials (id, name, type, weight) VALUES ('C22900', 'Ốp đáy cánh cửa đi', 1, '0.476');
INSERT INTO materials (id, name, type, weight) VALUES ('C3329', 'Ốp đáy cánh cửa đi', 1, '0.428');
INSERT INTO materials (id, name, type, weight) VALUES ('C3319', 'Ngưỡng cửa đi', 1, '0.689');
INSERT INTO materials (id, name, type, weight) VALUES ('C3291', 'Nẹp kính', 1, '0.206');
INSERT INTO materials (id, name, type, weight) VALUES ('C3225', 'Nẹp kính', 1, '0.211');
INSERT INTO materials (id, name, type, weight) VALUES ('C3296', 'Nẹp kính', 1, '0.237');
INSERT INTO materials (id, name, type, weight) VALUES ('F347', 'Ke góc', 5, '4.957');
INSERT INTO materials (id, name, type, weight) VALUES ('C3246', 'Nẹp kính', 1, '0.216');
INSERT INTO materials (id, name, type, weight) VALUES ('C3286', 'Nẹp kính', 1, '0.223');
INSERT INTO materials (id, name, type, weight) VALUES ('C3236', 'Nẹp kính', 1, '0.227');
INSERT INTO materials (id, name, type, weight) VALUES ('C3206', 'Nẹp kính', 1, '0.257');
INSERT INTO materials (id, name, type, weight) VALUES ('C3295', 'Nẹp kính', 1, '0.271');
INSERT INTO materials (id, name, type, weight) VALUES ('C3318', 'Khung cửa sổ', 1, '0.876');
INSERT INTO materials (id, name, type, weight) VALUES ('C8092', 'Cánh cửa sổ mở ngoài (có gân)', 1, '1.064');
INSERT INTO materials (id, name, type, weight) VALUES ('C3202', 'Cánh cửa sổ mở ngoài (có gân)', 1, '1.088');
INSERT INTO materials (id, name, type, weight) VALUES ('C18762', 'Cánh cửa sổ mở ngoài (không gân)', 1, '1.081');
INSERT INTO materials (id, name, type, weight) VALUES ('C3312', 'Cánh cửa sổ mở ngoài (có gân & bo cạnh)', 1, '1.159');
INSERT INTO materials (id, name, type, weight) VALUES ('C22922', 'Cánh cửa sổ mở ngoài (không gân & bo cạnh', 1, '1.118');
INSERT INTO materials (id, name, type, weight) VALUES ('C3033', 'Đố động cửa sổ', 1, '0.825');
INSERT INTO materials (id, name, type, weight) VALUES ('C3313', 'Đố cố định trên khung', 1, '1.126');
INSERT INTO materials (id, name, type, weight) VALUES ('C3209', 'Khung vách kính', 1, '0.876');
INSERT INTO materials (id, name, type, weight) VALUES ('C3203', 'Đố cố định (có lỗ vít)', 1, '0.314');
INSERT INTO materials (id, name, type, weight) VALUES ('F077', 'Pano', 1, '0.664');
INSERT INTO materials (id, name, type, weight) VALUES ('E1283', 'Khung lá sách', 1, '0.290');
INSERT INTO materials (id, name, type, weight) VALUES ('E192', 'Lá sách', 1, '0.317');
INSERT INTO materials (id, name, type, weight) VALUES ('B507', 'Nan dán trang trí', 1, '0.150');
INSERT INTO materials (id, name, type, weight) VALUES ('C3300', 'Nối khung', 1, '0.347');
INSERT INTO materials (id, name, type, weight) VALUES ('C3310', 'Nối khung', 1, '1.308');
INSERT INTO materials (id, name, type, weight) VALUES ('C3210', 'Nối khung 90 độ (bo cạnh)', 1, '1.275');
INSERT INTO materials (id, name, type, weight) VALUES ('C920', 'Nối khung 90 độ (vuông cạnh)', 1, '1.126');
INSERT INTO materials (id, name, type, weight) VALUES ('C910', 'Nối khung 135 độ', 1, '0.916');
INSERT INTO materials (id, name, type, weight) VALUES ('C459', 'Thanh truyền khóa', 1, '0.139');
INSERT INTO materials (id, name, type, weight) VALUES ('C3317', 'Pát liên kết (đố cố định với Fix)', 1, '1.105');
INSERT INTO materials (id, name, type, weight) VALUES ('C3207', 'Pát liên kết (đố cố định với Fix)', 1, '1.154');
INSERT INTO materials (id, name, type, weight) VALUES ('C1687', 'Ke góc', 5, '3.134');
INSERT INTO materials (id, name, type, weight) VALUES ('C4137', 'Ke góc', 5, '1.879');
INSERT INTO materials (id, name, type, weight) VALUES ('C1697', 'Ke góc', 5, '2.436');
INSERT INTO materials (id, name, type, weight) VALUES ('C38019', 'Khung cửa đi bản 100', 1, '2.057');
INSERT INTO materials (id, name, type, weight) VALUES ('C38038', 'Khung cửa sổ bản 100', 1, '1.421');
INSERT INTO materials (id, name, type, weight) VALUES ('C38039', 'Khung vách kính bản 100 (loại 1 nẹp)', 1, '1.375');
INSERT INTO materials (id, name, type, weight) VALUES ('C48949', 'Khung vách kính bản 100 (loại 2 nẹp)', 1, '1.272');
INSERT INTO materials (id, name, type, weight) VALUES ('C48954', 'Đố cố định bản 100 (loại 1 nẹp)', 1, '1.521');
INSERT INTO materials (id, name, type, weight) VALUES ('C48953', 'Đố cố định bản 100 (loại 2 nẹp)', 1, '1.405');
INSERT INTO materials (id, name, type, weight) VALUES ('C38010', 'Nối khung bản 100', 1, '0.617');
INSERT INTO materials (id, name, type, weight) VALUES ('C48980', 'Nối khung 90 độ bản 100', 1, '2.090');
INSERT INTO materials (id, name, type, weight) VALUES ('C48945', 'Nẹp phụ bản 100', 1, '0.346');
INSERT INTO materials (id, name, type, weight) VALUES ('CX283', 'Khung cửa đi', 1, '1.533');
INSERT INTO materials (id, name, type, weight) VALUES ('CX281', 'Cánh cửa đi mở ngoài', 1, '1.839');
INSERT INTO materials (id, name, type, weight) VALUES ('CX282', 'Cánh ngang dưới cửa đi', 1, '3.033');
INSERT INTO materials (id, name, type, weight) VALUES ('CX568', 'Đố động cửa đi', 1, '1.195');
INSERT INTO materials (id, name, type, weight) VALUES ('CX309', 'Nối khung', 1, '0.427');
INSERT INTO materials (id, name, type, weight) VALUES ('CX267', 'Khung cửa sổ, vách kính', 1, '1.057');
INSERT INTO materials (id, name, type, weight) VALUES ('CX264', 'Cánh cửa sổ mở ngoài', 1, '1.419');
INSERT INTO materials (id, name, type, weight) VALUES ('CX750', 'Đố động cửa sổ', 1, '0.985');
INSERT INTO materials (id, name, type, weight) VALUES ('CX266', 'Đố cố định (có lỗ vít)', 1, '1.233');
INSERT INTO materials (id, name, type, weight) VALUES ('CX265', 'Đố cố định (không lỗ vít)', 1, '1.163');
INSERT INTO materials (id, name, type, weight) VALUES ('C25899', 'Khung bao chuyển hướng', 1, '0.727');
INSERT INTO materials (id, name, type, weight) VALUES ('CX311', 'Nối khung vách kính', 1, '1.461');
INSERT INTO materials (id, name, type, weight) VALUES ('CX310', 'Thanh nối góc 90 độ', 1, '1.614');
INSERT INTO materials (id, name, type, weight) VALUES ('C1757', 'Ke góc', 5, '2.167');
INSERT INTO materials (id, name, type, weight) VALUES ('C40988', 'Khung cửa đi và cửa sổ', 1, '0.862');
INSERT INTO materials (id, name, type, weight) VALUES ('C48952', 'Cánh cửa đi vát cạnh bằng', 1, '0.991');
INSERT INTO materials (id, name, type, weight) VALUES ('C40912', 'Cánh cửa đi vát cạnh lệch', 1, '1.008');
INSERT INTO materials (id, name, type, weight) VALUES ('C48942', 'Cánh cửa sổ vát cạnh bằng', 1, '0.908');
INSERT INTO materials (id, name, type, weight) VALUES ('C40902', 'Cánh cửa sổ vát cạnh lệch', 1, '0.924');
INSERT INTO materials (id, name, type, weight) VALUES ('C40983', 'Đố cố định vát cạnh bằng', 1, '0.977');
INSERT INTO materials (id, name, type, weight) VALUES ('C40984', 'Đố cố định vát cạnh lệch', 1, '1.021');
INSERT INTO materials (id, name, type, weight) VALUES ('C44249', 'Khung vách kính', 1, '0.753');
INSERT INTO materials (id, name, type, weight) VALUES ('C44234', 'Đố cố định (có lỗ vít)', 1, '0.857');
INSERT INTO materials (id, name, type, weight) VALUES ('C40869', 'Đố động cửa đi và cửa sổ', 1, '0.701');
INSERT INTO materials (id, name, type, weight) VALUES ('C40973', 'Đố cố định trên khung', 1, '0.860');
INSERT INTO materials (id, name, type, weight) VALUES ('C40978', 'Ốp đáy cánh cửa đi', 1, '0.375');
INSERT INTO materials (id, name, type, weight) VALUES ('E17523', 'Pano', 1, '0.605');
INSERT INTO materials (id, name, type, weight) VALUES ('C44226', 'Nẹp kính', 1, '0.199');
INSERT INTO materials (id, name, type, weight) VALUES ('C40979', 'Nẹp kính', 1, '0.218');
INSERT INTO materials (id, name, type, weight) VALUES ('F605', 'Khung ngang trên', 1, '3.107');
INSERT INTO materials (id, name, type, weight) VALUES ('F606', 'Khung đứng', 1, '1.027');
INSERT INTO materials (id, name, type, weight) VALUES ('F4116', 'Khung đứng (khoá đa điểm)', 1, '1.056');
INSERT INTO materials (id, name, type, weight) VALUES ('F607', 'Khung ngang dưới (ray nổi)', 1, '1.053');
INSERT INTO materials (id, name, type, weight) VALUES ('F2435', 'Khung ngang dưới (ray âm)', 1, '1.351');
INSERT INTO materials (id, name, type, weight) VALUES ('F523', 'Cánh cửa không lỗ vít (khoá đa điểm)', 1, '1.254');
INSERT INTO materials (id, name, type, weight) VALUES ('F4117', 'Cánh cửa không lỗ vít (khoá đa điểm)', 1, '1.269');
INSERT INTO materials (id, name, type, weight) VALUES ('F5017', 'Cánh cửa không lỗ vít (khoá đa điểm)', 1, '1.307');
INSERT INTO materials (id, name, type, weight) VALUES ('F522', 'Cánh cửa có lỗ vít (khoá đơn điểm)', 1, '1.336');
INSERT INTO materials (id, name, type, weight) VALUES ('F560', 'Đố cố định trên cánh', 1, '1.142');
INSERT INTO materials (id, name, type, weight) VALUES ('F520', 'Ốp giữa 2 cánh mở', 1, '0.241');
INSERT INTO materials (id, name, type, weight) VALUES ('F519', 'Ốp che nước mưa', 1, '0.177');
INSERT INTO materials (id, name, type, weight) VALUES ('F6029', 'Nẹp kính', 1, '0.276');
INSERT INTO materials (id, name, type, weight) VALUES ('F521', 'Nẹp kính', 1, '0.222');
INSERT INTO materials (id, name, type, weight) VALUES ('F608', 'Ke liên kết khung đứng với ngang trên', 5, '1.440');
INSERT INTO materials (id, name, type, weight) VALUES ('F609', 'Ke liên kết khung đứng với ngang dưới', 5, '1.377');
INSERT INTO materials (id, name, type, weight) VALUES ('F417', 'Ke góc', 5, '5.228');
INSERT INTO materials (id, name, type, weight) VALUES ('D23151', 'Khung cửa lùa', 1, '0.949');
INSERT INTO materials (id, name, type, weight) VALUES ('D45482', 'Khung cửa lùa 3 ray', 1, '1.414');
INSERT INTO materials (id, name, type, weight) VALUES ('D23156', 'Cánh cửa lùa', 1, '0.936');
INSERT INTO materials (id, name, type, weight) VALUES ('D23157', 'Ốp cánh cửa lùa', 1, '0.365');
INSERT INTO materials (id, name, type, weight) VALUES ('D23158', 'Nẹp đối đầu cửa 4 cánh', 1, '0.229');
INSERT INTO materials (id, name, type, weight) VALUES ('D23159', 'Ốp che nước mưa', 1, '0.279');
INSERT INTO materials (id, name, type, weight) VALUES ('D44329', 'Khung cửa lùa', 1, '0.885');
INSERT INTO materials (id, name, type, weight) VALUES ('D44035', 'Cánh cửa mở lùa', 1, '0.827');
INSERT INTO materials (id, name, type, weight) VALUES ('D44327', 'Ốp cánh cửa lùa', 1, '0.315');
INSERT INTO materials (id, name, type, weight) VALUES ('D44328', 'Nẹp đối đầu cửa 4 cánh', 1, '0.396');
INSERT INTO materials (id, name, type, weight) VALUES ('D47713', 'Khung cửa lùa', 1, '1.223');
INSERT INTO materials (id, name, type, weight) VALUES ('D45316', 'Cánh cửa mở lùa', 1, '1.319');
INSERT INTO materials (id, name, type, weight) VALUES ('D44564', 'Cánh cửa mở lùa', 1, '1.201');
INSERT INTO materials (id, name, type, weight) VALUES ('D47688', 'Nẹp đối đầu cửa 4 cánh', 1, '0.545');
INSERT INTO materials (id, name, type, weight) VALUES ('D46070', 'Ốp khóa đa điểm', 1, '0.364');
INSERT INTO materials (id, name, type, weight) VALUES ('D47679', 'Ốp đậy ray', 1, '0.096');
INSERT INTO materials (id, name, type, weight) VALUES ('D47678', 'Ốp đậy rãnh phụ kiện', 1, '0.073');
INSERT INTO materials (id, name, type, weight) VALUES ('D45478', 'Thanh ốp móc', 1, '0.383');
INSERT INTO materials (id, name, type, weight) VALUES ('D44569', 'Nẹp kính', 1, '0.199');
INSERT INTO materials (id, name, type, weight) VALUES ('D1541A', 'Khung ngang trên', 1, '1.459');
INSERT INTO materials (id, name, type, weight) VALUES ('D1551A', 'Đố chia cửa lùa với vách kính trên', 1, '2.164');
INSERT INTO materials (id, name, type, weight) VALUES ('D17182', 'Khung ngang dưới (ray bằng)', 1, '1.307');
INSERT INTO materials (id, name, type, weight) VALUES ('D1942', 'Khung ngang dưới (ray lệch)', 1, '1.561');
INSERT INTO materials (id, name, type, weight) VALUES ('D1542A', 'Khung ngang dưới (ray lệch)', 1, '1.706');
INSERT INTO materials (id, name, type, weight) VALUES ('D1543A', 'Khung đứng', 1, '1.134');
INSERT INTO materials (id, name, type, weight) VALUES ('D3213', 'Khung đứng (3 ray)', 1, '1.367');
INSERT INTO materials (id, name, type, weight) VALUES ('D3211', 'Khung ngang trên (3 ray)', 1, '1.959');
INSERT INTO materials (id, name, type, weight) VALUES ('D3212', 'Khung ngang dưới (3 ray)', 1, '2.295');
INSERT INTO materials (id, name, type, weight) VALUES ('D1544A', 'Cánh ngang trên', 1, '0.990');
INSERT INTO materials (id, name, type, weight) VALUES ('D1545A', 'Cánh ngang dưới', 1, '1.000');
INSERT INTO materials (id, name, type, weight) VALUES ('D1546A', 'Cánh đứng trơn', 1, '1.273');
INSERT INTO materials (id, name, type, weight) VALUES ('D1547A', 'Cánh đứng móc', 1, '1.098');
INSERT INTO materials (id, name, type, weight) VALUES ('D28144', 'Cánh ngang trên', 1, '1.115');
INSERT INTO materials (id, name, type, weight) VALUES ('D1555A', 'Cánh ngang dưới', 1, '1.243');
INSERT INTO materials (id, name, type, weight) VALUES ('D26146', 'Cánh đứng trơn', 1, '1.330');
INSERT INTO materials (id, name, type, weight) VALUES ('D28127', 'Cánh đứng móc', 1, '1.303');
INSERT INTO materials (id, name, type, weight) VALUES ('D1559A', 'Khung đứng vách kính', 1, '1.070');
INSERT INTO materials (id, name, type, weight) VALUES ('D2618', 'Đố cố định trên vách kính', 1, '1.546');
INSERT INTO materials (id, name, type, weight) VALUES ('D1354', 'Đố cố định trên cánh', 1, '0.696');
INSERT INTO materials (id, name, type, weight) VALUES ('D1548A', 'Nẹp đối đầu cửa 4 cánh', 1, '0.620');
INSERT INTO materials (id, name, type, weight) VALUES ('D1549A', 'Ốp khung vách kính', 1, '0.712');
INSERT INTO materials (id, name, type, weight) VALUES ('D1578', 'Nối khung vách kính', 1, '0.676');
INSERT INTO materials (id, name, type, weight) VALUES ('D2420', 'Nối góc 90 độ trái', 1, '2.292');
INSERT INTO materials (id, name, type, weight) VALUES ('D2490', 'Nối góc 90 độ phải', 1, '2.292');
INSERT INTO materials (id, name, type, weight) VALUES ('D34608', 'Thanh chuyển kính hộp', 1, '0.399');
INSERT INTO materials (id, name, type, weight) VALUES ('D1779', 'Nẹp kính', 1, '0.100');
INSERT INTO materials (id, name, type, weight) VALUES ('D1298', 'Nẹp kính', 1, '0.109');
INSERT INTO materials (id, name, type, weight) VALUES ('D1168', 'Nẹp kính', 1, '0.130');
INSERT INTO materials (id, name, type, weight) VALUES ('C101', 'Nẹp kính', 1, '0.133');
INSERT INTO materials (id, name, type, weight) VALUES ('F631', 'Cánh đứng', 1, '2.570');
INSERT INTO materials (id, name, type, weight) VALUES ('F632', 'Cánh ngang trên', 1, '2.382');
INSERT INTO materials (id, name, type, weight) VALUES ('F633', 'Cánh ngang dưới', 1, '2.382');
INSERT INTO materials (id, name, type, weight) VALUES ('F2084', 'Đố tĩnh', 1, '2.278');
INSERT INTO materials (id, name, type, weight) VALUES ('F630', 'Nẹp kính', 1, '0.173');
INSERT INTO materials (id, name, type, weight) VALUES ('F949', 'Nẹp kính', 1, '0.176');
INSERT INTO materials (id, name, type, weight) VALUES ('D47680', 'Ngưỡng nhôm', 1, '0.408');
INSERT INTO materials (id, name, type, weight) VALUES ('A1079', 'Nẹp lưới chống muỗi', 1, '0.087');
INSERT INTO materials (id, name, type, weight) VALUES ('A1080', 'Nẹp lưới chống muỗi', 1, '0.087');
INSERT INTO materials (id, name, type, weight) VALUES ('D47590', 'Ray nhôm cho cửa nhựa', 1, '0.040');
INSERT INTO materials (id, name, type, weight) VALUES ('GK461', 'Thanh đố đứng', 1, '2.138');
INSERT INTO materials (id, name, type, weight) VALUES ('GK471', 'Thanh đố đứng', 1, '2.281');
INSERT INTO materials (id, name, type, weight) VALUES ('GK481', 'Thanh đố đứng', 1, '2.424');
INSERT INTO materials (id, name, type, weight) VALUES ('GK491', 'Thanh đố đứng', 1, '2.567');
INSERT INTO materials (id, name, type, weight) VALUES ('GK501', 'Thanh đố đứng', 1, '2.711');
INSERT INTO materials (id, name, type, weight) VALUES ('E21451', 'Thanh đố đứng', 1, '2.347');
INSERT INTO materials (id, name, type, weight) VALUES ('GK581', 'Thanh đố đứng (kính hộp)', 1, '2.730');
INSERT INTO materials (id, name, type, weight) VALUES ('GK993', 'Thanh đố ngang', 1, '1.908');
INSERT INTO materials (id, name, type, weight) VALUES ('GK2053', 'Thanh đố ngang', 1, '1.863');
INSERT INTO materials (id, name, type, weight) VALUES ('GK2467', 'Thanh nêm đố ngang', 1, '0.304');
INSERT INTO materials (id, name, type, weight) VALUES ('GK858', 'Pat liên kết thang ngang', 1, '1.218');
INSERT INTO materials (id, name, type, weight) VALUES ('GK1073', 'Nắp đậy đố ngang', 1, '0.292');
INSERT INTO materials (id, name, type, weight) VALUES ('GK015', 'Đế ốp mặt ngoài', 1, '0.577');
INSERT INTO materials (id, name, type, weight) VALUES ('GK066', 'Nắp đậy đế ốp', 1, '0.404');
INSERT INTO materials (id, name, type, weight) VALUES ('GK780', 'Nối góc 90 độ ngoài', 1, '0.743');
INSERT INTO materials (id, name, type, weight) VALUES ('GK1495', 'Đế ốp mặt ngoài góc 90 độ', 1, '1.110');
INSERT INTO materials (id, name, type, weight) VALUES ('GK806', 'Nắp đậy đế ốp góc 90 độ', 1, '1.721');
INSERT INTO materials (id, name, type, weight) VALUES ('GK1035', 'Đế ốp mặt ngoài góc 135 độ', 1, '0.743');
INSERT INTO materials (id, name, type, weight) VALUES ('GK606', 'Nắp đậy đế ốp góc 135 độ', 1, '0.675');
INSERT INTO materials (id, name, type, weight) VALUES ('GK294', 'Nắp đậy che rãnh', 1, '0.138');
INSERT INTO materials (id, name, type, weight) VALUES ('GK2464', 'Nắp đậy khe rãnh', 1, '0.264');
INSERT INTO materials (id, name, type, weight) VALUES ('GK1255', 'Khung trên cửa sổ (dạng móc treo)', 1, '0.918');
INSERT INTO materials (id, name, type, weight) VALUES ('GK1325', 'Cánh trên cửa sổ (dạng móc treo)', 1, '0.791');
INSERT INTO materials (id, name, type, weight) VALUES ('GK1295', 'Khung cửa sổ', 1, '0.751');
INSERT INTO materials (id, name, type, weight) VALUES ('GK1365', 'Cánh cửa sổ', 1, '0.801');
INSERT INTO materials (id, name, type, weight) VALUES ('GK505', 'Thanh đố kính cho cánh cửa sổ', 1, '0.959');
INSERT INTO materials (id, name, type, weight) VALUES ('GK1215', 'Ke cửa sổ', 5, '0.959');
INSERT INTO materials (id, name, type, weight) VALUES ('GK001', 'Thanh đố đứng', 1, '1.923');
INSERT INTO materials (id, name, type, weight) VALUES ('GK011', 'Thanh đố đứng', 1, '2.211');
INSERT INTO materials (id, name, type, weight) VALUES ('GK021', 'Thanh đố đứng', 1, '2.495');
INSERT INTO materials (id, name, type, weight) VALUES ('GK251', 'Thanh đố đứng', 1, '2.638');
INSERT INTO materials (id, name, type, weight) VALUES ('GK261', 'Thanh đố đứng', 1, '3.051');
INSERT INTO materials (id, name, type, weight) VALUES ('GK813', 'Thanh đố ngang', 1, '1.733');
INSERT INTO materials (id, name, type, weight) VALUES ('GK853', 'Thanh đố ngang', 1, '1.757');
INSERT INTO materials (id, name, type, weight) VALUES ('GK413', 'Nắp đậy thanh đố ngang', 1, '0.217');
INSERT INTO materials (id, name, type, weight) VALUES ('GK1745', 'Pat liên kết thanh đố ngang', 1, '1.173');
INSERT INTO materials (id, name, type, weight) VALUES ('GK228', 'Nẹp kính trái', 1, '0.356');
INSERT INTO materials (id, name, type, weight) VALUES ('GK238', 'Nẹp kính phải', 1, '0.294');
INSERT INTO materials (id, name, type, weight) VALUES ('GK218', 'Nẹp kính trên', 1, '0.437');
INSERT INTO materials (id, name, type, weight) VALUES ('GK208', 'Nẹp kính dưới', 1, '0.383');
INSERT INTO materials (id, name, type, weight) VALUES ('GK255', 'Thanh móc treo kính', 1, '0.436');
INSERT INTO materials (id, name, type, weight) VALUES ('GK275', 'Thanh đố kính', 1, '0.245');
INSERT INTO materials (id, name, type, weight) VALUES ('GK1064', 'Chống nhấc cánh', 1, '0.257');
INSERT INTO materials (id, name, type, weight) VALUES ('GK534', 'Thanh đỡ kính cho cánh cửa sổ', 1, '0.195');
INSERT INTO materials (id, name, type, weight) VALUES ('GK454', 'Máng che cánh cửa sổ', 1, '0.288');
INSERT INTO materials (id, name, type, weight) VALUES ('E1214', 'Khung bao ngang trên', 1, '1.795');
INSERT INTO materials (id, name, type, weight) VALUES ('E1215', 'Khung bao dưới', 1, '0.976');
INSERT INTO materials (id, name, type, weight) VALUES ('E1216', 'Đố Lan Can', 1, '1.131');
INSERT INTO materials (id, name, type, weight) VALUES ('B1735', 'Đố Lan Can', 1, '1.347');
INSERT INTO materials (id, name, type, weight) VALUES ('E1217', 'Nối góc 90 độ', 1, '1.453');
INSERT INTO materials (id, name, type, weight) VALUES ('E1218', 'Nắp đậy che rãnh', 1, '0.110');
INSERT INTO materials (id, name, type, weight) VALUES ('B2831', 'Khung bao ngang trên', 1, '1.402');
INSERT INTO materials (id, name, type, weight) VALUES ('B2832', 'Nắp đậy che rãnh', 1, '0.177');
INSERT INTO materials (id, name, type, weight) VALUES ('B2846', 'Khung đứng', 1, '1.081');
INSERT INTO materials (id, name, type, weight) VALUES ('B2833', 'Đố Lan Can', 1, '1.404');
INSERT INTO materials (id, name, type, weight) VALUES ('B2834', 'Nối góc 90 độ', 1, '1.617');
INSERT INTO materials (id, name, type, weight) VALUES ('B2835', 'Nắp đậy rãnh khung đứng', 1, '0.109');
INSERT INTO materials (id, name, type, weight) VALUES ('B4425', 'Khung bao ngang trên', 1, '1.453');
INSERT INTO materials (id, name, type, weight) VALUES ('B4426', 'Nẹp kính', 1, '0.155');
INSERT INTO materials (id, name, type, weight) VALUES ('B4429', 'Khung bao đứng', 1, '0.765');
INSERT INTO materials (id, name, type, weight) VALUES ('B4428', 'Đố lan can', 1, '0.932');
INSERT INTO materials (id, name, type, weight) VALUES ('B4430', 'Nẹp kính', 1, '0.153');
INSERT INTO materials (id, name, type, weight) VALUES ('B4427', 'Nối góc 90 độ', 1, '1.197');
INSERT INTO materials (id, name, type, weight) VALUES ('B3730', 'Khung bao ngang trên', 1, '1.128');
INSERT INTO materials (id, name, type, weight) VALUES ('B3731', 'Đố đứng', 1, '0.920');
INSERT INTO materials (id, name, type, weight) VALUES ('B3732', 'Khung đứng', 1, '0.689');
INSERT INTO materials (id, name, type, weight) VALUES ('B3733', 'Nẹp kính', 1, '0.136');
INSERT INTO materials (id, name, type, weight) VALUES ('G-KHUNG', 'Gioang KHUNG', 4, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('KI-EP027E', 'Gioang KINH', 4, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('NE-EP008E', 'Gioang NEP', 4, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('Kinhdon6', 'Kinh Trang 6.36mm', 2, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('ACC-220-(PMA)', 'Chot canh phu - 22mm', 3, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('ACC-500-(PMA)', 'Chot canh phu - 50mm', 3, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('AKDD-(PMA)', 'Bo khoa da diem', 3, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('ALK2C', 'O khoa 2 mat', 3, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('ATCD19-(PMA)', 'TTD cua chinh, (L1800)', 3, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('AKBC-(PMA)', 'Dau Khoa bien Cao', 3, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('ABL2D-(PMA)', 'Ban le 2D tieu chuan', 3, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('10*100', 'Vit+No lap dat', 3, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('ST4*40', 'Vit lap ghep', 5, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('ST4*60', 'Vit ghep khung', 5, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('G-Long', 'Gioang long', 4, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('A-1800-(PMA)', 'TTD so truot 1800', 3, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('AVCT-(PMA)', 'Vau chot truot', 3, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('ACST-(PMA)', 'Chot canh SAP', 3, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('CTC-(KINLONG)', 'Chong thao canh truot', 3, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('9.08.0064', 'Dem chong rung canh truot', 3, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('9.01.0048', 'Bo t/nam 2 mat khong o khoa (+01 truc)', 3, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('9.05.0041', 'Banh xe cua di truot', 3, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('F50300', 'Ke goc 2600/4400', 5, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('F50510', 'Gioang canh', 4, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('AVCD1C-(PMA)', 'Vau chot cua di', 3, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('F54', 'Gioang san', 4, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('Ke.TC', 'Ke tang cung', 5, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('TIBON', 'Keo lien ket chong nuoc', 5, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('ATN-(PMA)', 'Tay nam so ngoai (TC)', 3, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('AQN-400-(PMA)', 'TTD so ngoai 400', 3, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('AVC-1C-(PMA)', 'Vau chot cua so', 3, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('ABLA-10-(KINLONG)', 'Ban le  A 18*10" -', 3, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('LS152', 'Chong gio tu hoi 6"', 3, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('AQN-1200-(PMA)', 'TTD so ngoai 1200', 3, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('ABLA-16-(PMA)', 'Ban le A  22*16"', 3, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('9.08.0061-(PMA)', 'Bo nap day Do dong', 3, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('AQN-600-(PMA)', 'TTD so ngoai 600', 3, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('ACG-8-(PMA)', 'Chong gio tu hoi 8" - SUS 304', 3, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('A-1400-(PMA)', 'TTD so truot 1400', 3, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('ABXK-(PMA)', 'Banh xe don so truot', 3, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('ADCN-(PMA)', 'Chong thao canh truot', 3, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('Kenhay', 'Ke goc Canh + Khung', 5, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('AD30', 'Gioang khung/canh', 4, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('ST4.2*19', 'Vit phu kien', 3, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('Ketangcung', 'Ke tang cung', 5, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('Ni5*6', 'Gioang ni', 4, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('GDE35', 'Gioang kinh', 4, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('AQN-1000-(PMA)', 'TTD so ngoai 1000', 3, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('ABLA-14-(PMA)', 'Ban le A  22*14"', 3, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('AQN-1400-(PMA)', 'TTD so ngoai 1400', 3, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('ABLA-12-(PMA)', 'Ban le A  22*12"', 3, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('Kinhtoi10', 'Kinh Temper 10mm', 2, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('ACG-10-(PMA)', 'Chong gio tu hoi 10" - SUS 304', 3, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('F50501', 'Gioang ong', 4, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('Kinhdon5+6+5', 'Kinh hop 5+6+5', 2, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('NapDo(KINLONG)', 'Bo nap day Do dong', 3, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('AQN-800-(PMA)', 'TTD so ngoai 800', 3, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('ABXD-(PMA)', 'Banh xe doi so truot', 3, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('Banle-(KINLONG)', 'Ban le trung gian 4 canh', 3, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('ABLA-18-(PMA)', 'Ban le A 22*18"', 3, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('Kinhdan6', 'Kinh Dan 6.38mm MO', 2, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('A-1600-(PMA)', 'TTD so truot 1600', 3, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('AQN-1600-(PMA)', 'TTD so ngoai 1600', 3, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('ACG-12-(PMA)', 'Chong gio tu hoi 12" - SUS 304', 3, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('ABLA-10-(PMA)', 'Ban le A  22*10"', 3, NULL);
INSERT INTO materials (id, name, type, weight) VALUES ('C3204', 'DO chia di+so - C3204', 1, '1.000');
INSERT INTO materials (id, name, type, weight) VALUES ('VN04', 'Pa-no - XingFa', 1, '1.000');
INSERT INTO materials (id, name, type, weight) VALUES ('LS01', 'La Sach', 1, '1.000');
INSERT INTO materials (id, name, type, weight) VALUES ('C102', 'Khung bao La Sach', 1, '1.000');
INSERT INTO materials (id, name, type, weight) VALUES ('D1541', 'Khung TREN truot  -He 2001', 1, '1.000');
INSERT INTO materials (id, name, type, weight) VALUES ('C101A', 'Nep kinh vach HE -93', 1, '1.000');
INSERT INTO materials (id, name, type, weight) VALUES ('D1578A', 'Noi khung lien ket', 1, '1.000');
INSERT INTO materials (id, name, type, weight) VALUES ('D1549', 'Thanh Op - Khung DUNG -D 1549', 1, '1.000');
INSERT INTO materials (id, name, type, weight) VALUES ('F4410', 'Canh cua he 4400', 1, '1.000');
INSERT INTO materials (id, name, type, weight) VALUES ('F4482', 'HEM CANH QUAY 4400', 1, '1.000');
INSERT INTO materials (id, name, type, weight) VALUES ('F4587', 'Op day canh', 1, '1.000');
INSERT INTO materials (id, name, type, weight) VALUES ('F2656', 'Do chia canh cua so', 1, '1.000');
INSERT INTO materials (id, name, type, weight) VALUES ('F100*9', 'Pa-no S.HAL', 1, '1.000');
INSERT INTO materials (id, name, type, weight) VALUES ('F4420', 'Khung bao he 4400', 1, '1.000');
INSERT INTO materials (id, name, type, weight) VALUES ('F4405', 'Do chia vach he 4400', 1, '1.000');
INSERT INTO materials (id, name, type, weight) VALUES ('F5016', 'Nep kinh 5mm', 1, '1.000');
INSERT INTO materials (id, name, type, weight) VALUES ('D1548', 'Hem truot 4 canh', 1, '1.000');
INSERT INTO materials (id, name, type, weight) VALUES ('PMA-55101B', 'Khung bao di quay', 1, '1.000');
INSERT INTO materials (id, name, type, weight) VALUES ('PMA55201B', 'Canh di mo trong Khong NEP', 1, '1.000');
INSERT INTO materials (id, name, type, weight) VALUES ('PMA55503', 'Op chan Canh', 1, '1.000');
INSERT INTO materials (id, name, type, weight) VALUES ('PMA55402', 'Do tinh trong, ngoai', 1, '1.000');
INSERT INTO materials (id, name, type, weight) VALUES ('PMA55505', 'Nep kinh FIX 5mm >>10 mm', 1, '1.000');
INSERT INTO materials (id, name, type, weight) VALUES ('PMA55301B', 'Canh so mo ngoai Ko Nep', 1, '1.000');
INSERT INTO materials (id, name, type, weight) VALUES ('PMA55601', 'Khung truot', 1, '1.000');
INSERT INTO materials (id, name, type, weight) VALUES ('PMA55602', 'Canh truot', 1, '1.000');
INSERT INTO materials (id, name, type, weight) VALUES ('PMA55603', 'Hem truot', 1, '1.000');
INSERT INTO materials (id, name, type, weight) VALUES ('F450', 'Khung bao he 450', 1, '1.000');
INSERT INTO materials (id, name, type, weight) VALUES ('F4451', 'Canh cua he 450', 1, '1.000');
INSERT INTO materials (id, name, type, weight) VALUES ('PMA55403', 'DO DONG 2 Canh', 1, '1.000');
INSERT INTO materials (id, name, type, weight) VALUES ('F4504', 'Hem dong he 450', 1, '1.000');
INSERT INTO materials (id, name, type, weight) VALUES ('PMA55202', 'Canh di mo trong', 1, '1.000');
INSERT INTO materials (id, name, type, weight) VALUES ('PMA55302', 'Do tinh', 1, '1.000');
INSERT INTO materials (id, name, type, weight) VALUES ('PMA55501', 'Nep kinh 5mm >>10 mm', 1, '1.000');
INSERT INTO materials (id, name, type, weight) VALUES ('PP-95', 'Pa-no 95*20', 1, '1.000');
INSERT INTO materials (id, name, type, weight) VALUES ('PMA55302B', 'Do tinh', 1, '1.000');
INSERT INTO materials (id, name, type, weight) VALUES ('C3208', 'Khung tren VACH - C3208', 1, '1.000');
INSERT INTO materials (id, name, type, weight) VALUES ('PMA55301', 'Canh so mo ngoai Co Nep', 1, '1.000');
INSERT INTO materials (id, name, type, weight) VALUES ('PMA55604', 'Hem truot doi dau', 1, '1.000');
INSERT INTO materials (id, name, type, weight) VALUES ('F5026', 'Nep kinh 10mm', 1, '1.000');
INSERT INTO materials (id, name, type, weight) VALUES ('PMA55201', 'Canh di mo ngoai', 1, '1.000');
INSERT INTO materials (id, name, type, weight) VALUES ('SFD05', 'Khung cua DAY Duoi', 1, '1.000');
INSERT INTO materials (id, name, type, weight) VALUES ('SFD02', 'Khung cua Dung', 1, '1.000');
INSERT INTO materials (id, name, type, weight) VALUES ('SFD01', 'Khung cua DAY Tren', 1, '1.000');
INSERT INTO materials (id, name, type, weight) VALUES ('SFD03', 'Canh cua Di', 1, '1.000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (1, 'C3328', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (3, 'C3303', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (4, 'C18772', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (5, 'C3332', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (6, 'C18782', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (7, 'C3322', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (8, 'C22912', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (9, 'C38032', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (10, 'C3304', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (11, 'C6614', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (12, 'C3323', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (13, 'C22903', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (14, 'C22900', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (15, 'C3329', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (16, 'C3319', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (17, 'C3291', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (18, 'C3225', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (19, 'C3296', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (20, 'F347', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (21, 'C3246', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (22, 'C3286', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (23, 'C3236', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (24, 'C3206', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (25, 'C3295', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (26, 'C3318', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (27, 'C8092', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (28, 'C3202', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (29, 'C18762', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (30, 'C3312', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (31, 'C22922', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (32, 'C3033', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (33, 'C3313', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (34, 'C3209', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (35, 'C3203', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (36, 'F077', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (37, 'E1283', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (38, 'E192', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (39, 'B507', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (40, 'C3300', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (41, 'C3310', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (42, 'C3210', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (43, 'C920', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (44, 'C910', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (45, 'C459', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (46, 'C3317', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (47, 'C3207', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (48, 'C1687', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (49, 'C4137', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (50, 'C1697', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (51, 'C38019', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (52, 'C38038', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (53, 'C38039', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (54, 'C48949', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (55, 'C48954', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (56, 'C48953', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (57, 'C38010', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (58, 'C48980', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (59, 'C48945', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (60, 'CX283', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (61, 'CX281', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (62, 'CX282', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (63, 'CX568', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (64, 'CX309', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (65, 'CX267', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (66, 'CX264', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (67, 'CX750', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (68, 'CX266', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (69, 'CX265', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (70, 'C25899', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (71, 'CX311', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (72, 'CX310', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (73, 'C1757', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (74, 'C40988', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (75, 'C48952', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (76, 'C40912', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (77, 'C48942', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (78, 'C40902', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (79, 'C40983', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (80, 'C40984', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (81, 'C44249', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (82, 'C44234', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (83, 'C40869', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (84, 'C40973', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (85, 'C40978', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (86, 'E17523', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (87, 'C44226', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (88, 'C40979', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (89, 'F605', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (90, 'F606', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (91, 'F4116', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (92, 'F607', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (93, 'F2435', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (94, 'F523', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (95, 'F4117', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (96, 'F5017', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (97, 'F522', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (98, 'F560', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (99, 'F520', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (100, 'F519', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (101, 'F6029', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (102, 'F521', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (103, 'F608', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (104, 'F609', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (105, 'F417', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (106, 'D23151', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (107, 'D45482', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (108, 'D23156', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (109, 'D23157', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (110, 'D23158', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (111, 'D23159', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (112, 'D44329', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (113, 'D44035', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (114, 'D44327', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (115, 'D44328', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (116, 'D47713', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (117, 'D45316', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (118, 'D44564', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (119, 'D47688', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (120, 'D46070', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (121, 'D47679', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (122, 'D47678', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (123, 'D45478', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (124, 'D44569', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (125, 'D1541A', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (126, 'D1551A', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (127, 'D17182', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (128, 'D1942', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (129, 'D1542A', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (130, 'D1543A', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (131, 'D3213', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (132, 'D3211', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (133, 'D3212', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (134, 'D1544A', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (135, 'D1545A', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (136, 'D1546A', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (137, 'D1547A', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (138, 'D28144', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (139, 'D1555A', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (140, 'D26146', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (141, 'D28127', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (142, 'D1559A', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (143, 'D2618', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (144, 'D1354', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (145, 'D1548A', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (146, 'D1549A', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (147, 'D1578', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (148, 'D2420', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (149, 'D2490', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (150, 'D34608', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (151, 'D1779', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (152, 'D1298', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (153, 'D1168', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (154, 'C101', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (155, 'F631', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (156, 'F632', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (157, 'F633', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (158, 'F2084', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (159, 'F630', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (160, 'F949', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (161, 'D47680', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (162, 'A1079', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (163, 'A1080', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (164, 'D47590', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (165, 'GK461', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (166, 'GK471', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (167, 'GK481', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (168, 'GK491', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (169, 'GK501', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (170, 'E21451', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (171, 'GK581', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (172, 'GK993', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (173, 'GK2053', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (174, 'GK2467', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (175, 'GK858', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (176, 'GK1073', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (177, 'GK015', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (178, 'GK066', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (179, 'GK780', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (180, 'GK1495', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (181, 'GK806', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (182, 'GK1035', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (183, 'GK606', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (184, 'GK294', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (185, 'GK2464', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (186, 'GK1255', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (187, 'GK1325', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (188, 'GK1295', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (189, 'GK1365', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (190, 'GK505', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (191, 'GK1215', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (192, 'GK001', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (193, 'GK011', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (194, 'GK021', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (195, 'GK251', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (196, 'GK261', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (197, 'GK813', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (198, 'GK853', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (199, 'GK413', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (200, 'GK1745', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (201, 'GK228', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (202, 'GK238', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (203, 'GK218', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (204, 'GK208', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (205, 'GK255', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (206, 'GK275', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (207, 'GK1064', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (208, 'GK534', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (209, 'GK454', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (210, 'E1214', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (211, 'E1215', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (212, 'E1216', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (213, 'B1735', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (214, 'E1217', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (215, 'E1218', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (216, 'B2831', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (217, 'B2832', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (218, 'B2846', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (219, 'B2833', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (220, 'B2834', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (221, 'B2835', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (222, 'B4425', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (223, 'B4426', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (224, 'B4429', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (225, 'B4428', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (226, 'B4430', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (227, 'B4427', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (228, 'B3730', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (229, 'B3731', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (230, 'B3732', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (231, 'B3733', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (2, 'C3328', '5800.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (232, 'PMA-55101B', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (233, 'PMA-55101B', '5800.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (234, 'PMA55201B', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (235, 'PMA55201B', '5800.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (236, 'PMA55402', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (237, 'PMA55402', '5800.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (238, 'PMA55403', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (239, 'PMA55403', '5800.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (240, 'PMA55503', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (241, 'PMA55503', '5800.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (242, 'PMA55505', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (243, 'PMA55505', '5800.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (244, 'VN04', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (245, 'VN04', '5800.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (246, 'C3204', '6000.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (247, 'C3204', '5800.000', '0.000', 0, '88000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (248, 'Kinhdon6', '745.000', '940.000', 0, '300000');
INSERT INTO material_stock (id, material_id, length, width, stock, base_price) VALUES (249, 'Kinhdon6', '745.000', '1250.000', 0, '300000');


INSERT INTO stock_import ("stock_id", "voucher_code","quantity_change", "quantity_after", "price", "date")
VALUES
(1, '20250110000000', 50, 150, 1250000, '2025-01-10'),
(2, '20250115000000', 30, 180, 1250000, '2025-01-15'),
(3, '20250115000000', 30, 180, 1250000, '2025-01-15'),
(4, '20250111000000', 20, 60, 890000, '2025-01-11'),
(5, '20250121000000', 40, 100, 885000, '2025-01-21'),
(6, '20250201000000', 10, 25, 1500000, '2025-02-01'),
(7, '20250105000000', 100, 100, 450000, '2025-01-05'),
(8, '20250203000000', 50, 150, 460000, '2025-02-03'),
(9, '20250202000000', 70, 70, 2150000, '2025-02-02'),
(10, '20250110000000', 25, 75, 900000, '2025-01-10'),
(11, '20250110000000', 40, 140, 910000, '2025-01-10'),
(12, '20250110000000', 15, 45, 880000, '2025-01-10'),
(13, '20250115000000', 20, 120, 700000, '2025-01-15'),
(14, '20250115000000', 50, 200, 760000, '2025-01-15'),
(15, '20250115000000', 35, 85, 830000, '2025-01-15'),
(16, '20250202000000', 60, 160, 1200000, '2025-02-02'),
(17, '20250202000000', 10, 110, 1180000, '2025-02-02'),
(18, '20250202000000', 25, 125, 1190000, '2025-02-02'),
(19, '20250105000000', 40, 140, 450000, '2025-01-05'),
(20, '20250105000000', 30, 130, 440000, '2025-01-05'),
(21, '20250203000000', 55, 155, 480000, '2025-02-03'),
(22, '20250202000000', 20, 120, 500000, '2025-02-03'),
(23, '20250202000000', 35, 135, 530000, '2025-02-03');

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
('Khách sạn Imperial Huế', 'Huế', 4, 'Kiến trúc Sông Hương', '2026-03-01', '2025-10-15 14:00:00', 'designs/hue_imperial.pdf', 'Production', 'rfq/imperial_docs.docx'),
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

-- Update PO-ALPHA-001 to Finished status with planned & actual dates
update production_orders
set status = 6, -- Finished
  planned_start_date = '2025-11-01 08:00:00',
  planned_finish_date = '2025-11-15 17:00:00',
  submitted_at = '2025-10-25 09:00:00',
  director_decision_at = '2025-10-26 10:00:00',
  qa_machines_checked_at = '2025-10-27 11:00:00',
  qa_material_checked_at = '2025-10-28 12:00:00',
  started_at = '2025-11-01 08:00:00',
  finished_at = '2025-11-16 10:00:00',
  updated_at = now()
where code = 'PO-ALPHA-001';

-- Update items for PO-ALPHA-001 with planned dates
update production_order_items
set planned_start_date = '2025-11-01 08:00:00',
  planned_finish_date = '2025-11-15 17:00:00',
  actual_start_date = '2025-11-01 08:30:00',
  actual_finish_date = '2025-11-16 10:00:00',
  is_completed = true,
  completed_at = '2025-11-16 10:00:00',
  is_stored = true,
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
  planned_finish_date = src.planned_start_date + (
      CASE 
          WHEN src.code = 'CUT_AL' THEN interval '0.5 days'
          WHEN src.code = 'MILL_LOCK' THEN interval '0.5 days'
          WHEN src.code = 'DOOR_CORNER_CUT' THEN interval '1 day'
          WHEN src.code = 'ASSEMBLE_FRAME' THEN interval '1.5 days'
          WHEN src.code = 'GLASS_INSTALL' THEN interval '1 day'
          WHEN src.code = 'PRESS_GASKET' THEN interval '0.5 days'
          WHEN src.code = 'INSTALL_ACCESSORIES' THEN interval '1 day'
          WHEN src.code = 'CUT_FLUSH' THEN interval '0.5 days'
          WHEN src.code = 'FINISH_SILICON' THEN interval '1 day'
          ELSE interval '0.5 days'
      END
  ),
  planned_time_hours = 
      CASE 
          WHEN src.code = 'CUT_AL' THEN 4.0
          WHEN src.code = 'MILL_LOCK' THEN 3.0
          WHEN src.code = 'DOOR_CORNER_CUT' THEN 2.0
          WHEN src.code = 'ASSEMBLE_FRAME' THEN 5.0
          WHEN src.code = 'GLASS_INSTALL' THEN 6.0
          WHEN src.code = 'PRESS_GASKET' THEN 3.0
          WHEN src.code = 'INSTALL_ACCESSORIES' THEN 4.0
          WHEN src.code = 'CUT_FLUSH' THEN 2.0
          WHEN src.code = 'FINISH_SILICON' THEN 3.0
          ELSE 2.0
      END,
  actual_start_date = src.planned_start_date + interval '0.1 days',
  actual_finish_date = src.planned_start_date + interval '5.5 days',
  actual_time_hours = 
      CASE 
          WHEN src.code = 'CUT_AL' THEN 4.5
          WHEN src.code = 'MILL_LOCK' THEN 3.5
          WHEN src.code = 'DOOR_CORNER_CUT' THEN 2.5
          WHEN src.code = 'ASSEMBLE_FRAME' THEN 5.5
          WHEN src.code = 'GLASS_INSTALL' THEN 6.5
          WHEN src.code = 'PRESS_GASKET' THEN 3.5
          WHEN src.code = 'INSTALL_ACCESSORIES' THEN 4.5
          WHEN src.code = 'CUT_FLUSH' THEN 2.5
          WHEN src.code = 'FINISH_SILICON' THEN 3.5
          ELSE 2.5
      END,
  is_completed = true,
  completed_at = src.planned_start_date + interval '6 days',
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
('Máy Cắt Góc', 1, 'Operational', '2025-02-20', NULL, 5, 'mm', NULL),
('Máy Phay Ổ Khóa', 2, 'Operational', '2025-03-10', NULL, 50, 'sản phẩm/giờ', NULL),
('Máy cắt nhôm 01', 1, 'Operational', '2025-04-01', NULL, NULL, 'mm/phút', NULL),
('Máy khoét khóa 01', 2, 'Operational', '2025-04-05', NULL, NULL, 'sản phẩm/giờ', NULL),
('Máy khóa bản lề 01', 2, 'Operational', '2025-04-10', NULL, NULL, 'sản phẩm/giờ', NULL),
('Máy phay đố 01', 1, 'Operational', '2025-04-15', NULL, NULL, 'mm/phút', NULL),
('Máy ép góc 01', 1, 'Operational', '2025-04-20', NULL, NULL, 'mm/phút', NULL)
ON CONFLICT DO NOTHING;

-- Gán máy cho từng loại giai đoạn (stage_types.machine_id)
UPDATE stage_types st
SET machine_id = m.id
FROM machines m
WHERE st.code = 'CUT_AL'
  AND m.name = 'Máy cắt nhôm 01';

UPDATE stage_types st
SET machine_id = m.id
FROM machines m
WHERE st.code = 'MILL_LOCK'
  AND m.name = 'Máy Phay Ổ Khóa';

UPDATE stage_types st
SET machine_id = m.id
FROM machines m
WHERE st.code = 'ASSEMBLE_FRAME'
  AND m.name = 'Máy khóa bản lề 01';

UPDATE stage_types st
SET machine_id = m.id
FROM machines m
WHERE st.code = 'CUT_FLUSH'
  AND m.name = 'Máy phay đố 01';

UPDATE stage_types st
SET machine_id = m.id
FROM machines m
WHERE st.code = 'DOOR_CORNER_CUT'
  AND m.name = 'Máy Cắt Góc';

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
