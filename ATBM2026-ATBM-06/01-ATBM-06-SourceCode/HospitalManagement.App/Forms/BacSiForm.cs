using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HospitalManagement.App.DataAccess;
using HospitalManagement.App.Helpers;
using Oracle.ManagedDataAccess.Client;
using HospitalManagement.App.Forms; 

namespace HospitalManagement.App.Forms
{
    public class BacSiForm : Form
    {
        // Header
        private Panel pnlHeader;
        private Label lblWelcome, lblRole;
        private Button btnLogout;

        // Tabs
        private TabControl tabMain;
        private TabPage tabHSBA, tabDV, tabBN, tabDT, tabInfo;

        // HSBA tab
        private DataGridView dgvHSBA;
        private Button btnRefreshHSBA, btnLuuHSBA;
        private Label lblHSBACount;

        // DV tab
        private DataGridView dgvDV;
        private Button btnRefreshDV, btnThemDV, btnXoaDV;
        private Label lblDVCount;
        private Panel pnlNewDV;
        private TextBox txtNewDv_MaHsba, txtNewDv_LoaiDv, txtNewDv_KetQua;
        private DateTimePicker dtpNewDv_Ngay;
        private ComboBox cboNewDv_MaKtv;
        private Button btnNewDv_Save, btnNewDv_Cancel;

        // BN tab
        private DataGridView dgvBN;
        private Button btnRefreshBN, btnLuuBN;
        private Label lblBNCount;

        // DT tab
        private DataGridView dgvDT;
        private Button btnRefreshDT, btnThemDT, btnLuuDT, btnXoaDT;
        private Label lblDTCount;
        private Panel pnlNewDT;
        private TextBox txtNewDt_MaHsba, txtNewDt_TenThuoc, txtNewDt_LieuDung;
        private DateTimePicker dtpNewDt_Ngay;
        private Button btnNewDt_Save, btnNewDt_Cancel;

        // Info tab
        private TextBox txtMaNV, txtHoTen, txtPhai, txtNgaySinh, txtCMND, txtVaiTro, txtChuyenKhoa;
        private TextBox txtQueQuan, txtSDT;
        private Button btnCapNhatTT;

        public BacSiForm()
        {
            InitializeComponent();
            LoadAll();
        }

        #region Init

        private void InitializeComponent()
        {
            UIHelper.StyleForm(this, "Hospital Management – Bác sĩ/Y sĩ", 1280, 780);
            this.DoubleBuffered = true;

            BuildHeader();
            BuildTabs();

            this.Controls.AddRange(new Control[] { pnlHeader, tabMain });
        }

        private void BuildHeader()
        {
            pnlHeader = new Panel { Dock = DockStyle.Top, Height = 65, BackColor = UIHelper.SecondaryDark };
            pnlHeader.Paint += (s, e) =>
            {
                using var pen = new Pen(UIHelper.BorderColor, 1);
                e.Graphics.DrawLine(pen, 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);
            };

            lblWelcome = new Label
            {
                Text = $"👨‍⚕️  Xin chào, BS. {OracleHelper.Instance.CurrentUser}",
                Font = new Font("Segoe UI Semibold", 14, FontStyle.Bold),
                ForeColor = UIHelper.TextPrimary,
                AutoSize = true,
                Location = new Point(25, 12),
                BackColor = Color.Transparent,
            };
            lblRole = new Label
            {
                Text = "Vai trò: Bác sĩ/Y sĩ",
                Font = UIHelper.SmallFont,
                ForeColor = UIHelper.AccentGreen,
                AutoSize = true,
                Location = new Point(27, 40),
                BackColor = Color.Transparent,
            };
            // btnLogout = UIHelper.CreateButton("🚪 Đăng xuất", UIHelper.AccentRed, 130, 36);
            // btnLogout.Location = new Point(1120, 15);
            // btnLogout.Click += (s, e) => this.Close();
            // pnlHeader.Controls.AddRange(new Control[] { lblWelcome, lblRole, btnLogout });
            var btnThongBao = UIHelper.CreateButton("🔔", UIHelper.SecondaryDark, 50, 36);
            btnThongBao.Location = new Point(1060, 15);
            btnThongBao.Font = new Font("Segoe UI Emoji", 14);
            btnThongBao.Click += (s, e) => new ThongBaoKhanForm().ShowDialog(this);

            btnLogout = UIHelper.CreateButton("🚪 Đăng xuất", UIHelper.AccentRed, 130, 36);
            btnLogout.Location = new Point(1120, 15);
            btnLogout.Click += (s, e) => this.Close();

            pnlHeader.Controls.AddRange(new Control[] { lblWelcome, lblRole, btnThongBao, btnLogout });
        }

        private void BuildTabs()
        {
            tabMain = new TabControl
            {
                Location = new Point(15, 75),
                Size = new Size(1245, 660),
            };
            UIHelper.StyleTabControl(tabMain);

            tabHSBA = new TabPage("📋 HSBA của tôi") { BackColor = UIHelper.PrimaryDark, Padding = new Padding(15) };
            tabDV = new TabPage("🧪 Dịch vụ") { BackColor = UIHelper.PrimaryDark, Padding = new Padding(15) };
            tabBN = new TabPage("👥 Bệnh nhân") { BackColor = UIHelper.PrimaryDark, Padding = new Padding(15) };
            tabDT = new TabPage("💊 Đơn thuốc") { BackColor = UIHelper.PrimaryDark, Padding = new Padding(15) };
            tabInfo = new TabPage("👤 Thông tin cá nhân") { BackColor = UIHelper.PrimaryDark, Padding = new Padding(15) };

            BuildTabHSBA();
            BuildTabDV();
            BuildTabBN();
            BuildTabDT();
            BuildTabInfo();

            tabMain.TabPages.AddRange(new TabPage[] { tabHSBA, tabDV, tabBN, tabDT, tabInfo });
        }

