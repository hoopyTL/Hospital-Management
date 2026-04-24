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
    public class KyThuatVienForm : Form
    {
        private TabControl tabMain;
        private TabPage tabDichVu;
        private TabPage tabThongTin;

        // Tab Dịch vụ
        private DataGridView dgvDichVu;
        private Button btnRefresh;
        private Button btnSave;
        private Label lblDvTitle;
        private Label lblDvCount;

        // Tab Thông tin cá nhân
        private TextBox txtMaNV, txtHoTen, txtPhai, txtNgaySinh, txtCMND;
        private TextBox txtVaiTro, txtChuyenKhoa;
        private TextBox txtQueQuan, txtSDT;
        private Button btnCapNhat;
        private Label lblInfoTitle;

        // Header
        private Panel pnlHeader;
        private Label lblWelcome;
        private Label lblRole;
        private Button btnLogout;

        public KyThuatVienForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            UIHelper.StyleForm(this, "Hospital Management – Kỹ thuật viên", 1150, 720);
            this.DoubleBuffered = true;

            // === Header Panel ===
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
                Text = $"👨‍⚕️  Xin chào, {OracleHelper.Instance.CurrentUser}",
                Font = new Font("Segoe UI Semibold", 14, FontStyle.Bold),
                ForeColor = UIHelper.TextPrimary,
                AutoSize = true,
                Location = new Point(25, 12),
                BackColor = Color.Transparent,
            };

            lblRole = new Label
            {
                Text = "Vai trò: Kỹ thuật viên",
                Font = UIHelper.SmallFont,
                ForeColor = UIHelper.AccentGreen,
                AutoSize = true,
                Location = new Point(27, 40),
                BackColor = Color.Transparent,
            };

            btnLogout = UIHelper.CreateButton("🚪 Đăng xuất", UIHelper.AccentRed, 130, 36);
            btnLogout.Location = new Point(990, 15);
            btnLogout.Click += (s, e) =>
            {
                this.Close();
            };

            pnlHeader.Controls.AddRange(new Control[] { lblWelcome, lblRole, btnLogout });

            // === Tab Control ===
            tabMain = new TabControl
            {
                Location = new Point(15, 75),
                Size = new Size(1110, 600),
            };
            UIHelper.StyleTabControl(tabMain);

            // ---- Tab 1: Dịch vụ được phân công ----
            tabDichVu = new TabPage("📋 Dịch vụ phân công");
            tabDichVu.BackColor = UIHelper.PrimaryDark;
            tabDichVu.Padding = new Padding(15);

            lblDvTitle = new Label
            {
                Text = "Danh sách dịch vụ được phân công",
                Font = UIHelper.HeadingFont,
                ForeColor = UIHelper.TextPrimary,
                AutoSize = true,
                Location = new Point(15, 15),
                BackColor = Color.Transparent,
            };

            lblDvCount = new Label
            {
                Text = "",
                Font = UIHelper.SmallFont,
                ForeColor = UIHelper.TextSecondary,
                AutoSize = true,
                Location = new Point(15, 48),
                BackColor = Color.Transparent,
            };

            dgvDichVu = new DataGridView
            {
                Location = new Point(15, 72),
                Size = new Size(1060, 420),
                ReadOnly = false,
            };
            UIHelper.StyleDataGridView(dgvDichVu);
            dgvDichVu.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dgvDichVu.AllowUserToOrderColumns = true;

            var pnlDvButtons = new Panel
            {
                Location = new Point(15, 500),
                Size = new Size(1060, 50),
                BackColor = Color.Transparent,
            };

            btnRefresh = UIHelper.CreateButton("🔄 Làm mới", UIHelper.PrimaryBlue, 150, 40);
            btnRefresh.Location = new Point(0, 5);
            btnRefresh.Click += (s, e) => LoadDichVu();

            btnSave = UIHelper.CreateButton("💾 Lưu kết quả", UIHelper.AccentGreen, 170, 40);
            btnSave.Location = new Point(170, 5);
            btnSave.Click += BtnSave_Click;

            pnlDvButtons.Controls.AddRange(new Control[] { btnRefresh, btnSave });

            tabDichVu.Controls.AddRange(new Control[] { lblDvTitle, lblDvCount, dgvDichVu, pnlDvButtons });

            // ---- Tab 2: Thông tin cá nhân ----
            tabThongTin = new TabPage("👤 Thông tin cá nhân");
            tabThongTin.BackColor = UIHelper.PrimaryDark;

            lblInfoTitle = new Label
            {
                Text = "Thông tin cá nhân",
                Font = UIHelper.HeadingFont,
                ForeColor = UIHelper.TextPrimary,
                AutoSize = true,
                Location = new Point(20, 20),
                BackColor = Color.Transparent,
            };

            // Readonly fields panel
            var pnlReadonly = UIHelper.CreateCard(500, 310);
            pnlReadonly.Location = new Point(20, 55);
            pnlReadonly.Paint += (s, e) =>
            {
                using var pen = new Pen(UIHelper.BorderColor, 1);
                e.Graphics.DrawRectangle(pen, 0, 0, pnlReadonly.Width - 1, pnlReadonly.Height - 1);
            };

            var lblRoTitle = UIHelper.CreateLabel("🔒 Thông tin không thể chỉnh sửa", UIHelper.LabelFont, UIHelper.TextSecondary);
            lblRoTitle.Location = new Point(20, 12);

            int yPos = 42;
            int labelWidth = 120;
            int gap = 45;

            AddFieldToPanel(pnlReadonly, "Mã NV:", out txtMaNV, ref yPos, labelWidth, true);
            AddFieldToPanel(pnlReadonly, "Họ tên:", out txtHoTen, ref yPos, labelWidth, true);
            AddFieldToPanel(pnlReadonly, "Phái:", out txtPhai, ref yPos, labelWidth, true);
            AddFieldToPanel(pnlReadonly, "Ngày sinh:", out txtNgaySinh, ref yPos, labelWidth, true);
            AddFieldToPanel(pnlReadonly, "CMND:", out txtCMND, ref yPos, labelWidth, true);
            AddFieldToPanel(pnlReadonly, "Vai trò:", out txtVaiTro, ref yPos, labelWidth, true);
            AddFieldToPanel(pnlReadonly, "Chuyên khoa:", out txtChuyenKhoa, ref yPos, labelWidth, true);

            pnlReadonly.Controls.Add(lblRoTitle);

            // Editable fields panel
            var pnlEditable = UIHelper.CreateCard(500, 180);
            pnlEditable.Location = new Point(20, 380);
            pnlEditable.Paint += (s, e) =>
            {
                using var pen = new Pen(UIHelper.AccentGreen, 1);
                e.Graphics.DrawRectangle(pen, 0, 0, pnlEditable.Width - 1, pnlEditable.Height - 1);
            };

            var lblEdTitle = UIHelper.CreateLabel("✏️ Thông tin có thể chỉnh sửa", UIHelper.LabelFont, UIHelper.AccentGreen);
            lblEdTitle.Location = new Point(20, 12);

            yPos = 42;
            AddFieldToPanel(pnlEditable, "Quê quán:", out txtQueQuan, ref yPos, labelWidth, false);
            AddFieldToPanel(pnlEditable, "SĐT:", out txtSDT, ref yPos, labelWidth, false);

            btnCapNhat = UIHelper.CreateButton("✅ Cập nhật thông tin", UIHelper.AccentGreen, 200, 40);
            btnCapNhat.Location = new Point(20, yPos + 5);
            btnCapNhat.Click += BtnCapNhat_Click;

            pnlEditable.Controls.AddRange(new Control[] { lblEdTitle, btnCapNhat });

            tabThongTin.Controls.AddRange(new Control[] { lblInfoTitle, pnlReadonly, pnlEditable });

            // === Add tabs ===
            tabMain.TabPages.AddRange(new TabPage[] { tabDichVu, tabThongTin });

            this.Controls.AddRange(new Control[] { pnlHeader, tabMain });
        }

        private void AddFieldToPanel(Panel panel, string label, out TextBox textBox, ref int yPos, int labelWidth, bool readOnly)
        {
            var lbl = UIHelper.CreateLabel(label);
            lbl.Location = new Point(20, yPos + 4);
            lbl.Size = new Size(labelWidth, 25);

            textBox = UIHelper.CreateTextBox(320);
            textBox.Location = new Point(145, yPos);
            textBox.ReadOnly = readOnly;
            if (readOnly)
            {
                textBox.BackColor = Color.FromArgb(30, 30, 50);
                textBox.ForeColor = UIHelper.TextSecondary;
            }

            panel.Controls.AddRange(new Control[] { lbl, textBox });
            yPos += 38;
        }

        private void LoadData()
        {
            LoadDichVu();
            LoadThongTinCaNhan();
        }

        private void LoadDichVu()
        {
            try
            {
                if (OracleHelper.Instance == null)
                {
                    UIHelper.ShowError("Chưa kết nối Oracle. Vui lòng đăng nhập lại.");
                    return;
                }

                // View tự lọc theo SESSION_USER; KTV không dùng VPD trên HSBA_DV.
                var dt = OracleHelper.Instance.ExecuteQuery(
                    "SELECT MA_HSBA, LOAI_DV, NGAY_DV, MA_KTV, KET_QUA FROM ADMIN.V_HSBA_DV_KTV ORDER BY NGAY_DV DESC");

                dgvDichVu.DataSource = dt;

                // Configure columns safely
                if (dgvDichVu.Columns.Count > 0)
                {
                    // Set AutoSizeColumnsMode specifically
                    dgvDichVu.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

                    foreach (DataGridViewColumn col in dgvDichVu.Columns)
                    {
                        switch (col.Name.ToUpper())
                        {
                            case "MA_HSBA":
                                col.HeaderText = "Mã HSBA";
                                col.ReadOnly = true;
                                col.Width = 100;
                                break;
                            case "LOAI_DV":
                                col.HeaderText = "Loại dịch vụ";
                                col.ReadOnly = true;
                                col.Width = 200;
                                break;
                            case "NGAY_DV":
                                col.HeaderText = "Ngày DV";
                                col.ReadOnly = true;
                                col.Width = 130;
                                col.DefaultCellStyle.Format = "dd/MM/yyyy";
                                break;
                            case "MA_KTV":
                                col.HeaderText = "Mã KTV";
                                col.ReadOnly = true;
                                col.Width = 100;
                                break;
                            case "KET_QUA":
                                col.HeaderText = "🔧 Kết quả (Sửa)";
                                col.ReadOnly = false;
                                col.DefaultCellStyle.BackColor = Color.FromArgb(30, 50, 40);
                                col.DefaultCellStyle.ForeColor = UIHelper.AccentGreen;
                                col.Width = 250;
                                break;
                        }
                    }
                    
                    // Now set back to Fill if desired, or let it be
                    dgvDichVu.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }

                if (dt != null)
                {
                    lblDvCount.Text = $"Tổng số dịch vụ: {dt.Rows.Count} | Chỉ hiển thị dịch vụ do bạn thực hiện (View)";
                }
            }
            catch (Exception ex)
            {
                UIHelper.ShowError("Không thể tải danh sách dịch vụ:\n" + ex.Message + "\n\n" + ex.StackTrace);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                int updatedCount = 0;
                var dt = dgvDichVu.DataSource as DataTable;
                if (dt == null) return;

                var changes = dt.GetChanges();
                if (changes != null)
                {
                    foreach (DataRow row in changes.Rows)
                    {
                        string maHsba = row["MA_HSBA"].ToString();
                        string loaiDv = row["LOAI_DV"].ToString();
                        DateTime ngayDv = Convert.ToDateTime(row["NGAY_DV"]);
                        string ketQua = row["KET_QUA"].ToString();

                        OracleHelper.Instance.ExecuteNonQuery(
                            "UPDATE ADMIN.V_HSBA_DV_KTV SET KET_QUA = :ketqua " +
                            "WHERE MA_HSBA = :mahsba AND LOAI_DV = :loaidv AND NGAY_DV = :ngaydv",
                            new OracleParameter("ketqua", ketQua),
                            new OracleParameter("mahsba", maHsba),
                            new OracleParameter("loaidv", loaiDv),
                            new OracleParameter("ngaydv", ngayDv));

                        updatedCount++;
                    }
                }

                if (updatedCount > 0)
                {
                    dt.AcceptChanges();
                    UIHelper.ShowSuccess($"Đã cập nhật {updatedCount} kết quả dịch vụ.\n(Đã ghi vết vào AUDIT_KETQUA)");
                }
                else
                {
                    UIHelper.ShowWarning("Không có thay đổi nào để lưu.");
                }
            }
            catch (Exception ex)
            {
                UIHelper.ShowError("Lỗi khi lưu:\n" + ex.Message);
            }
        }

        private void LoadThongTinCaNhan()
        {
            try
            {
                // View tự lọc theo SESSION_USER; KTV không dùng VPD trên NHAN_VIEN.
                var dt = OracleHelper.Instance.ExecuteQuery(
                    "SELECT MA_NV, HO_TEN, PHAI, NGAY_SINH, CMND, QUE_QUAN, SDT, VAI_TRO, CHUYEN_KHOA FROM ADMIN.V_NHAN_VIEN_SELF");

                if (dt.Rows.Count > 0)
                {
                    var row = dt.Rows[0];
                    txtMaNV.Text = row["MA_NV"]?.ToString() ?? "";
                    txtHoTen.Text = row["HO_TEN"]?.ToString() ?? "";
                    txtPhai.Text = row["PHAI"]?.ToString() ?? "";
                    txtNgaySinh.Text = row["NGAY_SINH"] != DBNull.Value
                        ? Convert.ToDateTime(row["NGAY_SINH"]).ToString("dd/MM/yyyy") : "";
                    txtCMND.Text = row["CMND"]?.ToString() ?? "";
                    txtVaiTro.Text = row["VAI_TRO"]?.ToString() ?? "";
                    txtChuyenKhoa.Text = row["CHUYEN_KHOA"]?.ToString() ?? "";
                    txtQueQuan.Text = row["QUE_QUAN"]?.ToString() ?? "";
                    txtSDT.Text = row["SDT"]?.ToString() ?? "";
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
                    "UPDATE ADMIN.V_NHAN_VIEN_SELF SET QUE_QUAN = :quequan, SDT = :sdt",
                    new OracleParameter("quequan", txtQueQuan.Text.Trim()),
                    new OracleParameter("sdt", txtSDT.Text.Trim()));

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
