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
    is_PremiumMember BOOLEAN NOT NULL,
    is_Deleted BOOLEAN NOT NULL
);

CREATE TABLE inventory (
    inventory_id BIGSERIAL PRIMARY KEY,
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
    reorder_auantity INTEGER,
    location TEXT,
    expiration_date TIMESTAMP,
    category INTEGER,
    is_listed BOOLEAN,
    is_lot BOOLEAN,
    notes TEXT
);

CREATE TABLE user_business (
    user_id INTEGER,
    business_id INTEGER,
    PRIMARY KEY (User_Id, Business_Id)
);

CREATE TABLE business (
    business_id SERIAL PRIMARY KEY,
    business_name TEXT,
    business_type TEXT,
    is_deleted BOOLEAN
);