set client_encoding to 'utf8';

drop table if exists machines;
drop table if exists machine_types;
drop table if exists stock_import;
drop table if exists material;
drop table if exists projects;
drop table if exists clients;
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

create table if not exists clients ( 
  "id" serial,
  "name" varchar(250) not null,
  "address" varchar(250),
  "phone" varchar(250) unique,
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
  "status" VARCHAR(250) NOT NULL DEFAULT 'Planning' CHECK ("status" IN ('Planning', 'Production', 'Deploying', 'Completed', 'Cancelled')),
  "document_path" VARCHAR(250),
  CONSTRAINT "pk_projects" PRIMARY KEY ("id"),
  CONSTRAINT "fk_projects_client_id" FOREIGN KEY ("client_id") REFERENCES clients ("id")
);


create table if not exists material (
    "id" varchar(250) primary key,
    "name" varchar(250) not null,
    "type" serial,
    "stock" int not null default 0,
    "weight" numeric(10,3),
    "thickness" numeric(10,3),
	constraint "fk_material_type" foreign key ("type") references material_type("id")
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