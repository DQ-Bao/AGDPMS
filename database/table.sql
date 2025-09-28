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
