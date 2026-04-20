-- 10_verify_demo.sql
-- Chạy bằng tài khoản có quyền DBA

SET SERVEROUTPUT ON;

COLUMN username FORMAT A20
COLUMN role FORMAT A20
COLUMN owner FORMAT A20
COLUMN table_name FORMAT A30
COLUMN privilege FORMAT A20

BEGIN DBMS_OUTPUT.PUT_LINE('USERS'); END;
/
SELECT username, account_status FROM dba_users
WHERE username IN ('ATBM_ADMIN','LAB_OWNER','DEV_A','DEV_B','APP_USER1','APP_USER2')
ORDER BY username;

BEGIN DBMS_OUTPUT.PUT_LINE('ROLES'); END;
/
SELECT role, password_required, authentication_type
FROM dba_roles
WHERE role IN ('RL_READONLY','RL_ANALYST','RL_PROGRAM')
ORDER BY role;

BEGIN DBMS_OUTPUT.PUT_LINE('OBJECTS'); END;
/
SELECT owner, object_name, object_type, status
FROM dba_objects
WHERE owner = 'LAB_OWNER'
  AND object_type IN ('TABLE','VIEW','PROCEDURE','FUNCTION')
ORDER BY object_type, object_name;

BEGIN DBMS_OUTPUT.PUT_LINE('SAMPLE PRIVILEGES'); END;
/
SELECT grantee, owner, table_name, privilege, grantable
FROM dba_tab_privs
WHERE grantee IN ('RL_READONLY','RL_ANALYST')
ORDER BY grantee, table_name, privilege;

SELECT grantee, granted_role, admin_option
FROM dba_role_privs
WHERE grantee IN ('DEV_A','DEV_B','APP_USER1')
ORDER BY grantee, granted_role;
