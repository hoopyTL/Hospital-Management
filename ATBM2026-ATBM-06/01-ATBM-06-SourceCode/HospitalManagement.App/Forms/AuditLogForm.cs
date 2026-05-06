using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using HospitalManagement.App.Models;
using HospitalManagement.App.Services;
using HospitalManagement.App.DataAccess;
using HospitalManagement.App.Helpers;

namespace HospitalManagement.App.Forms
{
    public class AuditLogForm : Form
    {
        private readonly DbConnectionSettings _settings;
        private readonly AuditLogService _auditService;

        private TabControl? _tabControl;
        private DataGridView? _dgvTodayLogs;
        private DataGridView? _dgvDataChanges;
        private DataGridView? _dgvErrors;
        private DataGridView? _dgvDeployment;
        private DataGridView? _dgvSummary;
        private DataGridView? _dgvStandardAudit;
        private DataGridView? _dgvUnifiedAudit;
        private DataGridView? _dgvAuditStats;

        private TextBox? _txtSearchUser;
        private TextBox? _txtSearchAction;
        private Button? _btnRefresh;
        private Button? _btnClear;
        private Button? _btnExport;
        private Label? _lblStatus;

        // FIXED: Added the missing constructor declaration and opening brace here
        public AuditLogForm(DbConnectionSettings settings)
        {
            _settings = settings;
            var connectionFactory = new OracleConnectionFactory(settings);
            _auditService = new AuditLogService(connectionFactory);

            Text = "📋 Audit Log Viewer - Xem lịch sử thay đổi";
            Size = new Size(1200, 700);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = UIHelper.PrimaryDark;

            InitializeComponent();
            LoadAllData();
        }

        private void InitializeComponent()
        {
            // === Header Panel ===
            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = UIHelper.CardBackground,
                Padding = new Padding(16, 12, 16, 12)
            };

            var lblTitle = new Label
            {
                Text = "Lịch Sử Audit - Theo Dõi Tất Cả Hành Động",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = UIHelper.TextPrimary,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            _lblStatus = new Label
            {
                Text = "⏳ Đang tải dữ liệu...",
                Font = UIHelper.SmallFont,
                ForeColor = UIHelper.TextSecondary,
                AutoSize = true,
                Location = new Point(0, 32)
            };

            headerPanel.Controls.AddRange(new Control[] { lblTitle, _lblStatus });

            // === Toolbar Panel ===
            var toolbarPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = UIHelper.CardBackground,
                Padding = new Padding(16, 8, 16, 8),
                FlowDirection = FlowDirection.LeftToRight,
                AutoScroll = false,
                WrapContents = false
            };

            _txtSearchUser = UIHelper.CreateTextBox(200);
            _txtSearchUser.PlaceholderText = "🔍 Tìm user...";

            _txtSearchAction = UIHelper.CreateTextBox(200);
            _txtSearchAction.PlaceholderText = "🔍 Tìm hành động...";

            _btnRefresh = UIHelper.CreateButton("🔄 Làm Mới", UIHelper.PrimaryBlue, 120, 35);
            _btnRefresh.Click += (s, e) => BtnRefresh_Click(s, e);

            _btnClear = UIHelper.CreateButton("🗑️ Xóa Cũ", UIHelper.AccentOrange, 120, 35);
            _btnClear.Click += (s, e) => BtnClear_Click(s, e);

            _btnExport = UIHelper.CreateButton("📥 Xuất CSV", UIHelper.AccentGreen, 120, 35);
            _btnExport.Click += (s, e) => BtnExport_Click(s, e);

            toolbarPanel.Controls.AddRange(new Control[] { _txtSearchUser, _txtSearchAction, _btnRefresh, _btnClear, _btnExport });

