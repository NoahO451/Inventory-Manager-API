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
    is_deleted BOOLEAN NOT NULL,
    sys_period tstzrange NOT NULL DEFAULT tstzrange(current_timestamp, NULL)
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
    notes TEXT,
    sys_period tstzrange NOT NULL DEFAULT tstzrange(current_timestamp, NULL)
);

CREATE TABLE business_inventory_item (
    inventory_item_id INTEGER, 
    business_id INTEGER,
    sys_period tstzrange NOT NULL DEFAULT tstzrange(current_timestamp, NULL),
    PRIMARY KEY (inventory_item_id, business_id)
);

CREATE TABLE user_business (
    user_id INTEGER,
    business_id INTEGER,
    sys_period tstzrange NOT NULL DEFAULT tstzrange(current_timestamp, NULL),
    PRIMARY KEY (user_id, business_id)
);

CREATE TABLE business (
    business_id SERIAL PRIMARY KEY,
    business_uuid UUID NOT NULL,
    business_name TEXT,
    business_type INTEGER,
    business_industry TEXT,
    is_deleted BOOLEAN,
    sys_period tstzrange NOT NULL DEFAULT tstzrange(current_timestamp, NULL)
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
    sys_period tstzrange NOT NULL DEFAULT tstzrange(current_timestamp, NULL), 
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

--changeset Carl:2 context:#19-2
--comment: Added temporal tables and audit table

-------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------
-- An audit history is important on most tables. Provide an audit trigger that logs to
-- a dedicated audit table for the major relations.
--
-- This file should be generic and not depend on application roles or structures,
-- as it's being listed here:
--
--    https://wiki.postgresql.org/wiki/Audit_trigger_91plus    
--
-- This trigger was originally based on
--   http://wiki.postgresql.org/wiki/Audit_trigger
-- but has been completely rewritten.
--
-- Should really be converted into a relocatable EXTENSION, with control and upgrade files.

CREATE EXTENSION IF NOT EXISTS hstore;

CREATE SCHEMA audit;
REVOKE ALL ON SCHEMA audit FROM public;

COMMENT ON SCHEMA audit IS 'Out-of-table audit/history logging tables and trigger functions';

--
-- Audited data. Lots of information is available, it's just a matter of how much
-- you really want to record. See:
--
--   http://www.postgresql.org/docs/9.1/static/functions-info.html
--
-- Remember, every column you add takes up more audit table space and slows audit
-- inserts.
--
-- Every index you add has a big impact too, so avoid adding indexes to the
-- audit table unless you REALLY need them. The hstore GIST indexes are
-- particularly expensive.
--
-- It is sometimes worth copying the audit table, or a coarse subset of it that
-- you're interested in, into a temporary table where you CREATE any useful
-- indexes and do your analysis.
--
CREATE TABLE audit.logged_actions (
    event_id bigserial primary key,
    schema_name text not null,
    table_name text not null,
    relid oid not null,
    session_user_name text,
    action_tstamp_tx TIMESTAMP WITH TIME ZONE NOT NULL,
    action_tstamp_stm TIMESTAMP WITH TIME ZONE NOT NULL,
    action_tstamp_clk TIMESTAMP WITH TIME ZONE NOT NULL,
    transaction_id bigint,
    application_name text,
    client_addr inet,
    client_port integer,
    client_query text,
    action TEXT NOT NULL CHECK (action IN ('I','D','U', 'T')),
    row_data hstore,
    changed_fields hstore,
    statement_only boolean not null
);

REVOKE ALL ON audit.logged_actions FROM public;

COMMENT ON TABLE audit.logged_actions IS 'History of auditable actions on audited tables, from audit.if_modified_func()';
COMMENT ON COLUMN audit.logged_actions.event_id IS 'Unique identifier for each auditable event';
COMMENT ON COLUMN audit.logged_actions.schema_name IS 'Database schema audited table for this event is in';
COMMENT ON COLUMN audit.logged_actions.table_name IS 'Non-schema-qualified table name of table event occured in';
COMMENT ON COLUMN audit.logged_actions.relid IS 'Table OID. Changes with drop/create. Get with ''tablename''::regclass';
COMMENT ON COLUMN audit.logged_actions.session_user_name IS 'Login / session user whose statement caused the audited event';
COMMENT ON COLUMN audit.logged_actions.action_tstamp_tx IS 'Transaction start timestamp for tx in which audited event occurred';
COMMENT ON COLUMN audit.logged_actions.action_tstamp_stm IS 'Statement start timestamp for tx in which audited event occurred';
COMMENT ON COLUMN audit.logged_actions.action_tstamp_clk IS 'Wall clock time at which audited event''s trigger call occurred';
COMMENT ON COLUMN audit.logged_actions.transaction_id IS 'Identifier of transaction that made the change. May wrap, but unique paired with action_tstamp_tx.';
COMMENT ON COLUMN audit.logged_actions.client_addr IS 'IP address of client that issued query. Null for unix domain socket.';
COMMENT ON COLUMN audit.logged_actions.client_port IS 'Remote peer IP port address of client that issued query. Undefined for unix socket.';
COMMENT ON COLUMN audit.logged_actions.client_query IS 'Top-level query that caused this auditable event. May be more than one statement.';
COMMENT ON COLUMN audit.logged_actions.application_name IS 'Application name set when this audit event occurred. Can be changed in-session by client.';
COMMENT ON COLUMN audit.logged_actions.action IS 'Action type; I = insert, D = delete, U = update, T = truncate';
COMMENT ON COLUMN audit.logged_actions.row_data IS 'Record value. Null for statement-level trigger. For INSERT this is the new tuple. For DELETE and UPDATE it is the old tuple.';
COMMENT ON COLUMN audit.logged_actions.changed_fields IS 'New values of fields changed by UPDATE. Null except for row-level UPDATE events.';
COMMENT ON COLUMN audit.logged_actions.statement_only IS '''t'' if audit event is from an FOR EACH STATEMENT trigger, ''f'' for FOR EACH ROW';

CREATE INDEX logged_actions_relid_idx ON audit.logged_actions(relid);
CREATE INDEX logged_actions_action_tstamp_tx_stm_idx ON audit.logged_actions(action_tstamp_stm);
CREATE INDEX logged_actions_action_idx ON audit.logged_actions(action);

CREATE OR REPLACE FUNCTION audit.if_modified_func() RETURNS TRIGGER AS $body$
DECLARE
    audit_row audit.logged_actions;
    include_values boolean;
    log_diffs boolean;
    h_old hstore;
    h_new hstore;
    excluded_cols text[] = ARRAY[]::text[];
BEGIN
    IF TG_WHEN <> 'AFTER' THEN
        RAISE EXCEPTION 'audit.if_modified_func() may only run as an AFTER trigger';
    END IF;

    audit_row = ROW(
        nextval('audit.logged_actions_event_id_seq'), -- event_id
        TG_TABLE_SCHEMA::text,                        -- schema_name
        TG_TABLE_NAME::text,                          -- table_name
        TG_RELID,                                     -- relation OID for much quicker searches
        session_user::text,                           -- session_user_name
        current_timestamp,                            -- action_tstamp_tx
        statement_timestamp(),                        -- action_tstamp_stm
        clock_timestamp(),                            -- action_tstamp_clk
        txid_current(),                               -- transaction ID
        current_setting('application_name'),          -- client application
        inet_client_addr(),                           -- client_addr
        inet_client_port(),                           -- client_port
        current_query(),                              -- top-level query or queries (if multistatement) from client
        substring(TG_OP,1,1),                         -- action
        NULL, NULL,                                   -- row_data, changed_fields
        'f'                                           -- statement_only
        );

    IF NOT TG_ARGV[0]::boolean IS DISTINCT FROM 'f'::boolean THEN
        audit_row.client_query = NULL;
    END IF;

    IF TG_ARGV[1] IS NOT NULL THEN
        excluded_cols = TG_ARGV[1]::text[];
    END IF;
    
    IF (TG_OP = 'UPDATE' AND TG_LEVEL = 'ROW') THEN
        audit_row.row_data = hstore(OLD.*) - excluded_cols;
        audit_row.changed_fields =  (hstore(NEW.*) - audit_row.row_data) - excluded_cols;
        IF audit_row.changed_fields = hstore('') THEN
            -- All changed fields are ignored. Skip this update.
            RETURN NULL;
        END IF;
    ELSIF (TG_OP = 'DELETE' AND TG_LEVEL = 'ROW') THEN
        audit_row.row_data = hstore(OLD.*) - excluded_cols;
    ELSIF (TG_OP = 'INSERT' AND TG_LEVEL = 'ROW') THEN
        audit_row.row_data = hstore(NEW.*) - excluded_cols;
    ELSIF (TG_LEVEL = 'STATEMENT' AND TG_OP IN ('INSERT','UPDATE','DELETE','TRUNCATE')) THEN
        audit_row.statement_only = 't';
    ELSE
        RAISE EXCEPTION '[audit.if_modified_func] - Trigger func added as trigger for unhandled case: %, %',TG_OP, TG_LEVEL;
        RETURN NULL;
    END IF;
    INSERT INTO audit.logged_actions VALUES (audit_row.*);
    RETURN NULL;
END;
$body$
LANGUAGE plpgsql
SECURITY DEFINER
SET search_path = pg_catalog, public;


COMMENT ON FUNCTION audit.if_modified_func() IS $body$
Track changes to a table at the statement and/or row level.

Optional parameters to trigger in CREATE TRIGGER call:

param 0: boolean, whether to log the query text. Default 't'.

param 1: text[], columns to ignore in updates. Default [].

         Updates to ignored cols are omitted from changed_fields.

         Updates with only ignored cols changed are not inserted
         into the audit log.

         Almost all the processing work is still done for updates
         that ignored. If you need to save the load, you need to use
         WHEN clause on the trigger instead.

         No warning or error is issued if ignored_cols contains columns
         that do not exist in the target table. This lets you specify
         a standard set of ignored columns.

There is no parameter to disable logging of values. Add this trigger as
a 'FOR EACH STATEMENT' rather than 'FOR EACH ROW' trigger if you do not
want to log row values.

Note that the user name logged is the login role for the session. The audit trigger
cannot obtain the active role because it is reset by the SECURITY DEFINER invocation
of the audit trigger its self.
$body$;



CREATE OR REPLACE FUNCTION audit.audit_table(target_table regclass, audit_rows boolean, audit_query_text boolean, ignored_cols text[]) RETURNS void AS $body$
DECLARE
  stm_targets text = 'INSERT OR UPDATE OR DELETE OR TRUNCATE';
  _q_txt text;
  _ignored_cols_snip text = '';
BEGIN
    EXECUTE 'DROP TRIGGER IF EXISTS audit_trigger_row ON ' || target_table;
    EXECUTE 'DROP TRIGGER IF EXISTS audit_trigger_stm ON ' || target_table;

    IF audit_rows THEN
        IF array_length(ignored_cols,1) > 0 THEN
            _ignored_cols_snip = ', ' || quote_literal(ignored_cols);
        END IF;
        _q_txt = 'CREATE TRIGGER audit_trigger_row AFTER INSERT OR UPDATE OR DELETE ON ' || 
                 target_table || 
                 ' FOR EACH ROW EXECUTE PROCEDURE audit.if_modified_func(' ||
                 quote_literal(audit_query_text) || _ignored_cols_snip || ');';
        RAISE NOTICE '%',_q_txt;
        EXECUTE _q_txt;
        stm_targets = 'TRUNCATE';
    ELSE
    END IF;

    _q_txt = 'CREATE TRIGGER audit_trigger_stm AFTER ' || stm_targets || ' ON ' ||
             target_table ||
             ' FOR EACH STATEMENT EXECUTE PROCEDURE audit.if_modified_func('||
             quote_literal(audit_query_text) || ');';
    RAISE NOTICE '%',_q_txt;
    EXECUTE _q_txt;

END;
$body$
language 'plpgsql';

COMMENT ON FUNCTION audit.audit_table(regclass, boolean, boolean, text[]) IS $body$
Add auditing support to a table.

Arguments:
   target_table:     Table name, schema qualified if not on search_path
   audit_rows:       Record each row change, or only audit at a statement level
   audit_query_text: Record the text of the client query that triggered the audit event?
   ignored_cols:     Columns to exclude from update diffs, ignore updates that change only ignored cols.
$body$;

-- Pg doesn't allow variadic calls with 0 params, so provide a wrapper
CREATE OR REPLACE FUNCTION audit.audit_table(target_table regclass, audit_rows boolean, audit_query_text boolean) RETURNS void AS $body$
SELECT audit.audit_table($1, $2, $3, ARRAY[]::text[]);
$body$ LANGUAGE SQL;

-- And provide a convenience call wrapper for the simplest case
-- of row-level logging with no excluded cols and query logging enabled.
--
CREATE OR REPLACE FUNCTION audit.audit_table(target_table regclass) RETURNS void AS $body$
SELECT audit.audit_table($1, BOOLEAN 't', BOOLEAN 't');
$body$ LANGUAGE 'sql';

COMMENT ON FUNCTION audit.audit_table(regclass) IS $body$
Add auditing support to the given table. Row-level changes will be logged with full client query text. No cols are ignored.
$body$;

CREATE OR REPLACE VIEW audit.tableslist AS 
 SELECT DISTINCT triggers.trigger_schema AS schema,
    triggers.event_object_table AS auditedtable
   FROM information_schema.triggers
    WHERE triggers.trigger_name::text IN ('audit_trigger_row'::text, 'audit_trigger_stm'::text)  
ORDER BY schema, auditedtable;

COMMENT ON VIEW audit.tableslist IS $body$
View showing all tables with auditing set up. Ordered by schema, then table.
$body$;

-- https://wiki.postgresql.org/wiki/Audit_trigger_91plus
-- https://github.com/2ndQuadrant/audit-trigger

-- Temporal script:
-------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------

SELECT audit.audit_table('user_data');
SELECT audit.audit_table('business');

-- version 0.5.0

CREATE OR REPLACE FUNCTION versioning()
RETURNS TRIGGER AS $$
DECLARE
  sys_period text;
  history_table text;
  manipulate jsonb;
  ignore_unchanged_values bool;
  commonColumns text[];
  time_stamp_to_use timestamptz;
  range_lower timestamptz;
  existing_range tstzrange;
  holder record;
  holder2 record;
  pg_version integer;
  newVersion record;
  oldVersion record;
  user_defined_system_time text;
BEGIN
  -- set custom system time if exists
  BEGIN
    SELECT current_setting('user_defined.system_time') INTO user_defined_system_time;
    IF NOT FOUND OR (user_defined_system_time <> '') IS NOT TRUE THEN
      time_stamp_to_use := CURRENT_TIMESTAMP;
    ELSE
      SELECT TO_TIMESTAMP(
          user_defined_system_time,
          'YYYY-MM-DD HH24:MI:SS.MS.US'
      ) INTO time_stamp_to_use;
    END IF;
    EXCEPTION WHEN OTHERS THEN
      time_stamp_to_use := CURRENT_TIMESTAMP;
  END;

  IF TG_WHEN != 'BEFORE' OR TG_LEVEL != 'ROW' THEN
    RAISE TRIGGER_PROTOCOL_VIOLATED USING
    MESSAGE = 'function "versioning" must be fired BEFORE ROW';
  END IF;

  IF TG_OP != 'INSERT' AND TG_OP != 'UPDATE' AND TG_OP != 'DELETE' THEN
    RAISE TRIGGER_PROTOCOL_VIOLATED USING
    MESSAGE = 'function "versioning" must be fired for INSERT or UPDATE or DELETE';
  END IF;

  IF TG_NARGS not in (3,4) THEN
    RAISE INVALID_PARAMETER_VALUE USING
    MESSAGE = 'wrong number of parameters for function "versioning"',
    HINT = 'expected 3 or 4 parameters but got ' || TG_NARGS;
  END IF;

  sys_period := TG_ARGV[0];
  history_table := TG_ARGV[1];
  ignore_unchanged_values := TG_ARGV[3];

  IF ignore_unchanged_values AND TG_OP = 'UPDATE' THEN
    IF NEW IS NOT DISTINCT FROM OLD THEN
      RETURN OLD;
    END IF;
  END IF;

  -- check if sys_period exists on original table
  SELECT atttypid, attndims INTO holder FROM pg_attribute WHERE attrelid = TG_RELID AND attname = sys_period AND NOT attisdropped;
  IF NOT FOUND THEN
    RAISE 'column "%" of relation "%" does not exist', sys_period, TG_TABLE_NAME USING
    ERRCODE = 'undefined_column';
  END IF;
  IF holder.atttypid != to_regtype('tstzrange') THEN
    IF holder.attndims > 0 THEN
      RAISE 'system period column "%" of relation "%" is not a range but an array', sys_period, TG_TABLE_NAME USING
      ERRCODE = 'datatype_mismatch';
    END IF;

    SELECT rngsubtype INTO holder2 FROM pg_range WHERE rngtypid = holder.atttypid;
    IF FOUND THEN
      RAISE 'system period column "%" of relation "%" is not a range of timestamp with timezone but of type %', sys_period, TG_TABLE_NAME, format_type(holder2.rngsubtype, null) USING
      ERRCODE = 'datatype_mismatch';
    END IF;

    RAISE 'system period column "%" of relation "%" is not a range but type %', sys_period, TG_TABLE_NAME, format_type(holder.atttypid, null) USING
    ERRCODE = 'datatype_mismatch';
  END IF;

  IF TG_OP = 'UPDATE' OR TG_OP = 'DELETE' THEN
    -- Ignore rows already modified in the current transaction
    IF OLD.xmin::text = (txid_current() % (2^32)::bigint)::text THEN
      IF TG_OP = 'DELETE' THEN
        RETURN OLD;
      END IF;

      RETURN NEW;
    END IF;

    SELECT current_setting('server_version_num')::integer
    INTO pg_version;

    -- to support postgres < 9.6
    IF pg_version < 90600 THEN
      -- check if history table exits
      IF to_regclass(history_table::cstring) IS NULL THEN
        RAISE 'relation "%" does not exist', history_table;
      END IF;
    ELSE
      IF to_regclass(history_table) IS NULL THEN
        RAISE 'relation "%" does not exist', history_table;
      END IF;
    END IF;

    -- check if history table has sys_period
    IF NOT EXISTS(SELECT * FROM pg_attribute WHERE attrelid = history_table::regclass AND attname = sys_period AND NOT attisdropped) THEN
      RAISE 'history relation "%" does not contain system period column "%"', history_table, sys_period USING
      HINT = 'history relation must contain system period column with the same name and data type as the versioned one';
    END IF;

    EXECUTE format('SELECT $1.%I', sys_period) USING OLD INTO existing_range;

    IF existing_range IS NULL THEN
      RAISE 'system period column "%" of relation "%" must not be null', sys_period, TG_TABLE_NAME USING
      ERRCODE = 'null_value_not_allowed';
    END IF;

    IF isempty(existing_range) OR NOT upper_inf(existing_range) THEN
      RAISE 'system period column "%" of relation "%" contains invalid value', sys_period, TG_TABLE_NAME USING
      ERRCODE = 'data_exception',
      DETAIL = 'valid ranges must be non-empty and unbounded on the high side';
    END IF;

    IF TG_ARGV[2] = 'true' THEN
      -- mitigate update conflicts
      range_lower := lower(existing_range);
      IF range_lower >= time_stamp_to_use THEN
        time_stamp_to_use := range_lower + interval '1 microseconds';
      END IF;
    END IF;

    WITH history AS
      (SELECT attname, atttypid
      FROM   pg_attribute
      WHERE  attrelid = history_table::regclass
      AND    attnum > 0
      AND    NOT attisdropped),
      main AS
      (SELECT attname, atttypid
      FROM   pg_attribute
      WHERE  attrelid = TG_RELID
      AND    attnum > 0
      AND    NOT attisdropped)
    SELECT
      history.attname AS history_name,
      main.attname AS main_name,
      history.atttypid AS history_type,
      main.atttypid AS main_type
    INTO holder
      FROM history
      INNER JOIN main
      ON history.attname = main.attname
    WHERE
      history.atttypid != main.atttypid;

    IF FOUND THEN
      RAISE 'column "%" of relation "%" is of type % but column "%" of history relation "%" is of type %',
        holder.main_name, TG_TABLE_NAME, format_type(holder.main_type, null), holder.history_name, history_table, format_type(holder.history_type, null)
      USING ERRCODE = 'datatype_mismatch';
    END IF;

    WITH history AS
      (SELECT attname
      FROM   pg_attribute
      WHERE  attrelid = history_table::regclass
      AND    attnum > 0
      AND    NOT attisdropped),
      main AS
      (SELECT attname
      FROM   pg_attribute
      WHERE  attrelid = TG_RELID
      AND    attnum > 0
      AND    NOT attisdropped)
    SELECT array_agg(quote_ident(history.attname)) INTO commonColumns
      FROM history
      INNER JOIN main
      ON history.attname = main.attname
      AND history.attname != sys_period;
    -- skip version if it would be identical to the previous version
    IF ignore_unchanged_values AND TG_OP = 'UPDATE' AND array_length(commonColumns, 1) > 0 THEN
      EXECUTE 'SELECT ROW($1.' || array_to_string(commonColumns , ', $1.') || ')'
        USING NEW
        INTO newVersion;
      EXECUTE 'SELECT ROW($1.' || array_to_string(commonColumns , ', $1.') || ')'
        USING OLD
        INTO oldVersion;
      IF newVersion IS NOT DISTINCT FROM oldVersion THEN
        RETURN NEW;
      END IF;
    END IF;
    EXECUTE ('INSERT INTO ' ||
      history_table ||
      '(' ||
      array_to_string(commonColumns , ',') ||
      ',' ||
      quote_ident(sys_period) ||
      ') VALUES ($1.' ||
      array_to_string(commonColumns, ',$1.') ||
      ',tstzrange($2, $3, ''[)''))')
       USING OLD, range_lower, time_stamp_to_use;
  END IF;

  IF TG_OP = 'UPDATE' OR TG_OP = 'INSERT' THEN
    manipulate := jsonb_set('{}'::jsonb, ('{' || sys_period || '}')::text[], to_jsonb(tstzrange(time_stamp_to_use, null, '[)')));

    RETURN jsonb_populate_record(NEW, manipulate);
  END IF;

  RETURN OLD;
END;
$$ LANGUAGE plpgsql;

-------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------

CREATE TABLE audit.business_history (LIKE public.business);
CREATE TABLE audit.business_inventory_item_history (LIKE public.business_inventory_item);
CREATE TABLE audit.inventory_item_history (LIKE public.inventory_item);
CREATE TABLE audit.user_data_history (LIKE public.user_data);
CREATE TABLE audit.user_business_history (LIKE public.user_business);
CREATE TABLE audit.user_role_history (LIKE public.user_role);

CREATE TRIGGER versioning_trigger
BEFORE INSERT OR UPDATE OR DELETE ON public.business
FOR EACH ROW EXECUTE PROCEDURE versioning(
  'sys_period', 'audit.business_history', true
);

CREATE TRIGGER versioning_trigger
BEFORE INSERT OR UPDATE OR DELETE ON public.business_inventory_item
FOR EACH ROW EXECUTE PROCEDURE versioning(
  'sys_period', 'audit.business_inventory_item_history', true
);

CREATE TRIGGER versioning_trigger
BEFORE INSERT OR UPDATE OR DELETE ON public.inventory_item
FOR EACH ROW EXECUTE PROCEDURE versioning(
  'sys_period', 'audit.inventory_item_history', true
);

CREATE TRIGGER versioning_trigger
BEFORE INSERT OR UPDATE OR DELETE ON public.user_data
FOR EACH ROW EXECUTE PROCEDURE versioning(
  'sys_period', 'audit.user_data_history', true
);

CREATE TRIGGER versioning_trigger
BEFORE INSERT OR UPDATE OR DELETE ON public.user_business
FOR EACH ROW EXECUTE PROCEDURE versioning(
  'sys_period', 'audit.user_business_history', true
);

CREATE TRIGGER versioning_trigger
BEFORE INSERT OR UPDATE OR DELETE ON public.user_role
FOR EACH ROW EXECUTE PROCEDURE versioning(
  'sys_period', 'audit.user_role_history', true
);