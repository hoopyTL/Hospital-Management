-- 30_demo_scenarios.sql
-- Một vài lệnh kiểm thử nhanh cho lúc demo phân hệ 1

-- 1) Grant system privilege cho user
GRANT CREATE SESSION TO DEV_A;

-- 2) Grant role cho user với admin option
GRANT RL_ANALYST TO DEV_A WITH ADMIN OPTION;

-- 3) Grant object privilege mức bảng
GRANT SELECT ON LAB_OWNER.EMPLOYEES TO DEV_B;

-- 4) Grant object privilege mức cột
-- Oracle khong co GRANT SELECT(cot) tren bang; app dung VPD (DBMS_RLS) de an cac cot khong duoc phep.
-- User SELECT truc tiep tren bang goc, cac cot bi han che tra ve NULL.
GRANT UPDATE (FULL_NAME, EMAIL) ON LAB_OWNER.EMPLOYEES TO APP_USER2;

-- 5) Grant execute trên procedure/function
GRANT EXECUTE ON LAB_OWNER.PR_RAISE_SALARY TO DEV_A;
GRANT EXECUTE ON LAB_OWNER.FN_EMP_COUNT TO DEV_A;

-- 6) Revoke thử
REVOKE RL_ANALYST FROM DEV_A;
REVOKE SELECT ON LAB_OWNER.EMPLOYEES FROM DEV_B;
