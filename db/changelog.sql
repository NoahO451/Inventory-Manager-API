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
    user_uuid UUID NOT NULL,
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
    inventory_item_uuid UUID NOT NULL,
    name TEXT NOT NULL,
    description TEXT,
    sku TEXT,
    cost INTEGER, -- stored in pennies
    serial_number TEXT,
    purchase_date TIMESTAMP,
    supplier TEXT,
    brand TEXT,
    model TEXT,
    quantity INTEGER NOT NULL,
    reorder_quantity INTEGER, 
    location TEXT,
    expiration_date TIMESTAMP,
    category INTEGER,
    custom_package_uuid UUID,
    item_weight_g integer, -- weight is stored in grams 
    is_listed BOOLEAN NOT NULL,
    is_lot BOOLEAN NOT NULL,
    notes TEXT
);

CREATE TABLE business_inventory_item (
    inventory_item_id BIGSERIAL, 
    business_id INTEGER,
    PRIMARY KEY (inventory_item_id, business_id)
);

CREATE TABLE user_business (
    user_id INTEGER,
    business_id INTEGER,
    PRIMARY KEY (user_id, business_id)
);

CREATE TABLE business (
    business_id SERIAL PRIMARY KEY,
    business_uuid UUID NOT NULL,
    business_name TEXT,
    business_type INTEGER,
    business_industry TEXT,
    is_deleted BOOLEAN
);

--changeset Carl:3 context:#9
--comment: added custom packaging table, removed packaging ID and added foreign key to inventory_items for packages
-- Create custom_package table
CREATE TABLE custom_package (
    custom_package_id BIGSERIAL PRIMARY KEY,
    custom_package_uuid UUID NOT NULL,
    name TEXT NOT NULL, 
    weight_g INTEGER, -- weight is stored in grams
    width_cm INTEGER, -- package dimensions are stored in cm
    height_cm INTEGER, 
    length_cm INTEGER
);

--changeset Carl:4 context:#16
--comment: Removed role from user data
ALTER TABLE user_data
DROP COLUMN role;

-- Adding the IdP provider that the user signed up with
ALTER TABLE user_data
ADD COLUMN auth_provider TEXT NOT NULL;

ALTER TABLE user_data
DROP COLUMN username;