using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HospitalManagement.App.Helpers
{
    /// <summary>
    /// Helper class chứa các tiện ích UI chung cho toàn ứng dụng.
    /// Tạo giao diện modern, chuyên nghiệp cho WinForms.
    /// </summary>
    public static class UIHelper
    {
        // ===== Color Palette =====
        public static readonly Color PrimaryBlue = Color.FromArgb(41, 98, 255);
        public static readonly Color PrimaryDark = Color.FromArgb(25, 25, 45);
        public static readonly Color SecondaryDark = Color.FromArgb(35, 35, 60);
        public static readonly Color CardBackground = Color.FromArgb(45, 45, 75);
        public static readonly Color SurfaceColor = Color.FromArgb(55, 55, 85);
        public static readonly Color AccentGreen = Color.FromArgb(0, 200, 150);
        public static readonly Color AccentOrange = Color.FromArgb(255, 165, 0);
        public static readonly Color AccentRed = Color.FromArgb(255, 82, 82);
        public static readonly Color TextPrimary = Color.FromArgb(240, 240, 255);
        public static readonly Color TextSecondary = Color.FromArgb(160, 170, 200);
        public static readonly Color BorderColor = Color.FromArgb(70, 70, 110);
        public static readonly Color InputBackground = Color.FromArgb(35, 35, 55);
        public static readonly Color RowAlternate = Color.FromArgb(40, 40, 65);
        public static readonly Color HoverColor = Color.FromArgb(60, 60, 100);

        // ===== Fonts =====
        public static readonly Font TitleFont = new Font("Segoe UI", 22, FontStyle.Bold);
        public static readonly Font SubtitleFont = new Font("Segoe UI", 14, FontStyle.Regular);
        public static readonly Font HeadingFont = new Font("Segoe UI Semibold", 16, FontStyle.Bold);
        public static readonly Font LabelFont = new Font("Segoe UI", 11, FontStyle.Regular);
        public static readonly Font InputFont = new Font("Segoe UI", 11, FontStyle.Regular);
        public static readonly Font ButtonFont = new Font("Segoe UI Semibold", 11, FontStyle.Bold);
        public static readonly Font SmallFont = new Font("Segoe UI", 9, FontStyle.Regular);
        public static readonly Font DataGridFont = new Font("Segoe UI", 10, FontStyle.Regular);

        /// <summary>
        /// Tạo nút bấm modern với gradient background
        /// </summary>
        public static Button CreateButton(string text, Color bgColor, int width = 180, int height = 42)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(width, height),
                FlatStyle = FlatStyle.Flat,
                BackColor = bgColor,
                ForeColor = Color.White,
                Font = ButtonFont,
                Cursor = Cursors.Hand,
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = ControlPaint.Light(bgColor, 0.15f);
            btn.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(bgColor, 0.1f);

            // Rounded corners
            btn.Region = CreateRoundedRegion(btn.Size, 8);
            btn.Resize += (s, e) => btn.Region = CreateRoundedRegion(btn.Size, 8);

            return btn;
        }

        /// <summary>
        /// Tạo TextBox modern với style dark theme
        /// </summary>
        public static TextBox CreateTextBox(int width = 300)
        {
            return new TextBox
            {
                Size = new Size(width, 36),
                BackColor = InputBackground,
                ForeColor = TextPrimary,
                Font = InputFont,
                BorderStyle = BorderStyle.FixedSingle,
            };
        }

        /// <summary>
        /// Tạo Label tiêu chuẩn
        /// </summary>
        public static Label CreateLabel(string text, Font font = null, Color? color = null)
        {
            return new Label
            {
                Text = text,
                Font = font ?? LabelFont,
                ForeColor = color ?? TextPrimary,
                AutoSize = true,
                BackColor = Color.Transparent,
            };
        }

        /// <summary>
        /// Style cho DataGridView dark theme, modern
        /// </summary>
        public static void StyleDataGridView(DataGridView dgv)
        {
            dgv.BackgroundColor = PrimaryDark;
            dgv.BorderStyle = BorderStyle.None;
            dgv.GridColor = BorderColor;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;

            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = SecondaryDark,
                ForeColor = TextPrimary,
                Font = new Font("Segoe UI Semibold", 10, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding = new Padding(8, 4, 8, 4),
                SelectionBackColor = SecondaryDark,
                SelectionForeColor = TextPrimary,
            };
            dgv.ColumnHeadersHeight = 44;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            dgv.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = PrimaryDark,
                ForeColor = TextPrimary,
                Font = DataGridFont,
                SelectionBackColor = PrimaryBlue,
                SelectionForeColor = Color.White,
                Padding = new Padding(6, 3, 6, 3),
            };

            dgv.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = RowAlternate,
                ForeColor = TextPrimary,
                SelectionBackColor = PrimaryBlue,
                SelectionForeColor = Color.White,
            };

            dgv.RowHeadersVisible = false;
            dgv.RowTemplate.Height = 38;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.ScrollBars = ScrollBars.Both;
        }

        /// <summary>
        /// Style cho TabControl dark theme
        /// </summary>
        public static void StyleTabControl(TabControl tc)
        {
            tc.DrawMode = TabDrawMode.OwnerDrawFixed;
            tc.SizeMode = TabSizeMode.Fixed;
            tc.ItemSize = new Size(180, 42);
            tc.DrawItem += (sender, e) =>
            {
                var tabCtrl = sender as TabControl;
                bool isSelected = e.Index == tabCtrl.SelectedIndex;
                var bgColor = isSelected ? PrimaryBlue : SecondaryDark;
                var fgColor = isSelected ? Color.White : TextSecondary;

                using var bgBrush = new SolidBrush(bgColor);
                e.Graphics.FillRectangle(bgBrush, e.Bounds);

                var tabText = tabCtrl.TabPages[e.Index].Text;
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                };
                using var fgBrush = new SolidBrush(fgColor);
                e.Graphics.DrawString(tabText, ButtonFont, fgBrush, e.Bounds, sf);
            };
        }

        /// <summary>
        /// Tạo Panel card với style dark theme
        /// </summary>
        public static Panel CreateCard(int width, int height)
        {
            return new Panel
            {
                Size = new Size(width, height),
                BackColor = CardBackground,
                Padding = new Padding(20),
            };
        }

        /// <summary>
        /// Style cho Form mặc định
        /// </summary>
        public static void StyleForm(Form form, string title, int width = 1100, int height = 700)
        {
            form.Text = title;
            form.Size = new Size(width, height);
            form.StartPosition = FormStartPosition.CenterScreen;
            form.BackColor = PrimaryDark;
            form.ForeColor = TextPrimary;
            form.Font = LabelFont;
            form.FormBorderStyle = FormBorderStyle.FixedSingle;
            form.MaximizeBox = false;
        }

        /// <summary>
        /// Tạo region bo góc cho control
        /// </summary>
        public static Region CreateRoundedRegion(Size size, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(0, 0, radius * 2, radius * 2, 180, 90);
            path.AddArc(size.Width - radius * 2, 0, radius * 2, radius * 2, 270, 90);
            path.AddArc(size.Width - radius * 2, size.Height - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(0, size.Height - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure();
            return new Region(path);
        }

        /// <summary>
        /// Hiển thị thông báo thành công
        /// </summary>
        public static void ShowSuccess(string message)
        {
            MessageBox.Show(message, "✅ Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Hiển thị thông báo lỗi
        /// </summary>
        public static void ShowError(string message)
        {
            MessageBox.Show(message, "❌ Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Hiển thị thông báo cảnh báo
        /// </summary>
        public static void ShowWarning(string message)
        {
            MessageBox.Show(message, "⚠️ Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
