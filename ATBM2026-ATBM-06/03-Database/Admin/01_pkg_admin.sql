-- PROCEDURE.sql
-- Package PKG_ADMIN: toan bo logic nghiep vu
-- quan tri user, role, privilege tren Oracle.
-- Chay bang ATBM_ADMIN sau khi da chay 00_bootstrap_demo.sql

SET DEFINE OFF;

-- Bang ghi lai SELECT muc cot qua VPD (Oracle khong ho tro GRANT SELECT(cot) truc tiep).
-- Luu y: VPD (sec_relevant_cols_opt = ALL_ROWS) NULL hoa gia tri cot bi an,
-- nhung user van thay ten cot qua DESCRIBE. Day la thiet ke cua Oracle VPD.
BEGIN
    EXECUTE IMMEDIATE q'[
CREATE TABLE APP_VPD_COL_GRANTS (
    GRANTEE VARCHAR2(128) NOT NULL,
    OWNER VARCHAR2(128) NOT NULL,
    TABLE_NAME VARCHAR2(128) NOT NULL,
    TABLE_TYPE VARCHAR2(32) NOT NULL,
    ALLOWED_COLUMNS VARCHAR2(4000) NOT NULL,
    HIDDEN_COLUMNS VARCHAR2(4000) NOT NULL,
    POLICY_NAME VARCHAR2(128) NOT NULL,
    FUNC_NAME VARCHAR2(128) NOT NULL,
    WITH_GRANT_OPTION NUMBER(1) DEFAULT 0,
    CREATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP,
    CONSTRAINT PK_VPD_COL_GRANT PRIMARY KEY (GRANTEE, OWNER, TABLE_NAME)
)]';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE != -955 THEN
            RAISE;
        END IF;
END;
/

-- PACKAGE SPECIFICATION

CREATE OR REPLACE PACKAGE PKG_ADMIN AUTHID CURRENT_USER AS

    PROCEDURE SP_CREATE_USER(
        p_username IN VARCHAR2,
        p_password IN VARCHAR2,
        p_default_ts IN VARCHAR2 DEFAULT 'USERS',
        p_temp_ts IN VARCHAR2 DEFAULT 'TEMP',
        p_quota IN VARCHAR2 DEFAULT 'UNLIMITED'
    );

    PROCEDURE SP_ALTER_USER_PASSWORD(
        p_username IN VARCHAR2,
        p_new_password IN VARCHAR2
    );

    PROCEDURE SP_ALTER_USER_DEFAULT_TS(
        p_username IN VARCHAR2,
        p_default_ts IN VARCHAR2
    );

    PROCEDURE SP_ALTER_USER_TEMP_TS(
        p_username IN VARCHAR2,
        p_temp_ts IN VARCHAR2
    );

    PROCEDURE SP_ALTER_USER_PROFILE(
        p_username IN VARCHAR2,
        p_profile IN VARCHAR2
    );

    PROCEDURE SP_GET_TABLESPACES(
        p_cursor OUT SYS_REFCURSOR
    );

    PROCEDURE SP_GET_PROFILES(
        p_cursor OUT SYS_REFCURSOR
    );
    
    PROCEDURE SP_LOCK_USER(
        p_username IN VARCHAR2,
        p_lock IN NUMBER
    );

    PROCEDURE SP_DROP_USER(
        p_username IN VARCHAR2,
        p_cascade IN NUMBER
    );

    PROCEDURE SP_CREATE_ROLE(
        p_role_name IN VARCHAR2,
        p_password IN VARCHAR2 DEFAULT NULL
    );

    PROCEDURE SP_ALTER_ROLE_PASSWORD(
        p_role_name IN VARCHAR2,
        p_password IN VARCHAR2 DEFAULT NULL
    );

    PROCEDURE SP_DROP_ROLE(
        p_role_name IN VARCHAR2
    );

    PROCEDURE SP_GRANT_SYSTEM_PRIV(
        p_principal IN VARCHAR2,
        p_privilege IN VARCHAR2,
        p_with_admin IN NUMBER DEFAULT 0
    );

    PROCEDURE SP_GRANT_ROLE(
        p_role_name IN VARCHAR2,
        p_principal IN VARCHAR2,
        p_with_admin IN NUMBER DEFAULT 0
    );

    PROCEDURE SP_GRANT_OBJECT_PRIV(
        p_principal IN VARCHAR2,
        p_owner IN VARCHAR2,
        p_object_name IN VARCHAR2,
        p_object_type IN VARCHAR2,
        p_privilege IN VARCHAR2,
        p_columns IN VARCHAR2 DEFAULT NULL,
        p_with_grant IN NUMBER DEFAULT 0
    );

    PROCEDURE SP_REVOKE_SYSTEM_PRIV(
        p_principal IN VARCHAR2,
        p_privilege IN VARCHAR2
    );

    PROCEDURE SP_REVOKE_ROLE(
        p_role_name IN VARCHAR2,
        p_principal IN VARCHAR2
    );

    PROCEDURE SP_REVOKE_OBJECT_PRIV(
        p_principal IN VARCHAR2,
        p_owner IN VARCHAR2,
        p_object_name IN VARCHAR2,
        p_object_type IN VARCHAR2,
        p_privilege IN VARCHAR2,
        p_columns IN VARCHAR2 DEFAULT NULL
    );

    PROCEDURE SP_GET_DB_INFO(p_cursor OUT SYS_REFCURSOR);

    PROCEDURE SP_GET_USERS(
        p_keyword IN VARCHAR2 DEFAULT NULL,
        p_cursor OUT SYS_REFCURSOR
    );

    PROCEDURE SP_GET_ROLES(
        p_keyword IN VARCHAR2 DEFAULT NULL,
        p_cursor OUT SYS_REFCURSOR
    );

    PROCEDURE SP_GET_USER_NAMES(p_cursor OUT SYS_REFCURSOR);
    PROCEDURE SP_GET_ROLE_NAMES(p_cursor OUT SYS_REFCURSOR);

    PROCEDURE SP_GET_MANAGED_OBJECTS(
        p_owner IN VARCHAR2 DEFAULT NULL,
        p_cursor OUT SYS_REFCURSOR
    );

    PROCEDURE SP_GET_COLUMNS(
        p_owner IN VARCHAR2,
        p_object_name IN VARCHAR2,
        p_cursor OUT SYS_REFCURSOR
    );

    PROCEDURE SP_GET_PRINCIPAL_SYS_PRIVS(
        p_principal IN VARCHAR2,
        p_cursor OUT SYS_REFCURSOR
    );

    PROCEDURE SP_GET_PRINCIPAL_ROLE_GRANTS(
        p_principal IN VARCHAR2,
        p_cursor OUT SYS_REFCURSOR
    );

    PROCEDURE SP_GET_PRINCIPAL_OBJ_PRIVS(
        p_principal IN VARCHAR2,
        p_cursor OUT SYS_REFCURSOR
    );

    PROCEDURE SP_GET_PRINCIPAL_COL_PRIVS(
        p_principal IN VARCHAR2,
        p_cursor OUT SYS_REFCURSOR
    );

    PROCEDURE SP_GET_PRINCIPAL_SELECT_COL_GRANTS(
        p_principal IN VARCHAR2,
        p_cursor OUT SYS_REFCURSOR
    );
    

