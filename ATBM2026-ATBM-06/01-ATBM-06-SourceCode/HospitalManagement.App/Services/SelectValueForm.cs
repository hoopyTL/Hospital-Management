namespace HospitalManagement.App.Services;

public class SelectValueForm : Form
{
    private ComboBox _combo = new ComboBox();
    private Button _btnOk = new Button();
    private Button _btnCancel = new Button();

    public string SelectedValue => _combo.Text;

    public SelectValueForm(string title, IEnumerable<string> values, string? current = null)
    {
        Text = title;
        Width = 350;
        Height = 150;
        StartPosition = FormStartPosition.CenterParent;

        _combo.Left = 20;
        _combo.Top = 20;
        _combo.Width = 280;
        _combo.DropDownStyle = ComboBoxStyle.DropDownList;

        _combo.Items.AddRange(values.Cast<object>().ToArray());

        if (!string.IsNullOrWhiteSpace(current))
            _combo.SelectedItem = current;

        _btnOk.Text = "OK";
        _btnOk.Left = 140;
        _btnOk.Top = 60;
        _btnOk.Click += (_, _) => DialogResult = DialogResult.OK;

        _btnCancel.Text = "Cancel";
        _btnCancel.Left = 220;
        _btnCancel.Top = 60;
        _btnCancel.Click += (_, _) => DialogResult = DialogResult.Cancel;

        Controls.Add(_combo);
        Controls.Add(_btnOk);
        Controls.Add(_btnCancel);
    }
}
