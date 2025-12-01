set client_encoding to 'utf8';

drop table if exists stage_review_criteria_results;
drop table if exists stage_reviews;
drop table if exists stage_criteria;
drop table if exists production_item_stages;
drop table if exists production_order_items;
drop table if exists production_orders;
drop table if exists stage_types;
drop table if exists products;
drop table if exists material_plannings_details;
drop table if exists material_plannings;
drop table if exists stock_export;
drop table if exists stock_import;
drop table if exists machines;
drop table if exists machine_types;
drop table if exists cavity_boms;
drop table if exists cavities;
drop table if exists stock_import;
drop table if exists projects;
drop table if exists clients;
drop table if exists materials;
drop table if exists material_type;
drop table if exists users;
drop table if exists roles;

create table if not exists roles ( 
	"id" serial primary key,
	"name" varchar(250) not null,
	constraint "uq_roles_name" unique("name")
);

create table if not exists users ( 
	"id" serial primary key,
	"fullname" varchar(250) not null,
	"phone" varchar(20) not null,
	"password_hash" text not null,
	"created_at" timestamp null default now(),
	"need_change_password" boolean not null default true,
	"role_id" integer not null,
    "active" boolean not null default true,
    "email" varchar(250),
    "date_of_birth" date,
	constraint "fk_users_role_id" foreign key ("role_id") references roles("id"),
	constraint "uq_users_phone" unique("phone")
);

create table if not exists material_type (
	"id" serial primary key,
	"name" varchar(250) not null,
    constraint "uq_material_type_name" unique("name")
);

create table if not exists materials (
    "id" varchar(250) primary key,
    "name" varchar(250) not null,
    "type" integer,
	"weight" numeric(10,3),
	constraint "fk_material_type" foreign key ("type") references material_type("id")
);
insert into material_type ("name")
values
    ('aluminum'),
    ('glass'),
    ('accessory'),
    ('gasket'),
    ('auxiliary');

create table if not exists material_stock (
	"id" serial primary key,
	"material_id" varchar(250) not null,
	"length" numeric(10,3) default 0,
	"width" numeric(10,3) default 0,
	"stock" int not null default 0,
	constraint "fk_material_type" foreign key ("material_id") references material("id")
);

create table if not exists materials (
    "id" varchar(250) primary key,
    "name" varchar(250) not null,
    "type" integer,
	"weight" numeric(10,3),
	constraint "fk_material_type" foreign key ("type") references material_type("id")
);

create table if not exists material_stock (
	"id" serial primary key,
	"material_id" varchar(250) not null,
	"length" numeric(10,3) default 0,
	"width" numeric(10,3) default 0,
	"stock" int not null default 0,
	"base_price" numeric(20,0) not null default 0
	constraint "fk_material_type" foreign key ("material_id") references materials("id")
);

create table if not exists clients ( 
  "id" serial,
  "name" varchar(250) not null,
  "address" varchar(250),
  "phone" varchar(250),
  "email" varchar(250),
  "sales_in_charge_id" integer,
  constraint "pk_clients" primary key ("id"),
  constraint "fk_clients_sales_id" foreign key ("sales_in_charge_id") references users ("id")
);

create table if not exists projects (
  "id" serial,
  "name" varchar(250) not null,
  "location" varchar(250) not null,
  "client_id" integer not null,
  "design_company" varchar(250),
  "completion_date" date not null,
  "created_at" timestamp default now(),
  "design_file_path" varchar(250),
  "status" varchar(250) not null default 'Pending' check ("status" in ('Pending', 'Scheduled', 'Active', 'Completed')),
  "document_path" VARCHAR(250),
  constraint "pk_projects" primary key ("id"),
  constraint "fk_projects_client_id" foreign key ("client_id") references clients ("id")
);

create table if not exists cavities (
    "id" serial primary key,
    "code" varchar(250) not null,
    "project_id" integer not null,
    "location" varchar(250),
    "quantity" integer not null,
    "aluminum_vendor" varchar(250),
    "description" varchar(250),
    "width" numeric(10,2) not null,
    "height" numeric(10,2) not null,
    constraint "fk_cavities_project_id" foreign key ("project_id") references projects("id"),
  constraint "fk_projects_client_id" foreign key ("client_id") references clients ("id")
);

