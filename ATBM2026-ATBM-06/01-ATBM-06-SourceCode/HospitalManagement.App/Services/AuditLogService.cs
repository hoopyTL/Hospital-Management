using System;
using System.Collections.Generic;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using HospitalManagement.App.Models;
using HospitalManagement.App.DataAccess;

namespace HospitalManagement.App.Services
{
    /// <summary>
    /// Service để quản lý Audit Log - theo dõi các thay đổi và triển khai
    /// </summary>
    public class AuditLogService
    {
        private readonly OracleConnectionFactory _connectionFactory;

        public AuditLogService(OracleConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        /// <summary>
        /// Ghi một sự kiện vào Audit Log
        /// </summary>
        public void LogAudit(AuditLog auditLog)
        {
            try
            {
                using (OracleConnection connection = _connectionFactory.CreateOpenConnection())
                {
                    using (OracleCommand command = new OracleCommand("admin.sp_log_audit", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("p_username", OracleDbType.Varchar2).Value = auditLog.Username ?? "";
                        command.Parameters.Add("p_full_name", OracleDbType.Varchar2).Value = auditLog.FullName ?? "";
                        command.Parameters.Add("p_action_type", OracleDbType.Varchar2).Value = auditLog.ActionType ?? "";
                        command.Parameters.Add("p_object_name", OracleDbType.Varchar2).Value = auditLog.ObjectName ?? "";
                        command.Parameters.Add("p_object_schema", OracleDbType.Varchar2).Value = auditLog.ObjectSchema ?? "ADMIN";
                        command.Parameters.Add("p_old_value", OracleDbType.Clob).Value = auditLog.OldValue ?? "";
                        command.Parameters.Add("p_new_value", OracleDbType.Clob).Value = auditLog.NewValue ?? "";
                        command.Parameters.Add("p_sql_statement", OracleDbType.Clob).Value = auditLog.SqlStatement ?? "";
                        command.Parameters.Add("p_result", OracleDbType.Varchar2).Value = auditLog.Result ?? "SUCCESS";
                        command.Parameters.Add("p_error_code", OracleDbType.Varchar2).Value = auditLog.ErrorCode ?? "";
                        command.Parameters.Add("p_error_message", OracleDbType.Varchar2).Value = auditLog.ErrorMessage ?? "";
                        command.Parameters.Add("p_ip_address", OracleDbType.Varchar2).Value = auditLog.IpAddress ?? GetClientIpAddress();
                        command.Parameters.Add("p_machine_name", OracleDbType.Varchar2).Value = auditLog.MachineName ?? Environment.MachineName;
                        command.Parameters.Add("p_notes", OracleDbType.Varchar2).Value = auditLog.Notes ?? "";
                        command.Parameters.Add("p_record_id", OracleDbType.Varchar2).Value = auditLog.RecordId ?? "";
                        command.Parameters.Add("p_deployment_type", OracleDbType.Varchar2).Value = auditLog.DeploymentType ?? "";
                        command.Parameters.Add("p_application_version", OracleDbType.Varchar2).Value = auditLog.ApplicationVersion ?? GetApplicationVersion();
                        command.Parameters.Add("p_deployment_description", OracleDbType.Varchar2).Value = auditLog.DeploymentDescription ?? "";

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi ghi audit log: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy các audit log hôm nay
        /// </summary>
        public List<AuditLog> GetTodayAuditLogs()
        {
            List<AuditLog> auditLogs = new List<AuditLog>();

            try
            {
                using (OracleConnection connection = _connectionFactory.CreateOpenConnection())
                {
                    string query = @"
                        SELECT audit_id, username, full_name, action_type, 
                               result, action_timestamp, notes
                        FROM admin.v_audit_log_today
                        ORDER BY action_timestamp DESC";

                    using (OracleCommand command = new OracleCommand(query, connection))
                    {
                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                auditLogs.Add(new AuditLog
                                {
                                    AuditId = reader["audit_id"].ToString(),
                                    Username = reader["username"].ToString(),
                                    FullName = reader["full_name"].ToString(),
                                    ActionType = reader["action_type"].ToString(),
                                    Result = reader["result"].ToString(),
                                    ActionTimestamp = Convert.ToDateTime(reader["action_timestamp"]),
                                    Notes = reader["notes"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy audit log: {ex.Message}");
            }

            return auditLogs;
        }

        /// <summary>
        /// Lấy các thay đổi dữ liệu
        /// </summary>
        public List<AuditLog> GetDataChanges(int limit = 20)
        {
            List<AuditLog> changes = new List<AuditLog>();

            try
            {
                using (OracleConnection connection = _connectionFactory.CreateOpenConnection())
                {
                    string query = $@"
                        SELECT * FROM (
                            SELECT audit_id, username, full_name, action_type, record_id,
                                   old_value, new_value, result, action_timestamp
                            FROM admin.v_audit_log_data_changes
                            ORDER BY action_timestamp DESC
                        )
                        WHERE ROWNUM <= {limit}";

                    using (OracleCommand command = new OracleCommand(query, connection))
                    {
                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                changes.Add(new AuditLog
                                {
                                    AuditId = reader["audit_id"].ToString(),
                                    Username = reader["username"].ToString(),
                                    FullName = reader["full_name"].ToString(),
                                    ActionType = reader["action_type"].ToString(),
                                    RecordId = reader["record_id"].ToString(),
                                    OldValue = reader["old_value"].ToString(),
                                    NewValue = reader["new_value"].ToString(),
                                    Result = reader["result"].ToString(),
                                    ActionTimestamp = Convert.ToDateTime(reader["action_timestamp"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy thay đổi dữ liệu: {ex.Message}");
            }

            return changes;
        }

        /// <summary>
        /// Lấy các lỗi đã xảy ra
        /// </summary>
        public List<AuditLog> GetErrors()
        {
            List<AuditLog> errors = new List<AuditLog>();

            try
            {
                using (OracleConnection connection = _connectionFactory.CreateOpenConnection())
                {
                    string query = @"
                        SELECT audit_id, username, full_name, action_type,
                               error_code, error_message, result, action_timestamp
                        FROM admin.v_audit_log_errors";

                    using (OracleCommand command = new OracleCommand(query, connection))
                    {
                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                errors.Add(new AuditLog
                                {
                                    AuditId = reader["audit_id"].ToString(),
                                    Username = reader["username"].ToString(),
                                    FullName = reader["full_name"].ToString(),
                                    ActionType = reader["action_type"].ToString(),
                                    ErrorCode = reader["error_code"].ToString(),
                                    ErrorMessage = reader["error_message"].ToString(),
                                    Result = reader["result"].ToString(),
                                    ActionTimestamp = Convert.ToDateTime(reader["action_timestamp"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy danh sách errors: {ex.Message}");
            }

            return errors;
        }

        /// <summary>
        /// Lấy audit log theo user
        /// </summary>
        public List<AuditLog> GetAuditLogByUser(string username, int limit = 20)
        {
            List<AuditLog> logs = new List<AuditLog>();

            try
            {
                using (OracleConnection connection = _connectionFactory.CreateOpenConnection())
                {
                    string query = $@"
                        SELECT * FROM (
                            SELECT audit_id, username, full_name, action_type, result, action_timestamp
                            FROM admin.AUDIT_LOG
                            WHERE UPPER(username) = UPPER(:username)
                            ORDER BY action_timestamp DESC
                        )
                        WHERE ROWNUM <= {limit}";

                    using (OracleCommand command = new OracleCommand(query, connection))
                    {
                        command.Parameters.Add(":username", username);

                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                logs.Add(new AuditLog
                                {
                                    AuditId = reader["audit_id"].ToString(),
                                    Username = reader["username"].ToString(),
                                    FullName = reader["full_name"].ToString(),
                                    ActionType = reader["action_type"].ToString(),
                                    Result = reader["result"].ToString(),
                                    ActionTimestamp = Convert.ToDateTime(reader["action_timestamp"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy audit log của user: {ex.Message}");
            }

            return logs;
        }

        /// <summary>
        /// Lấy lịch sử đăng nhập
        /// </summary>
        public List<AuditLog> GetLoginHistory(int limit = 50)
        {
            List<AuditLog> loginLogs = new List<AuditLog>();

            try
            {
                using (OracleConnection connection = _connectionFactory.CreateOpenConnection())
                {
                    string query = $@"
                        SELECT * FROM (
                            SELECT audit_id, username, full_name, result, action_timestamp, ip_address
                            FROM admin.AUDIT_LOG
                            WHERE action_type = 'LOGIN'
                            ORDER BY action_timestamp DESC
                        )
                        WHERE ROWNUM <= {limit}";

                    using (OracleCommand command = new OracleCommand(query, connection))
                    {
                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                loginLogs.Add(new AuditLog
                                {
                                    AuditId = reader["audit_id"].ToString(),
                                    Username = reader["username"].ToString(),
                                    FullName = reader["full_name"].ToString(),
                                    Result = reader["result"].ToString(),
                                    ActionTimestamp = Convert.ToDateTime(reader["action_timestamp"]),
                                    IpAddress = reader["ip_address"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy lịch sử đăng nhập: {ex.Message}");
            }

            return loginLogs;
        }

        /// <summary>
        /// Lấy thông tin triển khai
        /// </summary>
        public DataTable GetDeploymentInfo()
        {
            DataTable dt = new DataTable();

            try
            {
                using (OracleConnection connection = _connectionFactory.CreateOpenConnection())
                {
                    string query = @"
                        SELECT deployment_type, application_version, deployment_description,
                               COUNT(*) as so_thao_tac, MIN(action_timestamp) as thoi_gian_bat_dau,
                               MAX(action_timestamp) as thoi_gian_ket_thuc
                        FROM admin.AUDIT_LOG
                        WHERE deployment_type IS NOT NULL
                        GROUP BY deployment_type, application_version, deployment_description
                        ORDER BY MIN(action_timestamp) DESC";

                    using (OracleDataAdapter adapter = new OracleDataAdapter(query, connection))
                    {
                        adapter.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy thông tin triển khai: {ex.Message}");
            }

            return dt;
        }

        /// <summary>
        /// Lấy thống kê audit log
        /// </summary>
        public DataTable GetAuditStatistics()
        {
            DataTable dt = new DataTable();

            try
            {
                using (OracleConnection connection = _connectionFactory.CreateOpenConnection())
                {
                    string query = @"
                        SELECT * FROM admin.v_audit_log_summary";

                    using (OracleDataAdapter adapter = new OracleDataAdapter(query, connection))
                    {
                        adapter.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy thống kê audit: {ex.Message}");
            }

            return dt;
        }

        /// <summary>
        /// Ghi audit log cho hành động login
        /// </summary>
        public void LogLogin(string username, string fullName, string ipAddress, bool success)
        {
            LogAudit(new AuditLog
            {
                Username = username,
                FullName = fullName,
                ActionType = "LOGIN",
                ObjectSchema = "SYSTEM",
                Result = success ? "SUCCESS" : "FAILED",
                IpAddress = ipAddress,
                MachineName = Environment.MachineName
            });
        }

        /// <summary>
        /// Ghi audit log cho hành động INSERT
        /// </summary>
        public void LogInsert(string username, string fullName, string recordId, string newValue)
        {
            LogAudit(new AuditLog
            {
                Username = username,
                FullName = fullName,
                ActionType = "INSERT",
                ObjectSchema = "ADMIN",
                NewValue = newValue,
                RecordId = recordId,
                Result = "SUCCESS"
            });
        }

        /// <summary>
        /// Ghi audit log cho hành động UPDATE
        /// </summary>
        public void LogUpdate(string username, string fullName, string recordId, string oldValue, string newValue)
        {
            LogAudit(new AuditLog
            {
                Username = username,
                FullName = fullName,
                ActionType = "UPDATE",
                ObjectSchema = "ADMIN",
                OldValue = oldValue,
                NewValue = newValue,
                RecordId = recordId,
                Result = "SUCCESS"
            });
        }

        /// <summary>
        /// Ghi audit log cho hành động DELETE
        /// </summary>
        public void LogDelete(string username, string fullName, string recordId, string oldValue)
        {
            LogAudit(new AuditLog
            {
                Username = username,
                FullName = fullName,
                ActionType = "DELETE",
                ObjectSchema = "ADMIN",
                OldValue = oldValue,
                RecordId = recordId,
                Result = "SUCCESS"
            });
        }

        /// <summary>
        /// Ghi audit log cho triển khai
        /// </summary>
        public void LogDeployment(string deploymentType, string applicationVersion, string description, string username, string fullName)
        {
            LogAudit(new AuditLog
            {
                Username = username,
                FullName = fullName,
                ActionType = "DEPLOYMENT",
                ObjectSchema = "SYSTEM",
                DeploymentType = deploymentType,
                ApplicationVersion = applicationVersion,
                DeploymentDescription = description,
                Result = "SUCCESS"
            });
        }

        /// <summary>
        /// Lấy địa chỉ IP của client
        /// </summary>
        private string GetClientIpAddress()
        {
            try
            {
                var hostEntry = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                return hostEntry.AddressList.Length > 0 ? hostEntry.AddressList[0].ToString() : "UNKNOWN";
            }
            catch
            {
                return "UNKNOWN";
            }
        }

        /// <summary>
        /// Lấy phiên bản ứng dụng
        /// </summary>
        private string GetApplicationVersion()
        {
            try
            {
                return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
            catch
            {
                return "1.0.0.0";
            }
        }

        /// <summary>
        /// Tắt Standard Audit trên một bảng
        /// </summary>
        public bool DisableStandardAudit(string tableName, string actionType = "ALL")
        {
            try
            {
                using (OracleConnection connection = _connectionFactory.CreateOpenConnection())
                {
                    // Sử dụng anonymous PL/SQL block để thực thi NOAUDIT
                    string query = $@"DECLARE
                                        v_sql VARCHAR2(1000);
                                      BEGIN
                                        v_sql := 'NOAUDIT {actionType} ON admin.{tableName}';
                                        EXECUTE IMMEDIATE v_sql;
                                        DBMS_OUTPUT.PUT_LINE('Tắt audit {actionType} trên bảng {tableName} thành công');
                                      END;";

                    using (OracleCommand command = new OracleCommand(query, connection))
                    {
                        command.ExecuteNonQuery();
                        Console.WriteLine($"Tắt audit {actionType} trên bảng {tableName} thành công");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi tắt standard audit: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Tắt tất cả Standard Audit
        /// </summary>
        public bool DisableAllStandardAudit()
        {
            try
            {
                using (OracleConnection connection = _connectionFactory.CreateOpenConnection())
                {
                    string query = @"DECLARE
                                      v_sql VARCHAR2(100);
                                    BEGIN
                                      v_sql := 'NOAUDIT ALL';
                                      EXECUTE IMMEDIATE v_sql;
                                      DBMS_OUTPUT.PUT_LINE('Tắt tất cả Standard Audit thành công');
                                    END;";

                    using (OracleCommand command = new OracleCommand(query, connection))
                    {
                        command.ExecuteNonQuery();
                        Console.WriteLine("Tắt tất cả Standard Audit thành công");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi tắt tất cả standard audit: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Tắt tất cả Unified Audit Policies
        /// </summary>
        public bool DisableAllUnifiedAuditPolicies()
        {
            try
            {
                using (OracleConnection connection = _connectionFactory.CreateOpenConnection())
                {
                    // Danh sách các Unified Audit policies cần tắt
                    var policies = new[] {
                        "UNIFIED_AUDIT_UPDATE_DONTHUOC",
                        "UNIFIED_AUDIT_ILLEGAL_UPDATE_HSBA",
                        "UNIFIED_AUDIT_ILLEGAL_DML_HSBA_DV"
                    };

                    foreach (var policyName in policies)
                    {
                        string query = $@"DECLARE
                                          v_sql VARCHAR2(500);
                                        BEGIN
                                          v_sql := 'NOAUDIT POLICY {policyName}';
                                          EXECUTE IMMEDIATE v_sql;
                                          DBMS_OUTPUT.PUT_LINE('Tắt Unified Audit Policy {policyName} thành công');
                                        EXCEPTION
                                          WHEN OTHERS THEN
                                            DBMS_OUTPUT.PUT_LINE('Policy {policyName} không tồn tại hoặc đã xóa');
                                        END;";

                        try
                        {
                            using (OracleCommand command = new OracleCommand(query, connection))
                            {
                                command.ExecuteNonQuery();
                                Console.WriteLine($"Tắt Unified Audit Policy '{policyName}' thành công");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Cảnh báo khi tắt {policyName}: {ex.Message}");
                            // Tiếp tục với policy tiếp theo nếu có lỗi
                        }
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi tắt tất cả Unified Audit policies: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Tắt Unified Audit Policy
        /// </summary>
        public bool DisableUnifiedAuditPolicy(string policyName)
        {
            try
            {
                using (OracleConnection connection = _connectionFactory.CreateOpenConnection())
                {
                    string query = $@"DECLARE
                                      v_sql VARCHAR2(500);
                                    BEGIN
                                      v_sql := 'NOAUDIT POLICY {policyName}';
                                      EXECUTE IMMEDIATE v_sql;
                                      DBMS_OUTPUT.PUT_LINE('Tắt Unified Audit Policy {policyName} thành công');
                                    EXCEPTION
                                      WHEN OTHERS THEN
                                        DBMS_OUTPUT.PUT_LINE('Policy {policyName} không tồn tại: ' || SQLERRM);
                                    END;";

                    using (OracleCommand command = new OracleCommand(query, connection))
                    {
                        command.ExecuteNonQuery();
                        Console.WriteLine($"Tắt Unified Audit Policy '{policyName}' thành công");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi tắt unified audit policy: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Xóa Audit Trail Records
        /// </summary>
        public bool ClearAuditTrail(int daysToKeep = 30)
        {
            try
            {
                using (OracleConnection connection = _connectionFactory.CreateOpenConnection())
                {
                    string query = $@"DELETE FROM admin.AUDIT_LOG 
                                     WHERE action_timestamp < SYSDATE - {daysToKeep}";

                    using (OracleCommand command = new OracleCommand(query, connection))
                    {
                        int rowsDeleted = command.ExecuteNonQuery();
                        
                        using (OracleCommand commitCmd = new OracleCommand("COMMIT", connection))
                        {
                            commitCmd.ExecuteNonQuery();
                        }
                        
                        Console.WriteLine($"Xóa {rowsDeleted} bản ghi audit cũ hơn {daysToKeep} ngày");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi xóa audit trail: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Xóa tất cả Audit Trail Records
        /// </summary>
        public bool ClearAllAuditTrail()
        {
            try
            {
                using (OracleConnection connection = _connectionFactory.CreateOpenConnection())
                {
                    string query = "DELETE FROM admin.AUDIT_LOG";

                    using (OracleCommand command = new OracleCommand(query, connection))
                    {
                        int rowsDeleted = command.ExecuteNonQuery();
                        
                        using (OracleCommand commitCmd = new OracleCommand("COMMIT", connection))
                        {
                            commitCmd.ExecuteNonQuery();
                        }
                        
                        Console.WriteLine($"Xóa tất cả {rowsDeleted} bản ghi audit");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi xóa tất cả audit trail: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Lấy Standard Audit Logs từ DBA_AUDIT_TRAIL
        /// </summary>
        public List<AuditLog> GetStandardAuditLogs(string tableName = "", string actionType = "", int limit = 100)
        {
            List<AuditLog> logs = new List<AuditLog>();

            try
            {
                using (OracleConnection connection = _connectionFactory.CreateOpenConnection())
                {
                    string whereClause = "WHERE 1=1";
                    if (!string.IsNullOrEmpty(tableName))
                        whereClause += $" AND UPPER(obj_name) LIKE '%{tableName.ToUpper()}%'";
                    if (!string.IsNullOrEmpty(actionType))
                        whereClause += $" AND UPPER(action_name) = '{actionType.ToUpper()}'";

                    string query = $@"
                        SELECT * FROM (
                            SELECT 
                                ROWNUM as audit_id,
                                username,
                                '' as full_name,
                                action_name,
                                obj_name,
                                returncode,
                                TO_CHAR(timestamp, 'YYYY-MM-DD HH24:MI:SS') as action_timestamp,
                                CASE WHEN returncode = 0 THEN 'SUCCESS' ELSE 'FAILED' END as result_status
                            FROM dba_audit_trail
                            {whereClause}
                            ORDER BY timestamp DESC
                        )
                        WHERE ROWNUM <= {limit}";

                    using (OracleCommand command = new OracleCommand(query, connection))
                    {
                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                logs.Add(new AuditLog
                                {
                                    AuditId = reader["audit_id"].ToString(),
                                    Username = reader["username"].ToString(),
                                    ActionType = reader["action_name"].ToString(),
                                    ObjectName = reader["obj_name"].ToString(),
                                    ErrorCode = reader["returncode"].ToString(),
                                    Result = reader["result_status"].ToString(),
                                    ActionTimestamp = DateTime.Parse(reader["action_timestamp"].ToString())
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy Standard Audit Logs: {ex.Message}");
            }

            return logs;
        }

        /// <summary>
        /// Lấy Unified Audit Logs từ UNIFIED_AUDIT_TRAIL
        /// </summary>
        public List<AuditLog> GetUnifiedAuditLogs(string policyName = "", string tableName = "", int limit = 100)
        {
            List<AuditLog> logs = new List<AuditLog>();

            try
            {
                using (OracleConnection connection = _connectionFactory.CreateOpenConnection())
                {
                    string whereClause = "WHERE 1=1";
                    if (!string.IsNullOrEmpty(policyName))
                        whereClause += $" AND UPPER(audit_option) LIKE '%{policyName.ToUpper()}%'";
                    if (!string.IsNullOrEmpty(tableName))
                        whereClause += $" AND UPPER(object_name) LIKE '%{tableName.ToUpper()}%'";

                    string query = $@"
                        SELECT * FROM (
                            SELECT 
                                ROWNUM as audit_id,
                                database_user,
                                object_name,
                                audit_option,
                                action_name,
                                TO_CHAR(event_timestamp, 'YYYY-MM-DD HH24:MI:SS') as action_timestamp,
                                SUBSTR(sql_text, 1, 200) as sql_preview
                            FROM unified_audit_trail
                            {whereClause}
                            ORDER BY event_timestamp DESC
                        )
                        WHERE ROWNUM <= {limit}";

                    using (OracleCommand command = new OracleCommand(query, connection))
                    {
                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                logs.Add(new AuditLog
                                {
                                    AuditId = reader["audit_id"].ToString(),
                                    Username = reader["database_user"].ToString(),
                                    ObjectName = reader["object_name"].ToString(),
                                    ActionType = reader["action_name"].ToString(),
                                    Notes = reader["audit_option"].ToString() + " | " + reader["sql_preview"].ToString(),
                                    Result = "SUCCESS",
                                    ActionTimestamp = DateTime.Parse(reader["action_timestamp"].ToString())
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy Unified Audit Logs: {ex.Message}");
            }

            return logs;
        }

        /// <summary>
        /// Lấy Audit Statistics từ DBA_AUDIT_TRAIL
        /// </summary>
        public DataTable GetAuditStatisticsFromDBA()
        {
            DataTable dt = new DataTable();

            try
            {
                using (OracleConnection connection = _connectionFactory.CreateOpenConnection())
                {
                    string query = @"
                        SELECT 
                            action_name,
                            COUNT(*) as so_lan_thuc_hien,
                            SUM(CASE WHEN returncode = 0 THEN 1 ELSE 0 END) as thanh_cong,
                            SUM(CASE WHEN returncode != 0 THEN 1 ELSE 0 END) as that_bai
                        FROM dba_audit_trail
                        WHERE obj_name IN ('HSBA', 'DON_THUOC', 'HSBA_DV', 'BENH_NHAN')
                        GROUP BY action_name
                        ORDER BY so_lan_thuc_hien DESC";

                    using (OracleDataAdapter adapter = new OracleDataAdapter(query, connection))
                    {
                        adapter.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy Audit Statistics: {ex.Message}");
            }

            return dt;
        }

        /// <summary>
        /// Lấy Unified Audit Statistics
        /// </summary>
        public DataTable GetUnifiedAuditStatistics()
        {
            DataTable dt = new DataTable();

            try
            {
                using (OracleConnection connection = _connectionFactory.CreateOpenConnection())
                {
                    string query = @"
                        SELECT 
                            action_name,
                            COUNT(*) as so_lan_ghi_vay,
                            MIN(event_timestamp) as lan_dau_tien,
                            MAX(event_timestamp) as lan_cuoi_cung
                        FROM unified_audit_trail
                        WHERE object_name IN ('HSBA', 'DON_THUOC', 'HSBA_DV', 'BENH_NHAN')
                        GROUP BY action_name
                        ORDER BY so_lan_ghi_vay DESC";

                    using (OracleDataAdapter adapter = new OracleDataAdapter(query, connection))
                    {
                        adapter.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy Unified Audit Statistics: {ex.Message}");
            }

            return dt;
        }

        /// <summary>
        /// Lấy trạng thái Audit System
        /// </summary>
        public DataTable GetAuditStatus()
        {
            DataTable dt = new DataTable();

            try
            {
                using (OracleConnection connection = _connectionFactory.CreateOpenConnection())
                {
                    string query = "SELECT name, value FROM v$parameter WHERE name = 'audit_trail'";

                    using (OracleDataAdapter adapter = new OracleDataAdapter(query, connection))
                    {
                        adapter.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy trạng thái audit: {ex.Message}");
            }

            return dt;
        }
    }
}
