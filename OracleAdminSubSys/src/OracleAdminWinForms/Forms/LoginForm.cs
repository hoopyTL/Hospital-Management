using OracleAdminWinForms.Models;
using OracleAdminWinForms.Services;
using OracleAdminWinForms.Utils;

namespace OracleAdminWinForms.Forms;

public class LoginForm : Form
{
    private const string RequiredAdminUsername = "ATBM_ADMIN";

    private readonly TextBox _txtHost = UiHelper.CreateTextBox("localhost");
    private readonly TextBox _txtPort = UiHelper.CreateTextBox("1521");
    private readonly TextBox _txtService = UiHelper.CreateTextBox("XEPDB1");
    private readonly TextBox _txtUsername = UiHelper.CreateTextBox("ATBM_ADMIN");
    private readonly TextBox _txtPassword = UiHelper.CreateTextBox("", true);
    private readonly CheckBox _chkSysDba = new() { Text = "Kết nối với SYSDBA", AutoSize = true };
    private readonly Label _lblStatus = new() { AutoSize = true, ForeColor = Color.DarkSlateBlue };
    private readonly Button _btnTest = new() { Text = "Kiểm tra kết nối", Width = 140, Height = 34 };
    private readonly Button _btnLogin = new() { Text = "Đăng nhập", Width = 140, Height = 34 };

    public DbConnectionSettings? ConnectionSettings { get; private set; }

    public LoginForm()
    {
        Text = "Phân hệ 1 - Kết nối Oracle";
        StartPosition = FormStartPosition.CenterScreen;
        Width = 520;
        Height = 390;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        _txtHost.Text = "localhost";
        _txtPort.Text = "1521";
        _txtService.Text = "XEPDB1";
        _txtUsername.Text = RequiredAdminUsername;
        _txtPassword.Text = "";
        _txtUsername.ReadOnly = true;
        _txtUsername.TabStop = false;
        _chkSysDba.Checked = false;
        _chkSysDba.Enabled = false;
        _chkSysDba.Visible = false;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            ColumnCount = 2,
            RowCount = 8
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        AddRow(layout, 0, "Host", _txtHost);
        AddRow(layout, 1, "Port", _txtPort);
        AddRow(layout, 2, "Service name", _txtService);
        AddRow(layout, 3, "Username", _txtUsername);
        AddRow(layout, 4, "Password", _txtPassword);


        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true
        };
        buttonPanel.Controls.AddRange([_btnTest, _btnLogin]);

        layout.Controls.Add(new Label { Text = "", AutoSize = true }, 0, 6);
        layout.Controls.Add(buttonPanel, 1, 6);

        layout.Controls.Add(new Label { Text = "", AutoSize = true }, 0, 7);
        layout.Controls.Add(_lblStatus, 1, 7);

        Controls.Add(layout);

        _btnTest.Click += (_, _) => TestConnection(showSuccess: true);
        _btnLogin.Click += (_, _) =>
        {
            if (TestConnection(showSuccess: false))
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        };
    }

    private void AddRow(TableLayoutPanel layout, int rowIndex, string label, Control control)
    {
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.Controls.Add(new Label
        {
            Text = label,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            TextAlign = ContentAlignment.MiddleLeft
        }, 0, rowIndex);
        control.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        layout.Controls.Add(control, 1, rowIndex);
    }

    private bool TestConnection(bool showSuccess)
    {
        try
        {
            var settings = new DbConnectionSettings
            {
                Host = _txtHost.Text.Trim(),
                Port = _txtPort.Text.Trim(),
                ServiceName = _txtService.Text.Trim(),
                Username = RequiredAdminUsername,
                Password = _txtPassword.Text,
                UseSysDba = false
            };

            var service = new OracleAdminService(settings);
            service.TestConnection();
            ConnectionSettings = settings;
            _lblStatus.Text = $"Kết nối thành công: {settings}";
            if (showSuccess)
                UiHelper.ShowInfo("Kết nối thành công.");
            return true;
        }
        catch (Exception ex)
        {
            _lblStatus.Text = "Kết nối thất bại.";
            UiHelper.ShowError(ex, "Không thể kết nối Oracle");
            return false;
        }
    }
}
