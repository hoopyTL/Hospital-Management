using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace HospitalManagement.App.DataAccess
{
    public class OracleHelper : IDisposable
    {
        private OracleConnection _connection;
        private string _connectionString;
        public string CurrentUser { get; private set; }

        private static OracleHelper _instance;
        public static OracleHelper Instance => _instance;

        /// <summary>
        /// Khởi tạo kết nối Oracle với user/password được truyền vào.
        /// Connection string kết nối trực tiếp bằng tài khoản Oracle user 
        /// để VPD policies hoạt động đúng context.
        /// </summary>
        public static OracleHelper Initialize(string username, string password, 
            string host = "localhost", int port = 1521, string serviceName = "XEPDB1")
        {
            _instance?.Dispose();

            var connStr = $"User Id={username};Password={password};" +
                          $"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={host})(PORT={port}))" +
                          $"(CONNECT_DATA=(SERVICE_NAME={serviceName})))";

            _instance = new OracleHelper
            {
                _connectionString = connStr,
                CurrentUser = username.ToUpper()
            };

            return _instance;
        }

        /// <summary>
        /// Mở kết nối (nếu chưa mở)
        /// </summary>
        public OracleConnection GetConnection()
        {
            if (_connection == null)
            {
                _connection = new OracleConnection(_connectionString);
            }
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }
            return _connection;
        }

        /// <summary>
        /// Test kết nối - trả về true nếu thành công
        /// </summary>
        public bool TestConnection()
        {
            try
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();
                conn.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Thực thi SELECT query, trả về DataTable
        /// </summary>
        public DataTable ExecuteQuery(string sql, params OracleParameter[] parameters)
        {
            var dt = new DataTable();
            using var conn = new OracleConnection(_connectionString);
            conn.Open();
            using var cmd = new OracleCommand(sql, conn)
            {
                BindByName = true,
            };
            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters);
            }
            using var adapter = new OracleDataAdapter(cmd);
            adapter.Fill(dt);
            return dt;
        }

        /// <summary>
        /// Thực thi INSERT/UPDATE/DELETE, trả về số dòng bị ảnh hưởng
        /// </summary>
        public int ExecuteNonQuery(string sql, params OracleParameter[] parameters)
        {
            using var conn = new OracleConnection(_connectionString);
            conn.Open();
            using var cmd = new OracleCommand(sql, conn)
            {
                BindByName = true,
            };
            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters);
            }
            return cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Thực thi query trả về giá trị đơn
        /// </summary>
        public object ExecuteScalar(string sql, params OracleParameter[] parameters)
        {
            using var conn = new OracleConnection(_connectionString);
            conn.Open();
            using var cmd = new OracleCommand(sql, conn)
            {
                BindByName = true,
            };
            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters);
            }
            return cmd.ExecuteScalar();
        }

        /// <summary>
        /// Kiểm tra user có role cụ thể không
        /// </summary>
        public bool HasRole(string roleName)
        {
            try
            {
                var dt = ExecuteQuery(
                    "SELECT GRANTED_ROLE FROM USER_ROLE_PRIVS WHERE GRANTED_ROLE = :role",
                    new OracleParameter("role", roleName.ToUpper()));
                return dt.Rows.Count > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Lấy danh sách roles của user hiện tại
        /// </summary>
        public DataTable GetUserRoles()
        {
            return ExecuteQuery("SELECT GRANTED_ROLE FROM USER_ROLE_PRIVS");
        }

        /// <summary>
        /// MA_BN kế tiếp theo quy ước seed: BN###### (6 chữ số).
        /// </summary>
        public string AllocNextMaBenhNhan()
        {
            const string sql = @"
                SELECT 'BN' || LPAD(NVL(MAX(TO_NUMBER(SUBSTR(MA_BN, 3))), 0) + 1, 6, '0')
                  FROM ADMIN.BENH_NHAN
                 WHERE REGEXP_LIKE(MA_BN, '^BN[0-9]{6}$')";

            var o = ExecuteScalar(sql);
            var s = o?.ToString()?.Trim();
            return string.IsNullOrEmpty(s) ? "BN000001" : s;
        }

        /// <summary>
        /// Bind cột <c>NVARCHAR2</c>: nếu để mặc định, chuỗi Unicode có thể bị mất dấu và hiện '?'.
        /// </summary>
        public static OracleParameter ParamNvarchar2(string name, object value)
        {
            var p = new OracleParameter(name, OracleDbType.NVarchar2);
            p.Value = value is null || ReferenceEquals(value, DBNull.Value) ? DBNull.Value : value;
            return p;
        }

        public void Dispose()
        {
            if (_connection != null)
            {
                if (_connection.State == ConnectionState.Open)
                    _connection.Close();
                _connection.Dispose();
                _connection = null;
            }
        }
    }
}
