using System;
using System.Drawing;
using System.IO;
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
        private DataGridView? _dgvStandardAudit;
        private DataGridView? _dgvUnifiedAudit;
        private Button? _btnRefresh;
        private Button? _btnDisableAudit;
        private Button? _btnEnableAudit;
        private Label? _lblStatus;
        

        public AuditLogForm(DbConnectionSettings settings)
        {
            _settings = settings;
            var connectionFactory = new OracleConnectionFactory(settings);
            _auditService = new AuditLogService(connectionFactory);

            Text = "📋 Audit Log Viewer";
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
                Dock = DockStyle.Top, Height = 70, BackColor = UIHelper.CardBackground, Padding = new Padding(16, 12, 16, 12)
            };

            var lblTitle = new Label
            {
                Text = "Lịch Sử Audit",
                Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = UIHelper.TextPrimary, AutoSize = true, Location = new Point(0, 0)
            };

            _lblStatus = new Label
            {
                Text = "⏳ Đang tải dữ liệu...",
                Font = UIHelper.SmallFont, ForeColor = UIHelper.TextSecondary, AutoSize = true, Location = new Point(0, 32)
            };

            headerPanel.Controls.AddRange(new Control[] { lblTitle, _lblStatus });

            // === Toolbar Panel ===
            var toolbarPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, Height = 50, BackColor = UIHelper.CardBackground, Padding = new Padding(16, 8, 16, 8), FlowDirection = FlowDirection.LeftToRight
            };

            _btnRefresh = UIHelper.CreateButton("🔄 Làm Mới", UIHelper.PrimaryBlue, 120, 35);
            _btnRefresh.Click += (s, e) => BtnRefresh_Click(s, e);
            toolbarPanel.Controls.Add(_btnRefresh);

            _btnDisableAudit = UIHelper.CreateButton("🛑 Tắt Audit", UIHelper.AccentRed, 120, 35);
            _btnDisableAudit.Click += (s, e) => BtnDisableAudit_Click(s, e);

            _btnEnableAudit = UIHelper.CreateButton("✅ Bật Audit", Color.MediumSeaGreen, 120, 35);
            _btnEnableAudit.Click += (s, e) => BtnEnableAudit_Click(s, e);

            toolbarPanel.Controls.Add(_btnRefresh);
            toolbarPanel.Controls.Add(_btnEnableAudit);
            toolbarPanel.Controls.Add(_btnDisableAudit);

            // === Tab Control ===
            _tabControl = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9) };
            _tabControl.TabPages.Add(BuildStandardAuditTab());
            _tabControl.TabPages.Add(BuildUnifiedAuditTab());

            // === Main Layout ===
            Controls.AddRange(new Control[] { _tabControl, toolbarPanel, headerPanel });
            Controls.SetChildIndex(headerPanel, 2);
            Controls.SetChildIndex(toolbarPanel, 1);
        }

        private TabPage BuildStandardAuditTab()
        {
            var tab = new TabPage("🔍 Standard Audit (DBA)");
            _dgvStandardAudit = CreateDataGrid();
            
            _dgvStandardAudit.Columns.Add("username", "Tài khoản");
            _dgvStandardAudit.Columns.Add("action_timestamp", "Thời gian");
            _dgvStandardAudit.Columns.Add("object_name", "Tên bảng");
            _dgvStandardAudit.Columns.Add("action_type", "Hành động");
            _dgvStandardAudit.Columns.Add("returncode", "Mã kết quả");
            _dgvStandardAudit.Columns.Add("result_status", "Trạng thái");
            _dgvStandardAudit.Columns.Add("sql_text", "Câu lệnh SQL");

            SetColumnWidths(_dgvStandardAudit, new[] { 100, 150, 120, 100, 80, 100, 300 });
            tab.Controls.Add(_dgvStandardAudit);
            return tab;
        }

        private TabPage BuildUnifiedAuditTab()
        {
            var tab = new TabPage("🛡️ Unified Audit");
            _dgvUnifiedAudit = CreateDataGrid();

            _dgvUnifiedAudit.Columns.Add("username", "Tài khoản");
            _dgvUnifiedAudit.Columns.Add("action_timestamp", "Thời gian");
            _dgvUnifiedAudit.Columns.Add("action_type", "Hành động");
            _dgvUnifiedAudit.Columns.Add("object_schema", "Schema");
            _dgvUnifiedAudit.Columns.Add("object_name", "Tên bảng");
            _dgvUnifiedAudit.Columns.Add("policy", "Tên Policy vi phạm");
            _dgvUnifiedAudit.Columns.Add("sql_text", "Câu lệnh SQL");
            _dgvUnifiedAudit.Columns.Add("client_program", "Phần mềm kết nối");

            SetColumnWidths(_dgvUnifiedAudit, new[] { 100, 150, 100, 80, 120, 250, 300, 150 });
            tab.Controls.Add(_dgvUnifiedAudit);
            return tab;
        }

        private DataGridView CreateDataGrid()
        {
            return new DataGridView
            {
                Dock = DockStyle.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                ReadOnly = true, AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders,
                BackgroundColor = UIHelper.PrimaryDark, ForeColor = UIHelper.TextPrimary, GridColor = UIHelper.BorderColor,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = UIHelper.PrimaryBlue, ForeColor = Color.White, Font = new Font("Segoe UI", 9, FontStyle.Bold) },
                DefaultCellStyle = new DataGridViewCellStyle { BackColor = UIHelper.PrimaryDark, ForeColor = UIHelper.TextPrimary, SelectionBackColor = UIHelper.PrimaryBlue, SelectionForeColor = Color.White }
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

                // Load Standard Audit
                var standardAudits = _auditService.GetStandardAuditLogs(200);
                foreach (var audit in standardAudits)
                {
                    _dgvStandardAudit!.Rows.Add(
                        audit.Username, audit.ActionTimestamp.ToString("yyyy-MM-dd HH:mm:ss"), 
                        audit.ObjectName, audit.ActionType, audit.ErrorCode, audit.Result, audit.SqlStatement
                    );
                }

                // Load Unified Audit
                var unifiedAudits = _auditService.GetUnifiedAuditLogs(200);
                foreach (var audit in unifiedAudits)
                {
                    _dgvUnifiedAudit!.Rows.Add(
                        audit.Username, audit.ActionTimestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                        audit.ActionType, audit.ObjectSchema, audit.ObjectName, 
                        audit.Notes, audit.SqlStatement, audit.MachineName
                    );
                }

                _lblStatus.Text = $"✅ Tải thành công | Standard: {_dgvStandardAudit!.Rows.Count} dòng | Unified: {_dgvUnifiedAudit!.Rows.Count} dòng";
            }
            catch (Exception ex)
            {
                _lblStatus!.ForeColor = UIHelper.AccentRed;
                _lblStatus!.Text = $"❌ Lỗi: {ex.Message}";
            }
        }

        private void BtnRefresh_Click(object? sender, EventArgs e)
        {
            _dgvStandardAudit?.Rows.Clear();
            _dgvUnifiedAudit?.Rows.Clear();
            LoadAllData();
        }

        private void BtnEnableAudit_Click(object? sender, EventArgs e)
            {
                try
                {
                    _lblStatus!.Text = "⏳ Đang chạy script bật Audit...";
                    _lblStatus.ForeColor = UIHelper.TextSecondary;
                    Application.DoEvents();

                    // Đường dẫn tới file 01_audit.sql
                    string basePath = AppDomain.CurrentDomain.BaseDirectory;
                    string scriptPath = Path.GetFullPath(Path.Combine(basePath, @"..\..\..\..\..\03-Database\Audit\01_audit.sql"));
                    
                    _auditService.ExecuteSqlScript(scriptPath);

                    _lblStatus.Text = "✅ Đã bật Audit thành công!";
                    _lblStatus.ForeColor = Color.MediumSeaGreen;
                    MessageBox.Show("Đã thực thi script bật Audit (01_audit.sql) thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // Sau khi bật audit, làm mới lại dữ liệu để xem log mới nhất
                    BtnRefresh_Click(null, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    _lblStatus!.Text = "❌ Lỗi khi bật Audit!";
                    _lblStatus.ForeColor = UIHelper.AccentRed;
                    MessageBox.Show($"Lỗi thực thi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        private void BtnDisableAudit_Click(object? sender, EventArgs e)
        {
            var confirmResult = MessageBox.Show(
                "Bạn có chắc chắn muốn TẮT Audit không? Thao tác này sẽ làm ngừng việc ghi log hệ thống.",
                "Xác nhận tắt Audit",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirmResult == DialogResult.Yes)
            {
                try
                {
                    _lblStatus!.Text = "⏳ Đang chạy script tắt Audit...";
                    _lblStatus.ForeColor = UIHelper.TextSecondary;
                    Application.DoEvents();

                    // Xác định đường dẫn tương đối (AppDomain.CurrentDomain.BaseDirectory thường trỏ vào thư mục bin/Debug/netx.x)
                    string basePath = AppDomain.CurrentDomain.BaseDirectory;
                    string scriptPath = Path.GetFullPath(Path.Combine(basePath, @"..\..\..\..\..\03-Database\Audit\03_disable_audit.sql"));
                    
                    // Gọi Service để chạy script
                    _auditService.ExecuteSqlScript(scriptPath);

                    _lblStatus.Text = "✅ Đã tắt Audit thành công!";
                    _lblStatus.ForeColor = Color.MediumSeaGreen;
                    MessageBox.Show("Đã thực thi script tắt Audit thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    _lblStatus!.Text = "❌ Lỗi khi tắt Audit!";
                    _lblStatus.ForeColor = UIHelper.AccentRed;
                    MessageBox.Show($"Lỗi thực thi: {ex.Message}\n\nĐường dẫn script có thể chưa chính xác.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}