END PKG_ADMIN;
/

-- PACKAGE BODY

CREATE OR REPLACE PACKAGE BODY PKG_ADMIN AS

    -- Private helpers

    FUNCTION validate_identifier(p_value IN VARCHAR2, p_field IN VARCHAR2)
    RETURN VARCHAR2 IS
        v_norm VARCHAR2(128);
    BEGIN
        IF p_value IS NULL OR LENGTH(TRIM(p_value)) = 0 THEN
            RAISE_APPLICATION_ERROR(-20001, p_field || ' khong duoc de trong.');
        END IF;
        v_norm := UPPER(TRIM(p_value));
        IF NOT REGEXP_LIKE(v_norm, '^[A-Z][A-Z0-9_$#]{0,127}$') THEN
            RAISE_APPLICATION_ERROR(-20002, p_field || ' khong hop le. Chi ho tro dinh danh Oracle khong dau nhay kep.');
        END IF;
        RETURN v_norm;
    END validate_identifier;

    FUNCTION validate_password(p_password IN VARCHAR2)
    RETURN VARCHAR2 IS
        v_trimmed VARCHAR2(64);
    BEGIN
        IF p_password IS NULL OR LENGTH(TRIM(p_password)) = 0 THEN
            RAISE_APPLICATION_ERROR(-20003, 'Mat khau khong duoc de trong.');
        END IF;
        v_trimmed := TRIM(p_password);
        IF NOT REGEXP_LIKE(v_trimmed, '^[A-Za-z0-9_@$#.!?\-]{6,64}$') THEN
            RAISE_APPLICATION_ERROR(-20004, 'Mat khau chi nen dung ky tu chu, so va _ @ $ # . ! ? - ; do dai 6-64.');
        END IF;
        RETURN v_trimmed;
    END validate_password;

    FUNCTION validate_privilege(p_privilege IN VARCHAR2)
    RETURN VARCHAR2 IS
    BEGIN
        IF p_privilege IS NULL OR LENGTH(TRIM(p_privilege)) = 0 THEN
            RAISE_APPLICATION_ERROR(-20005, 'Privilege khong duoc de trong.');
        END IF;
        RETURN UPPER(TRIM(p_privilege));
    END validate_privilege;

    -- 8 ky tu HEX (uppercase) dung cho hau to ten policy VPD.
    FUNCTION num_to_hex8(p_num IN NUMBER) RETURN VARCHAR2 IS
        v_tmp NUMBER := TRUNC(MOD(p_num + 4294967296, 4294967296));
        v_hex VARCHAR2(8) := '';
    BEGIN
        IF v_tmp = 0 THEN
            RETURN '00000000';
        END IF;
        WHILE v_tmp > 0 LOOP
            v_hex := SUBSTR('0123456789ABCDEF', MOD(v_tmp, 16) + 1, 1) || v_hex;
            v_tmp := TRUNC(v_tmp / 16);
        END LOOP;
        RETURN LPAD(v_hex, 8, '0');
    END num_to_hex8;

    -- Ten policy VPD va function cho SELECT muc cot.
    -- Ket hop ten object + principal + hash de giảm rủi ro collision 32-bit.
    FUNCTION vpd_policy_name(
        p_owner IN VARCHAR2,
        p_object IN VARCHAR2,
        p_principal IN VARCHAR2
    ) RETURN VARCHAR2 IS
        v_seed VARCHAR2(4000);
        v_hash VARCHAR2(8);
    BEGIN
        v_seed := p_owner || '|' || p_object || '|' || p_principal;
        v_hash := num_to_hex8(DBMS_UTILITY.GET_HASH_VALUE(v_seed, 0, 2147483647));
        RETURN 'VPD_' || SUBSTR(p_object, 1, 40) || '_'
            || SUBSTR(p_principal, 1, 40) || '_' || v_hash;
    END vpd_policy_name;

    FUNCTION vpd_func_name(
        p_owner IN VARCHAR2,
        p_object IN VARCHAR2,
        p_principal IN VARCHAR2
    ) RETURN VARCHAR2 IS
    BEGIN
        RETURN 'FN_' || vpd_policy_name(p_owner, p_object, p_principal);
    END vpd_func_name;

    -- Tinh danh sach cot can AN (tat ca cot cua object tru cac cot duoc phep).
    FUNCTION get_hidden_columns(
        p_owner IN VARCHAR2,
        p_object IN VARCHAR2,
        p_allowed_csv IN VARCHAR2
    ) RETURN VARCHAR2 IS
        v_hidden VARCHAR2(4000) := '';
        v_allowed VARCHAR2(4000) := ',' || UPPER(REPLACE(p_allowed_csv, ' ', '')) || ',';
    BEGIN
        FOR r IN (
            SELECT column_name
            FROM all_tab_columns
            WHERE owner = p_owner AND table_name = p_object
            ORDER BY column_id
        ) LOOP
            IF INSTR(v_allowed, ',' || r.column_name || ',') = 0 THEN
                IF LENGTH(v_hidden) > 0 THEN
                    v_hidden := v_hidden || ',';
                END IF;
                v_hidden := v_hidden || r.column_name;
            END IF;
        END LOOP;
        RETURN v_hidden;
    END get_hidden_columns;

    -- User management

    PROCEDURE SP_CREATE_USER(
        p_username IN VARCHAR2,
        p_password IN VARCHAR2,
        p_default_ts IN VARCHAR2 DEFAULT 'USERS',
        p_temp_ts IN VARCHAR2 DEFAULT 'TEMP',
        p_quota IN VARCHAR2 DEFAULT 'UNLIMITED'
    ) IS
        v_user VARCHAR2(128) := validate_identifier(p_username, 'Username');
        v_pass VARCHAR2(64) := validate_password(p_password);
        v_dts VARCHAR2(128) := validate_identifier(p_default_ts, 'Default tablespace');
        v_tts VARCHAR2(128) := validate_identifier(p_temp_ts, 'Temporary tablespace');
    BEGIN
        EXECUTE IMMEDIATE
            'CREATE USER ' || v_user
            || ' IDENTIFIED BY "' || v_pass || '"'
            || ' DEFAULT TABLESPACE ' || v_dts
            || ' TEMPORARY TABLESPACE ' || v_tts
            || ' QUOTA ' || p_quota || ' ON ' || v_dts;
    END SP_CREATE_USER;

    PROCEDURE SP_ALTER_USER_PASSWORD(
        p_username IN VARCHAR2,
        p_new_password IN VARCHAR2
    ) IS
        v_user VARCHAR2(128) := validate_identifier(p_username, 'Username');
        v_pass VARCHAR2(64) := validate_password(p_new_password);
    BEGIN
        EXECUTE IMMEDIATE 'ALTER USER ' || v_user || ' IDENTIFIED BY "' || v_pass || '"';
    END SP_ALTER_USER_PASSWORD;

    PROCEDURE SP_ALTER_USER_DEFAULT_TS(
        p_username IN VARCHAR2,
        p_default_ts IN VARCHAR2
    ) IS
        v_user VARCHAR2(128) := validate_identifier(p_username, 'Username');
        v_dts  VARCHAR2(128) := validate_identifier(p_default_ts, 'Default tablespace');
    BEGIN
        EXECUTE IMMEDIATE 'ALTER USER ' || v_user || ' DEFAULT TABLESPACE ' || v_dts;
    END SP_ALTER_USER_DEFAULT_TS;

    PROCEDURE SP_ALTER_USER_TEMP_TS(
        p_username IN VARCHAR2,
        p_temp_ts IN VARCHAR2
    ) IS
        v_user VARCHAR2(128) := validate_identifier(p_username, 'Username');
        v_tts  VARCHAR2(128) := validate_identifier(p_temp_ts, 'Temporary tablespace');
    BEGIN
        EXECUTE IMMEDIATE 'ALTER USER ' || v_user || ' TEMPORARY TABLESPACE ' || v_tts;
    END SP_ALTER_USER_TEMP_TS;

    PROCEDURE SP_ALTER_USER_PROFILE(
        p_username IN VARCHAR2,
        p_profile IN VARCHAR2
    ) IS
        v_user    VARCHAR2(128) := validate_identifier(p_username, 'Username');
        v_profile VARCHAR2(128) := validate_identifier(p_profile, 'Profile');
    BEGIN
        EXECUTE IMMEDIATE 'ALTER USER ' || v_user || ' PROFILE ' || v_profile;
    END SP_ALTER_USER_PROFILE;

    PROCEDURE SP_GET_TABLESPACES(p_cursor OUT SYS_REFCURSOR) IS
    BEGIN
        OPEN p_cursor FOR
            SELECT tablespace_name
            FROM dba_tablespaces
            ORDER BY tablespace_name;
    END SP_GET_TABLESPACES;

    PROCEDURE SP_GET_PROFILES(p_cursor OUT SYS_REFCURSOR) IS
    BEGIN
        OPEN p_cursor FOR
            SELECT DISTINCT profile
            FROM dba_profiles
            ORDER BY profile;
    END SP_GET_PROFILES;

    PROCEDURE SP_LOCK_USER(
        p_username IN VARCHAR2,
        p_lock IN NUMBER
    ) IS
        v_user VARCHAR2(128) := validate_identifier(p_username, 'Username');
    BEGIN
        IF p_lock = 1 THEN
            EXECUTE IMMEDIATE 'ALTER USER ' || v_user || ' ACCOUNT LOCK';
        ELSE
            EXECUTE IMMEDIATE 'ALTER USER ' || v_user || ' ACCOUNT UNLOCK';
        END IF;
    END SP_LOCK_USER;

    PROCEDURE SP_DROP_USER(
        p_username IN VARCHAR2,
        p_cascade IN NUMBER
    ) IS
        v_user VARCHAR2(128) := validate_identifier(p_username, 'Username');
    BEGIN
        IF p_cascade = 1 THEN
            EXECUTE IMMEDIATE 'DROP USER ' || v_user || ' CASCADE';
        ELSE
            EXECUTE IMMEDIATE 'DROP USER ' || v_user;
        END IF;
    END SP_DROP_USER;

    -- Role management

    PROCEDURE SP_CREATE_ROLE(
        p_role_name IN VARCHAR2,
        p_password IN VARCHAR2 DEFAULT NULL
    ) IS
        v_role VARCHAR2(128) := validate_identifier(p_role_name, 'Role');
        v_pass VARCHAR2(64);
    BEGIN
        IF p_password IS NULL OR LENGTH(TRIM(p_password)) = 0 THEN
            EXECUTE IMMEDIATE 'CREATE ROLE ' || v_role;
        ELSE
            v_pass := validate_password(p_password);
            EXECUTE IMMEDIATE 'CREATE ROLE ' || v_role || ' IDENTIFIED BY "' || v_pass || '"';
        END IF;
    END SP_CREATE_ROLE;

    PROCEDURE SP_ALTER_ROLE_PASSWORD(
        p_role_name IN VARCHAR2,
        p_password IN VARCHAR2 DEFAULT NULL
    ) IS
        v_role VARCHAR2(128) := validate_identifier(p_role_name, 'Role');
        v_pass VARCHAR2(64);
    BEGIN
        IF p_password IS NULL OR LENGTH(TRIM(p_password)) = 0 THEN
            EXECUTE IMMEDIATE 'ALTER ROLE ' || v_role || ' NOT IDENTIFIED';
        ELSE
            v_pass := validate_password(p_password);
            EXECUTE IMMEDIATE 'ALTER ROLE ' || v_role || ' IDENTIFIED BY "' || v_pass || '"';
        END IF;
    END SP_ALTER_ROLE_PASSWORD;

    PROCEDURE SP_DROP_ROLE(
        p_role_name IN VARCHAR2
    ) IS
        v_role VARCHAR2(128) := validate_identifier(p_role_name, 'Role');
    BEGIN
        EXECUTE IMMEDIATE 'DROP ROLE ' || v_role;
    END SP_DROP_ROLE;

    -- Grant

    PROCEDURE SP_GRANT_SYSTEM_PRIV(
        p_principal IN VARCHAR2,
        p_privilege IN VARCHAR2,
        p_with_admin IN NUMBER DEFAULT 0
    ) IS
        v_principal VARCHAR2(128) := validate_identifier(p_principal, 'Principal');
        v_priv VARCHAR2(200) := validate_privilege(p_privilege);
        v_ddl VARCHAR2(4000);
    BEGIN
        v_ddl := 'GRANT ' || v_priv || ' TO ' || v_principal;
        IF p_with_admin = 1 THEN
            v_ddl := v_ddl || ' WITH ADMIN OPTION';
        END IF;
        EXECUTE IMMEDIATE v_ddl;
    END SP_GRANT_SYSTEM_PRIV;

    PROCEDURE SP_GRANT_ROLE(
        p_role_name IN VARCHAR2,
        p_principal IN VARCHAR2,
        p_with_admin IN NUMBER DEFAULT 0
    ) IS
        v_role VARCHAR2(128) := validate_identifier(p_role_name, 'Granted role');
        v_principal VARCHAR2(128) := validate_identifier(p_principal, 'Principal');
        v_ddl VARCHAR2(4000);
    BEGIN
        v_ddl := 'GRANT ' || v_role || ' TO ' || v_principal;
        IF p_with_admin = 1 THEN
            v_ddl := v_ddl || ' WITH ADMIN OPTION';
        END IF;
        EXECUTE IMMEDIATE v_ddl;
    END SP_GRANT_ROLE;

    PROCEDURE SP_GRANT_OBJECT_PRIV(
        p_principal IN VARCHAR2,
        p_owner IN VARCHAR2,
        p_object_name IN VARCHAR2,
        p_object_type IN VARCHAR2,
        p_privilege IN VARCHAR2,
        p_columns IN VARCHAR2 DEFAULT NULL,
        p_with_grant IN NUMBER DEFAULT 0
    ) IS
        v_principal VARCHAR2(128) := validate_identifier(p_principal, 'Principal');
        v_owner VARCHAR2(128) := validate_identifier(p_owner, 'Owner');
        v_obj VARCHAR2(128) := validate_identifier(p_object_name, 'Object name');
        v_type VARCHAR2(128) := validate_identifier(p_object_type, 'Object type');
        v_priv VARCHAR2(200) := validate_privilege(p_privilege);
        v_qualified VARCHAR2(260) := v_owner || '.' || v_obj;
        v_ddl VARCHAR2(4000);
        v_policy VARCHAR2(128);
        v_func VARCHAR2(128);
        v_hidden VARCHAR2(4000);
    BEGIN
        IF p_columns IS NOT NULL AND LENGTH(TRIM(p_columns)) > 0 THEN
            IF v_type NOT IN ('TABLE', 'VIEW') THEN
                RAISE_APPLICATION_ERROR(-20010, 'Chi TABLE/VIEW moi ho tro phan quyen muc cot.');
            END IF;
            IF v_priv NOT IN ('SELECT', 'UPDATE') THEN
                RAISE_APPLICATION_ERROR(-20011, 'Chi SELECT/UPDATE moi ho tro phan quyen muc cot.');
            END IF;

            IF v_priv = 'UPDATE' THEN
                v_ddl := 'GRANT UPDATE (' || p_columns || ') ON ' || v_qualified || ' TO ' || v_principal;
                IF p_with_grant = 1 THEN
                    v_ddl := v_ddl || ' WITH GRANT OPTION';
                END IF;
                EXECUTE IMMEDIATE v_ddl;
            ELSE
                -- SELECT muc cot: dung VPD (DBMS_RLS) de an cac cot khong duoc phep.
                v_hidden := get_hidden_columns(v_owner, v_obj, p_columns);

                v_ddl := 'GRANT SELECT ON ' || v_qualified || ' TO ' || v_principal;
                IF p_with_grant = 1 THEN
                    v_ddl := v_ddl || ' WITH GRANT OPTION';
                END IF;
                EXECUTE IMMEDIATE v_ddl;

                IF v_hidden IS NOT NULL AND LENGTH(TRIM(v_hidden)) > 0 THEN
                    v_policy := vpd_policy_name(v_owner, v_obj, v_principal);
                    v_func := vpd_func_name(v_owner, v_obj, v_principal);

                    EXECUTE IMMEDIATE
                        'CREATE OR REPLACE FUNCTION ' || v_func
                        || '(p_s VARCHAR2, p_o VARCHAR2) RETURN VARCHAR2 AS '
                        || 'BEGIN '
                        || 'IF SYS_CONTEXT(''USERENV'',''SESSION_USER'') = '''
                        || v_principal || ''' THEN RETURN ''1=1''; END IF; '
                        || 'RETURN NULL; END;';

                    BEGIN
                        DBMS_RLS.DROP_POLICY(v_owner, v_obj, v_policy);
                    EXCEPTION WHEN OTHERS THEN NULL;
                    END;

                    DBMS_RLS.ADD_POLICY(
                        object_schema       => v_owner,
                        object_name         => v_obj,
                        policy_name         => v_policy,
                        function_schema     => USER,
                        policy_function     => v_func,
                        statement_types     => 'SELECT',
                        sec_relevant_cols   => v_hidden,
                        sec_relevant_cols_opt => DBMS_RLS.ALL_ROWS
                    );

                    DELETE FROM APP_VPD_COL_GRANTS
                     WHERE GRANTEE = v_principal AND OWNER = v_owner AND TABLE_NAME = v_obj;
                    INSERT INTO APP_VPD_COL_GRANTS (
                        GRANTEE, OWNER, TABLE_NAME, TABLE_TYPE,
                        ALLOWED_COLUMNS, HIDDEN_COLUMNS,
                        POLICY_NAME, FUNC_NAME, WITH_GRANT_OPTION
                    ) VALUES (
                        v_principal, v_owner, v_obj, v_type,
                        p_columns, v_hidden,
                        v_policy, v_func, p_with_grant
                    );
                END IF;
            END IF;
        ELSE
            v_ddl := 'GRANT ' || v_priv || ' ON ' || v_qualified || ' TO ' || v_principal;
            IF p_with_grant = 1 THEN
                v_ddl := v_ddl || ' WITH GRANT OPTION';
            END IF;
            EXECUTE IMMEDIATE v_ddl;
        END IF;
    END SP_GRANT_OBJECT_PRIV;

    -- Revoke

    PROCEDURE SP_REVOKE_SYSTEM_PRIV(
        p_principal IN VARCHAR2,
        p_privilege IN VARCHAR2
    ) IS
        v_principal VARCHAR2(128) := validate_identifier(p_principal, 'Principal');
        v_priv VARCHAR2(200) := validate_privilege(p_privilege);
    BEGIN
        EXECUTE IMMEDIATE 'REVOKE ' || v_priv || ' FROM ' || v_principal;
    END SP_REVOKE_SYSTEM_PRIV;

    PROCEDURE SP_REVOKE_ROLE(
        p_role_name IN VARCHAR2,
        p_principal IN VARCHAR2
    ) IS
        v_role VARCHAR2(128) := validate_identifier(p_role_name, 'Granted role');
        v_principal VARCHAR2(128) := validate_identifier(p_principal, 'Principal');
    BEGIN
        EXECUTE IMMEDIATE 'REVOKE ' || v_role || ' FROM ' || v_principal;
    END SP_REVOKE_ROLE;

    PROCEDURE SP_REVOKE_OBJECT_PRIV(
        p_principal IN VARCHAR2,
        p_owner IN VARCHAR2,
        p_object_name IN VARCHAR2,
        p_object_type IN VARCHAR2,
        p_privilege IN VARCHAR2,
        p_columns IN VARCHAR2 DEFAULT NULL
    ) IS
        v_principal VARCHAR2(128) := validate_identifier(p_principal, 'Principal');
        v_owner VARCHAR2(128) := validate_identifier(p_owner, 'Owner');
        v_obj VARCHAR2(128) := validate_identifier(p_object_name, 'Object name');
        v_type VARCHAR2(128) := validate_identifier(p_object_type, 'Object type');
        v_priv VARCHAR2(200) := validate_privilege(p_privilege);
        v_qualified VARCHAR2(260) := v_owner || '.' || v_obj;
        v_ddl VARCHAR2(4000);
        v_policy VARCHAR2(128);
        v_func VARCHAR2(128);
    BEGIN
        IF p_columns IS NOT NULL AND LENGTH(TRIM(p_columns)) > 0 THEN
            IF v_type NOT IN ('TABLE', 'VIEW') THEN
                RAISE_APPLICATION_ERROR(-20010, 'Chi TABLE/VIEW moi ho tro thu hoi quyen muc cot.');
            END IF;
            IF v_priv NOT IN ('SELECT', 'UPDATE') THEN
                RAISE_APPLICATION_ERROR(-20011, 'Chi SELECT/UPDATE moi ho tro thu hoi quyen muc cot.');
            END IF;

            IF v_priv = 'UPDATE' THEN
                -- Oracle khong ho tro REVOKE theo cot; phai REVOKE toan bo UPDATE tren object.
                v_ddl := 'REVOKE UPDATE ON ' || v_qualified || ' FROM ' || v_principal;
                EXECUTE IMMEDIATE v_ddl;
            ELSE
                -- Thu hoi VPD policy + REVOKE SELECT
                v_policy := vpd_policy_name(v_owner, v_obj, v_principal);
                v_func := vpd_func_name(v_owner, v_obj, v_principal);

                BEGIN
                    DBMS_RLS.DROP_POLICY(v_owner, v_obj, v_policy);
                EXCEPTION WHEN OTHERS THEN NULL;
                END;

                BEGIN
                    EXECUTE IMMEDIATE 'DROP FUNCTION ' || v_func;
                EXCEPTION WHEN OTHERS THEN NULL;
                END;

                EXECUTE IMMEDIATE 'REVOKE SELECT ON ' || v_qualified || ' FROM ' || v_principal;

                DELETE FROM APP_VPD_COL_GRANTS
                 WHERE GRANTEE = v_principal AND OWNER = v_owner AND TABLE_NAME = v_obj;
            END IF;
        ELSE
            v_ddl := 'REVOKE ' || v_priv || ' ON ' || v_qualified || ' FROM ' || v_principal;
            EXECUTE IMMEDIATE v_ddl;
        END IF;
    END SP_REVOKE_OBJECT_PRIV;

    -- Queries

    PROCEDURE SP_GET_DB_INFO(p_cursor OUT SYS_REFCURSOR) IS
    BEGIN
        OPEN p_cursor FOR
            SELECT
                SYS_CONTEXT('USERENV', 'SESSION_USER') AS SESSION_USER,
                SYS_CONTEXT('USERENV', 'DB_NAME') AS DB_NAME,
                SYSDATE AS SERVER_TIME,
                (SELECT banner FROM v$version WHERE ROWNUM = 1) AS VERSION_BANNER
            FROM dual;
    END SP_GET_DB_INFO;

    PROCEDURE SP_GET_USERS(
        p_keyword IN VARCHAR2 DEFAULT NULL,
        p_cursor OUT SYS_REFCURSOR
    ) IS
        v_kw VARCHAR2(200);
    BEGIN
        v_kw := UPPER(TRIM(p_keyword));
        OPEN p_cursor FOR
            SELECT username, account_status, created, default_tablespace, temporary_tablespace, profile
            FROM dba_users
            WHERE (NVL(oracle_maintained, 'N') = 'N')
              AND (v_kw IS NULL OR UPPER(username) LIKE '%' || v_kw || '%')
            ORDER BY username;
    END SP_GET_USERS;

    PROCEDURE SP_GET_ROLES(
        p_keyword IN VARCHAR2 DEFAULT NULL,
        p_cursor OUT SYS_REFCURSOR
    ) IS
        v_kw VARCHAR2(200);
    BEGIN
        v_kw := UPPER(TRIM(p_keyword));
        OPEN p_cursor FOR
            SELECT role, password_required, authentication_type
            FROM dba_roles
            WHERE (NVL(oracle_maintained, 'N') = 'N')
              AND (v_kw IS NULL OR UPPER(role) LIKE '%' || v_kw || '%')
            ORDER BY role;
    END SP_GET_ROLES;

    PROCEDURE SP_GET_USER_NAMES(p_cursor OUT SYS_REFCURSOR) IS
    BEGIN
        OPEN p_cursor FOR
            SELECT username
            FROM dba_users
            WHERE NVL(oracle_maintained, 'N') = 'N'
            ORDER BY username;
    END SP_GET_USER_NAMES;

    PROCEDURE SP_GET_ROLE_NAMES(p_cursor OUT SYS_REFCURSOR) IS
    BEGIN
        OPEN p_cursor FOR
            SELECT role
            FROM dba_roles
            WHERE NVL(oracle_maintained, 'N') = 'N'
            ORDER BY role;
    END SP_GET_ROLE_NAMES;

    PROCEDURE SP_GET_MANAGED_OBJECTS(
        p_owner IN VARCHAR2 DEFAULT NULL,
        p_cursor OUT SYS_REFCURSOR
    ) IS
        v_owner VARCHAR2(128);
    BEGIN
        v_owner := UPPER(TRIM(p_owner));
        -- Mặc định hiển thị object mẫu trong schema lab.
        IF v_owner IS NULL OR LENGTH(TRIM(v_owner)) = 0 THEN
            v_owner := 'LAB_OWNER';
        END IF;
        OPEN p_cursor FOR
            SELECT owner, object_name, object_type, status, created
            FROM dba_objects
            WHERE object_type IN ('TABLE', 'VIEW', 'PROCEDURE', 'FUNCTION')
            AND owner NOT IN ('SYS', 'SYSTEM', 'XDB', 'MDSYS', 'CTXSYS', 'DBSNMP', 'OUTLN')
            AND owner = v_owner
            ORDER BY owner, object_type, object_name;
    END SP_GET_MANAGED_OBJECTS;

    PROCEDURE SP_GET_COLUMNS(
        p_owner IN VARCHAR2,
        p_object_name IN VARCHAR2,
        p_cursor OUT SYS_REFCURSOR
    ) IS
        v_owner VARCHAR2(128) := validate_identifier(p_owner, 'Owner');
        v_obj VARCHAR2(128) := validate_identifier(p_object_name, 'Object name');
    BEGIN
        OPEN p_cursor FOR
            SELECT column_name
            FROM all_tab_columns
            WHERE owner = v_owner AND table_name = v_obj
            ORDER BY column_id;
    END SP_GET_COLUMNS;

    PROCEDURE SP_GET_PRINCIPAL_SYS_PRIVS(
        p_principal IN VARCHAR2,
        p_cursor OUT SYS_REFCURSOR
    ) IS
        v_p VARCHAR2(128) := validate_identifier(p_principal, 'Principal');
    BEGIN
        OPEN p_cursor FOR
            SELECT grantee, privilege, admin_option, 'DIRECT' AS source, CAST(NULL AS VARCHAR2(128)) AS via_role
            FROM dba_sys_privs
            WHERE grantee = v_p
            UNION ALL
            SELECT v_p AS grantee, sp.privilege, sp.admin_option, 'VIA_ROLE' AS source, rp.granted_role AS via_role
            FROM dba_role_privs rp
            JOIN dba_sys_privs sp ON sp.grantee = rp.granted_role
            WHERE rp.grantee = v_p
            ORDER BY privilege, source, via_role;
    END SP_GET_PRINCIPAL_SYS_PRIVS;

    PROCEDURE SP_GET_PRINCIPAL_ROLE_GRANTS(
        p_principal IN VARCHAR2,
        p_cursor OUT SYS_REFCURSOR
    ) IS
        v_p VARCHAR2(128) := validate_identifier(p_principal, 'Principal');
    BEGIN
        OPEN p_cursor FOR
            SELECT grantee, granted_role, admin_option, default_role
            FROM dba_role_privs
            WHERE grantee = v_p
            ORDER BY granted_role;
    END SP_GET_PRINCIPAL_ROLE_GRANTS;

    PROCEDURE SP_GET_PRINCIPAL_OBJ_PRIVS(
        p_principal IN VARCHAR2,
        p_cursor OUT SYS_REFCURSOR
    ) IS
        v_p VARCHAR2(128) := validate_identifier(p_principal, 'Principal');
    BEGIN
        OPEN p_cursor FOR
            SELECT grantee, owner, table_name, privilege, grantable, 'DIRECT' AS source, CAST(NULL AS VARCHAR2(128)) AS via_role
            FROM dba_tab_privs
            WHERE grantee = v_p
            UNION ALL
            SELECT v_p AS grantee, tp.owner, tp.table_name, tp.privilege, tp.grantable, 'VIA_ROLE' AS source, rp.granted_role AS via_role
            FROM dba_role_privs rp
            JOIN dba_tab_privs tp ON tp.grantee = rp.granted_role
            WHERE rp.grantee = v_p
            ORDER BY owner, table_name, privilege, source, via_role;
    END SP_GET_PRINCIPAL_OBJ_PRIVS;

    PROCEDURE SP_GET_PRINCIPAL_COL_PRIVS(
        p_principal IN VARCHAR2,
        p_cursor OUT SYS_REFCURSOR
    ) IS
        v_p VARCHAR2(128) := validate_identifier(p_principal, 'Principal');
    BEGIN
        OPEN p_cursor FOR
            SELECT grantee, owner, table_name, column_name, privilege, grantable, 'DIRECT' AS source, CAST(NULL AS VARCHAR2(128)) AS via_role
            FROM dba_col_privs
            WHERE grantee = v_p
            UNION ALL
            SELECT v_p AS grantee, cp.owner, cp.table_name, cp.column_name, cp.privilege, cp.grantable, 'VIA_ROLE' AS source, rp.granted_role AS via_role
            FROM dba_role_privs rp
            JOIN dba_col_privs cp ON cp.grantee = rp.granted_role
            WHERE rp.grantee = v_p
            UNION ALL
            SELECT g.grantee, g.owner, g.table_name,
                   TRIM(REGEXP_SUBSTR(g.allowed_columns, '[^,]+', 1, lvl.n)) AS column_name,
                   'SELECT' AS privilege,
                   CASE WHEN g.with_grant_option = 1 THEN 'YES' ELSE 'NO' END AS grantable,
                   'VPD' AS source,
                   CAST(NULL AS VARCHAR2(128)) AS via_role
            FROM app_vpd_col_grants g,
                 (SELECT LEVEL AS n FROM dual CONNECT BY LEVEL <= 100) lvl
            WHERE g.grantee = v_p
              AND lvl.n <= REGEXP_COUNT(g.allowed_columns, ',') + 1
              AND TRIM(REGEXP_SUBSTR(g.allowed_columns, '[^,]+', 1, lvl.n)) IS NOT NULL
            ORDER BY owner, table_name, column_name, privilege, source, via_role;
    END SP_GET_PRINCIPAL_COL_PRIVS;

    PROCEDURE SP_GET_PRINCIPAL_SELECT_COL_GRANTS(
        p_principal IN VARCHAR2,
        p_cursor OUT SYS_REFCURSOR
    ) IS
        v_p VARCHAR2(128) := validate_identifier(p_principal, 'Principal');
    BEGIN
        OPEN p_cursor FOR
            SELECT
                grantee,
                owner,
                table_name,
                table_type,
                allowed_columns,
                hidden_columns,
                policy_name,
                CASE WHEN with_grant_option = 1 THEN 'YES' ELSE 'NO' END AS with_grant,
                created_at
            FROM app_vpd_col_grants
            WHERE grantee = v_p
            ORDER BY owner, table_name;
    END SP_GET_PRINCIPAL_SELECT_COL_GRANTS;

    

END PKG_ADMIN;
/

BEGIN DBMS_OUTPUT.PUT_LINE('PKG_ADMIN da duoc tao thanh cong.'); END;
/
