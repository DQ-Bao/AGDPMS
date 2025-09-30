SET CLIENT_ENCODING TO 'utf8';
/*
DROP TABLE IF EXISTS takeoff;
DROP TABLE IF EXISTS inventory_receipts;
DROP TABLE IF EXISTS glass;
DROP TABLE IF EXISTS projects;
*/

DROP TABLE IF EXISTS profiles;
DROP TABLE IF EXISTS users;
drop table if exists roles;

create table if not exists roles ( 
  "id" serial,
  "name" varchar(250) not null,
  constraint "PK_roles" primary key ("id"),
  constraint "UQ_roles_name" unique ("name")
);

create table if not exists users ( 
  "id" serial,
  "fullname" varchar(250) not null,
  "phone" varchar(20) not null,
  "password_hash" text not null,
  "created_at" timestamp null default now() ,
  "need_change_password" boolean not null default true ,
  "role_id" integer not null,
  constraint "PK_users" primary key ("id"),
  constraint "FK_users_role_id" foreign key ("role_id") references roles ("id"),
  constraint "UQ_users_phone" unique ("phone")
);

CREATE TABLE IF NOT EXISTS profiles (
    "profile_id" varchar(250),
    "profile_name" varchar(250),
    "linear_density" numeric(10,3),
    PRIMARY KEY ("profile_id")
)
/*
CREATE TABLE IF NOT EXISTS projects (
	project_id varchar,
	PRIMARY KEY (project_id)
);

CREATE TABLE IF NOT EXISTS glass (
	glass_id varchar,
	glass_name varchar,
	PRIMARY KEY (glass_id)
)

CREATE TABLE IF NOT EXISTS inventory_receipts (
    receipt_id varchar,
	PRIMARY KEY (receipt_id)
);

CREATE TABLE IF NOT EXISTS takeoff (
	project_id varchar,
	material_id varchar references materials(material_id),
	item_count int,
	PRIMARY KEY (project_id)
);
*/
