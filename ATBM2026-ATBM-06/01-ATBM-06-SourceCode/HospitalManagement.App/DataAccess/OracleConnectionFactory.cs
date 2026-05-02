using Oracle.ManagedDataAccess.Client;
using HospitalManagement.App.Models;

namespace HospitalManagement.App.DataAccess;

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