        #endregion

        #region Tab HSBA của tôi

        private void BuildTabHSBA()
        {
            var lbl = new Label
            {
                Text = "Hồ sơ bệnh án do bạn điều trị (VPD tự lọc MA_BS = bạn)",
                Font = UIHelper.HeadingFont,
                ForeColor = UIHelper.TextPrimary,
                AutoSize = true,
                Location = new Point(15, 10),
                BackColor = Color.Transparent,
            };

            btnRefreshHSBA = UIHelper.CreateButton("🔄 Làm mới", UIHelper.SecondaryDark, 110, 36);
            btnRefreshHSBA.Location = new Point(15, 50);
            btnRefreshHSBA.Click += (s, e) => LoadHSBA();

            btnLuuHSBA = UIHelper.CreateButton("💾 Lưu (CHUAN_DOAN, DIEU_TRI, KET_LUAN)", UIHelper.AccentGreen, 340, 36);
            btnLuuHSBA.Location = new Point(135, 50);
            btnLuuHSBA.Click += BtnLuuHSBA_Click;

            lblHSBACount = new Label
            {
                Text = "",
                Font = UIHelper.SmallFont,
                ForeColor = UIHelper.TextSecondary,
                AutoSize = true,
                Location = new Point(15, 95),
                BackColor = Color.Transparent,
            };

            dgvHSBA = new DataGridView { Location = new Point(15, 120), Size = new Size(1195, 475) };
            UIHelper.StyleDataGridView(dgvHSBA);
            dgvHSBA.SelectionMode = DataGridViewSelectionMode.CellSelect;

            tabHSBA.Controls.AddRange(new Control[] { lbl, btnRefreshHSBA, btnLuuHSBA, lblHSBACount, dgvHSBA });
        }

        private void LoadHSBA()
        {
            try
            {
                var dt = OracleHelper.Instance.ExecuteQuery(
                    "SELECT MA_HSBA, MA_BN, NGAY, MA_KHOA, CHUAN_DOAN, DIEU_TRI, KET_LUAN " +
                    "FROM ADMIN.HSBA ORDER BY NGAY DESC");
                dgvHSBA.DataSource = dt;
                if (dgvHSBA.Columns.Count > 0)
                {
                    dgvHSBA.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                    foreach (DataGridViewColumn col in dgvHSBA.Columns)
                    {
                        switch (col.Name.ToUpper())
                        {
                            case "MA_HSBA": col.HeaderText = "Mã HSBA"; col.Width = 100; col.ReadOnly = true; break;
                            case "MA_BN": col.HeaderText = "Mã BN"; col.Width = 90; col.ReadOnly = true; break;
                            case "NGAY": col.HeaderText = "Ngày"; col.Width = 100; col.ReadOnly = true;
                                col.DefaultCellStyle.Format = "dd/MM/yyyy"; break;
                            case "MA_KHOA": col.HeaderText = "Khoa"; col.Width = 80; col.ReadOnly = true; break;
                            case "CHUAN_DOAN": col.HeaderText = "🔧 Chẩn đoán"; col.Width = 280; col.ReadOnly = false;
                                col.DefaultCellStyle.BackColor = Color.FromArgb(30, 50, 40); break;
                            case "DIEU_TRI": col.HeaderText = "🔧 Điều trị"; col.Width = 280; col.ReadOnly = false;
                                col.DefaultCellStyle.BackColor = Color.FromArgb(30, 50, 40); break;
                            case "KET_LUAN": col.HeaderText = "🔧 Kết luận"; col.Width = 250; col.ReadOnly = false;
                                col.DefaultCellStyle.BackColor = Color.FromArgb(30, 50, 40); break;
                        }
                    }
                }
                lblHSBACount.Text = $"Tổng: {dt.Rows.Count} HSBA do bạn điều trị.";
            }
            catch (Exception ex)
            {
                UIHelper.ShowError("Không thể tải HSBA:\n" + ex.Message);
            }
        }

        private void BtnLuuHSBA_Click(object sender, EventArgs e)
        {
            try
            {
                var dt = dgvHSBA.DataSource as DataTable;
                if (dt == null) return;
                var changes = dt.GetChanges(DataRowState.Modified);
                if (changes == null || changes.Rows.Count == 0)
                {
                    UIHelper.ShowWarning("Không có thay đổi.");
                    return;
                }
                int ok = 0;
                foreach (DataRow row in changes.Rows)
                {
                    OracleHelper.Instance.ExecuteNonQuery(
                        "UPDATE ADMIN.HSBA SET CHUAN_DOAN = :cd, DIEU_TRI = :dt, KET_LUAN = :kl " +
                        "WHERE MA_HSBA = :mh",
                        new OracleParameter("cd", row["CHUAN_DOAN"] ?? DBNull.Value),
                        new OracleParameter("dt", row["DIEU_TRI"] ?? DBNull.Value),
                        new OracleParameter("kl", row["KET_LUAN"] ?? DBNull.Value),
                        new OracleParameter("mh", row["MA_HSBA"]));
                    ok++;
                }
                dt.AcceptChanges();
                UIHelper.ShowSuccess($"Đã cập nhật {ok} HSBA.");
            }
            catch (Exception ex)
            {
                UIHelper.ShowError("Lỗi:\n" + ex.Message);
            }
        }

