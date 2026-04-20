using OracleAdminWinForms.Forms;

namespace OracleAdminWinForms;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        using var loginForm = new LoginForm();
        if (loginForm.ShowDialog() == DialogResult.OK && loginForm.ConnectionSettings is not null)
        {
            Application.Run(new MainForm(loginForm.ConnectionSettings));
        }
    }
}
