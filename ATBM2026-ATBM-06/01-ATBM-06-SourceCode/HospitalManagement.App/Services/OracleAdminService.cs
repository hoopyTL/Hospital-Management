using System.Data;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using HospitalManagement.App.DataAccess;
using HospitalManagement.App.Models;
using HospitalManagement.App.Helpers;

namespace HospitalManagement.App.Services;

public class OracleAdminService
{
    private const string RequiredAdminUsername = "ATBM_ADMIN";
    private readonly OracleConnectionFactory _factory;

    public static readonly string[] CommonSystemPrivileges =
    [
        "CREATE SESSION",
        "CREATE TABLE",
        "CREATE VIEW",
        "CREATE PROCEDURE",
        "CREATE ROLE",
        "ALTER USER",
        "DROP USER",
        "SELECT ANY TABLE",
        "INSERT ANY TABLE",
        "UPDATE ANY TABLE",
        "DELETE ANY TABLE",
        "EXECUTE ANY PROCEDURE",
        "UNLIMITED TABLESPACE"
    ];

    public static readonly string[] ObjectPrivilegesForTableOrView =
    [
        "SELECT", "INSERT", "UPDATE", "DELETE"
    ];

    public static readonly string[] ObjectPrivilegesForProgramUnit =
    [
        "EXECUTE"
    ];

