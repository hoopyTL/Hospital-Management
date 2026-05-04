using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HospitalManagement.App.DataAccess;
using HospitalManagement.App.Helpers;
using Oracle.ManagedDataAccess.Client;

namespace HospitalManagement.App.Forms
{
    public class BenhNhanForm : Form
    {
        // Header
        private Panel pnlHeader;
        private Label lblWelcome;
        private Label lblRole;
        private Button btnLogout;

        // Info panels
        private Panel pnlReadonly;
        private Panel pnlEditable;

        // Readonly fields
        private TextBox txtMaBN, txtTenBN, txtPhai, txtNgaySinh, txtCCCD;

        // Editable fields
        private TextBox txtSoNha, txtTenDuong, txtQuanHuyen, txtTinhTP;
        private TextBox txtTienSuBenh, txtTienSuBenhGD, txtDiUngThuoc;

        // Button
        private Button btnCapNhat;

        public BenhNhanForm()
        {
            InitializeComponent();
            LoadThongTin();
        }

        private void InitializeComponent()
        {
            UIHelper.StyleForm(this, "Hospital Management – Bệnh nhân", 900, 750);
            this.DoubleBuffered = true;
            this.AutoScroll = true;

            // Paint gradient
            this.Paint += (s, e) =>
            {
                using var brush = new LinearGradientBrush(
                    this.ClientRectangle,
                    UIHelper.PrimaryDark,
                    Color.FromArgb(25, 25, 50),
                    LinearGradientMode.Vertical);
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            };

            // === Header ===
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 65,
                BackColor = UIHelper.SecondaryDark,
            };
            pnlHeader.Paint += (s, e) =>
            {
                using var pen = new Pen(UIHelper.BorderColor, 1);
                e.Graphics.DrawLine(pen, 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);
            };

            lblWelcome = new Label
            {
                Text = $"🧑‍🦰  Xin chào, {OracleHelper.Instance.CurrentUser}",
                Font = new Font("Segoe UI Semibold", 14, FontStyle.Bold),
                ForeColor = UIHelper.TextPrimary,
                AutoSize = true,
                Location = new Point(25, 12),
                BackColor = Color.Transparent,
            };

            lblRole = new Label
            {
                Text = "Vai trò: Bệnh nhân",
                Font = UIHelper.SmallFont,
                ForeColor = UIHelper.AccentOrange,
                AutoSize = true,
                Location = new Point(27, 40),
                BackColor = Color.Transparent,
            };

            btnLogout = UIHelper.CreateButton("🚪 Đăng xuất", UIHelper.AccentRed, 130, 36);
            btnLogout.Location = new Point(740, 15);
            btnLogout.Click += (s, e) => this.Close();

            pnlHeader.Controls.AddRange(new Control[] { lblWelcome, lblRole, btnLogout });

            // === Title ===
            var lblPageTitle = new Label
            {
                Text = "📋 Thông tin cá nhân bệnh nhân",
                Font = UIHelper.HeadingFont,
                ForeColor = UIHelper.TextPrimary,
                AutoSize = true,
                Location = new Point(30, 80),
                BackColor = Color.Transparent,
            };

            // === Readonly Panel ===
            pnlReadonly = UIHelper.CreateCard(830, 250);
            pnlReadonly.Location = new Point(30, 115);
            pnlReadonly.Paint += (s, e) =>
            {
                using var pen = new Pen(UIHelper.BorderColor, 1);
                e.Graphics.DrawRectangle(pen, 0, 0, pnlReadonly.Width - 1, pnlReadonly.Height - 1);
            };

            var lblRoTitle = UIHelper.CreateLabel("🔒 Thông tin không thể chỉnh sửa (Mã, Họ tên, Phái, Ngày sinh, CCCD)", UIHelper.SmallFont, UIHelper.TextSecondary);
            lblRoTitle.Location = new Point(20, 12);
            pnlReadonly.Controls.Add(lblRoTitle);

