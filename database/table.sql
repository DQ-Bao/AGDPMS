set client_encoding to 'utf8';

drop table if exists accessory;
drop table if exists glass;
drop table if exists aluminum;
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
  constraint "pk_users" primary key ("id"),
  constraint "fk_users_role_id" foreign key ("role_id") references roles ("id"),
  constraint "uq_users_phone" unique ("phone")
);

create table if not exists aluminum (
    "aluminum_id" varchar(250),
    "aluminum_name" varchar(250),
    "linear_density" numeric(10,3),
	"quantity" int default 0,
    primary key ("aluminum_id")
);

create table if not exists glass (
	"glass_id" varchar(250),
	"glass_name" varchar(250),
	"thickness" numeric(10,3),
	"quantity" int default 0,
	primary key ("glass_id")
);

create table if not exists accessory (
	"accessory_id" varchar(250),
	"accessory_name" varchar(250),
	"quantity" int default 0,
	primary key ("accessory_id")
);