    public OracleAdminService(DbConnectionSettings settings)
    {
        if (!string.Equals(settings.Username?.Trim(), RequiredAdminUsername, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException($"Ung dung chi cho phep dang nhap bang tai khoan {RequiredAdminUsername}.");
        }

        settings.Username = RequiredAdminUsername;
        settings.UseSysDba = false;
        _factory = new OracleConnectionFactory(settings);
    }

    public void TestConnection()
    {
        using var conn = _factory.CreateOpenConnection();
    }

    // Queries

    public DataTable GetDatabaseInfo()
    {
        return ExecuteProcedureCursor("PKG_ADMIN.SP_GET_DB_INFO", "p_cursor");
    }

    public DataTable GetUsers(string? keyword = null)
    {
        return ExecuteProcedureCursor("PKG_ADMIN.SP_GET_USERS", "p_cursor",
            ("p_keyword", OracleDbType.Varchar2,
             string.IsNullOrWhiteSpace(keyword)
                ? DBNull.Value
                : keyword!.Trim().ToUpperInvariant()));
    }

    public DataTable GetRoles(string? keyword = null)
    {
        return ExecuteProcedureCursor("PKG_ADMIN.SP_GET_ROLES", "p_cursor",
            ("p_keyword", OracleDbType.Varchar2,
             string.IsNullOrWhiteSpace(keyword)
                ? DBNull.Value
                : keyword!.Trim().ToUpperInvariant()));
    }

    public List<string> GetUserNames()
    {
        return ExecuteProcedureScalarList("PKG_ADMIN.SP_GET_USER_NAMES", "p_cursor");
    }

    public List<string> GetRoleNames()
    {
        return ExecuteProcedureScalarList("PKG_ADMIN.SP_GET_ROLE_NAMES", "p_cursor");
    }

    public DataTable GetManagedObjects(string? owner = null)
    {
        return ExecuteProcedureCursor("PKG_ADMIN.SP_GET_MANAGED_OBJECTS", "p_cursor",
            ("p_owner", OracleDbType.Varchar2,
             string.IsNullOrWhiteSpace(owner)
                ? DBNull.Value
                : owner!.Trim().ToUpperInvariant()));
    }

    public List<string> GetColumns(string owner, string objectName)
    {
        owner = IdentifierValidator.NormalizeSimpleIdentifier(owner, "Owner");
        objectName = IdentifierValidator.NormalizeSimpleIdentifier(objectName, "Object name");

        return ExecuteProcedureScalarList("PKG_ADMIN.SP_GET_COLUMNS", "p_cursor",
            ("p_owner", OracleDbType.Varchar2, (object)owner),
            ("p_object_name", OracleDbType.Varchar2, (object)objectName));
    }

    public DataTable GetPrincipalSystemPrivileges(string principal)
    {
        principal = IdentifierValidator.NormalizeSimpleIdentifier(principal, "Principal");
        return ExecuteProcedureCursor("PKG_ADMIN.SP_GET_PRINCIPAL_SYS_PRIVS", "p_cursor",
            ("p_principal", OracleDbType.Varchar2, (object)principal));
    }

    public DataTable GetPrincipalRoleGrants(string principal)
    {
        principal = IdentifierValidator.NormalizeSimpleIdentifier(principal, "Principal");
        return ExecuteProcedureCursor("PKG_ADMIN.SP_GET_PRINCIPAL_ROLE_GRANTS", "p_cursor",
            ("p_principal", OracleDbType.Varchar2, (object)principal));
    }

    public DataTable GetPrincipalObjectPrivileges(string principal)
    {
        principal = IdentifierValidator.NormalizeSimpleIdentifier(principal, "Principal");
        return ExecuteProcedureCursor("PKG_ADMIN.SP_GET_PRINCIPAL_OBJ_PRIVS", "p_cursor",
            ("p_principal", OracleDbType.Varchar2, (object)principal));
    }

    public DataTable GetPrincipalColumnPrivileges(string principal)
    {
        principal = IdentifierValidator.NormalizeSimpleIdentifier(principal, "Principal");
        return ExecuteProcedureCursor("PKG_ADMIN.SP_GET_PRINCIPAL_COL_PRIVS", "p_cursor",
            ("p_principal", OracleDbType.Varchar2, (object)principal));
    }

    public DataTable GetTablespaces()
    {
        return ExecuteProcedureCursor("PKG_ADMIN.SP_GET_TABLESPACES", "p_cursor");
    }

    public DataTable GetProfiles()
    {
        return ExecuteProcedureCursor("PKG_ADMIN.SP_GET_PROFILES", "p_cursor");
    }
    // User management

    public void CreateUser(string username, string password,
        string defaultTablespace = "USERS", string temporaryTablespace = "TEMP",
        string quota = "UNLIMITED")
    {
        username = IdentifierValidator.NormalizeSimpleIdentifier(username, "Username");
        password = IdentifierValidator.NormalizePassword(password);
        defaultTablespace = IdentifierValidator.NormalizeSimpleIdentifier(defaultTablespace, "Default tablespace");
        temporaryTablespace = IdentifierValidator.NormalizeSimpleIdentifier(temporaryTablespace, "Temporary tablespace");

        ExecuteProcedure("PKG_ADMIN.SP_CREATE_USER",
            ("p_username",   OracleDbType.Varchar2, (object)username),
            ("p_password",   OracleDbType.Varchar2, (object)password),
            ("p_default_ts", OracleDbType.Varchar2, (object)defaultTablespace),
            ("p_temp_ts",    OracleDbType.Varchar2, (object)temporaryTablespace),
            ("p_quota",      OracleDbType.Varchar2, (object)quota));
    }

    public void AlterUserPassword(string username, string newPassword)
    {
        username = IdentifierValidator.NormalizeSimpleIdentifier(username, "Username");
        newPassword = IdentifierValidator.NormalizePassword(newPassword);

        ExecuteProcedure("PKG_ADMIN.SP_ALTER_USER_PASSWORD",
            ("p_username",     OracleDbType.Varchar2, (object)username),
            ("p_new_password", OracleDbType.Varchar2, (object)newPassword));
    }
    public void AlterUserDefaultTablespace(string username, string defaultTablespace)
    {
        username = IdentifierValidator.NormalizeSimpleIdentifier(username, "Username");
        defaultTablespace = IdentifierValidator.NormalizeSimpleIdentifier(defaultTablespace, "Default tablespace");

        ExecuteProcedure("PKG_ADMIN.SP_ALTER_USER_DEFAULT_TS",
            ("p_username",   OracleDbType.Varchar2, (object)username),
            ("p_default_ts", OracleDbType.Varchar2, (object)defaultTablespace));
    }

    public void AlterUserTemporaryTablespace(string username, string temporaryTablespace)
    {
        username = IdentifierValidator.NormalizeSimpleIdentifier(username, "Username");
        temporaryTablespace = IdentifierValidator.NormalizeSimpleIdentifier(temporaryTablespace, "Temporary tablespace");

        ExecuteProcedure("PKG_ADMIN.SP_ALTER_USER_TEMP_TS",
            ("p_username", OracleDbType.Varchar2, (object)username),
            ("p_temp_ts",  OracleDbType.Varchar2, (object)temporaryTablespace));
    }

    public void AlterUserProfile(string username, string profile)
    {
        username = IdentifierValidator.NormalizeSimpleIdentifier(username, "Username");
        profile = IdentifierValidator.NormalizeSimpleIdentifier(profile, "Profile");

        ExecuteProcedure("PKG_ADMIN.SP_ALTER_USER_PROFILE",
            ("p_username", OracleDbType.Varchar2, (object)username),
            ("p_profile",  OracleDbType.Varchar2, (object)profile));
    }
    public void LockUser(string username, bool lockUser)
    {
        username = IdentifierValidator.NormalizeSimpleIdentifier(username, "Username");

        ExecuteProcedure("PKG_ADMIN.SP_LOCK_USER",
            ("p_username", OracleDbType.Varchar2, (object)username),
            ("p_lock",     OracleDbType.Int32,    lockUser ? 1 : 0));
    }

    public void DropUser(string username, bool cascade)
    {
        username = IdentifierValidator.NormalizeSimpleIdentifier(username, "Username");

        ExecuteProcedure("PKG_ADMIN.SP_DROP_USER",
            ("p_username", OracleDbType.Varchar2, (object)username),
            ("p_cascade",  OracleDbType.Int32,    cascade ? 1 : 0));
    }

    // Role management

    public void CreateRole(string roleName, string? password = null)
    {
        roleName = IdentifierValidator.NormalizeSimpleIdentifier(roleName, "Role");
        if (!string.IsNullOrWhiteSpace(password))
            password = IdentifierValidator.NormalizePassword(password);

        ExecuteProcedure("PKG_ADMIN.SP_CREATE_ROLE",
            ("p_role_name", OracleDbType.Varchar2, (object)roleName),
            ("p_password",  OracleDbType.Varchar2,
             string.IsNullOrWhiteSpace(password) ? DBNull.Value : (object)password!));
    }

    public void AlterRolePassword(string roleName, string? password = null)
    {
        roleName = IdentifierValidator.NormalizeSimpleIdentifier(roleName, "Role");
        if (!string.IsNullOrWhiteSpace(password))
            password = IdentifierValidator.NormalizePassword(password);

        ExecuteProcedure("PKG_ADMIN.SP_ALTER_ROLE_PASSWORD",
            ("p_role_name", OracleDbType.Varchar2, (object)roleName),
            ("p_password",  OracleDbType.Varchar2,
             string.IsNullOrWhiteSpace(password) ? DBNull.Value : (object)password!));
    }

    public void DropRole(string roleName)
    {
        roleName = IdentifierValidator.NormalizeSimpleIdentifier(roleName, "Role");

        ExecuteProcedure("PKG_ADMIN.SP_DROP_ROLE",
            ("p_role_name", OracleDbType.Varchar2, (object)roleName));
    }

    // Grant

    public void GrantSystemPrivilege(string principal, string privilege, bool withAdminOption)
    {
        principal = IdentifierValidator.NormalizeSimpleIdentifier(principal, "Principal");
        privilege = IdentifierValidator.NormalizePrivilege(privilege);

        ExecuteProcedure("PKG_ADMIN.SP_GRANT_SYSTEM_PRIV",
            ("p_principal",  OracleDbType.Varchar2, (object)principal),
            ("p_privilege",  OracleDbType.Varchar2, (object)privilege),
            ("p_with_admin", OracleDbType.Int32,    withAdminOption ? 1 : 0));
    }

    public void GrantRole(string roleName, string principal, bool withAdminOption)
    {
        roleName = IdentifierValidator.NormalizeSimpleIdentifier(roleName, "Granted role");
        principal = IdentifierValidator.NormalizeSimpleIdentifier(principal, "Principal");

        ExecuteProcedure("PKG_ADMIN.SP_GRANT_ROLE",
            ("p_role_name",  OracleDbType.Varchar2, (object)roleName),
            ("p_principal",  OracleDbType.Varchar2, (object)principal),
            ("p_with_admin", OracleDbType.Int32,    withAdminOption ? 1 : 0));
    }

    public void GrantObjectPrivilege(string principal, string owner, string objectName,
        string objectType, string privilege, IEnumerable<string>? columns, bool withGrantOption)
    {
        principal = IdentifierValidator.NormalizeSimpleIdentifier(principal, "Principal");
        owner = IdentifierValidator.NormalizeSimpleIdentifier(owner, "Owner");
        objectName = IdentifierValidator.NormalizeSimpleIdentifier(objectName, "Object name");
        objectType = IdentifierValidator.NormalizeSimpleIdentifier(objectType, "Object type");
        privilege = IdentifierValidator.NormalizePrivilege(privilege);

        var normalizedColumns = IdentifierValidator.NormalizeColumns(columns ?? []);
        var columnsCsv = normalizedColumns.Count > 0
            ? string.Join(", ", normalizedColumns)
            : null;

        ExecuteProcedure("PKG_ADMIN.SP_GRANT_OBJECT_PRIV",
            ("p_principal",   OracleDbType.Varchar2, (object)principal),
            ("p_owner",       OracleDbType.Varchar2, (object)owner),
            ("p_object_name", OracleDbType.Varchar2, (object)objectName),
            ("p_object_type", OracleDbType.Varchar2, (object)objectType),
            ("p_privilege",   OracleDbType.Varchar2, (object)privilege),
            ("p_columns",     OracleDbType.Varchar2, columnsCsv == null ? DBNull.Value : (object)columnsCsv),
            ("p_with_grant",  OracleDbType.Int32,    withGrantOption ? 1 : 0));
    }

    // Revoke

    public void RevokeSystemPrivilege(string principal, string privilege)
    {
        principal = IdentifierValidator.NormalizeSimpleIdentifier(principal, "Principal");
        privilege = IdentifierValidator.NormalizePrivilege(privilege);

        ExecuteProcedure("PKG_ADMIN.SP_REVOKE_SYSTEM_PRIV",
            ("p_principal", OracleDbType.Varchar2, (object)principal),
            ("p_privilege", OracleDbType.Varchar2, (object)privilege));
    }

    public void RevokeRole(string roleName, string principal)
    {
        roleName = IdentifierValidator.NormalizeSimpleIdentifier(roleName, "Granted role");
        principal = IdentifierValidator.NormalizeSimpleIdentifier(principal, "Principal");

        ExecuteProcedure("PKG_ADMIN.SP_REVOKE_ROLE",
            ("p_role_name", OracleDbType.Varchar2, (object)roleName),
            ("p_principal", OracleDbType.Varchar2, (object)principal));
    }

    public void RevokeObjectPrivilege(string principal, string owner, string objectName,
        string objectType, string privilege, IEnumerable<string>? columns)
    {
        principal = IdentifierValidator.NormalizeSimpleIdentifier(principal, "Principal");
        owner = IdentifierValidator.NormalizeSimpleIdentifier(owner, "Owner");
        objectName = IdentifierValidator.NormalizeSimpleIdentifier(objectName, "Object name");
        objectType = IdentifierValidator.NormalizeSimpleIdentifier(objectType, "Object type");
        privilege = IdentifierValidator.NormalizePrivilege(privilege);

        var normalizedColumns = IdentifierValidator.NormalizeColumns(columns ?? []);
        var columnsCsv = normalizedColumns.Count > 0
            ? string.Join(", ", normalizedColumns)
            : null;

        ExecuteProcedure("PKG_ADMIN.SP_REVOKE_OBJECT_PRIV",
            ("p_principal",   OracleDbType.Varchar2, (object)principal),
            ("p_owner",       OracleDbType.Varchar2, (object)owner),
            ("p_object_name", OracleDbType.Varchar2, (object)objectName),
            ("p_object_type", OracleDbType.Varchar2, (object)objectType),
            ("p_privilege",   OracleDbType.Varchar2, (object)privilege),
            ("p_columns",     OracleDbType.Varchar2, columnsCsv == null ? DBNull.Value : (object)columnsCsv));
    }

    // Private helpers: goi stored procedure

    private void ExecuteProcedure(string procedureName,
        params (string Name, OracleDbType DbType, object Value)[] parameters)
    {
        using var conn = _factory.CreateOpenConnection();
        using var cmd = new OracleCommand
        {
            Connection = conn,
            CommandText = procedureName,
            CommandType = CommandType.StoredProcedure,
            BindByName = true
        };

        foreach (var (name, dbType, value) in parameters)
        {
            var p = new OracleParameter(name, dbType) { Direction = ParameterDirection.Input, Value = value ?? DBNull.Value };
            cmd.Parameters.Add(p);
        }

        cmd.ExecuteNonQuery();
    }

    private DataTable ExecuteProcedureCursor(string procedureName, string cursorParamName,
        params (string Name, OracleDbType DbType, object Value)[] inputParameters)
    {
        using var conn = _factory.CreateOpenConnection();
        using var cmd = new OracleCommand
        {
            Connection = conn,
            CommandText = procedureName,
            CommandType = CommandType.StoredProcedure,
            BindByName = true
        };

        foreach (var (name, dbType, value) in inputParameters)
        {
            var p = new OracleParameter(name, dbType) { Direction = ParameterDirection.Input, Value = value ?? DBNull.Value };
            cmd.Parameters.Add(p);
        }

        cmd.Parameters.Add(cursorParamName, OracleDbType.RefCursor).Direction = ParameterDirection.Output;

        using var adapter = new OracleDataAdapter(cmd);
        var table = new DataTable();
        adapter.Fill(table);
        return table;
    }

    private List<string> ExecuteProcedureScalarList(string procedureName, string cursorParamName,
        params (string Name, OracleDbType DbType, object Value)[] inputParameters)
    {
        using var conn = _factory.CreateOpenConnection();
        using var cmd = new OracleCommand
        {
            Connection = conn,
            CommandText = procedureName,
            CommandType = CommandType.StoredProcedure,
            BindByName = true
        };

        foreach (var (name, dbType, value) in inputParameters)
        {
            var p = new OracleParameter(name, dbType) { Direction = ParameterDirection.Input, Value = value ?? DBNull.Value };
            cmd.Parameters.Add(p);
        }

        cmd.Parameters.Add(cursorParamName, OracleDbType.RefCursor).Direction = ParameterDirection.Output;

        var results = new List<string>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            results.Add(reader.GetString(0));
        }
        return results;
    }
}
