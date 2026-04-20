using Oracle.ManagedDataAccess.Client;
using OracleAdminWinForms.Models;

namespace OracleAdminWinForms.Data;

public class OracleConnectionFactory
{
    private readonly DbConnectionSettings _settings;

    public OracleConnectionFactory(DbConnectionSettings settings)
    {
        _settings = settings;
    }

    public OracleConnection CreateOpenConnection()
    {
        var connection = new OracleConnection(_settings.BuildConnectionString());
        connection.Open();
        return connection;
    }
}
