namespace HospitalManagement.App.Models;

public class DbConnectionSettings
{
    public string Host { get; set; } = "localhost";
    public string Port { get; set; } = "1521";
    public string ServiceName { get; set; } = "XEPDB1";
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public bool UseSysDba { get; set; }

    public string BuildConnectionString()
    {
        var baseString = $"User Id={Username};Password={Password};Data Source={Host}:{Port}/{ServiceName};";
        return UseSysDba ? $"{baseString}DBA Privilege=SYSDBA;" : baseString;
    }

    public override string ToString()
        => $"{Username}@{Host}:{Port}/{ServiceName}" + (UseSysDba ? " (SYSDBA)" : string.Empty);
}
