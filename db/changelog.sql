--liquibase formatted sql

--changeset Carl:1 context:#19
--comment: Initial
CREATE TABLE user_data (
    user_id SERIAL PRIMARY KEY,
    user_uuid UUID NOT NULL,
    auth0_id TEXT UNIQUE NOT NULL,
    full_name TEXT NOT NULL,
    first_name TEXT NULL,
    last_name TEXT NULL,
    nickname TEXT NOT NULL, 
    email TEXT NOT NULL,
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
    inventory_item_id INTEGER, 
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

CREATE TABLE custom_package (
    custom_package_id BIGSERIAL PRIMARY KEY,
    custom_package_uuid UUID NOT NULL,
    name TEXT NOT NULL, 
    weight_g INTEGER, -- weight is stored in grams
    width_cm INTEGER, -- package dimensions are stored in cm
    height_cm INTEGER, 
    length_cm INTEGER
);

CREATE TABLE role (
    role_id SERIAL PRIMARY KEY,
    role_name TEXT NOT NULL
);

CREATE TABLE permission (
    permission_id SERIAL PRIMARY KEY,
    permission_name TEXT NOT NULL
);

CREATE TABLE role_permission (
    role_id INTEGER,
    permission_id INTEGER,
    PRIMARY KEY (role_id, permission_id)   
);

CREATE TABLE user_role (
    user_id INTEGER,
    role_id INTEGER,
    PRIMARY KEY (user_id, role_id)   
);

INSERT INTO permission (permission_name)
VALUES ('create:user');
INSERT INTO permission (permission_name)
VALUES ('get:user');
INSERT INTO permission (permission_name)
VALUES ('delete:user');
INSERT INTO permission (permission_name)
VALUES ('update:user');
INSERT INTO permission (permission_name)
VALUES ('create:inventory-item');
INSERT INTO permission (permission_name)
VALUES ('get:inventory-item');
INSERT INTO permission (permission_name)
VALUES ('delete:inventory-item');
INSERT INTO permission (permission_name)
VALUES ('update:inventory-item');

INSERT INTO role_permission (role_id, permission_id)
VALUES (1, 1);
INSERT INTO role_permission (role_id, permission_id)
VALUES (1, 2);
INSERT INTO role_permission (role_id, permission_id)
VALUES (1, 3);
INSERT INTO role_permission (role_id, permission_id)
VALUES (1, 4);
INSERT INTO role_permission (role_id, permission_id)
VALUES (1, 5);
INSERT INTO role_permission (role_id, permission_id)
VALUES (1, 6);
INSERT INTO role_permission (role_id, permission_id)
VALUES (1, 7);
INSERT INTO role_permission (role_id, permission_id)
VALUES (1, 8);
INSERT INTO role_permission (role_id, permission_id)
VALUES (1, 9);


INSERT INTO user_data (user_uuid, auth0_id, full_name, first_name, last_name, nickname, email, created_at, last_login, is_premium_member, is_deleted)
VALUES ('e7bd758c-e8bb-45f0-ab4d-e7a331b60729','auth0|65fe478e4e87f7a8c0a6684a', 'Carl Ryckeley', 'Carl', 'Ryckeley','Carl The Great', 'bm-app@carlthegreat.com', NOW(), NOW(), False, False);

INSERT INTO business (business_uuid, business_name, business_type, business_industry, is_deleted)
VALUES ('b492be78-9a2f-4899-b516-79963418b985', 'test business',	0, 'test', False);

INSERT INTO user_business (user_id, business_id)
VALUES (1, 1);

INSERT INTO user_role (user_id, role_id)
VALUES (1, 1);