set client_encoding to 'utf8';

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
insert into material_type ("name")
values
    ('aluminum'),
    ('glass'),
    ('accessory'),
    ('gasket'),
    ('auxiliary');

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

CREATE TABLE IF NOT EXISTS projects (
  "id" SERIAL,
  "name" VARCHAR(250) NOT NULL,
  "location" VARCHAR(250) NOT NULL,
  "client_id" INTEGER NOT NULL,
  "design_company" VARCHAR(250),
  "completion_date" DATE NOT NULL,
  "created_at" TIMESTAMP DEFAULT now(),
  "design_file_path" VARCHAR(250),
  "status" VARCHAR(250) NOT NULL DEFAULT 'Pending' CHECK ("status" IN ('Pending', 'Scheduled', 'Active', 'Completed')),
  "document_path" VARCHAR(250),
  CONSTRAINT "pk_projects" PRIMARY KEY ("id"),
  CONSTRAINT "fk_projects_client_id" FOREIGN KEY ("client_id") REFERENCES clients ("id")
);

create table if not exists materials (
    "id" serial primary key,
    "code" varchar(250) not null,
    "name" varchar(250) not null,
    "type" integer,
    "stock" int not null default 0,
    "stock_length" numeric(10,2) not null default 0, -- lưu độ dài lõi nhôm, độ dài gioăng, độ dài kính
    "stock_width" numeric(10,2) not null default 0, -- lưu độ rộng kính
    "weight" numeric(10,3), -- lưu tỉ trọng profile nhôm (kg/m)
    "thickness" numeric(10,3), -- lưu độ dày kính
    "vendor" varchar(250),
	constraint "fk_material_type" foreign key ("type") references material_type("id")
);
create unique index if not exists uq_materials_aluminum on materials("code", "stock_length") where "type" = 1;
create unique index if not exists uq_materials_glass on materials("code", "stock_length", "stock_width") where "type" = 2;

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
    constraint "fk_cavities_project_id" foreign key ("project_id") references projects("id")
);

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

create table if not exists stock_import (
    "id" serial primary key,
    "material_id" integer not null,
    "quantity_change" int not null,
    "quantity_after" int not null,
    "price" money not null,
    "date" date default now(),
    constraint "fk_stock_import_material" foreign key ("material_id") references materials("id")
);

CREATE TABLE if not exists machine_types (
  "id" SERIAL PRIMARY KEY,
  "name" VARCHAR(250) NOT NULL
 
);

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
