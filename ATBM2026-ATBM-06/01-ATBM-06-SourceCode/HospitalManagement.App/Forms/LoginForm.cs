using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HospitalManagement.App.DataAccess;
using HospitalManagement.App.Helpers;

namespace HospitalManagement.App.Forms
{
    public class LoginForm : Form
    {
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Label lblStatus;
        private Label lblTitle;
        private Label lblSubtitle;
        private Panel pnlLoginCard;
        private PictureBox picIcon;

        public LoginForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // === Form Setup ===
            this.Text = "Hospital Management System – Đăng nhập";
            this.Size = new Size(520, 620);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = UIHelper.PrimaryDark;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.DoubleBuffered = true;

            // Paint gradient background
            this.Paint += (s, e) =>
            {
                using var brush = new LinearGradientBrush(
                    this.ClientRectangle,
                    Color.FromArgb(15, 15, 35),
                    Color.FromArgb(35, 35, 70),
                    LinearGradientMode.ForwardDiagonal);
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            };

            // === Hospital Icon (emoji-based) ===
            picIcon = new PictureBox
            {
                Size = new Size(72, 72),
                Location = new Point((520 - 72) / 2, 35),
                BackColor = Color.Transparent,
                SizeMode = PictureBoxSizeMode.CenterImage,
            };
            picIcon.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                // Draw circle background
                using var circleBrush = new SolidBrush(UIHelper.PrimaryBlue);
                e.Graphics.FillEllipse(circleBrush, 0, 0, 70, 70);
                // Draw cross icon
                using var pen = new Pen(Color.White, 4);
                e.Graphics.DrawLine(pen, 35, 18, 35, 52);
                e.Graphics.DrawLine(pen, 18, 35, 52, 35);
            };

            // === Title ===
            lblTitle = new Label
            {
                Text = "🏥 Hospital Management",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = UIHelper.TextPrimary,
                AutoSize = true,
                BackColor = Color.Transparent,
            };
            lblTitle.Location = new Point((520 - 340) / 2, 118);

            // === Subtitle ===
            lblSubtitle = new Label
            {
                Text = "Hệ thống Quản lý Bệnh viện",
                Font = UIHelper.SubtitleFont,
                ForeColor = UIHelper.TextSecondary,
                AutoSize = true,
                BackColor = Color.Transparent,
            };
            lblSubtitle.Location = new Point((520 - 260) / 2, 155);

            // === Login Card Panel ===
            pnlLoginCard = new Panel
            {
                Size = new Size(400, 320),
                Location = new Point(52, 195),
                BackColor = UIHelper.CardBackground,
            };
            pnlLoginCard.Paint += (s, e) =>
            {
                // Draw border
                using var pen = new Pen(UIHelper.BorderColor, 1);
                e.Graphics.DrawRectangle(pen, 0, 0, pnlLoginCard.Width - 1, pnlLoginCard.Height - 1);
            };

            // === Username ===
            var lblUser = UIHelper.CreateLabel("👤  Tên đăng nhập (Mã NV / Mã BN)");
            lblUser.Font = UIHelper.SmallFont;
            lblUser.ForeColor = UIHelper.TextSecondary;
            lblUser.Location = new Point(30, 30);

            txtUsername = UIHelper.CreateTextBox(340);
            txtUsername.Location = new Point(30, 52);
            txtUsername.Height = 38;
            txtUsername.CharacterCasing = CharacterCasing.Upper;

            // === Password ===
            var lblPass = UIHelper.CreateLabel("🔒  Mật khẩu");
            lblPass.Font = UIHelper.SmallFont;
            lblPass.ForeColor = UIHelper.TextSecondary;
            lblPass.Location = new Point(30, 105);

            txtPassword = UIHelper.CreateTextBox(340);
            txtPassword.Location = new Point(30, 127);
            txtPassword.Height = 38;
            txtPassword.UseSystemPasswordChar = true;

            // === Login Button ===
            btnLogin = UIHelper.CreateButton("🔑  ĐĂNG NHẬP", UIHelper.PrimaryBlue, 340, 46);
            btnLogin.Location = new Point(30, 195);
            btnLogin.Click += BtnLogin_Click;

