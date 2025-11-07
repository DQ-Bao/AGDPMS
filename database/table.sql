set client_encoding to 'utf8';

drop table if exists material_plannings_details;
drop table if exists material_plannings;
drop table if exists stock_export;
drop table if exists stock_import;
drop table if exists machines;
drop table if exists machine_types;
drop table if exists projects;
drop table if exists clients;
drop table if exists material;
drop table if exists material_type;
drop table if exists users;
drop table if exists roles;

create table if not exists roles ( 
	"id" serial,
	"name" varchar(250) not null,
	constraint "pk_roles" primary key ("id"),
	constraint "uq_roles_name" unique ("name")
);

create table if not exists users ( 
	"id" serial,
	"fullname" varchar(250) not null,
	"phone" varchar(20) not null,
	"password_hash" text not null,
	"created_at" timestamp null default now() ,
	"need_change_password" boolean not null default true ,
	"role_id" integer not null,
    "active" boolean not null default true,
    "email" varchar(250),
    "date_of_birth" date,
	constraint "pk_users" primary key ("id"),
	constraint "fk_users_role_id" foreign key ("role_id") references roles ("id"),
	constraint "uq_users_phone" unique ("phone")
);

create table if not exists material_type (
	"id" serial,
	"name" varchar(250),
	constraint "pk_material_type" primary key ("id")
);

create table if not exists material (
    "id" varchar(250) primary key,
    "name" varchar(250) not null,
    "type" integer,
    "stock" int not null default 0,
    "weight" numeric(10,3),
    "thickness" numeric(10,3),
	constraint "fk_material_type" foreign key ("type") references material_type("id")
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

create table if not exists machine_types (
  "id" serial primary key,
  "name" varchar(250) not null
);

create table if not exists  machines (
  "id" serial,
  "name" varchar(250) not null,
  "machine_type_id" integer not null,
  "status" varchar(50) not null default 'Operational' check ("status" in ('Operational', 'NeedsMaintenance', 'Broken')),
  "entry_date" date not null,
  "last_maintenance_date" date null,
  "expected_completion_date" date null,
  "capacity_value" numeric(10,2), -- <<  công suất 
  "capacity_unit" varchar(50),     -- <<  đơn vị (sản phẩm/phút, kg/giờ, mm/phút)
  constraint "machines_pkey" primary key ("id"),
  constraint "fk_machines_type_id" foreign key ("machine_type_id") references machine_types("id")
);

create table if not exists stock_import (
    "id" serial primary key,
    "material_id" varchar(250) not null,
    "quantity_change" int not null,
    "quantity_after" int not null,
    "price" numeric not null,
    "date" date default now(),
    constraint "fk_stock_import_material" foreign key ("material_id") references material("id")
);

create table if not exists stock_export (
	"id" serial primary key,
    "material_id" varchar(250) not null,
    "quantity_change" int not null,
    "quantity_after" int not null,
    "price" numeric not null,
    "date" date default now(),
    constraint "fk_stock_export_material" foreign key ("material_id") references material("id")
);

create table if not exists material_plannings (
	"id" serial,
	"made_by" integer not null,
	"project_id" integer not null,
	"status" varchar(250) not null,
	"created_at" timestamp null default now(),
	constraint "pk_material_planning" primary key ("id"),
	constraint "fk_material_planning_made_by" foreign key ("made_by") references "users"("id"),
	constraint "fk_material_planning_project_id" foreign key ("project_id") references "projects_rfq"("id")
);

create table if not exists material_planning_details (
	"id" serial,
	"planning_id" integer not null,
	"material_id" varchar(250) not null,
	"quantity" integer not null,
	"unit" varchar(250) null,
	"note" text null,
	constraint "pk_material_planning_details" primary key ("id"),
	constraint "fk_material_planning_material_id" foreign key ("material_id") references "material"("id"),
	constraint "fk_material_planning_planning_id" foreign key ("planning_id") references "material_plannings"("id")
);
