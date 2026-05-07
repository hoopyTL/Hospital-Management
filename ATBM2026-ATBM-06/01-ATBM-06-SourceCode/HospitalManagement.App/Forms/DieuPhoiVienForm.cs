using System;
using System.Collections.Generic;
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
    public class DieuPhoiVienForm : Form
    {
        // Header
        private Panel pnlHeader;
        private Label lblWelcome;
        private Label lblRole;
        private Button btnLogout;

        // Tabs
        private TabControl tabMain;
        private TabPage tabBN, tabHSBA, tabDV, tabInfo;

        // === Tab Bệnh nhân ===
        private DataGridView dgvBN;
        private TextBox txtSearchBN;
        private Button btnSearchBN, btnRefreshBN, btnThemBN, btnLuuBN;
        private Label lblBNCount;

        // === Tab HSBA ===
        private DataGridView dgvHSBA;
        private Button btnRefreshHSBA, btnTaoHSBA, btnDieuPhoiBS, btnLuuHSBA;
        private Label lblHSBACount;
        // Form tạo HSBA
        private Panel pnlNewHSBA;
        private TextBox txtNewHsba_MaHsba, txtNewHsba_MaBn, txtNewHsba_MaKhoa;
        private ComboBox cboNewHsba_MaBs;
        private DateTimePicker dtpNewHsba_Ngay;
        private Button btnNewHsba_Save, btnNewHsba_Cancel;

        // === Tab Dịch vụ ===
        private DataGridView dgvDV;
        private Button btnRefreshDV, btnTaoDV, btnLuuDV;
        private Label lblDVCount;
        // Form tạo DV
        private Panel pnlNewDV;
        private TextBox txtNewDv_MaHsba, txtNewDv_LoaiDv;
        private DateTimePicker dtpNewDv_Ngay;
        private ComboBox cboNewDv_MaKtv;
        private Button btnNewDv_Save, btnNewDv_Cancel;

        // === Tab Thông tin cá nhân ===
        private TextBox txtMaNV, txtHoTen, txtPhai, txtNgaySinh, txtCMND, txtVaiTro, txtChuyenKhoa;
        private TextBox txtQueQuan, txtSDT;
        private Button btnCapNhatTT;

        public DieuPhoiVienForm()
        {
            InitializeComponent();
            LoadAllData();
        }

        #region Init UI

        private void InitializeComponent()
        {
            UIHelper.StyleForm(this, "Hospital Management – Điều phối viên", 1280, 780);
            this.DoubleBuffered = true;

            BuildHeader();
            BuildTabs();

            this.Controls.AddRange(new Control[] { pnlHeader, tabMain });
        }

        private void BuildHeader()
        {
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
                Text = $"🧑‍💼  Xin chào, {OracleHelper.Instance.CurrentUser}",
                Font = new Font("Segoe UI Semibold", 14, FontStyle.Bold),
                ForeColor = UIHelper.TextPrimary,
                AutoSize = true,
                Location = new Point(25, 12),
                BackColor = Color.Transparent,
            };

            lblRole = new Label
            {
                Text = "Vai trò: Điều phối viên",
                Font = UIHelper.SmallFont,
                ForeColor = UIHelper.AccentOrange,
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
            btnLogout.Click += (s, e) => 
            {
                this.Close();
            };

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

            tabBN = new TabPage("👥 Bệnh nhân") { BackColor = UIHelper.PrimaryDark, Padding = new Padding(15) };
            tabHSBA = new TabPage("📋 Hồ sơ bệnh án") { BackColor = UIHelper.PrimaryDark, Padding = new Padding(15) };
            tabDV = new TabPage("🧪 Dịch vụ") { BackColor = UIHelper.PrimaryDark, Padding = new Padding(15) };
            tabInfo = new TabPage("👤 Thông tin cá nhân") { BackColor = UIHelper.PrimaryDark, Padding = new Padding(15) };

            BuildTabBN();
            BuildTabHSBA();
            BuildTabDV();
            BuildTabInfo();

            tabMain.TabPages.AddRange(new TabPage[] { tabBN, tabHSBA, tabDV, tabInfo });
        }

        #endregion

        #region Tab Bệnh nhân

        private void BuildTabBN()
        {
            var lblTitle = new Label
            {
                Text = "Danh sách bệnh nhân (toàn bộ)",
                Font = UIHelper.HeadingFont,
                ForeColor = UIHelper.TextPrimary,
                AutoSize = true,
                Location = new Point(15, 10),
                BackColor = Color.Transparent,
            };

            var lblSearch = UIHelper.CreateLabel("🔍 Tìm kiếm:");
            lblSearch.AutoSize = false;
            lblSearch.Size = new Size(158, 28);
            lblSearch.TextAlign = ContentAlignment.MiddleLeft;
            lblSearch.Location = new Point(15, 50);

            txtSearchBN = UIHelper.CreateTextBox(260);
            txtSearchBN.Location = new Point(178, 47);
            txtSearchBN.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { LoadBenhNhan(); e.Handled = e.SuppressKeyPress = true; }
            };

            btnSearchBN = UIHelper.CreateButton("Tìm", UIHelper.PrimaryBlue, 90, 36);
            btnSearchBN.Location = new Point(448, 47);
            btnSearchBN.Click += (s, e) => LoadBenhNhan();

            btnRefreshBN = UIHelper.CreateButton("🔄 Làm mới", UIHelper.SecondaryDark, 110, 36);
            btnRefreshBN.Location = new Point(548, 47);
            btnRefreshBN.Click += (s, e) => { txtSearchBN.Text = ""; LoadBenhNhan(); };

            btnThemBN = UIHelper.CreateButton("➕ Thêm BN", UIHelper.AccentGreen, 130, 36);
            btnThemBN.Location = new Point(940, 47);
            btnThemBN.Click += BtnThemBN_Click;

            btnLuuBN = UIHelper.CreateButton("💾 Lưu thay đổi", UIHelper.PrimaryBlue, 160, 36);
            btnLuuBN.Location = new Point(1075, 47);
            btnLuuBN.Click += BtnLuuBN_Click;

            lblBNCount = new Label
            {
                Text = "",
                Font = UIHelper.SmallFont,
                ForeColor = UIHelper.TextSecondary,
                AutoSize = true,
                Location = new Point(15, 90),
                BackColor = Color.Transparent,
            };

            dgvBN = new DataGridView
            {
                Location = new Point(15, 115),
                Size = new Size(1195, 480),
            };
            UIHelper.StyleDataGridView(dgvBN);
            dgvBN.SelectionMode = DataGridViewSelectionMode.CellSelect;

            tabBN.Controls.AddRange(new Control[] { lblTitle, lblSearch, txtSearchBN, btnSearchBN, btnRefreshBN, btnThemBN, btnLuuBN, lblBNCount, dgvBN });
        }

        private void LoadBenhNhan()
        {
            try
            {
                string keyword = (txtSearchBN.Text ?? "").Trim();
                string sql;
                OracleParameter[] prms;
                if (string.IsNullOrEmpty(keyword))
                {
                    sql = "SELECT MA_BN, TEN_BN, PHAI, NGAY_SINH, CCCD, SO_NHA, TEN_DUONG, " +
                          "QUAN_HUYEN, TINH_TP, TIEN_SU_BENH, TIEN_SU_BENH_GD, DI_UNG_THUOC " +
                          "FROM ADMIN.BENH_NHAN ORDER BY MA_BN FETCH FIRST 500 ROWS ONLY";
                    prms = Array.Empty<OracleParameter>();
                }
                else
                {
                    sql = "SELECT MA_BN, TEN_BN, PHAI, NGAY_SINH, CCCD, SO_NHA, TEN_DUONG, " +
                          "QUAN_HUYEN, TINH_TP, TIEN_SU_BENH, TIEN_SU_BENH_GD, DI_UNG_THUOC " +
                          "FROM ADMIN.BENH_NHAN " +
                          "WHERE UPPER(MA_BN) LIKE :kw OR UPPER(TEN_BN) LIKE :kw OR CCCD LIKE :kw " +
                          "ORDER BY MA_BN FETCH FIRST 500 ROWS ONLY";
                    string kw = "%" + keyword.ToUpper() + "%";
                    prms = new[] { OracleHelper.ParamNvarchar2("kw", kw) };
                }

                var dt = OracleHelper.Instance.ExecuteQuery(sql, prms);
                dgvBN.DataSource = dt;
                ConfigureBnGrid();
                lblBNCount.Text = $"Hiển thị: {dt.Rows.Count} (tối đa 500). Search theo Mã/Tên/CCCD.";
            }
            catch (Exception ex)
            {
                UIHelper.ShowError("Không thể tải danh sách bệnh nhân:\n" + ex.Message);
            }
        }

        private void ConfigureBnGrid()
        {
            if (dgvBN.Columns.Count == 0) return;
            dgvBN.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            foreach (DataGridViewColumn col in dgvBN.Columns)
            {
                switch (col.Name.ToUpper())
                {
                    case "MA_BN": col.HeaderText = "Mã BN"; col.Width = 80; col.ReadOnly = true;
                        col.DefaultCellStyle.BackColor = Color.FromArgb(30, 30, 50);
                        break;
                    case "TEN_BN": col.HeaderText = "Họ tên"; col.Width = 160; break;
                    case "PHAI": col.HeaderText = "Phái"; col.Width = 55; break;
                    case "NGAY_SINH": col.HeaderText = "Ngày sinh"; col.Width = 100;
                        col.DefaultCellStyle.Format = "dd/MM/yyyy"; break;
                    case "CCCD": col.HeaderText = "CCCD"; col.Width = 110; break;
                    case "SO_NHA": col.HeaderText = "Số nhà"; col.Width = 80; break;
                    case "TEN_DUONG": col.HeaderText = "Đường"; col.Width = 120; break;
                    case "QUAN_HUYEN": col.HeaderText = "Quận/Huyện"; col.Width = 100; break;
                    case "TINH_TP": col.HeaderText = "Tỉnh/TP"; col.Width = 100; break;
                    case "TIEN_SU_BENH": col.HeaderText = "Tiền sử bệnh"; col.Width = 160; break;
                    case "TIEN_SU_BENH_GD": col.HeaderText = "TSB gia đình"; col.Width = 140; break;
                    case "DI_UNG_THUOC": col.HeaderText = "Dị ứng thuốc"; col.Width = 140; break;
                }
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
                    UIHelper.ShowWarning("Không có thay đổi nào để lưu.");
                    return;
                }

                int ok = 0;
                foreach (DataRow row in changes.Rows)
                {
                    OracleHelper.Instance.ExecuteNonQuery(
                        "UPDATE ADMIN.BENH_NHAN SET " +
                        "TEN_BN = :tenbn, PHAI = :phai, NGAY_SINH = :ngs, CCCD = :cccd, " +
                        "SO_NHA = :sn, TEN_DUONG = :td, QUAN_HUYEN = :qh, TINH_TP = :tp, " +
                        "TIEN_SU_BENH = :tsb, TIEN_SU_BENH_GD = :tsbgd, DI_UNG_THUOC = :dut " +
                        "WHERE MA_BN = :mabn",
                        new OracleParameter("mabn", row["MA_BN"]),
                        OracleHelper.ParamNvarchar2("tenbn", row["TEN_BN"] ?? DBNull.Value),
                        OracleHelper.ParamNvarchar2("phai", row["PHAI"] ?? DBNull.Value),
                        new OracleParameter("ngs", row["NGAY_SINH"] ?? DBNull.Value),
                        new OracleParameter("cccd", row["CCCD"] ?? DBNull.Value),
                        OracleHelper.ParamNvarchar2("sn", row["SO_NHA"] ?? DBNull.Value),
                        OracleHelper.ParamNvarchar2("td", row["TEN_DUONG"] ?? DBNull.Value),
                        OracleHelper.ParamNvarchar2("qh", row["QUAN_HUYEN"] ?? DBNull.Value),
                        OracleHelper.ParamNvarchar2("tp", row["TINH_TP"] ?? DBNull.Value),
                        OracleHelper.ParamNvarchar2("tsb", row["TIEN_SU_BENH"] ?? DBNull.Value),
                        OracleHelper.ParamNvarchar2("tsbgd", row["TIEN_SU_BENH_GD"] ?? DBNull.Value),
                        OracleHelper.ParamNvarchar2("dut", row["DI_UNG_THUOC"] ?? DBNull.Value));
                    ok++;
                }
                dt.AcceptChanges();
                UIHelper.ShowSuccess($"Đã cập nhật {ok} bệnh nhân.");
            }
            catch (Exception ex)
            {
                UIHelper.ShowError("Lỗi khi lưu BN:\n" + ex.Message);
            }
        }

        private void BtnThemBN_Click(object sender, EventArgs e)
        {
            using var dlg = new BenhNhanEditDialog();
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                string mabn = OracleHelper.Instance.AllocNextMaBenhNhan();
                    OracleHelper.Instance.ExecuteNonQuery(
                        "INSERT INTO ADMIN.BENH_NHAN (MA_BN, TEN_BN, PHAI, NGAY_SINH, CCCD, " +
                        "SO_NHA, TEN_DUONG, QUAN_HUYEN, TINH_TP, TIEN_SU_BENH, TIEN_SU_BENH_GD, DI_UNG_THUOC) " +
                        "VALUES (:mabn, :tenbn, :phai, :ngs, :cccd, :sn, :td, :qh, :tp, :tsb, :tsbgd, :dut)",
                        new OracleParameter("mabn", mabn),
                        OracleHelper.ParamNvarchar2("tenbn", dlg.TenBN),
                        OracleHelper.ParamNvarchar2("phai", dlg.Phai ?? (object)DBNull.Value),
                        new OracleParameter("ngs", dlg.NgaySinh),
                        new OracleParameter("cccd", dlg.CCCD),
                        OracleHelper.ParamNvarchar2("sn", (object?)dlg.SoNha ?? DBNull.Value),
                        OracleHelper.ParamNvarchar2("td", (object?)dlg.TenDuong ?? DBNull.Value),
                        OracleHelper.ParamNvarchar2("qh", (object?)dlg.QuanHuyen ?? DBNull.Value),
                        OracleHelper.ParamNvarchar2("tp", (object?)dlg.TinhTP ?? DBNull.Value),
                        OracleHelper.ParamNvarchar2("tsb", (object?)dlg.TienSuBenh ?? DBNull.Value),
                        OracleHelper.ParamNvarchar2("tsbgd", (object?)dlg.TienSuBenhGD ?? DBNull.Value),
                        OracleHelper.ParamNvarchar2("dut", (object?)dlg.DiUngThuoc ?? DBNull.Value));
                UIHelper.ShowSuccess($"Đã thêm bệnh nhân mới ({mabn}).");
                LoadBenhNhan();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError("Không thể thêm BN:\n" + ex.Message);
            }
        }

        #endregion

        #region Tab HSBA

        private void BuildTabHSBA()
        {
            var lblTitle = new Label
            {
                Text = "Hồ sơ bệnh án (toàn bộ)",
                Font = UIHelper.HeadingFont,
                ForeColor = UIHelper.TextPrimary,
                AutoSize = true,
                Location = new Point(15, 10),
                BackColor = Color.Transparent,
            };

            btnRefreshHSBA = UIHelper.CreateButton("🔄 Làm mới", UIHelper.SecondaryDark, 110, 36);
            btnRefreshHSBA.Location = new Point(15, 50);
            btnRefreshHSBA.Click += (s, e) => LoadHsba();

            btnTaoHSBA = UIHelper.CreateButton("➕ Tạo HSBA", UIHelper.AccentGreen, 130, 36);
            btnTaoHSBA.Location = new Point(135, 50);
            btnTaoHSBA.Click += (s, e) => ShowNewHsbaPanel(true);

            btnLuuHSBA = UIHelper.CreateButton("💾 Lưu điều phối (MA_KHOA, MA_BS)", UIHelper.PrimaryBlue, 300, 36);
            btnLuuHSBA.Location = new Point(275, 50);
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

            dgvHSBA = new DataGridView
            {
                Location = new Point(15, 120),
                Size = new Size(1195, 475),
            };
            UIHelper.StyleDataGridView(dgvHSBA);
            dgvHSBA.SelectionMode = DataGridViewSelectionMode.CellSelect;

            BuildNewHsbaPanel();

            tabHSBA.Controls.AddRange(new Control[] { lblTitle, btnRefreshHSBA, btnTaoHSBA, btnLuuHSBA, lblHSBACount, dgvHSBA, pnlNewHSBA });
        }

        private void BuildNewHsbaPanel()
        {
            pnlNewHSBA = UIHelper.CreateCard(1195, 475);
            pnlNewHSBA.Location = new Point(15, 120);
            pnlNewHSBA.Visible = false;
            pnlNewHSBA.Paint += (s, e) =>
            {
                using var pen = new Pen(UIHelper.AccentGreen, 2);
                e.Graphics.DrawRectangle(pen, 0, 0, pnlNewHSBA.Width - 1, pnlNewHSBA.Height - 1);
            };

            var lbl = UIHelper.CreateLabel("➕ Tạo hồ sơ bệnh án mới", UIHelper.HeadingFont, UIHelper.AccentGreen);
            lbl.Location = new Point(20, 15);

            int y = 70;
            AddLabelAt(pnlNewHSBA, "Mã HSBA:", 20, y);
            txtNewHsba_MaHsba = UIHelper.CreateTextBox(200); txtNewHsba_MaHsba.Location = new Point(160, y - 5); txtNewHsba_MaHsba.CharacterCasing = CharacterCasing.Upper;
            pnlNewHSBA.Controls.Add(txtNewHsba_MaHsba);

            y += 50;
            AddLabelAt(pnlNewHSBA, "Mã BN:", 20, y);
            txtNewHsba_MaBn = UIHelper.CreateTextBox(200); txtNewHsba_MaBn.Location = new Point(160, y - 5); txtNewHsba_MaBn.CharacterCasing = CharacterCasing.Upper;
            pnlNewHSBA.Controls.Add(txtNewHsba_MaBn);

            y += 50;
            AddLabelAt(pnlNewHSBA, "Ngày lập:", 20, y);
            dtpNewHsba_Ngay = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Location = new Point(160, y - 5),
                Size = new Size(200, 32),
                Value = DateTime.Today,
            };
            pnlNewHSBA.Controls.Add(dtpNewHsba_Ngay);

            y += 50;
            AddLabelAt(pnlNewHSBA, "Mã khoa:", 20, y);
            txtNewHsba_MaKhoa = UIHelper.CreateTextBox(200); txtNewHsba_MaKhoa.Location = new Point(160, y - 5); txtNewHsba_MaKhoa.Text = "K01";
            pnlNewHSBA.Controls.Add(txtNewHsba_MaKhoa);

            y += 50;
            AddLabelAt(pnlNewHSBA, "Bác sĩ phụ trách:", 20, y);
            cboNewHsba_MaBs = new ComboBox
            {
                Location = new Point(160, y - 5),
                Size = new Size(400, 32),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = UIHelper.InputBackground,
                ForeColor = UIHelper.TextPrimary,
                FlatStyle = FlatStyle.Flat,
                Font = UIHelper.InputFont,
            };
            pnlNewHSBA.Controls.Add(cboNewHsba_MaBs);

            btnNewHsba_Save = UIHelper.CreateButton("💾 Lưu", UIHelper.AccentGreen, 130, 40);
            btnNewHsba_Save.Location = new Point(20, y + 60);
            btnNewHsba_Save.Click += BtnNewHsbaSave_Click;

            btnNewHsba_Cancel = UIHelper.CreateButton("✖ Hủy", UIHelper.AccentRed, 130, 40);
            btnNewHsba_Cancel.Location = new Point(160, y + 60);
            btnNewHsba_Cancel.Click += (s, e) => ShowNewHsbaPanel(false);

            pnlNewHSBA.Controls.AddRange(new Control[] { lbl, btnNewHsba_Save, btnNewHsba_Cancel });
        }

        private void ShowNewHsbaPanel(bool show)
        {
            pnlNewHSBA.Visible = show;
            dgvHSBA.Visible = !show;
            lblHSBACount.Visible = !show;
            if (show)
            {
                LoadBacSiCombo(cboNewHsba_MaBs);
                txtNewHsba_MaHsba.Focus();
            }
        }

        private void LoadBacSiCombo(ComboBox cbo)
        {
            try
            {
                cbo.Items.Clear();
                var dt = OracleHelper.Instance.ExecuteQuery("SELECT MA_NV, HO_TEN FROM ADMIN.MV_BACSI_LIST ORDER BY MA_NV");
                foreach (DataRow r in dt.Rows)
                {
                    cbo.Items.Add(new ComboItem(r["MA_NV"].ToString(), $"{r["MA_NV"]} - {r["HO_TEN"]}"));
                }
                if (cbo.Items.Count > 0) cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                UIHelper.ShowError("Không thể tải danh sách Bác sĩ:\n" + ex.Message);
            }
        }

        private void BtnNewHsbaSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtNewHsba_MaHsba.Text) ||
                    string.IsNullOrWhiteSpace(txtNewHsba_MaBn.Text) ||
                    cboNewHsba_MaBs.SelectedItem == null)
                {
                    UIHelper.ShowWarning("Vui lòng nhập đầy đủ Mã HSBA, Mã BN, Bác sĩ.");
                    return;
                }

                string maBs = ((ComboItem)cboNewHsba_MaBs.SelectedItem).Value;

                OracleHelper.Instance.ExecuteNonQuery(
                    "INSERT INTO ADMIN.HSBA (MA_HSBA, MA_BN, NGAY, MA_BS, MA_KHOA) " +
                    "VALUES (:mahsba, :mabn, :ngay, :mabs, :makhoa)",
                    new OracleParameter("mahsba", txtNewHsba_MaHsba.Text.Trim()),
                    new OracleParameter("mabn", txtNewHsba_MaBn.Text.Trim()),
                    new OracleParameter("ngay", dtpNewHsba_Ngay.Value.Date),
                    new OracleParameter("mabs", maBs),
                    new OracleParameter("makhoa", txtNewHsba_MaKhoa.Text.Trim()));

                UIHelper.ShowSuccess("Đã tạo HSBA mới.");
                ShowNewHsbaPanel(false);
                LoadHsba();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError("Lỗi khi tạo HSBA:\n" + ex.Message);
            }
        }

        private void LoadHsba()
        {
            try
            {
                var dt = OracleHelper.Instance.ExecuteQuery(
                    "SELECT MA_HSBA, MA_BN, NGAY, MA_KHOA, MA_BS, CHUAN_DOAN, DIEU_TRI, KET_LUAN " +
                    "FROM ADMIN.HSBA ORDER BY NGAY DESC FETCH FIRST 500 ROWS ONLY");
                dgvHSBA.DataSource = dt;
                if (dgvHSBA.Columns.Count > 0)
                {
                    dgvHSBA.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                    foreach (DataGridViewColumn col in dgvHSBA.Columns)
                    {
                        switch (col.Name.ToUpper())
                        {
                            case "MA_HSBA": col.HeaderText = "Mã HSBA"; col.Width = 90; col.ReadOnly = true; break;
                            case "MA_BN": col.HeaderText = "Mã BN"; col.Width = 90; col.ReadOnly = true; break;
                            case "NGAY": col.HeaderText = "Ngày"; col.Width = 100; col.ReadOnly = true;
                                col.DefaultCellStyle.Format = "dd/MM/yyyy"; break;
                            case "MA_KHOA": col.HeaderText = "🔧 Mã khoa"; col.Width = 100; col.ReadOnly = false;
                                col.DefaultCellStyle.BackColor = Color.FromArgb(30, 50, 40); break;
                            case "MA_BS": col.HeaderText = "🔧 Bác sĩ"; col.Width = 100; col.ReadOnly = false;
                                col.DefaultCellStyle.BackColor = Color.FromArgb(30, 50, 40); break;
                            case "CHUAN_DOAN": col.HeaderText = "Chẩn đoán"; col.Width = 200; col.ReadOnly = true; break;
                            case "DIEU_TRI": col.HeaderText = "Điều trị"; col.Width = 200; col.ReadOnly = true; break;
                            case "KET_LUAN": col.HeaderText = "Kết luận"; col.Width = 200; col.ReadOnly = true; break;
                        }
                    }
                }
                lblHSBACount.Text = $"Tổng HSBA: {dt.Rows.Count} (tối đa 500). Sửa MA_KHOA, MA_BS rồi bấm Lưu.";
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
                        "UPDATE ADMIN.HSBA SET MA_KHOA = :mk, MA_BS = :mbs WHERE MA_HSBA = :mh",
                        new OracleParameter("mk", row["MA_KHOA"] ?? DBNull.Value),
                        new OracleParameter("mbs", row["MA_BS"] ?? DBNull.Value),
                        new OracleParameter("mh", row["MA_HSBA"]));
                    ok++;
                }
                dt.AcceptChanges();
                UIHelper.ShowSuccess($"Đã điều phối {ok} HSBA.");
            }
            catch (Exception ex)
            {
                UIHelper.ShowError("Lỗi điều phối:\n" + ex.Message);
            }
        }

        #endregion

        #region Tab Dịch vụ

        private void BuildTabDV()
        {
            var lblTitle = new Label
            {
                Text = "Dịch vụ (HSBA_DV) – điều phối KTV",
                Font = UIHelper.HeadingFont,
                ForeColor = UIHelper.TextPrimary,
                AutoSize = true,
                Location = new Point(15, 10),
                BackColor = Color.Transparent,
            };

            btnRefreshDV = UIHelper.CreateButton("🔄 Làm mới", UIHelper.SecondaryDark, 110, 36);
            btnRefreshDV.Location = new Point(15, 50);
            btnRefreshDV.Click += (s, e) => LoadDv();

            btnTaoDV = UIHelper.CreateButton("➕ Tạo DV", UIHelper.AccentGreen, 130, 36);
            btnTaoDV.Location = new Point(135, 50);
            btnTaoDV.Click += (s, e) => ShowNewDvPanel(true);

            btnLuuDV = UIHelper.CreateButton("💾 Lưu điều phối (MA_KTV)", UIHelper.PrimaryBlue, 240, 36);
            btnLuuDV.Location = new Point(275, 50);
            btnLuuDV.Click += BtnLuuDV_Click;

            lblDVCount = new Label
            {
                Text = "",
                Font = UIHelper.SmallFont,
                ForeColor = UIHelper.TextSecondary,
                AutoSize = true,
                Location = new Point(15, 95),
                BackColor = Color.Transparent,
            };

            dgvDV = new DataGridView
            {
                Location = new Point(15, 120),
                Size = new Size(1195, 475),
            };
            UIHelper.StyleDataGridView(dgvDV);
            dgvDV.SelectionMode = DataGridViewSelectionMode.CellSelect;

            BuildNewDvPanel();

            tabDV.Controls.AddRange(new Control[] { lblTitle, btnRefreshDV, btnTaoDV, btnLuuDV, lblDVCount, dgvDV, pnlNewDV });
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

            var lbl = UIHelper.CreateLabel("➕ Tạo dịch vụ mới", UIHelper.HeadingFont, UIHelper.AccentGreen);
            lbl.Location = new Point(20, 15);

            int y = 70;
            AddLabelAt(pnlNewDV, "Mã HSBA:", 20, y);
            txtNewDv_MaHsba = UIHelper.CreateTextBox(200); txtNewDv_MaHsba.Location = new Point(160, y - 5); txtNewDv_MaHsba.CharacterCasing = CharacterCasing.Upper;
            pnlNewDV.Controls.Add(txtNewDv_MaHsba);

            y += 50;
            AddLabelAt(pnlNewDV, "Loại DV:", 20, y);
            txtNewDv_LoaiDv = UIHelper.CreateTextBox(300); txtNewDv_LoaiDv.Location = new Point(160, y - 5);
            pnlNewDV.Controls.Add(txtNewDv_LoaiDv);

            y += 50;
            AddLabelAt(pnlNewDV, "Ngày DV:", 20, y);
            dtpNewDv_Ngay = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Location = new Point(160, y - 5),
                Size = new Size(200, 32),
                Value = DateTime.Today,
            };
            pnlNewDV.Controls.Add(dtpNewDv_Ngay);

            y += 50;
            AddLabelAt(pnlNewDV, "Kỹ thuật viên:", 20, y);
            cboNewDv_MaKtv = new ComboBox
            {
                Location = new Point(160, y - 5),
                Size = new Size(400, 32),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = UIHelper.InputBackground,
                ForeColor = UIHelper.TextPrimary,
                FlatStyle = FlatStyle.Flat,
                Font = UIHelper.InputFont,
            };
            pnlNewDV.Controls.Add(cboNewDv_MaKtv);

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
            if (show)
            {
                LoadKtvCombo(cboNewDv_MaKtv);
                txtNewDv_MaHsba.Focus();
            }
        }

        /// <summary>Danh sách mã được phép gán vào HSBA_DV.MA_KTV (đồng bộ MV_KTV_LIST).</summary>
        private static HashSet<string> LoadKtvMaSet()
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var dt = OracleHelper.Instance.ExecuteQuery("SELECT MA_NV FROM ADMIN.MV_KTV_LIST");
            foreach (DataRow r in dt.Rows)
            {
                var s = r["MA_NV"]?.ToString()?.Trim();
                if (!string.IsNullOrEmpty(s))
                    set.Add(s);
            }
            return set;
        }

        private void LoadKtvCombo(ComboBox cbo)
        {
            try
            {
                cbo.Items.Clear();
                var dt = OracleHelper.Instance.ExecuteQuery("SELECT MA_NV, HO_TEN FROM ADMIN.MV_KTV_LIST ORDER BY MA_NV");
                foreach (DataRow r in dt.Rows)
                {
                    cbo.Items.Add(new ComboItem(r["MA_NV"].ToString(), $"{r["MA_NV"]} - {r["HO_TEN"]}"));
                }
                if (cbo.Items.Count > 0) cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                UIHelper.ShowError("Không thể tải danh sách KTV:\n" + ex.Message);
            }
        }

        private void BtnNewDvSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtNewDv_MaHsba.Text) ||
                    string.IsNullOrWhiteSpace(txtNewDv_LoaiDv.Text) ||
                    cboNewDv_MaKtv.SelectedItem == null)
                {
                    UIHelper.ShowWarning("Vui lòng nhập đầy đủ Mã HSBA, Loại DV, KTV.");
                    return;
                }

                string maKtv = ((ComboItem)cboNewDv_MaKtv.SelectedItem).Value;
                OracleHelper.Instance.ExecuteNonQuery(
                    "INSERT INTO ADMIN.HSBA_DV (MA_HSBA, LOAI_DV, NGAY_DV, MA_KTV) " +
                    "VALUES (:mh, :ldv, :ngay, :mk)",
                    new OracleParameter("mh", txtNewDv_MaHsba.Text.Trim()),
                    OracleHelper.ParamNvarchar2("ldv", txtNewDv_LoaiDv.Text.Trim()),
                    new OracleParameter("ngay", dtpNewDv_Ngay.Value.Date),
                    new OracleParameter("mk", maKtv));

                UIHelper.ShowSuccess("Đã tạo dịch vụ mới.");
                ShowNewDvPanel(false);
                LoadDv();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError("Lỗi khi tạo DV:\n" + ex.Message);
            }
        }

        private void LoadDv()
        {
            try
            {
                var dt = OracleHelper.Instance.ExecuteQuery(
                    "SELECT MA_HSBA, LOAI_DV, NGAY_DV, MA_KTV, KET_QUA FROM ADMIN.HSBA_DV " +
                    "ORDER BY NGAY_DV DESC FETCH FIRST 500 ROWS ONLY");
                dgvDV.DataSource = dt;
                if (dgvDV.Columns.Count > 0)
                {
                    dgvDV.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                    foreach (DataGridViewColumn col in dgvDV.Columns)
                    {
                        switch (col.Name.ToUpper())
                        {
                            case "MA_HSBA": col.HeaderText = "Mã HSBA"; col.Width = 100; col.ReadOnly = true; break;
                            case "LOAI_DV": col.HeaderText = "Loại DV"; col.Width = 220; col.ReadOnly = true; break;
                            case "NGAY_DV": col.HeaderText = "Ngày DV"; col.Width = 110; col.ReadOnly = true;
                                col.DefaultCellStyle.Format = "dd/MM/yyyy"; break;
                            case "MA_KTV": col.HeaderText = "🔧 KTV"; col.Width = 110; col.ReadOnly = false;
                                col.DefaultCellStyle.BackColor = Color.FromArgb(30, 50, 40); break;
                            case "KET_QUA": col.HeaderText = "Kết quả"; col.Width = 250; col.ReadOnly = true; break;
                        }
                    }
                }
                lblDVCount.Text = $"Tổng DV: {dt.Rows.Count} (tối đa 500). DPV chỉ sửa được MA_KTV.";
            }
            catch (Exception ex)
            {
                UIHelper.ShowError("Không thể tải danh sách dịch vụ:\n" + ex.Message);
            }
        }

        private void BtnLuuDV_Click(object sender, EventArgs e)
        {
            try
            {
                var dt = dgvDV.DataSource as DataTable;
                if (dt == null) return;
                var changes = dt.GetChanges(DataRowState.Modified);
                if (changes == null || changes.Rows.Count == 0)
                {
                    UIHelper.ShowWarning("Không có thay đổi.");
                    return;
                }
                HashSet<string> allowedKtv = LoadKtvMaSet();
                if (allowedKtv.Count == 0)
                {
                    UIHelper.ShowWarning("Không tải được danh sách KTV (MV_KTV_LIST). Không lưu.");
                    return;
                }

                int ok = 0;
                foreach (DataRow row in changes.Rows)
                {
                    object mkRaw = row["MA_KTV"];
                    string mk = mkRaw == null || mkRaw == DBNull.Value
                        ? ""
                        : mkRaw.ToString()?.Trim() ?? "";

                    object mkParam;
                    if (string.IsNullOrEmpty(mk))
                    {
                        mkParam = DBNull.Value;
                    }
                    else if (!allowedKtv.Contains(mk))
                    {
                        UIHelper.ShowWarning(
                            $"Mã KTV '{mk}' không hợp lệ.\n\n" +
                            "Chỉ được gán mã nhân viên có trong MV_KTV_LIST (kỹ thuật viên), không dùng mã điều phối / bác sĩ.");
                        return;
                    }
                    else
                    {
                        mkParam = mk;
                    }

                    OracleHelper.Instance.ExecuteNonQuery(
                        "UPDATE ADMIN.HSBA_DV SET MA_KTV = :mk " +
                        "WHERE MA_HSBA = :mh AND LOAI_DV = :ldv AND NGAY_DV = :ngay",
                        new OracleParameter("mk", mkParam),
                        new OracleParameter("mh", row["MA_HSBA"]),
                        OracleHelper.ParamNvarchar2("ldv", row["LOAI_DV"] ?? DBNull.Value),
                        new OracleParameter("ngay", row["NGAY_DV"]));
                    ok++;
                }
                dt.AcceptChanges();
                UIHelper.ShowSuccess($"Đã điều phối {ok} dịch vụ.");
            }
            catch (Exception ex)
            {
                UIHelper.ShowError("Lỗi:\n" + ex.Message);
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
                    OracleHelper.ParamNvarchar2("qq", txtQueQuan.Text.Trim()),
                    new OracleParameter("sdt", txtSDT.Text.Trim()),
                    new OracleParameter("mn", OracleHelper.Instance.CurrentUser));
                UIHelper.ShowSuccess("Đã cập nhật thông tin cá nhân.");
            }
            catch (Exception ex)
            {
                UIHelper.ShowError("Lỗi:\n" + ex.Message);
            }
        }

        #endregion

        #region Helpers & lifecycle

        private static void AddLabelAt(Panel panel, string text, int x, int y)
        {
            var lbl = UIHelper.CreateLabel(text);
            lbl.Location = new Point(x, y);
            lbl.Size = new Size(140, 24);
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

        private void LoadAllData()
        {
            LoadBenhNhan();
            LoadHsba();
            LoadDv();
            LoadThongTin();
        }

        #endregion

        // Helper inner class cho combobox item
        private class ComboItem
        {
            public string Value { get; }
            public string Label { get; }
            public ComboItem(string value, string label) { Value = value; Label = label; }
            public override string ToString() => Label;
        }
    }
}
