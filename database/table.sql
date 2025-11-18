    set client_encoding to 'utf8';

    -- drop new production tables first (children before parents)
    drop table if exists production_issue_reports;
    drop table if exists production_reject_reports;
    drop table if exists production_item_stages;
    drop table if exists production_order_items;
    drop table if exists production_orders;
    drop table if exists stage_types;
    drop table if exists products;
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
    "status" smallint not null default 0,
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

    -- production issue reports (renamed from reject reports)
    create table if not exists production_issue_reports (
        "id" serial,
        "production_item_stage_id" integer not null,
        "created_by_user_id" integer not null,
        "status" smallint not null default 0,
        "priority" smallint not null default 0,
        "reason" text not null,
        "resolved_at" timestamp null,
        "resolved_by_user_id" integer null,
        "created_at" timestamp null default now(),
        constraint "pk_production_issue_reports" primary key ("id"),
        constraint "fk_issue_reports_stage" foreign key ("production_item_stage_id") references production_item_stages ("id"),
        constraint "fk_issue_reports_created_by" foreign key ("created_by_user_id") references users ("id"),
        constraint "fk_issue_reports_resolved_by" foreign key ("resolved_by_user_id") references users ("id")
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