            int yPos = 45;
            int leftCol = 20;
            int rightCol = 420;

            AddFieldToPanel(pnlReadonly, "Mã BN:", out txtMaBN, leftCol, ref yPos, true);
            int yRight = 45;
            AddFieldToPanel(pnlReadonly, "Tên BN:", out txtTenBN, rightCol, ref yRight, true);

            AddFieldToPanel(pnlReadonly, "Phái:", out txtPhai, leftCol, ref yPos, true);
            AddFieldToPanel(pnlReadonly, "Ngày sinh:", out txtNgaySinh, rightCol, ref yRight, true);

            AddFieldToPanel(pnlReadonly, "CCCD:", out txtCCCD, leftCol, ref yPos, true);

            // === Editable Panel ===
            pnlEditable = UIHelper.CreateCard(830, 380);
            pnlEditable.Location = new Point(30, 380);
            pnlEditable.Paint += (s, e) =>
            {
                using var pen = new Pen(UIHelper.AccentGreen, 1);
                e.Graphics.DrawRectangle(pen, 0, 0, pnlEditable.Width - 1, pnlEditable.Height - 1);
            };

            var lblEdTitle = UIHelper.CreateLabel("✏️ Thông tin có thể chỉnh sửa", UIHelper.LabelFont, UIHelper.AccentGreen);
            lblEdTitle.Location = new Point(20, 12);
            pnlEditable.Controls.Add(lblEdTitle);

            int yEd = 45;
            int yEdR = 45;
            AddFieldToPanel(pnlEditable, "Số nhà:", out txtSoNha, leftCol, ref yEd, false);
            AddFieldToPanel(pnlEditable, "Tên đường:", out txtTenDuong, rightCol, ref yEdR, false);

            AddFieldToPanel(pnlEditable, "Quận/Huyện:", out txtQuanHuyen, leftCol, ref yEd, false);
            AddFieldToPanel(pnlEditable, "Tỉnh/TP:", out txtTinhTP, rightCol, ref yEdR, false);

            AddFieldToPanel(pnlEditable, "Tiền sử bệnh:", out txtTienSuBenh, leftCol, ref yEd, false, 350);
            yEdR = yEd; // Align
            AddFieldToPanel(pnlEditable, "TS bệnh GĐ:", out txtTienSuBenhGD, leftCol, ref yEd, false, 350);
            AddFieldToPanel(pnlEditable, "Dị ứng thuốc:", out txtDiUngThuoc, leftCol, ref yEd, false, 350);

            btnCapNhat = UIHelper.CreateButton("✅ Cập nhật thông tin", UIHelper.AccentGreen, 220, 44);
            btnCapNhat.Location = new Point(20, yEd + 5);
            btnCapNhat.Click += BtnCapNhat_Click;

            pnlEditable.Controls.Add(btnCapNhat);

            this.Controls.AddRange(new Control[] { pnlHeader, lblPageTitle, pnlReadonly, pnlEditable });
        }

        private void AddFieldToPanel(Panel panel, string label, out TextBox textBox,
            int xBase, ref int yPos, bool readOnly, int textBoxWidth = 250)
        {
            var lbl = UIHelper.CreateLabel(label);
            lbl.Location = new Point(xBase, yPos + 4);
            lbl.Size = new Size(110, 25);

            textBox = UIHelper.CreateTextBox(textBoxWidth);
            textBox.Location = new Point(xBase + 115, yPos);
            textBox.ReadOnly = readOnly;
            if (readOnly)
            {
                textBox.BackColor = Color.FromArgb(30, 30, 50);
                textBox.ForeColor = UIHelper.TextSecondary;
            }

            panel.Controls.AddRange(new Control[] { lbl, textBox });
            yPos += 42;
        }

