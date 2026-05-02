using System;
using System.Drawing;
using System.Windows.Forms;
using HospitalManagement.App.DataAccess;
using HospitalManagement.App.Helpers;

namespace HospitalManagement.App.Forms
{
    public class ThongBaoKhanForm : Form
    {
        private DataGridView dgvThongBao;
        private Label lblTitle;
        private Label lblSubTitle;
        private Label lblCount;
        private Button btnRefresh;
        private Button btnClose;

        public ThongBaoKhanForm()
        {
            InitializeComponent();
            LoadThongBao();
        }

        private void InitializeComponent()
        {
            UIHelper.StyleForm(this, "Thông báo cuộc họp khẩn", 1050, 680);
            this.StartPosition = FormStartPosition.CenterParent;

            lblTitle = new Label
            {
                Text = "📢 Thông báo cuộc họp khẩn",
                Font = UIHelper.HeadingFont,
                ForeColor = UIHelper.TextPrimary,
                AutoSize = true,
                Location = new Point(25, 20),
                BackColor = Color.Transparent
            };

            lblSubTitle = new Label
            {
                Text = $"Thông báo phù hợp với tài khoản {OracleHelper.Instance.CurrentUser}",
                Font = UIHelper.SmallFont,
                ForeColor = UIHelper.TextSecondary,
                AutoSize = true,
                Location = new Point(27, 55),
                BackColor = Color.Transparent
            };

            btnRefresh = UIHelper.CreateButton("🔄 Làm mới", UIHelper.PrimaryBlue, 130, 36);
            btnRefresh.Location = new Point(25, 95);
            btnRefresh.Click += (s, e) => LoadThongBao();

            btnClose = UIHelper.CreateButton("Đóng", UIHelper.SecondaryDark, 100, 36);
            btnClose.Location = new Point(165, 95);
            btnClose.Click += (s, e) => this.Close();

            lblCount = new Label
            {
                Text = "",
                Font = UIHelper.SmallFont,
                ForeColor = UIHelper.AccentGreen,
                AutoSize = true,
                Location = new Point(285, 103),
                BackColor = Color.Transparent
            };

            dgvThongBao = new DataGridView
            {
                Location = new Point(25, 145),
                Size = new Size(990, 475),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false
            };

            UIHelper.StyleDataGridView(dgvThongBao);

            this.Controls.AddRange(new Control[]
            {
                lblTitle, lblSubTitle, btnRefresh, btnClose, lblCount, dgvThongBao
            });
        }

        private void LoadThongBao()
        {
            try
            {
                var dt = OracleHelper.Instance.ExecuteQuery(
                    "SELECT MA_TB, NOI_DUNG, NGAY_GIO, DIA_DIEM FROM ADMIN.THONGBAO ORDER BY NGAY_GIO DESC");

                dgvThongBao.DataSource = dt;

                if (dgvThongBao.Columns.Count > 0)
                {
                    dgvThongBao.Columns["MA_TB"].HeaderText = "Mã TB";
                    dgvThongBao.Columns["NOI_DUNG"].HeaderText = "Nội dung cuộc họp";
                    dgvThongBao.Columns["NGAY_GIO"].HeaderText = "Ngày giờ";
                    dgvThongBao.Columns["DIA_DIEM"].HeaderText = "Địa điểm";
                    
                    dgvThongBao.Columns["MA_TB"].Width = 80;
                    dgvThongBao.Columns["NOI_DUNG"].Width = 420;
                    dgvThongBao.Columns["NGAY_GIO"].Width = 180;
                    dgvThongBao.Columns["DIA_DIEM"].Width = 220;

                    // format ngày giờ
                    dgvThongBao.Columns["NGAY_GIO"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                    dgvThongBao.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                    dgvThongBao.Columns["NGAY_GIO"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }

                lblCount.Text = $"Có {dt.Rows.Count} thông báo phù hợp.";
            }
            catch (Exception ex)
            {
                UIHelper.ShowError("Không thể tải thông báo khẩn:\n" + ex.Message);
            }
        }
    }
}