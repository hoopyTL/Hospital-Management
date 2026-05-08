using System;
using System.IO;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using HospitalManagement.App.Models;
using HospitalManagement.App.DataAccess;

namespace HospitalManagement.App.Services
{
    public class AuditLogService
    {
        public void ExecuteSqlScript(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Không tìm thấy file script tại: {filePath}");
            }

            // Đọc toàn bộ nội dung file
            string scriptContent = File.ReadAllText(filePath);
            
            // OracleCommand không hỗ trợ chạy nhiều lệnh cùng lúc được ngăn cách bởi dấu ';' 
            // nên chúng ta sẽ tách các câu lệnh ra (áp dụng cho các lệnh NOAUDIT cơ bản)
            string[] commands = scriptContent.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            using (OracleConnection connection = _connectionFactory.CreateOpenConnection())
            {
                foreach (string cmdText in commands)
                {
                    string cleanCmd = cmdText.Trim();
                    // Bỏ qua các dấu '/' (thường dùng trong PL/SQL) hoặc dòng trống
                    if (string.IsNullOrWhiteSpace(cleanCmd) || cleanCmd == "/") continue;

                    using (OracleCommand command = new OracleCommand(cleanCmd, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        private readonly OracleConnectionFactory _connectionFactory;

        public AuditLogService(OracleConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        /// <summary>
        /// Lấy Standard Audit Logs theo đúng logic của file 04_show_log.sql
        /// </summary>
        public List<AuditLog> GetStandardAuditLogs(int limit = 100)
        {
            List<AuditLog> logs = new List<AuditLog>();
            try
            {
                using (OracleConnection connection = _connectionFactory.CreateOpenConnection())
                {
                    string query = $@"
                        SELECT * FROM (
                            SELECT 
                                username, 
                                timestamp, 
                                obj_name, 
                                action_name, 
                                returncode,
                                CASE WHEN returncode = 0 THEN 'SUCCESS' ELSE 'FAILED' END as result_status,
                                sql_text
                            FROM dba_audit_trail
                            WHERE 
                                (obj_name = 'HSBA' AND action_name = 'SELECT')
                                OR (obj_name = 'DON_THUOC' AND action_name = 'UPDATE' AND returncode != 0)
                                OR (obj_name = 'HSBA_DV' AND action_name = 'DELETE' AND returncode = 0)
                                OR (username = 'NV0051' AND action_name IN ('LOGON', 'LOGOFF'))
                                OR (obj_name = 'BENH_NHAN' AND action_name = 'INSERT' AND returncode = 0)
                            ORDER BY timestamp DESC
                        ) WHERE ROWNUM <= {limit}";

                    using (OracleCommand command = new OracleCommand(query, connection))
                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        int idCounter = 1;
                        while (reader.Read())
                        {
                            logs.Add(new AuditLog
                            {
                                AuditId = (idCounter++).ToString(),
                                Username = reader["username"].ToString(),
                                ActionTimestamp = Convert.ToDateTime(reader["timestamp"]),
                                ObjectName = reader["obj_name"].ToString(),
                                ActionType = reader["action_name"].ToString(),
                                ErrorCode = reader["returncode"].ToString(),
                                Result = reader["result_status"].ToString(),
                                SqlStatement = reader["sql_text"].ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine($"Lỗi Standard Audit: {ex.Message}"); }
            return logs;
        }

        /// <summary>
        /// Lấy Unified Audit Logs theo các Policies trong file 04_show_log.sql
        /// </summary>
        public List<AuditLog> GetUnifiedAuditLogs(int limit = 100)
        {
            List<AuditLog> logs = new List<AuditLog>();
            try
            {
                using (OracleConnection connection = _connectionFactory.CreateOpenConnection())
                {
                    string query = $@"
                        SELECT * FROM (
                            SELECT 
                                dbusername, 
                                event_timestamp, 
                                action_name, 
                                object_schema,
                                object_name, 
                                unified_audit_policies, 
                                sql_text,
                                client_program_name
                            FROM unified_audit_trail
                            WHERE 
                                unified_audit_policies LIKE '%UNIFIED_AUDIT_UPDATE_DONTHUOC%'
                                OR unified_audit_policies LIKE '%UNIFIED_AUDIT_ILLEGAL_UPDATE_HSBA%'
                                OR unified_audit_policies LIKE '%UNIFIED_AUDIT_ILLEGAL_DML_HSBA_DV%'
                            ORDER BY event_timestamp DESC
                        ) WHERE ROWNUM <= {limit}";

                    using (OracleCommand command = new OracleCommand(query, connection))
                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        int idCounter = 1;
                        while (reader.Read())
                        {
                            logs.Add(new AuditLog
                            {
                                AuditId = (idCounter++).ToString(),
                                Username = reader["dbusername"].ToString(),
                                ActionTimestamp = Convert.ToDateTime(reader["event_timestamp"]),
                                ActionType = reader["action_name"].ToString(),
                                ObjectSchema = reader["object_schema"].ToString(),
                                ObjectName = reader["object_name"].ToString(),
                                Notes = reader["unified_audit_policies"].ToString(), 
                                SqlStatement = reader["sql_text"].ToString(),
                                MachineName = reader["client_program_name"].ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine($"Lỗi Unified Audit: {ex.Message}"); }
            return logs;
        }
    }
}