        #endregion

        #region Tab Dịch vụ

        private void BuildTabDV()
        {
            var lbl = new Label
            {
                Text = "Dịch vụ liên quan HSBA của bạn",
                Font = UIHelper.HeadingFont,
                ForeColor = UIHelper.TextPrimary,
                AutoSize = true,
                Location = new Point(15, 10),
                BackColor = Color.Transparent,
            };

            btnRefreshDV = UIHelper.CreateButton("🔄 Làm mới", UIHelper.SecondaryDark, 110, 36);
            btnRefreshDV.Location = new Point(15, 50);
            btnRefreshDV.Click += (s, e) => LoadDV();

            btnThemDV = UIHelper.CreateButton("➕ Thêm DV", UIHelper.AccentGreen, 130, 36);
            btnThemDV.Location = new Point(135, 50);
            btnThemDV.Click += (s, e) => ShowNewDvPanel(true);

            btnXoaDV = UIHelper.CreateButton("🗑 Xóa DV", UIHelper.AccentRed, 130, 36);
            btnXoaDV.Location = new Point(275, 50);
            btnXoaDV.Click += BtnXoaDV_Click;

            lblDVCount = new Label
            {
                Text = "",
                Font = UIHelper.SmallFont,
                ForeColor = UIHelper.TextSecondary,
                AutoSize = true,
                Location = new Point(15, 95),
                BackColor = Color.Transparent,
            };

            dgvDV = new DataGridView { Location = new Point(15, 120), Size = new Size(1195, 475) };
            UIHelper.StyleDataGridView(dgvDV);
            dgvDV.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvDV.ReadOnly = true;

            BuildNewDvPanel();

            tabDV.Controls.AddRange(new Control[] { lbl, btnRefreshDV, btnThemDV, btnXoaDV, lblDVCount, dgvDV, pnlNewDV });
        }

        private void BuildNewDvPanel()
        {
            pnlNewDV = UIHelper.CreateCard(1195, 475);
            pnlNewDV.Location = new Point(15, 120);
            pnlNewDV.Visible = false;
            pnlNewDV.Paint += (s, e) =>
            {
                using var pen = new Pen(UIHelper.AccentGreen, 2);
                e.Graphics.DrawRectangle(pen, 0, 0, pnlNewDV.Width - 1, pnlNewDV.Height - 1);
            };

            var lbl = UIHelper.CreateLabel("➕ Thêm dịch vụ mới", UIHelper.HeadingFont, UIHelper.AccentGreen);
            lbl.Location = new Point(20, 15);

            int y = 70;
            AddLabelAt(pnlNewDV, "Mã HSBA (của bạn):", 20, y);
            txtNewDv_MaHsba = UIHelper.CreateTextBox(200); txtNewDv_MaHsba.Location = new Point(200, y - 5); txtNewDv_MaHsba.CharacterCasing = CharacterCasing.Upper;
            pnlNewDV.Controls.Add(txtNewDv_MaHsba);

            y += 50;
            AddLabelAt(pnlNewDV, "Loại DV:", 20, y);
            txtNewDv_LoaiDv = UIHelper.CreateTextBox(300); txtNewDv_LoaiDv.Location = new Point(200, y - 5);
            pnlNewDV.Controls.Add(txtNewDv_LoaiDv);

            y += 50;
            AddLabelAt(pnlNewDV, "Ngày DV:", 20, y);
            dtpNewDv_Ngay = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Location = new Point(200, y - 5),
                Size = new Size(200, 32),
                Value = DateTime.Today,
            };
            pnlNewDV.Controls.Add(dtpNewDv_Ngay);

            y += 50;
            AddLabelAt(pnlNewDV, "Kỹ thuật viên:", 20, y);
            cboNewDv_MaKtv = new ComboBox
            {
                Location = new Point(200, y - 5),
                Size = new Size(400, 32),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = UIHelper.InputBackground,
                ForeColor = UIHelper.TextPrimary,
                FlatStyle = FlatStyle.Flat,
                Font = UIHelper.InputFont,
            };
            pnlNewDV.Controls.Add(cboNewDv_MaKtv);

            y += 50;
            AddLabelAt(pnlNewDV, "Kết quả (có thể để trống):", 20, y);
            txtNewDv_KetQua = UIHelper.CreateTextBox(400); txtNewDv_KetQua.Location = new Point(200, y - 5);
            pnlNewDV.Controls.Add(txtNewDv_KetQua);

            btnNewDv_Save = UIHelper.CreateButton("💾 Lưu", UIHelper.AccentGreen, 130, 40);
            btnNewDv_Save.Location = new Point(20, y + 60);
            btnNewDv_Save.Click += BtnNewDvSave_Click;

            btnNewDv_Cancel = UIHelper.CreateButton("✖ Hủy", UIHelper.AccentRed, 130, 40);
            btnNewDv_Cancel.Location = new Point(160, y + 60);
            btnNewDv_Cancel.Click += (s, e) => ShowNewDvPanel(false);

