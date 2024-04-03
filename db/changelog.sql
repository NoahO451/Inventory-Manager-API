--liquibase formatted sql

--changeset Carl:1 context:#8
--comment: this is a test table that will be deleted soon.
CREATE TABLE accounts (
  user_id SERIAL PRIMARY KEY, 
  username VARCHAR (50) UNIQUE NOT NULL, 
  password VARCHAR (50) NOT NULL, 
  email VARCHAR (255) UNIQUE NOT NULL, 
  created_at TIMESTAMP NOT NULL, 
  last_login TIMESTAMP
);

--changeset Carl:2 context:#9
--comment: this is a test table that will be deleted soon.
DROP TABLE accounts;

CREATE TABLE user_data (
    user_id SERIAL PRIMARY KEY,
    auth0_id TEXT UNIQUE NOT NULL,
    username TEXT UNIQUE NOT NULL,
    first_name TEXT NOT NULL,
    last_name TEXT NOT NULL,
    email TEXT NOT NULL,
    role INTEGER NOT NULL,
    created_at TIMESTAMP NOT NULL,
    last_login TIMESTAMP,
    is_premium_member BOOLEAN NOT NULL,
    is_deleted BOOLEAN NOT NULL
);

CREATE TABLE inventory_item (
    inventory_item_id BIGSERIAL PRIMARY KEY,
    name TEXT,
    description TEXT,
    sku TEXT,
    cost NUMERIC(19,4),
    serial_number TEXT,
    purchased_date TIMESTAMP,
    supplier TEXT,
    brand TEXT,
    model TEXT,
    quantity INTEGER,
    reorder_quantity INTEGER, 
    location TEXT,
    expiration_date TIMESTAMP,
    category INTEGER,
    packaging INTEGER, 
    item_weight integer, -- weight is stored in grams 
    is_listed BOOLEAN,
    is_lot BOOLEAN,
    notes TEXT
);

CREATE TABLE business_inventory_item (
    inventory_item_id BIGSERIAL, 
    business_id INTEGER
);

CREATE TABLE user_business (
    user_id INTEGER,
    business_id INTEGER,
    PRIMARY KEY (user_id, business_id)
);

CREATE TABLE business (
    business_id SERIAL PRIMARY KEY,
    business_name TEXT,
    business_type INTEGER,
    business_industry TEXT,
    is_deleted BOOLEAN
);