namespace HospitalManagement.App.Helpers;

public static class AdminUiHelper
{
    public static void ShowInfo(string message, string title = "Thông báo")
        => MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);

    public static void ShowError(Exception ex, string title = "Lỗi")
        => MessageBox.Show(ex.Message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);

    public static bool Confirm(string message, string title = "Xác nhận")
        => MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

    public static Label SectionLabel(string text) => new()
    {
        AutoSize = true,
        Text = text,
        Font = new Font("Segoe UI", 10F, FontStyle.Bold),
        Margin = new Padding(3, 12, 3, 6)
    };

    public static TextBox CreateTextBox(string placeholder = "", bool password = false)
    {
        return new TextBox
        {
            Width = 220,
            UseSystemPasswordChar = password,
            PlaceholderText = placeholder
        };
    }

    public static ComboBox CreateComboBox()
    {
        return new ComboBox
        {
            Width = 220,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
    }
}