            // === Tab Control ===
            _tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9)
            };

            _tabControl.TabPages.Add(BuildTodayTab());
            _tabControl.TabPages.Add(BuildDataChangesTab());
            _tabControl.TabPages.Add(BuildErrorsTab());
            _tabControl.TabPages.Add(BuildDeploymentTab());
            _tabControl.TabPages.Add(BuildSummaryTab());
            _tabControl.TabPages.Add(BuildStandardAuditTab());
            _tabControl.TabPages.Add(BuildUnifiedAuditTab());
            _tabControl.TabPages.Add(BuildAuditStatsTab());

            // === Main Layout ===
            Controls.AddRange(new Control[] { _tabControl, toolbarPanel, headerPanel });
            Controls.SetChildIndex(headerPanel, 2);
            Controls.SetChildIndex(toolbarPanel, 1);
        }

        private TabPage BuildTodayTab()
        {
            var tab = new TabPage("📅 Hôm Nay");

            _dgvTodayLogs = CreateDataGrid();
            _dgvTodayLogs.Columns.Add("audit_id", "ID Audit");
            _dgvTodayLogs.Columns.Add("username", "Người Dùng");
            _dgvTodayLogs.Columns.Add("full_name", "Tên Đầy Đủ");
            _dgvTodayLogs.Columns.Add("action_type", "Hành Động");
            _dgvTodayLogs.Columns.Add("result", "Kết Quả");
            _dgvTodayLogs.Columns.Add("action_timestamp", "Thời Gian");
            _dgvTodayLogs.Columns.Add("notes", "Ghi Chú");

            SetColumnWidths(_dgvTodayLogs, new[] { 100, 100, 150, 100, 80, 150, 200 });

            tab.Controls.Add(_dgvTodayLogs);
            return tab;
        }

        private TabPage BuildDataChangesTab()
        {
            var tab = new TabPage("📝 Thay Đổi Dữ Liệu");

            _dgvDataChanges = CreateDataGrid();
            _dgvDataChanges.Columns.Add("audit_id", "ID Audit");
            _dgvDataChanges.Columns.Add("username", "Người Dùng");
            _dgvDataChanges.Columns.Add("full_name", "Tên");
            _dgvDataChanges.Columns.Add("action_type", "Loại");
            _dgvDataChanges.Columns.Add("record_id", "Record ID");
            _dgvDataChanges.Columns.Add("old_value", "Giá Trị Cũ");
            _dgvDataChanges.Columns.Add("new_value", "Giá Trị Mới");
            _dgvDataChanges.Columns.Add("result", "Kết Quả");
            _dgvDataChanges.Columns.Add("action_timestamp", "Thời Gian");

            SetColumnWidths(_dgvDataChanges, new[] { 80, 80, 100, 70, 80, 150, 150, 70, 150 });

            tab.Controls.Add(_dgvDataChanges);
            return tab;
        }

        private TabPage BuildErrorsTab()
        {
            var tab = new TabPage("⚠️ Lỗi");

            _dgvErrors = CreateDataGrid();
            _dgvErrors.Columns.Add("audit_id", "ID Audit");
            _dgvErrors.Columns.Add("username", "Người Dùng");
            _dgvErrors.Columns.Add("full_name", "Tên");
            _dgvErrors.Columns.Add("action_type", "Hành Động");
            _dgvErrors.Columns.Add("error_code", "Mã Lỗi");
            _dgvErrors.Columns.Add("error_message", "Thông Báo Lỗi");
            _dgvErrors.Columns.Add("result", "Kết Quả");
            _dgvErrors.Columns.Add("action_timestamp", "Thời Gian");

            SetColumnWidths(_dgvErrors, new[] { 80, 80, 100, 100, 80, 250, 80, 150 });

            tab.Controls.Add(_dgvErrors);
            return tab;
        }

        private TabPage BuildDeploymentTab()
        {
            var tab = new TabPage("🚀 Triển Khai");

            _dgvDeployment = CreateDataGrid();
            _dgvDeployment.Columns.Add("deployment_type", "Loại Triển Khai");
            _dgvDeployment.Columns.Add("application_version", "Phiên Bản");
            _dgvDeployment.Columns.Add("deployment_description", "Mô Tả");
            _dgvDeployment.Columns.Add("so_thao_tac", "Số Thao Tác");
            _dgvDeployment.Columns.Add("thoi_gian_bat_dau", "Bắt Đầu");
            _dgvDeployment.Columns.Add("thoi_gian_ket_thuc", "Kết Thúc");

            SetColumnWidths(_dgvDeployment, new[] { 150, 120, 250, 100, 150, 150 });

            tab.Controls.Add(_dgvDeployment);
            return tab;
        }

        private TabPage BuildSummaryTab()
        {
            var tab = new TabPage("📊 Thống Kê");

            _dgvSummary = CreateDataGrid();
            _dgvSummary.Columns.Add("action_type", "Loại Hành Động");
            _dgvSummary.Columns.Add("so_lan_thuc_hien", "Tổng Số");
            _dgvSummary.Columns.Add("thanh_cong", "Thành Công");
            _dgvSummary.Columns.Add("that_bai", "Thất Bại");
            _dgvSummary.Columns.Add("so_user_thuc_hien", "Số User");

            SetColumnWidths(_dgvSummary, new[] { 150, 100, 100, 100, 100 });

            tab.Controls.Add(_dgvSummary);
            return tab;
        }

        private TabPage BuildStandardAuditTab()
        {
            var tab = new TabPage("🔍 Standard Audit (DBA)");

            _dgvStandardAudit = CreateDataGrid();
            _dgvStandardAudit.Columns.Add("audit_id", "ID");
            _dgvStandardAudit.Columns.Add("username", "User");
            _dgvStandardAudit.Columns.Add("action_name", "Hành Động");
            _dgvStandardAudit.Columns.Add("returncode", "Mã Lỗi");
            _dgvStandardAudit.Columns.Add("result_status", "Kết Quả");
            _dgvStandardAudit.Columns.Add("action_timestamp", "Thời Gian");

            SetColumnWidths(_dgvStandardAudit, new[] { 60, 80, 100, 70, 90, 150 });

            tab.Controls.Add(_dgvStandardAudit);
            return tab;
        }

        private TabPage BuildUnifiedAuditTab()
        {
            var tab = new TabPage("🛡️ Unified Audit");

            _dgvUnifiedAudit = CreateDataGrid();
            _dgvUnifiedAudit.Columns.Add("audit_id", "ID");
            _dgvUnifiedAudit.Columns.Add("database_user", "Database User");
            _dgvUnifiedAudit.Columns.Add("audit_option", "Audit Option");
            _dgvUnifiedAudit.Columns.Add("object_name", "Bảng");
            _dgvUnifiedAudit.Columns.Add("action_name", "Hành Động");
            _dgvUnifiedAudit.Columns.Add("action_timestamp", "Thời Gian");
            _dgvUnifiedAudit.Columns.Add("sql_preview", "SQL Preview");

            SetColumnWidths(_dgvUnifiedAudit, new[] { 60, 100, 180, 120, 100, 150, 300 });

            tab.Controls.Add(_dgvUnifiedAudit);
            return tab;
        }

        private TabPage BuildAuditStatsTab()
        {
            var tab = new TabPage("📈 Audit Statistics");

            _dgvAuditStats = CreateDataGrid();
            _dgvAuditStats.Columns.Add("stat_type", "Loại Thống Kê");
            _dgvAuditStats.Columns.Add("metric_name", "Chỉ Số");
            _dgvAuditStats.Columns.Add("metric_value", "Giá Trị");
            _dgvAuditStats.Columns.Add("updated_time", "Cập Nhật");

            SetColumnWidths(_dgvAuditStats, new[] { 150, 200, 150, 150 });

            tab.Controls.Add(_dgvAuditStats);
            return tab;
        }

        private DataGridView CreateDataGrid()
        {
            return new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToOrderColumns = true,
                ReadOnly = true,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders,
                BackgroundColor = UIHelper.PrimaryDark,
                ForeColor = UIHelper.TextPrimary,
                GridColor = UIHelper.BorderColor,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = UIHelper.PrimaryBlue,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold)
                },
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = UIHelper.PrimaryDark,
                    ForeColor = UIHelper.TextPrimary,
                    SelectionBackColor = UIHelper.PrimaryBlue,
                    SelectionForeColor = Color.White
                }
            };
        }

        private void SetColumnWidths(DataGridView dgv, int[] widths)
        {
            for (int i = 0; i < dgv.Columns.Count && i < widths.Length; i++)
            {
                dgv.Columns[i].Width = widths[i];
            }
        }

        private void LoadAllData()
        {
            try
            {
                _lblStatus!.Text = "⏳ Đang tải dữ liệu...";
                Application.DoEvents();

                // Load Today
                var todayLogs = _auditService.GetTodayAuditLogs();
                foreach (var log in todayLogs)
                {
                    _dgvTodayLogs!.Rows.Add(
                        log.AuditId, log.Username, log.FullName, log.ActionType,
                        log.Result, log.ActionTimestamp.ToString("yyyy-MM-dd HH:mm:ss"), log.Notes
                    );
                }

                // Load Data Changes
                var changes = _auditService.GetDataChanges(100);
                foreach (var change in changes)
                {
                    _dgvDataChanges!.Rows.Add(
                        change.AuditId, change.Username, change.FullName, change.ActionType,
                        change.RecordId, change.OldValue, change.NewValue,
                        change.Result, change.ActionTimestamp.ToString("yyyy-MM-dd HH:mm:ss")
                    );
                }

                // Load Errors
                var errors = _auditService.GetErrors();
                foreach (var err in errors)
                {
                    _dgvErrors!.Rows.Add(
                        err.AuditId, err.Username, err.FullName, err.ActionType,
                        err.ErrorCode, err.ErrorMessage, err.Result,
                        err.ActionTimestamp.ToString("yyyy-MM-dd HH:mm:ss")
                    );
                }

                // Load Deployment
                var deploymentDt = _auditService.GetDeploymentInfo();
                foreach (DataRow row in deploymentDt.Rows)
                {
                    _dgvDeployment!.Rows.Add(
                        row["deployment_type"], row["application_version"], row["deployment_description"],
                        row["so_thao_tac"], row["thoi_gian_bat_dau"], row["thoi_gian_ket_thuc"]
                    );
                }

                // Load Summary
                var summaryDt = _auditService.GetAuditStatistics();
                foreach (DataRow row in summaryDt.Rows)
                {
                    _dgvSummary!.Rows.Add(
                        row["action_type"], row["so_lan_thuc_hien"], row["thanh_cong"],
                        row["that_bai"], row["so_user_thuc_hien"]
                    );
                }

                // Load Standard Audit Logs (từ DBA_AUDIT_TRAIL)
                var standardAudits = _auditService.GetStandardAuditLogs("", "", 50);
                foreach (var audit in standardAudits)
                {
                    _dgvStandardAudit!.Rows.Add(
                        audit.AuditId, audit.Username, audit.ActionType,
                        audit.ErrorCode, audit.Result,
                        audit.ActionTimestamp.ToString("yyyy-MM-dd HH:mm:ss")
                    );
                }

                // Load Unified Audit Logs (từ UNIFIED_AUDIT_TRAIL)
                var unifiedAudits = _auditService.GetUnifiedAuditLogs("", "", 50);
                foreach (var audit in unifiedAudits)
                {
                    var noteParts = audit.Notes?.Split('|') ?? new[] { "", "" };
                    _dgvUnifiedAudit!.Rows.Add(
                        audit.AuditId, audit.Username, noteParts[0].Trim(),
                        audit.ObjectName, audit.ActionType,
                        audit.ActionTimestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                        noteParts.Length > 1 ? noteParts[1].Trim() : ""
                    );
                }

                // Load Audit Statistics
                var auditStatsDt = _auditService.GetAuditStatisticsFromDBA();
                foreach (DataRow row in auditStatsDt.Rows)
                {
                    _dgvAuditStats!.Rows.Add(
                        "Standard Audit", row["action_name"], row["so_lan_thuc_hien"], DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    );
                }

                var unifiedStatsDt = _auditService.GetUnifiedAuditStatistics();
                foreach (DataRow row in unifiedStatsDt.Rows)
                {
                    _dgvAuditStats!.Rows.Add(
                        "Unified Audit", row["action_name"], row["so_lan_ghi_vay"], DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    );
                }

                _lblStatus.Text = $"✅ Tải thành công | Hôm nay: {_dgvTodayLogs!.Rows.Count} bản ghi | Standard Audit: {_dgvStandardAudit!.Rows.Count} | Unified Audit: {_dgvUnifiedAudit!.Rows.Count}";
            }
            catch (Exception ex)
            {
                _lblStatus!.ForeColor = UIHelper.AccentRed;
                _lblStatus!.Text = $"❌ Lỗi: {ex.Message}";
            }
        }

        private void BtnRefresh_Click(object? sender, EventArgs e)
        {
            _dgvTodayLogs?.Rows.Clear();
            _dgvDataChanges?.Rows.Clear();
            _dgvErrors?.Rows.Clear();
            _dgvDeployment?.Rows.Clear();
            _dgvSummary?.Rows.Clear();
            _dgvStandardAudit?.Rows.Clear();
            _dgvUnifiedAudit?.Rows.Clear();
            _dgvAuditStats?.Rows.Clear();

            LoadAllData();
        }

        private void BtnClear_Click(object? sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Bạn muốn xóa các audit log cũ hơn 30 ngày?\n\nHành động này không thể hoàn tác!",
                "Xác Nhận Xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                try
                {
                    _auditService.ClearAuditTrail(30);
                    UIHelper.ShowSuccess("Đã xóa audit log cũ hơn 30 ngày.");
                    BtnRefresh_Click(sender, e);
                }
                catch (Exception ex)
                {
                    UIHelper.ShowError($"Lỗi khi xóa: {ex.Message}");
                }
            }
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv|Excel Files (*.xlsx)|*.xlsx",
                    DefaultExt = "csv",
                    FileName = $"AuditLog_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    if (saveDialog.FileName.EndsWith(".csv"))
                    {
                        ExportToCSV(saveDialog.FileName);
                    }
                    UIHelper.ShowSuccess($"Đã xuất dữ liệu thành công!");
                }
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Lỗi khi xuất: {ex.Message}");
            }
        }

        private void ExportToCSV(string filePath)
        {
            using (var fileStream = System.IO.File.Create(filePath))
            using (var writer = new System.IO.StreamWriter(fileStream, System.Text.Encoding.UTF8))
            {
                var dgv = _dgvTodayLogs;

                // Write headers
                var headers = new System.Collections.Generic.List<string>();
                foreach (DataGridViewColumn col in dgv!.Columns)
                {
                    headers.Add($"\"{col.HeaderText}\"");
                }
                writer.WriteLine(string.Join(",", headers));

                // Write rows
                foreach (DataGridViewRow row in dgv.Rows)
                {
                    var cells = new System.Collections.Generic.List<string>();
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        cells.Add($"\"{cell.Value?.ToString() ?? ""}\"");
                    }
                    writer.WriteLine(string.Join(",", cells));
                }
            }
        }
    }
}