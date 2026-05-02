using HospitalManagement.App.Helpers;

namespace HospitalManagement.App.Forms;

public class RoleDialogForm : Form
{
    private readonly TextBox _txtRole = AdminUiHelper.CreateTextBox();
    private readonly TextBox _txtPassword = AdminUiHelper.CreateTextBox("", true);
    private readonly Button _btnOk = new() { Text = "OK", Width = 90, Height = 32 };
    private readonly Button _btnCancel = new() { Text = "Hủy", Width = 90, Height = 32 };

    public string RoleName => _txtRole.Text.Trim();
    public string PasswordValue => _txtPassword.Text;

    public RoleDialogForm(string title, bool isEditPasswordOnly = false, string? roleName = null)
    {
        Text = title;
        StartPosition = FormStartPosition.CenterParent;
        Width = 420;
        Height = 190;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        _txtRole.Text = roleName ?? "";
        _txtRole.ReadOnly = isEditPasswordOnly;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16),
            ColumnCount = 2
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        AddRow(layout, 0, "Role", _txtRole);
        AddRow(layout, 1, "Password", _txtPassword);

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        buttons.Controls.AddRange([_btnCancel, _btnOk]);

        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.Controls.Add(new Label(), 0, 2);
        layout.Controls.Add(buttons, 1, 2);

        Controls.Add(layout);

        _btnOk.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(_txtRole.Text))
            {
                AdminUiHelper.ShowInfo("Vui lòng nhập role.");
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
