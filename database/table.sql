set client_encoding to 'utf8';

drop table if exists stock_import;
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
