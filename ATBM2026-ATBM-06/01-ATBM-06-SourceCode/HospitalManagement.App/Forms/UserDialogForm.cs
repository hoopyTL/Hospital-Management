using HospitalManagement.App.Helpers;

namespace HospitalManagement.App.Forms;

public class UserDialogForm : Form
{
    private readonly TextBox _txtUsername = AdminUiHelper.CreateTextBox();
    private readonly TextBox _txtPassword = AdminUiHelper.CreateTextBox("", true);
    private readonly TextBox _txtDefaultTs = AdminUiHelper.CreateTextBox("USERS");
    private readonly TextBox _txtTempTs = AdminUiHelper.CreateTextBox("TEMP");
    private readonly Button _btnOk = new() { Text = "OK", Width = 90, Height = 32 };
    private readonly Button _btnCancel = new() { Text = "Hủy", Width = 90, Height = 32 };

    public string Username => _txtUsername.Text.Trim();
    public string PasswordValue => _txtPassword.Text;
    public string DefaultTablespace => string.IsNullOrWhiteSpace(_txtDefaultTs.Text) ? "USERS" : _txtDefaultTs.Text.Trim();
    public string TemporaryTablespace => string.IsNullOrWhiteSpace(_txtTempTs.Text) ? "TEMP" : _txtTempTs.Text.Trim();

    public UserDialogForm(string title, bool isEditPasswordOnly = false, string? username = null)
    {
        Text = title;
        StartPosition = FormStartPosition.CenterParent;
        Width = 440;
        Height = isEditPasswordOnly ? 220 : 300;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        _txtUsername.Text = username ?? "";
        _txtUsername.ReadOnly = isEditPasswordOnly;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16),
            ColumnCount = 2
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        AddRow(layout, 0, "Username", _txtUsername);
        AddRow(layout, 1, "Password", _txtPassword);

        if (!isEditPasswordOnly)
        {
            AddRow(layout, 2, "Default tablespace", _txtDefaultTs);
            AddRow(layout, 3, "Temporary tablespace", _txtTempTs);
        }

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        buttons.Controls.AddRange([_btnCancel, _btnOk]);

        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        layout.Controls.Add(new Label(), 0, isEditPasswordOnly ? 2 : 4);
        layout.Controls.Add(buttons, 1, isEditPasswordOnly ? 2 : 4);

        Controls.Add(layout);

        _btnOk.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(_txtUsername.Text) || string.IsNullOrWhiteSpace(_txtPassword.Text))
            {
                AdminUiHelper.ShowInfo("Vui lòng nhập username và password.");
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        };
        _btnCancel.Click += (_, _) => Close();
    }

    private static void AddRow(TableLayoutPanel layout, int rowIndex, string label, Control control)
    {
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        layout.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left }, 0, rowIndex);
        control.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        layout.Controls.Add(control, 1, rowIndex);
    }
}