create table if not exists machine_types (
  "id" serial primary key,
  "name" varchar(250) not null
);

create table if not exists products (
    "id" serial,
    "name" varchar(250),
    "project_id" integer null,
    constraint "pk_products" primary key ("id"),
    constraint "fk_products_project" foreign key ("project_id") references projects ("id")
);

create index if not exists "ix_products_project" on products ("project_id");

-- production orders
create table if not exists production_orders (
    "id" serial,
    "project_id" integer not null,
    "code" varchar(50),
    "status" smallint not null,
    "is_cancelled" boolean not null default false,
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

create index if not exists "ix_production_orders_project_status"
    on production_orders ("project_id", "status");

-- stage types (Vietnamese names for UI, English codes)
create table if not exists stage_types (
    "id" serial,
    "code" varchar(50) not null,
    "name" varchar(250) not null,
    "is_active" boolean not null default true,
    "is_default" boolean not null default false,
    "created_at" timestamp null default now(),
    "updated_at" timestamp null,
    constraint "pk_stage_types" primary key ("id"),
    constraint "uq_stage_types_code" unique ("code")
);

    -- production order items
create table if not exists production_order_items (
    "id" serial,
    "production_order_id" integer not null,
    "product_id" integer not null,
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
    "is_canceled" boolean not null default false,
    "created_at" timestamp null default now(),
    "updated_at" timestamp null,
    constraint "pk_production_order_items" primary key ("id"),
    constraint "fk_order_items_order" foreign key ("production_order_id") references production_orders ("id"),
    constraint "fk_order_items_product" foreign key ("product_id") references products ("id")
);

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

create table if not exists cavity_boms (
    "id" serial primary key,
    "cavity_id" integer not null,
    "material_code" varchar(250) not null,
    "quantity" integer not null,
    "length" numeric(10,2) not null default 0, -- lưu độ dài cắt nhôm, độ dài gioăng, độ dài cắt kính
    "width" numeric(10,2) not null default 0, -- lưu độ rộng cắt kính
    "color" varchar(250),
    "unit" varchar(250),
    constraint "fk_cavity_boms_cavity_id" foreign key ("cavity_id") references cavities("id")
);
create index idx_cavity_boms_material_code on cavity_boms("material_code");

CREATE TABLE if not exists  machines (
  "id" SERIAL,
  "name" VARCHAR(250) NOT NULL,
  "machine_type_id" INTEGER NOT NULL,
  "status" VARCHAR(50) NOT NULL DEFAULT 'Operational' CHECK ("status" IN ('Operational', 'NeedsMaintenance', 'Broken')),
  "entry_date" DATE NOT NULL,
  "last_maintenance_date" DATE NULL,
  "expected_completion_date" DATE NULL,
  "capacity_value" NUMERIC(10,2), -- <<  công suất 
  "capacity_unit" VARCHAR(50),     -- <<  đơn vị (sản phẩm/phút, kg/giờ, mm/phút)
  CONSTRAINT "machines_pkey" PRIMARY KEY ("id"),
  CONSTRAINT "fk_machines_type_id" FOREIGN KEY ("machine_type_id") REFERENCES machine_types("id")
);
create table if not exists material_plannings (
	"id" serial,
	"made_by" integer not null,
	"project_id" integer not null,
	"status" varchar(250) not null,
	"created_at" timestamp null default now(),
	constraint "pk_material_planning" primary key ("id"),
	constraint "fk_material_planning_made_by" foreign key ("made_by") references users("id"),
	constraint "fk_material_planning_project_id" foreign key ("project_id") references projects("id")
);

create table if not exists material_planning_details (
	"id" serial,
	"planning_id" integer not null,
	"material_id" varchar(250) not null,
	"quantity" integer not null,
	"unit" varchar(250) null,
	"note" text null,
	constraint "pk_material_planning_details" primary key ("id"),
	constraint "fk_material_planning_material_id" foreign key ("material_id") references materials("id"),
	constraint "fk_material_planning_planning_id" foreign key ("planning_id") references material_plannings("id")
);