        private void LoadThongTin()
        {
            try
            {
                // View tự lọc theo SESSION_USER; bệnh nhân không dùng VPD trên BENH_NHAN.
                var dt = OracleHelper.Instance.ExecuteQuery(
                    "SELECT MA_BN, TEN_BN, PHAI, NGAY_SINH, CCCD, SO_NHA, TEN_DUONG, " +
                    "QUAN_HUYEN, TINH_TP, TIEN_SU_BENH, TIEN_SU_BENH_GD, DI_UNG_THUOC " +
                    "FROM ADMIN.V_BENH_NHAN_SELF");

                if (dt.Rows.Count > 0)
                {
                    var row = dt.Rows[0];
                    txtMaBN.Text = row["MA_BN"]?.ToString() ?? "";
                    txtTenBN.Text = row["TEN_BN"]?.ToString() ?? "";
                    txtPhai.Text = row["PHAI"]?.ToString() ?? "";
                    txtNgaySinh.Text = row["NGAY_SINH"] != DBNull.Value
                        ? Convert.ToDateTime(row["NGAY_SINH"]).ToString("dd/MM/yyyy") : "";
                    txtCCCD.Text = row["CCCD"]?.ToString() ?? "";
                    txtSoNha.Text = row["SO_NHA"]?.ToString() ?? "";
                    txtTenDuong.Text = row["TEN_DUONG"]?.ToString() ?? "";
                    txtQuanHuyen.Text = row["QUAN_HUYEN"]?.ToString() ?? "";
                    txtTinhTP.Text = row["TINH_TP"]?.ToString() ?? "";
                    txtTienSuBenh.Text = row["TIEN_SU_BENH"]?.ToString() ?? "";
                    txtTienSuBenhGD.Text = row["TIEN_SU_BENH_GD"]?.ToString() ?? "";
                    txtDiUngThuoc.Text = row["DI_UNG_THUOC"]?.ToString() ?? "";
                }
                else
                {
                    UIHelper.ShowWarning("Không tìm thấy thông tin bệnh nhân qua view cá nhân.");
                }
            }
            catch (Exception ex)
            {
                UIHelper.ShowError("Không thể tải thông tin cá nhân:\n" + ex.Message);
            }
        }

        private void BtnCapNhat_Click(object sender, EventArgs e)
        {
            try
            {
                var result = MessageBox.Show(
                    "Bạn có chắc muốn cập nhật thông tin cá nhân?",
                    "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result != DialogResult.Yes) return;

                int affected = OracleHelper.Instance.ExecuteNonQuery(
                    "UPDATE ADMIN.V_BENH_NHAN_SELF SET " +
                    "SO_NHA = :sonha, TEN_DUONG = :tenduong, QUAN_HUYEN = :quanhuyen, " +
                    "TINH_TP = :tinhtp, TIEN_SU_BENH = :tsb, TIEN_SU_BENH_GD = :tsbgd, " +
                    "DI_UNG_THUOC = :dut",
                    OracleHelper.ParamNvarchar2("sonha", txtSoNha.Text.Trim()),
                    OracleHelper.ParamNvarchar2("tenduong", txtTenDuong.Text.Trim()),
                    OracleHelper.ParamNvarchar2("quanhuyen", txtQuanHuyen.Text.Trim()),
                    OracleHelper.ParamNvarchar2("tinhtp", txtTinhTP.Text.Trim()),
                    OracleHelper.ParamNvarchar2("tsb", txtTienSuBenh.Text.Trim()),
                    OracleHelper.ParamNvarchar2("tsbgd", txtTienSuBenhGD.Text.Trim()),
                    OracleHelper.ParamNvarchar2("dut", txtDiUngThuoc.Text.Trim()));

                if (affected > 0)
                    UIHelper.ShowSuccess("Đã cập nhật thông tin cá nhân thành công!");
                else
                    UIHelper.ShowWarning("Không có thay đổi nào được thực hiện.");
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                if (msg.Contains("ORA-01031") || msg.Contains("ORA-00942"))
                    msg = "Bạn không có quyền thực hiện thao tác này.";
                UIHelper.ShowError("Lỗi khi cập nhật:\n" + msg);
            }
        }
    }
}
