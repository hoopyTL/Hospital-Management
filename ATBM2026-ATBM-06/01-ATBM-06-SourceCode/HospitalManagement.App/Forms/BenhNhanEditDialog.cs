using System;
using System.Drawing;
using System.Windows.Forms;
using HospitalManagement.App.Helpers;

namespace HospitalManagement.App.Forms
{
    /// <summary>
    /// Dialog dùng cho Điều phối viên khi thêm mới Bệnh nhân.
    /// Trả về thông tin qua các property khi DialogResult = OK.
    /// </summary>
    public class BenhNhanEditDialog : Form
    {
        private TextBox txtTenBN, txtCCCD;
        private ComboBox cboPhai;
        private DateTimePicker dtpNgaySinh;
        private TextBox txtSoNha, txtTenDuong, txtQuanHuyen, txtTinhTP;
        private TextBox txtTienSuBenh, txtTienSuBenhGD, txtDiUngThuoc;
        private Button btnOK, btnCancel;

        public string TenBN => txtTenBN.Text.Trim();
        public string Phai => cboPhai.SelectedItem?.ToString();
        public DateTime NgaySinh => dtpNgaySinh.Value.Date;
        public string CCCD => txtCCCD.Text.Trim();
        public string SoNha => Nullify(txtSoNha.Text);
        public string TenDuong => Nullify(txtTenDuong.Text);
        public string QuanHuyen => Nullify(txtQuanHuyen.Text);
        public string TinhTP => Nullify(txtTinhTP.Text);
        public string TienSuBenh => Nullify(txtTienSuBenh.Text);
        public string TienSuBenhGD => Nullify(txtTienSuBenhGD.Text);
        public string DiUngThuoc => Nullify(txtDiUngThuoc.Text);

        private static string Nullify(string s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

        public BenhNhanEditDialog()
        {
            Init();
        }

        private void Init()
        {
            UIHelper.StyleForm(this, "Thêm bệnh nhân mới", 580, 720);
            this.StartPosition = FormStartPosition.CenterParent;
            AutoScroll = false;

            var lblTitle = new Label
            {
                Text = "➕ Thêm bệnh nhân mới",
                Font = UIHelper.HeadingFont,
                ForeColor = UIHelper.AccentGreen,
                AutoSize = true,
                Location = new Point(20, 14),
                BackColor = Color.Transparent,
            };
            this.Controls.Add(lblTitle);

            int y = 54;
            AddField("Họ tên (*):", out txtTenBN, ref y);

            // Phái combobox
            var lblPhai = UIHelper.CreateLabel("Phái (*):");
            lblPhai.Location = new Point(20, y + 4);
            lblPhai.Size = new Size(130, 24);
            cboPhai = new ComboBox
            {
                Location = new Point(160, y),
                Size = new Size(150, 32),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = UIHelper.InputBackground,
                ForeColor = UIHelper.TextPrimary,
                FlatStyle = FlatStyle.Flat,
                Font = UIHelper.InputFont,
            };
            cboPhai.Items.AddRange(new object[] { "Nam", "Nữ" });
            cboPhai.SelectedIndex = 0;
            this.Controls.AddRange(new Control[] { lblPhai, cboPhai });
            y += 45;

            // Ngày sinh
            var lblNgs = UIHelper.CreateLabel("Ngày sinh (*):");
            lblNgs.Location = new Point(20, y + 4);
            lblNgs.Size = new Size(130, 24);
            dtpNgaySinh = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Location = new Point(160, y),
                Size = new Size(200, 32),
                Value = new DateTime(1990, 1, 1),
            };
            this.Controls.AddRange(new Control[] { lblNgs, dtpNgaySinh });
            y += 45;

            AddField("CCCD (*):", out txtCCCD, ref y);
            AddField("Số nhà:", out txtSoNha, ref y);
            AddField("Tên đường:", out txtTenDuong, ref y);
            AddField("Quận/Huyện:", out txtQuanHuyen, ref y);
            AddField("Tỉnh/TP:", out txtTinhTP, ref y);
            AddField("Tiền sử bệnh:", out txtTienSuBenh, ref y);
            AddField("TS bệnh GĐ:", out txtTienSuBenhGD, ref y);
            AddField("Dị ứng thuốc:", out txtDiUngThuoc, ref y);

            const int gapBeforeButtons = 18;
            btnOK = UIHelper.CreateButton("✅ Lưu", UIHelper.AccentGreen, 160, 44);
            btnOK.Location = new Point(118, y + gapBeforeButtons);
            btnOK.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(TenBN) || string.IsNullOrWhiteSpace(CCCD))
                {
                    UIHelper.ShowWarning("Họ tên và CCCD là bắt buộc.");
                    return;
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            btnCancel = UIHelper.CreateButton("✖ Hủy", UIHelper.AccentRed, 160, 44);
            btnCancel.Location = new Point(283, y + gapBeforeButtons);
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.AddRange(new Control[] { btnOK, btnCancel });

            // Client area (không tính thanh tiêu đề): form Height=640 không đủ → hay bị cắt nút Lưu/Hủy
            const int clientW = 560;
            this.ClientSize = new Size(clientW, btnOK.Bottom + 28);
        }

        private void AddField(string label, out TextBox tb, ref int y)
        {
            var lbl = UIHelper.CreateLabel(label);
            lbl.Location = new Point(20, y + 4);
            lbl.Size = new Size(130, 24);

            tb = UIHelper.CreateTextBox(370);
            tb.Location = new Point(160, y);

            this.Controls.AddRange(new Control[] { lbl, tb });
            y += 42;
        }
    }
}