            // === Status ===
            lblStatus = new Label
            {
                Text = "",
                Font = UIHelper.SmallFont,
                ForeColor = UIHelper.AccentRed,
                AutoSize = false,
                Size = new Size(340, 40),
                Location = new Point(30, 255),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
            };

            // Add controls to card
            pnlLoginCard.Controls.AddRange(new Control[] { lblUser, txtUsername, lblPass, txtPassword, btnLogin, lblStatus });

            // Add to form
            this.Controls.AddRange(new Control[] { picIcon, lblTitle, lblSubtitle, pnlLoginCard });

            // Enter key login
            this.AcceptButton = btnLogin;
            txtUsername.Focus();
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            var username = txtUsername.Text.Trim();
            var password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                lblStatus.ForeColor = UIHelper.AccentOrange;
                lblStatus.Text = "⚠️ Vui lòng nhập đầy đủ thông tin.";
                return;
            }

            lblStatus.ForeColor = UIHelper.TextSecondary;
            lblStatus.Text = "⏳ Đang kết nối...";
            btnLogin.Enabled = false;
            Application.DoEvents();

            try
            {
                if (username.Equals("ATBM_ADMIN", StringComparison.OrdinalIgnoreCase))
                {
                    var settings = new Models.DbConnectionSettings
                    {
                        Username = username,
                        Password = password,
                        Host = "localhost",
                        Port = "1521",
                        ServiceName = "XEPDB1",
                        UseSysDba = false
                    };
                    var adminService = new Services.OracleAdminService(settings);
                    adminService.TestConnection();

                    this.Hide();
                    Form adminForm = new AdminMainForm(settings);
                    adminForm.FormClosed += (s2, e2) => this.Close();
                    adminForm.Show();
                    return;
                }

                // Kết nối bằng chính tài khoản user để SESSION_USER đúng cho VPD và self-view.
                var helper = OracleHelper.Initialize(username, password);

                if (!helper.TestConnection())
                {
                    lblStatus.ForeColor = UIHelper.AccentRed;
                    lblStatus.Text = "❌ Không thể kết nối đến Oracle.";
                    btnLogin.Enabled = true;
                    return;
                }

                // Kiểm tra role để điều hướng
                bool isDPV = helper.HasRole("RL_DIEUPHOIVIEN");
                bool isBS = helper.HasRole("RL_BACSI");
                bool isKTV = helper.HasRole("RL_KYTHUATVIEN");
                bool isBN = helper.HasRole("RL_BENHNHAN");

                this.Hide();

                Form mainForm;
                if (isDPV)
                {
                    mainForm = new DieuPhoiVienForm();
                }
                else if (isBS)
                {
                    mainForm = new BacSiForm();
                }
                else if (isKTV)
                {
                    mainForm = new KyThuatVienForm();
                }
                else if (isBN)
                {
                    mainForm = new BenhNhanForm();
                }
                else
                {
                    UIHelper.ShowWarning("Tài khoản của bạn chưa được gán vai trò hợp lệ.\nVui lòng liên hệ quản trị viên.");
                    this.Show();
                    btnLogin.Enabled = true;
                    return;
                }

                mainForm.FormClosed += (s2, e2) =>
                {
                    OracleHelper.Instance?.Dispose();

                    txtUsername.Text = "";
                    txtPassword.Text = "";
                    lblStatus.Text = "";
                    btnLogin.Enabled = true;

                    this.WindowState = FormWindowState.Normal;
                    this.Show();
                    this.BringToFront();
                    this.Activate();

                    txtUsername.Focus();
                };
                mainForm.Show();
            }
            catch (Exception ex)
            {
                lblStatus.ForeColor = UIHelper.AccentRed;
                string msg = ex.Message;
                if (msg.Contains("ORA-01017"))
                    msg = "Sai tên đăng nhập hoặc mật khẩu.";
                else if (msg.Contains("ORA-28000"))
                    msg = "Tài khoản đã bị khóa.";
                else if (msg.Contains("ORA-12541") || msg.Contains("ORA-12170"))
                    msg = "Không thể kết nối đến máy chủ Oracle.";

                lblStatus.Text = "❌ " + msg;
                btnLogin.Enabled = true;
            }
        }
    }
}
