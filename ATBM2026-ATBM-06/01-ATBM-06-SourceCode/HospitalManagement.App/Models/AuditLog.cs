using System;

namespace HospitalManagement.App.Models
{
    /// <summary>
    /// Model lưu trữ các thay đổi và sự kiện trong hệ thống (Audit Log)
    /// Dùng để theo dõi triển khai, thay đổi dữ liệu và hành vi người dùng
    /// </summary>
    public class AuditLog
    {
        /// <summary>
        /// ID duy nhất của audit log
        /// </summary>
        public string AuditId { get; set; }

        /// <summary>
        /// Username của người thực hiện hành động
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Tên đầy đủ của người dùng
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Loại hành động: CREATE, READ, UPDATE, DELETE, EXECUTE, LOGIN, LOGOUT
        /// </summary>
        public string ActionType { get; set; }

        /// <summary>
        /// Đối tượng bị tác động: Table name, View name, Procedure name
        /// </summary>
        public string ObjectName { get; set; }

        /// <summary>
        /// Schema của đối tượng
        /// </summary>
        public string ObjectSchema { get; set; }

        /// <summary>
        /// Giá trị cũ trước khi thay đổi (chỉ dùng cho UPDATE)
        /// </summary>
        public string OldValue { get; set; }

        /// <summary>
        /// Giá trị mới sau khi thay đổi (chỉ dùng cho UPDATE)
        /// </summary>
        public string NewValue { get; set; }

        /// <summary>
        /// Câu SQL hoặc lệnh được thực thi
        /// </summary>
        public string SqlStatement { get; set; }

        /// <summary>
        /// Kết quả: SUCCESS hoặc FAILED
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// Mã lỗi (nếu có)
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// Thông báo lỗi (nếu có)
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Địa chỉ IP của người dùng
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// Tên máy tính
        /// </summary>
        public string MachineName { get; set; }

        /// <summary>
        /// Thời gian thực hiện hành động
        /// </summary>
        public DateTime ActionTimestamp { get; set; }

        /// <summary>
        /// Ghi chú hoặc mô tả thêm
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Mã bản ghi bị tác động (VD: MaHsba, MaBN, etc)
        /// </summary>
        public string RecordId { get; set; }

        /// <summary>
        /// Loại triển khai: INITIAL_SETUP, PATCH, HOTFIX, FEATURE, DATA_MIGRATION
        /// </summary>
        public string DeploymentType { get; set; }

        /// <summary>
        /// Phiên bản ứng dụng
        /// </summary>
        public string ApplicationVersion { get; set; }

        /// <summary>
        /// Mô tả chi tiết về triển khai
        /// </summary>
        public string DeploymentDescription { get; set; }
    }
}