            pnlNewDV.Controls.AddRange(new Control[] { lbl, btnNewDv_Save, btnNewDv_Cancel });
        }

        private void ShowNewDvPanel(bool show)
        {
            pnlNewDV.Visible = show;
            dgvDV.Visible = !show;
            lblDVCount.Visible = !show;
            if (show) { LoadKtvCombo(cboNewDv_MaKtv); txtNewDv_MaHsba.Focus(); }
        }

        private void BtnNewDvSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtNewDv_MaHsba.Text) ||
                    string.IsNullOrWhiteSpace(txtNewDv_LoaiDv.Text) ||
                    cboNewDv_MaKtv.SelectedItem == null)
                {
                    UIHelper.ShowWarning("Nhập đầy đủ Mã HSBA, Loại DV và chọn KTV.");
                    return;
                }
                string maKtv = ((ComboItem)cboNewDv_MaKtv.SelectedItem).Value;
                OracleHelper.Instance.ExecuteNonQuery(
                    "INSERT INTO ADMIN.HSBA_DV (MA_HSBA, LOAI_DV, NGAY_DV, MA_KTV, KET_QUA) " +
                    "VALUES (:mh, :ldv, :ng, :mk, :kq)",
                    new OracleParameter("mh", txtNewDv_MaHsba.Text.Trim()),
                    new OracleParameter("ldv", txtNewDv_LoaiDv.Text.Trim()),
                    new OracleParameter("ng", dtpNewDv_Ngay.Value.Date),
                    new OracleParameter("mk", maKtv),
                    new OracleParameter("kq", string.IsNullOrWhiteSpace(txtNewDv_KetQua.Text)
                        ? (object)DBNull.Value : txtNewDv_KetQua.Text.Trim()));
                UIHelper.ShowSuccess("Đã thêm dịch vụ.");
                ShowNewDvPanel(false);
                LoadDV();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                if (msg.Contains("ORA-28115") || msg.Contains("ORA-28116"))
                    msg = "VPD từ chối: Mã HSBA không phải của bạn.";
                UIHelper.ShowError("Lỗi:\n" + msg);
            }
        }

        private void LoadKtvCombo(ComboBox cbo)
        {
            try
            {
                cbo.Items.Clear();
                var dt = OracleHelper.Instance.ExecuteQuery("SELECT MA_NV, HO_TEN FROM ADMIN.MV_KTV_LIST ORDER BY MA_NV");
                foreach (DataRow r in dt.Rows)
                    cbo.Items.Add(new ComboItem(r["MA_NV"].ToString(), $"{r["MA_NV"]} - {r["HO_TEN"]}"));
                if (cbo.Items.Count > 0) cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                UIHelper.ShowError("Không thể tải KTV:\n" + ex.Message);
            }
        }

        private void LoadDV()
        {
            try
            {
                var dt = OracleHelper.Instance.ExecuteQuery(
                    "SELECT MA_HSBA, LOAI_DV, NGAY_DV, MA_KTV, KET_QUA FROM ADMIN.HSBA_DV " +
                    "ORDER BY NGAY_DV DESC");
                dgvDV.DataSource = dt;
                if (dgvDV.Columns.Count > 0)
                {
                    dgvDV.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                    foreach (DataGridViewColumn col in dgvDV.Columns)
                    {
                        switch (col.Name.ToUpper())
                        {
                            case "MA_HSBA": col.HeaderText = "Mã HSBA"; col.Width = 100; break;
                            case "LOAI_DV": col.HeaderText = "Loại DV"; col.Width = 220; break;
                            case "NGAY_DV": col.HeaderText = "Ngày DV"; col.Width = 110;
                                col.DefaultCellStyle.Format = "dd/MM/yyyy"; break;
                            case "MA_KTV": col.HeaderText = "KTV"; col.Width = 110; break;
                            case "KET_QUA": col.HeaderText = "Kết quả"; col.Width = 300; break;
                        }
                    }
                }
                lblDVCount.Text = $"Tổng: {dt.Rows.Count} dịch vụ liên quan HSBA của bạn.";
            }
            catch (Exception ex)
            {
                UIHelper.ShowError("Không thể tải dịch vụ:\n" + ex.Message);
            }
        }

        private void BtnXoaDV_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvDV.CurrentRow == null) return;
                var row = dgvDV.CurrentRow;
                var confirm = MessageBox.Show("Xóa dịch vụ đang chọn?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (confirm != DialogResult.Yes) return;

                OracleHelper.Instance.ExecuteNonQuery(
                    "DELETE FROM ADMIN.HSBA_DV WHERE MA_HSBA = :mh AND LOAI_DV = :ldv AND NGAY_DV = :ng",
                    new OracleParameter("mh", row.Cells["MA_HSBA"].Value),
                    new OracleParameter("ldv", row.Cells["LOAI_DV"].Value),
                    new OracleParameter("ng", row.Cells["NGAY_DV"].Value));
                UIHelper.ShowSuccess("Đã xóa dịch vụ.");
                LoadDV();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError("Lỗi xóa:\n" + ex.Message);
            }
        }

        #endregion

        #region Tab Bệnh nhân

        private void BuildTabBN()
        {
            var lbl = new Label
            {
                Text = "Bệnh nhân liên quan HSBA do bạn điều trị (VPD lọc tự động)",
                Font = UIHelper.HeadingFont,
                ForeColor = UIHelper.TextPrimary,
                AutoSize = true,
                Location = new Point(15, 10),
                BackColor = Color.Transparent,
            };

            btnRefreshBN = UIHelper.CreateButton("🔄 Làm mới", UIHelper.SecondaryDark, 110, 36);
            btnRefreshBN.Location = new Point(15, 50);
            btnRefreshBN.Click += (s, e) => LoadBN();

            btnLuuBN = UIHelper.CreateButton("💾 Lưu (TIEN_SU_BENH, TIEN_SU_BENH_GD, DI_UNG_THUOC)", UIHelper.AccentGreen, 440, 36);
            btnLuuBN.Location = new Point(135, 50);
            btnLuuBN.Click += BtnLuuBN_Click;

            lblBNCount = new Label
            {
                Text = "",
                Font = UIHelper.SmallFont,
                ForeColor = UIHelper.TextSecondary,
                AutoSize = true,
                Location = new Point(15, 95),
                BackColor = Color.Transparent,
            };

            dgvBN = new DataGridView { Location = new Point(15, 120), Size = new Size(1195, 475) };
            UIHelper.StyleDataGridView(dgvBN);
            dgvBN.SelectionMode = DataGridViewSelectionMode.CellSelect;

            tabBN.Controls.AddRange(new Control[] { lbl, btnRefreshBN, btnLuuBN, lblBNCount, dgvBN });
        }

        private void LoadBN()
        {
            try
            {
                var dt = OracleHelper.Instance.ExecuteQuery(
                    "SELECT MA_BN, TEN_BN, PHAI, NGAY_SINH, CCCD, " +
                    "TIEN_SU_BENH, TIEN_SU_BENH_GD, DI_UNG_THUOC " +
                    "FROM ADMIN.BENH_NHAN ORDER BY MA_BN");
                dgvBN.DataSource = dt;
                if (dgvBN.Columns.Count > 0)
                {
                    dgvBN.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                    foreach (DataGridViewColumn col in dgvBN.Columns)
                    {
                        switch (col.Name.ToUpper())
                        {
                            case "MA_BN": col.HeaderText = "Mã BN"; col.Width = 90; col.ReadOnly = true; break;
                            case "TEN_BN": col.HeaderText = "Họ tên"; col.Width = 160; col.ReadOnly = true; break;
                            case "PHAI": col.HeaderText = "Phái"; col.Width = 55; col.ReadOnly = true; break;
                            case "NGAY_SINH": col.HeaderText = "Ngày sinh"; col.Width = 100; col.ReadOnly = true;
                                col.DefaultCellStyle.Format = "dd/MM/yyyy"; break;
                            case "CCCD": col.HeaderText = "CCCD"; col.Width = 110; col.ReadOnly = true; break;
                            case "TIEN_SU_BENH": col.HeaderText = "🔧 Tiền sử bệnh"; col.Width = 220; col.ReadOnly = false;
                                col.DefaultCellStyle.BackColor = Color.FromArgb(30, 50, 40); break;
                            case "TIEN_SU_BENH_GD": col.HeaderText = "🔧 TSB gia đình"; col.Width = 220; col.ReadOnly = false;
                                col.DefaultCellStyle.BackColor = Color.FromArgb(30, 50, 40); break;
                            case "DI_UNG_THUOC": col.HeaderText = "🔧 Dị ứng thuốc"; col.Width = 200; col.ReadOnly = false;
                                col.DefaultCellStyle.BackColor = Color.FromArgb(30, 50, 40); break;
                        }
                    }
                }
                lblBNCount.Text = $"Tổng: {dt.Rows.Count} bệnh nhân liên quan HSBA của bạn.";
            }
            catch (Exception ex)
            {
                UIHelper.ShowError("Không thể tải BN:\n" + ex.Message);
            }
        }

        private void BtnLuuBN_Click(object sender, EventArgs e)
        {
            try
            {
                var dt = dgvBN.DataSource as DataTable;
                if (dt == null) return;
                var changes = dt.GetChanges(DataRowState.Modified);
                if (changes == null || changes.Rows.Count == 0)
                {
                    UIHelper.ShowWarning("Không có thay đổi.");
                    return;
                }
                int ok = 0;
                foreach (DataRow row in changes.Rows)
                {
                    OracleHelper.Instance.ExecuteNonQuery(
                        "UPDATE ADMIN.BENH_NHAN SET TIEN_SU_BENH = :tsb, TIEN_SU_BENH_GD = :tsbgd, " +
                        "DI_UNG_THUOC = :dut WHERE MA_BN = :mb",
                        new OracleParameter("tsb", row["TIEN_SU_BENH"] ?? DBNull.Value),
                        new OracleParameter("tsbgd", row["TIEN_SU_BENH_GD"] ?? DBNull.Value),
                        new OracleParameter("dut", row["DI_UNG_THUOC"] ?? DBNull.Value),
                        new OracleParameter("mb", row["MA_BN"]));
                    ok++;
                }
                dt.AcceptChanges();
                UIHelper.ShowSuccess($"Đã cập nhật {ok} bệnh nhân.");
            }
            catch (Exception ex)
            {
                UIHelper.ShowError("Lỗi:\n" + ex.Message);
            }
        }

        #endregion

        #region Tab Đơn thuốc

        private void BuildTabDT()
        {
            var lbl = new Label
            {
                Text = "Đơn thuốc liên quan HSBA của bạn",
                Font = UIHelper.HeadingFont,
                ForeColor = UIHelper.TextPrimary,
                AutoSize = true,
                Location = new Point(15, 10),
                BackColor = Color.Transparent,
            };

            btnRefreshDT = UIHelper.CreateButton("🔄 Làm mới", UIHelper.SecondaryDark, 110, 36);
            btnRefreshDT.Location = new Point(15, 50);
            btnRefreshDT.Click += (s, e) => LoadDT();

            btnThemDT = UIHelper.CreateButton("➕ Thêm", UIHelper.AccentGreen, 110, 36);
            btnThemDT.Location = new Point(135, 50);
            btnThemDT.Click += (s, e) => ShowNewDtPanel(true);

            btnLuuDT = UIHelper.CreateButton("💾 Lưu (TEN_THUOC, LIEU_DUNG)", UIHelper.PrimaryBlue, 280, 36);
            btnLuuDT.Location = new Point(255, 50);
            btnLuuDT.Click += BtnLuuDT_Click;

            btnXoaDT = UIHelper.CreateButton("🗑 Xóa", UIHelper.AccentRed, 110, 36);
            btnXoaDT.Location = new Point(545, 50);
            btnXoaDT.Click += BtnXoaDT_Click;

            lblDTCount = new Label
            {
                Text = "",
                Font = UIHelper.SmallFont,
                ForeColor = UIHelper.TextSecondary,
                AutoSize = true,
                Location = new Point(15, 95),
                BackColor = Color.Transparent,
            };

            dgvDT = new DataGridView { Location = new Point(15, 120), Size = new Size(1195, 475) };
            UIHelper.StyleDataGridView(dgvDT);
            dgvDT.SelectionMode = DataGridViewSelectionMode.CellSelect;

            BuildNewDtPanel();

            tabDT.Controls.AddRange(new Control[] { lbl, btnRefreshDT, btnThemDT, btnLuuDT, btnXoaDT, lblDTCount, dgvDT, pnlNewDT });
        }

        private void BuildNewDtPanel()
        {
            pnlNewDT = UIHelper.CreateCard(1195, 475);
            pnlNewDT.Location = new Point(15, 120);
            pnlNewDT.Visible = false;
            pnlNewDT.Paint += (s, e) =>
            {
                using var pen = new Pen(UIHelper.AccentGreen, 2);
                e.Graphics.DrawRectangle(pen, 0, 0, pnlNewDT.Width - 1, pnlNewDT.Height - 1);
            };

            var lbl = UIHelper.CreateLabel("➕ Thêm đơn thuốc", UIHelper.HeadingFont, UIHelper.AccentGreen);
            lbl.Location = new Point(20, 15);

            int y = 70;
            AddLabelAt(pnlNewDT, "Mã HSBA:", 20, y);
            txtNewDt_MaHsba = UIHelper.CreateTextBox(200); txtNewDt_MaHsba.Location = new Point(160, y - 5); txtNewDt_MaHsba.CharacterCasing = CharacterCasing.Upper;
            pnlNewDT.Controls.Add(txtNewDt_MaHsba);

            y += 50;
            AddLabelAt(pnlNewDT, "Ngày ĐT:", 20, y);
            dtpNewDt_Ngay = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Location = new Point(160, y - 5),
                Size = new Size(200, 32),
                Value = DateTime.Today,
            };
            pnlNewDT.Controls.Add(dtpNewDt_Ngay);

            y += 50;
            AddLabelAt(pnlNewDT, "Tên thuốc:", 20, y);
            txtNewDt_TenThuoc = UIHelper.CreateTextBox(400); txtNewDt_TenThuoc.Location = new Point(160, y - 5);
            pnlNewDT.Controls.Add(txtNewDt_TenThuoc);

            y += 50;
            AddLabelAt(pnlNewDT, "Liều dùng:", 20, y);
            txtNewDt_LieuDung = UIHelper.CreateTextBox(500); txtNewDt_LieuDung.Location = new Point(160, y - 5);
            pnlNewDT.Controls.Add(txtNewDt_LieuDung);

            btnNewDt_Save = UIHelper.CreateButton("💾 Lưu", UIHelper.AccentGreen, 130, 40);
            btnNewDt_Save.Location = new Point(20, y + 60);
            btnNewDt_Save.Click += BtnNewDtSave_Click;

            btnNewDt_Cancel = UIHelper.CreateButton("✖ Hủy", UIHelper.AccentRed, 130, 40);
            btnNewDt_Cancel.Location = new Point(160, y + 60);
            btnNewDt_Cancel.Click += (s, e) => ShowNewDtPanel(false);

            pnlNewDT.Controls.AddRange(new Control[] { lbl, btnNewDt_Save, btnNewDt_Cancel });
        }

        private void ShowNewDtPanel(bool show)
        {
            pnlNewDT.Visible = show;
            dgvDT.Visible = !show;
            lblDTCount.Visible = !show;
            if (show) txtNewDt_MaHsba.Focus();
        }

        private void BtnNewDtSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtNewDt_MaHsba.Text) ||
                    string.IsNullOrWhiteSpace(txtNewDt_TenThuoc.Text))
                {
                    UIHelper.ShowWarning("Nhập Mã HSBA và Tên thuốc.");
                    return;
                }
                OracleHelper.Instance.ExecuteNonQuery(
                    "INSERT INTO ADMIN.DON_THUOC (MA_HSBA, NGAY_DT, TEN_THUOC, LIEU_DUNG) " +
                    "VALUES (:mh, :ng, :tt, :ld)",
                    new OracleParameter("mh", txtNewDt_MaHsba.Text.Trim()),
                    new OracleParameter("ng", dtpNewDt_Ngay.Value.Date),
                    new OracleParameter("tt", txtNewDt_TenThuoc.Text.Trim()),
                    new OracleParameter("ld", txtNewDt_LieuDung.Text.Trim()));
                UIHelper.ShowSuccess("Đã thêm đơn thuốc.");
                ShowNewDtPanel(false);
                LoadDT();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                if (msg.Contains("ORA-28115") || msg.Contains("ORA-28116"))
                    msg = "VPD từ chối: Mã HSBA không phải của bạn.";
                UIHelper.ShowError("Lỗi:\n" + msg);
            }
        }

        private void LoadDT()
        {
            try
            {
                var dt = OracleHelper.Instance.ExecuteQuery(
                    "SELECT MA_HSBA, NGAY_DT, TEN_THUOC, LIEU_DUNG FROM ADMIN.DON_THUOC " +
                    "ORDER BY NGAY_DT DESC");
                dgvDT.DataSource = dt;
                if (dgvDT.Columns.Count > 0)
                {
                    dgvDT.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                    foreach (DataGridViewColumn col in dgvDT.Columns)
                    {
                        switch (col.Name.ToUpper())
                        {
                            case "MA_HSBA": col.HeaderText = "Mã HSBA"; col.Width = 100; col.ReadOnly = true; break;
                            case "NGAY_DT": col.HeaderText = "Ngày ĐT"; col.Width = 110; col.ReadOnly = true;
                                col.DefaultCellStyle.Format = "dd/MM/yyyy"; break;
                            case "TEN_THUOC": col.HeaderText = "🔧 Tên thuốc"; col.Width = 250; col.ReadOnly = false;
                                col.DefaultCellStyle.BackColor = Color.FromArgb(30, 50, 40); break;
                            case "LIEU_DUNG": col.HeaderText = "🔧 Liều dùng"; col.Width = 500; col.ReadOnly = false;
                                col.DefaultCellStyle.BackColor = Color.FromArgb(30, 50, 40); break;
                        }
                    }
                }
                lblDTCount.Text = $"Tổng: {dt.Rows.Count} đơn thuốc.";
            }
            catch (Exception ex)
            {
                UIHelper.ShowError("Không thể tải đơn thuốc:\n" + ex.Message);
            }
        }

        private void BtnLuuDT_Click(object sender, EventArgs e)
        {
            try
            {
                var dt = dgvDT.DataSource as DataTable;
                if (dt == null) return;
                var changes = dt.GetChanges(DataRowState.Modified);
                if (changes == null || changes.Rows.Count == 0)
                {
                    UIHelper.ShowWarning("Không có thay đổi.");
                    return;
                }
                int ok = 0;
                foreach (DataRow row in changes.Rows)
                {
                    // TEN_THUOC là part của PK - đổi TEN_THUOC cần cách xử lý khác
                    // (xóa + insert). Ở đây ta chỉ cho update LIEU_DUNG thật sự, và TEN_THUOC cũ.
                    OracleHelper.Instance.ExecuteNonQuery(
                        "UPDATE ADMIN.DON_THUOC SET LIEU_DUNG = :ld " +
                        "WHERE MA_HSBA = :mh AND NGAY_DT = :ng AND TEN_THUOC = :tt",
                        new OracleParameter("ld", row["LIEU_DUNG"] ?? DBNull.Value),
                        new OracleParameter("mh", row["MA_HSBA", DataRowVersion.Original]),
                        new OracleParameter("ng", row["NGAY_DT", DataRowVersion.Original]),
                        new OracleParameter("tt", row["TEN_THUOC", DataRowVersion.Original]));
                    ok++;
                }
                dt.AcceptChanges();
                UIHelper.ShowSuccess($"Đã cập nhật {ok} dòng.");
            }
            catch (Exception ex)
            {
                UIHelper.ShowError("Lỗi:\n" + ex.Message);
            }
        }

        private void BtnXoaDT_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvDT.CurrentRow == null) return;
                var row = dgvDT.CurrentRow;
                var confirm = MessageBox.Show("Xóa đơn thuốc đang chọn?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (confirm != DialogResult.Yes) return;

                OracleHelper.Instance.ExecuteNonQuery(
                    "DELETE FROM ADMIN.DON_THUOC WHERE MA_HSBA = :mh AND NGAY_DT = :ng AND TEN_THUOC = :tt",
                    new OracleParameter("mh", row.Cells["MA_HSBA"].Value),
                    new OracleParameter("ng", row.Cells["NGAY_DT"].Value),
                    new OracleParameter("tt", row.Cells["TEN_THUOC"].Value));
                UIHelper.ShowSuccess("Đã xóa.");
                LoadDT();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError("Lỗi xóa:\n" + ex.Message);
            }
        }

        #endregion

        #region Tab Thông tin cá nhân

        private void BuildTabInfo()
        {
            var lblTitle = new Label
            {
                Text = "Thông tin cá nhân",
                Font = UIHelper.HeadingFont,
                ForeColor = UIHelper.TextPrimary,
                AutoSize = true,
                Location = new Point(15, 10),
                BackColor = Color.Transparent,
            };

            var pnlRO = UIHelper.CreateCard(550, 340);
            pnlRO.Location = new Point(15, 50);
            pnlRO.Paint += (s, e) =>
            {
                using var pen = new Pen(UIHelper.BorderColor, 1);
                e.Graphics.DrawRectangle(pen, 0, 0, pnlRO.Width - 1, pnlRO.Height - 1);
            };
            var lblRO = UIHelper.CreateLabel("🔒 Thông tin không chỉnh sửa", UIHelper.LabelFont, UIHelper.TextSecondary);
            lblRO.Location = new Point(20, 12);
            int y = 45;
            AddInfoField(pnlRO, "Mã NV:", out txtMaNV, ref y, true);
            AddInfoField(pnlRO, "Họ tên:", out txtHoTen, ref y, true);
            AddInfoField(pnlRO, "Phái:", out txtPhai, ref y, true);
            AddInfoField(pnlRO, "Ngày sinh:", out txtNgaySinh, ref y, true);
            AddInfoField(pnlRO, "CMND:", out txtCMND, ref y, true);
            AddInfoField(pnlRO, "Vai trò:", out txtVaiTro, ref y, true);
            AddInfoField(pnlRO, "Chuyên khoa:", out txtChuyenKhoa, ref y, true);
            pnlRO.Controls.Add(lblRO);

            var pnlED = UIHelper.CreateCard(550, 200);
            pnlED.Location = new Point(585, 50);
            pnlED.Paint += (s, e) =>
            {
                using var pen = new Pen(UIHelper.AccentGreen, 1);
                e.Graphics.DrawRectangle(pen, 0, 0, pnlED.Width - 1, pnlED.Height - 1);
            };
            var lblED = UIHelper.CreateLabel("✏️ Thông tin có thể chỉnh sửa", UIHelper.LabelFont, UIHelper.AccentGreen);
            lblED.Location = new Point(20, 12);
            y = 45;
            AddInfoField(pnlED, "Quê quán:", out txtQueQuan, ref y, false);
            AddInfoField(pnlED, "SĐT:", out txtSDT, ref y, false);

            btnCapNhatTT = UIHelper.CreateButton("✅ Cập nhật", UIHelper.AccentGreen, 180, 40);
            btnCapNhatTT.Location = new Point(20, y + 10);
            btnCapNhatTT.Click += BtnCapNhatTT_Click;
            pnlED.Controls.AddRange(new Control[] { lblED, btnCapNhatTT });

            tabInfo.Controls.AddRange(new Control[] { lblTitle, pnlRO, pnlED });
        }

        private void LoadThongTin()
        {
            try
            {
                var dt = OracleHelper.Instance.ExecuteQuery(
                    "SELECT MA_NV, HO_TEN, PHAI, NGAY_SINH, CMND, QUE_QUAN, SDT, VAI_TRO, CHUYEN_KHOA " +
                    "FROM ADMIN.NHAN_VIEN");
                if (dt.Rows.Count == 0) return;
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
            catch (Exception ex)
            {
                UIHelper.ShowError("Không thể tải thông tin cá nhân:\n" + ex.Message);
            }
        }

        private void BtnCapNhatTT_Click(object sender, EventArgs e)
        {
            try
            {
                OracleHelper.Instance.ExecuteNonQuery(
                    "UPDATE ADMIN.NHAN_VIEN SET QUE_QUAN = :qq, SDT = :sdt WHERE MA_NV = :mn",
                    new OracleParameter("qq", txtQueQuan.Text.Trim()),
                    new OracleParameter("sdt", txtSDT.Text.Trim()),
                    new OracleParameter("mn", OracleHelper.Instance.CurrentUser));
                UIHelper.ShowSuccess("Đã cập nhật.");
            }
            catch (Exception ex)
            {
                UIHelper.ShowError("Lỗi:\n" + ex.Message);
            }
        }

        #endregion

        #region Helpers

        private static void AddLabelAt(Panel panel, string text, int x, int y)
        {
            var lbl = UIHelper.CreateLabel(text);
            lbl.Location = new Point(x, y);
            lbl.Size = new Size(180, 24);
            panel.Controls.Add(lbl);
        }

        private static void AddInfoField(Panel panel, string label, out TextBox tb, ref int y, bool readOnly)
        {
            var lbl = UIHelper.CreateLabel(label);
            lbl.Location = new Point(20, y + 4);
            lbl.Size = new Size(120, 24);

            tb = UIHelper.CreateTextBox(360);
            tb.Location = new Point(145, y);
            tb.ReadOnly = readOnly;
            if (readOnly)
            {
                tb.BackColor = Color.FromArgb(30, 30, 50);
                tb.ForeColor = UIHelper.TextSecondary;
            }

            panel.Controls.AddRange(new Control[] { lbl, tb });
            y += 40;
        }

        private void LoadAll()
        {
            LoadHSBA();
            LoadDV();
            LoadBN();
            LoadDT();
            LoadThongTin();
        }

        #endregion

        private class ComboItem
        {
            public string Value { get; }
            public string Label { get; }
            public ComboItem(string value, string label) { Value = value; Label = label; }
            public override string ToString() => Label;
        }
    }
}
