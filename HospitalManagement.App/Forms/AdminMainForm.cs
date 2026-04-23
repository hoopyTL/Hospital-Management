using System.Data;
using HospitalManagement.App.Models;
using HospitalManagement.App.Services;
using HospitalManagement.App.Helpers;

namespace HospitalManagement.App.Forms;

public class AdminMainForm : Form
{
    private readonly DbConnectionSettings _settings;
    private readonly OracleAdminService _service;

    private readonly Label _lblConnection = new() { AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
    private readonly Label _lblSummary = new() { AutoSize = true, ForeColor = Color.DimGray };

    private readonly DataGridView _dgvDashboard = CreateGrid();
    private readonly DataGridView _dgvUsers = CreateGrid();
    private readonly DataGridView _dgvRoles = CreateGrid();
    private readonly DataGridView _dgvObjects = CreateGrid();
    private readonly DataGridView _dgvSysPrivs = CreateGrid();
    private readonly DataGridView _dgvRolePrivs = CreateGrid();
    private readonly DataGridView _dgvObjPrivs = CreateGrid();
    private readonly DataGridView _dgvColPrivs = CreateGrid();

    private readonly TextBox _txtUserSearch = AdminUiHelper.CreateTextBox("Tìm username");
    private readonly TextBox _txtRoleSearch = AdminUiHelper.CreateTextBox("Tìm role");
    private readonly TextBox _txtObjectOwnerFilter = AdminUiHelper.CreateTextBox("Owner filter");

    private readonly ComboBox _cboGrantPrincipalType = AdminUiHelper.CreateComboBox();
    private readonly ComboBox _cboGrantPrincipal = AdminUiHelper.CreateComboBox();
    private readonly ComboBox _cboGrantMode = AdminUiHelper.CreateComboBox();
    private readonly ComboBox _cboGrantSystemPriv = AdminUiHelper.CreateComboBox();
    private readonly ComboBox _cboGrantRoleName = AdminUiHelper.CreateComboBox();
    private readonly TextBox _txtGrantOwner = AdminUiHelper.CreateTextBox("LAB_OWNER");
    private readonly ComboBox _cboGrantObjectName = AdminUiHelper.CreateComboBox();
    private readonly ComboBox _cboGrantObjectType = AdminUiHelper.CreateComboBox();
    private readonly ComboBox _cboGrantObjectPriv = AdminUiHelper.CreateComboBox();
    private readonly CheckedListBox _clbGrantColumns = new() { Height = 140, CheckOnClick = true };
    private readonly CheckBox _chkGrantDelegable = new() { Text = "Cho phép cấp tiếp (WITH GRANT OPTION)", AutoSize = true };

    private readonly ComboBox _cboRevokePrincipalType = AdminUiHelper.CreateComboBox();
    private readonly ComboBox _cboRevokePrincipal = AdminUiHelper.CreateComboBox();
    private readonly ComboBox _cboRevokeMode = AdminUiHelper.CreateComboBox();
    private readonly ComboBox _cboRevokeSystemPriv = AdminUiHelper.CreateComboBox();
    private readonly ComboBox _cboRevokeRoleName = AdminUiHelper.CreateComboBox();
    private readonly TextBox _txtRevokeOwner = AdminUiHelper.CreateTextBox("LAB_OWNER");
    private readonly ComboBox _cboRevokeObjectName = AdminUiHelper.CreateComboBox();
    private readonly ComboBox _cboRevokeObjectType = AdminUiHelper.CreateComboBox();
    private readonly ComboBox _cboRevokeObjectPriv = AdminUiHelper.CreateComboBox();
    private readonly CheckedListBox _clbRevokeColumns = new() { Height = 140, CheckOnClick = true };

    private readonly ComboBox _cboExplorerPrincipalType = AdminUiHelper.CreateComboBox();
    private readonly ComboBox _cboExplorerPrincipal = AdminUiHelper.CreateComboBox();

    private readonly Panel _grantSystemPanel = new() { Dock = DockStyle.Top, Height = 90 };
    private readonly Panel _grantRolePanel = new() { Dock = DockStyle.Top, Height = 90 };
    private readonly Panel _grantObjectPanel = new() { Dock = DockStyle.Top, Height = 280 };
    private readonly Panel _revokeSystemPanel = new() { Dock = DockStyle.Top, Height = 90 };
    private readonly Panel _revokeRolePanel = new() { Dock = DockStyle.Top, Height = 90 };
    private readonly Panel _revokeObjectPanel = new() { Dock = DockStyle.Top, Height = 280 };

    public AdminMainForm(DbConnectionSettings settings)
    {
        _settings = settings;
        _service = new OracleAdminService(settings);

        Text = "Phân hệ 1 - Ứng dụng quản trị CSDL Oracle";
        WindowState = FormWindowState.Maximized;
        _txtGrantOwner.Text = "LAB_OWNER";
        _txtRevokeOwner.Text = "LAB_OWNER";

        InitializeForm();
        BindEvents();
        LoadAll();
    }

    private void InitializeForm()
    {
        _lblConnection.Text = $"Đang kết nối: {_settings}";
        _lblSummary.Text = "Quản trị user/role, cấp quyền, thu hồi quyền và xem quyền trên Oracle.";

        var header = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 70,
            Padding = new Padding(16, 12, 16, 8),
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false
        };
        header.Controls.Add(_lblConnection);
        header.Controls.Add(_lblSummary);

        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(BuildDashboardTab());
        tabs.TabPages.Add(BuildUsersTab());
        tabs.TabPages.Add(BuildRolesTab());
        tabs.TabPages.Add(BuildObjectsTab());
        tabs.TabPages.Add(BuildGrantTab());
        tabs.TabPages.Add(BuildRevokeTab());
        tabs.TabPages.Add(BuildExplorerTab());

        Controls.Add(tabs);
        Controls.Add(header);
    }

    private void BindEvents()
    {
        _cboGrantMode.SelectedIndexChanged += (_, _) => ToggleGrantPanels();
        _cboRevokeMode.SelectedIndexChanged += (_, _) => ToggleRevokePanels();
        _cboGrantPrincipalType.SelectedIndexChanged += (_, _) => RefreshPrincipalCombos(_cboGrantPrincipalType, _cboGrantPrincipal);
        _cboRevokePrincipalType.SelectedIndexChanged += (_, _) => RefreshPrincipalCombos(_cboRevokePrincipalType, _cboRevokePrincipal);
        _cboExplorerPrincipalType.SelectedIndexChanged += (_, _) => RefreshPrincipalCombos(_cboExplorerPrincipalType, _cboExplorerPrincipal);

        _cboGrantObjectPriv.SelectedIndexChanged += (_, _) => ToggleGrantColumnChooser();
        _cboGrantObjectType.SelectedIndexChanged += (_, _) => ToggleGrantObjectPrivilegeSource();
        _cboGrantObjectName.SelectedIndexChanged += (_, _) => ToggleGrantColumnChooser();
        _txtGrantOwner.TextChanged += (_, _) => RefreshGrantObjectNames();
        _cboRevokeObjectPriv.SelectedIndexChanged += (_, _) => ToggleRevokeColumnChooser();
        _cboRevokeObjectType.SelectedIndexChanged += (_, _) => ToggleRevokeObjectPrivilegeSource();
        _cboRevokeObjectName.SelectedIndexChanged += (_, _) => ToggleRevokeColumnChooser();
        _txtRevokeOwner.TextChanged += (_, _) => RefreshRevokeObjectNames();

        _txtUserSearch.KeyDown += (_, e) =>
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            LoadUsers();
        };

        _txtRoleSearch.KeyDown += (_, e) =>
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            LoadRoles();
        };
    }

    private TabPage BuildDashboardTab()
    {
        var page = new TabPage("Tổng quan");

        var refreshButton = new Button { Text = "Làm mới", Width = 120, Height = 34 };
        refreshButton.Click += (_, _) => LoadDashboard();

        var info = new Label
        {
            Text = "Tab này dùng để kiểm tra phiên làm việc, version DB và xác nhận app đang nhìn thấy metadata cần thiết.",
            AutoSize = true
        };

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 86,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(16, 16, 16, 0)
        };
        panel.Controls.Add(info);
        panel.Controls.Add(refreshButton);

        _dgvDashboard.Dock = DockStyle.Fill;

        page.Controls.Add(_dgvDashboard);
        page.Controls.Add(panel);
        return page;
    }

    private TabPage BuildUsersTab()
    {
        var page = new TabPage("Users");
        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 78,
            Padding = new Padding(12),
            WrapContents = true
        };

        var btnRefresh = new Button { Text = "Làm mới", Width = 100, Height = 34 };
        var btnCreate = new Button { Text = "Tạo user", Width = 100, Height = 34 };
        var btnAlter = new Button { Text = "Sửa pass", Width = 120, Height = 34 };
        var btnDefaultTs = new Button { Text = "Sửa Default TS", Width = 150, Height = 34 };
        var btnTempTs = new Button { Text = "Sửa Temp TS", Width = 150, Height = 34 };
        var btnProfile = new Button { Text = "Sửa Profile", Width = 130, Height = 34 };
        var btnLock = new Button { Text = "Lock/Unlock", Width = 120, Height = 34 };
        var btnDrop = new Button { Text = "Drop user", Width = 100, Height = 34 };

        btnRefresh.Click += (_, _) => LoadUsers();
        btnCreate.Click += (_, _) => CreateUser();
        btnAlter.Click += (_, _) => AlterUserPassword();
        btnDefaultTs.Click += (_, _) => AlterUserDefaultTablespace();
        btnTempTs.Click += (_, _) => AlterUserTemporaryTablespace();
        btnProfile.Click += (_, _) => AlterUserProfile();
        btnLock.Click += (_, _) => ToggleSelectedUserLock();
        btnDrop.Click += (_, _) => DropSelectedUser();

        toolbar.Controls.AddRange([
            new Label { Text = "Tìm kiếm:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) },
            _txtUserSearch, btnRefresh, btnCreate, btnAlter, btnDefaultTs, btnTempTs, btnProfile, btnLock, btnDrop
        ]);

        _txtUserSearch.TextChanged += (_, _) => LoadUsers();

        _dgvUsers.Dock = DockStyle.Fill;
        page.Controls.Add(_dgvUsers);
        page.Controls.Add(toolbar);
        return page;
    }

    private TabPage BuildRolesTab()
    {
        var page = new TabPage("Roles");
        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 78,
            Padding = new Padding(12),
            WrapContents = true
        };

        var btnRefresh = new Button { Text = "Làm mới", Width = 100, Height = 34 };
        var btnCreate = new Button { Text = "Tạo role", Width = 100, Height = 34 };
        var btnAlter = new Button { Text = "Sửa pass", Width = 120, Height = 34 };
        var btnDrop = new Button { Text = "Drop role", Width = 100, Height = 34 };

        btnRefresh.Click += (_, _) => LoadRoles();
        btnCreate.Click += (_, _) => CreateRole();
        btnAlter.Click += (_, _) => AlterRolePassword();
        btnDrop.Click += (_, _) => DropSelectedRole();

        toolbar.Controls.AddRange([
            new Label { Text = "Tìm kiếm:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) },
            _txtRoleSearch, btnRefresh, btnCreate, btnAlter, btnDrop
        ]);

        _txtRoleSearch.TextChanged += (_, _) => LoadRoles();

        _dgvRoles.Dock = DockStyle.Fill;
        page.Controls.Add(_dgvRoles);
        page.Controls.Add(toolbar);
        return page;
    }


    private TabPage BuildObjectsTab()
    {
        var page = new TabPage("Objects demo");
        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 72,
            Padding = new Padding(12)
        };

        var btnRefresh = new Button { Text = "Làm mới object", Width = 120, Height = 34 };
        btnRefresh.Click += (_, _) => LoadObjects();

        _txtObjectOwnerFilter.Text = "LAB_OWNER";
        _txtObjectOwnerFilter.Enabled = false;
        _txtObjectOwnerFilter.Visible = false;

        toolbar.Controls.AddRange([btnRefresh]);

        _txtObjectOwnerFilter.TextChanged += (_, _) => LoadObjects();
        _dgvObjects.Dock = DockStyle.Fill;

        page.Controls.Add(_dgvObjects);
        page.Controls.Add(toolbar);
        return page;
    }

    private TabPage BuildGrantTab()
    {
        var page = new TabPage("Grant");

        _cboGrantPrincipalType.Items.AddRange(["USER", "ROLE"]);
        _cboGrantPrincipalType.SelectedIndex = 0;

        _cboGrantMode.Items.AddRange(["SYSTEM PRIVILEGE", "OBJECT PRIVILEGE", "ROLE"]);
        _cboGrantMode.SelectedIndex = 0;

        _cboGrantSystemPriv.Items.AddRange(OracleAdminService.CommonSystemPrivileges);
        _cboGrantSystemPriv.SelectedIndex = 0;

        _cboGrantObjectType.Items.AddRange(["TABLE", "VIEW", "PROCEDURE", "FUNCTION"]);
        _cboGrantObjectType.SelectedIndex = 0;
        _cboGrantObjectPriv.SelectedIndex = -1;

        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 110,
            Padding = new Padding(16, 12, 16, 0),
            ColumnCount = 4
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        AddHeaderField(header, 0, "Principal type", _cboGrantPrincipalType, "Principal", _cboGrantPrincipal);
        AddHeaderField(header, 1, "Grant mode", _cboGrantMode, "", new Label());
        header.Controls.Add(_chkGrantDelegable, 1, 2);
        header.SetColumnSpan(_chkGrantDelegable, 3);

        BuildGrantSystemPanel();
        BuildGrantRolePanel();
        BuildGrantObjectPanel();

        var btnExecute = new Button { Text = "Thực hiện GRANT", Width = 160, Height = 38 };
        var btnRefreshMeta = new Button { Text = "Làm mới danh sách", Width = 140, Height = 38 };
        btnExecute.Click += (_, _) => ExecuteGrant();
        btnRefreshMeta.Click += (_, _) => RefreshLists();

        var footer = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 60,
            Padding = new Padding(16, 8, 16, 8)
        };
        footer.Controls.AddRange([btnExecute, btnRefreshMeta]);

        page.Controls.Add(footer);
        page.Controls.Add(_grantObjectPanel);
        page.Controls.Add(_grantRolePanel);
        page.Controls.Add(_grantSystemPanel);
        page.Controls.Add(header);

        return page;
    }

    private TabPage BuildRevokeTab()
    {
        var page = new TabPage("Revoke");

        _cboRevokePrincipalType.Items.AddRange(["USER", "ROLE"]);
        _cboRevokePrincipalType.SelectedIndex = 0;

        _cboRevokeMode.Items.AddRange(["SYSTEM PRIVILEGE", "OBJECT PRIVILEGE", "ROLE"]);
        _cboRevokeMode.SelectedIndex = 0;

        _cboRevokeSystemPriv.Items.AddRange(OracleAdminService.CommonSystemPrivileges);
        _cboRevokeSystemPriv.SelectedIndex = 0;

        _cboRevokeObjectType.Items.AddRange(["TABLE", "VIEW", "PROCEDURE", "FUNCTION"]);
        _cboRevokeObjectType.SelectedIndex = 0;
        _cboRevokeObjectPriv.SelectedIndex = -1;

        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 92,
            Padding = new Padding(16, 12, 16, 0),
            ColumnCount = 4
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        AddHeaderField(header, 0, "Principal type", _cboRevokePrincipalType, "Principal", _cboRevokePrincipal);
        AddHeaderField(header, 1, "Revoke mode", _cboRevokeMode, "", new Label());

        BuildRevokeSystemPanel();
        BuildRevokeRolePanel();
        BuildRevokeObjectPanel();

        var btnExecute = new Button { Text = "Thực hiện REVOKE", Width = 160, Height = 38 };
        var btnRefreshMeta = new Button { Text = "Làm mới danh sách", Width = 140, Height = 38 };
        btnExecute.Click += (_, _) => ExecuteRevoke();
        btnRefreshMeta.Click += (_, _) => RefreshLists();

        var footer = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 60,
            Padding = new Padding(16, 8, 16, 8)
        };
        footer.Controls.AddRange([btnExecute, btnRefreshMeta]);

        page.Controls.Add(footer);
        page.Controls.Add(_revokeObjectPanel);
        page.Controls.Add(_revokeRolePanel);
        page.Controls.Add(_revokeSystemPanel);
        page.Controls.Add(header);

        return page;
    }

    private TabPage BuildExplorerTab()
    {
        var page = new TabPage("Tra cứu quyền");

        _cboExplorerPrincipalType.Items.AddRange(["USER", "ROLE"]);
        _cboExplorerPrincipalType.SelectedIndex = 0;

        var top = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 70,
            Padding = new Padding(12),
            WrapContents = true
        };

        var btnLoad = new Button { Text = "Xem quyền", Width = 120, Height = 34 };
        btnLoad.Click += (_, _) => LoadPrivilegeExplorer();

        top.Controls.AddRange([
            new Label { Text = "Principal type", AutoSize = true, Padding = new Padding(0, 8, 0, 0) },
            _cboExplorerPrincipalType,
            new Label { Text = "Principal", AutoSize = true, Padding = new Padding(12, 8, 0, 0) },
            _cboExplorerPrincipal,
            btnLoad
        ]);

        var explorerHint = new Label
        {
            Dock = DockStyle.Top,
            Height = 44,
            Padding = new Padding(12, 4, 12, 4),
            ForeColor = Color.DimGray,
            Text =
                "Column Privileges gộp DBA_COL_PRIVS (UPDATE — DIRECT) và VPD SELECT theo cột (SOURCE = VPD). " +
                "Cột bị ẩn qua VPD trả về NULL, user truy vấn trực tiếp bảng gốc."
        };

        var innerTabs = new TabControl { Dock = DockStyle.Fill };
        innerTabs.TabPages.Add(CreateGridTab("System Privileges", _dgvSysPrivs));
        innerTabs.TabPages.Add(CreateGridTab("Role Grants", _dgvRolePrivs));
        innerTabs.TabPages.Add(CreateGridTab("Object Privileges", _dgvObjPrivs));
        innerTabs.TabPages.Add(CreateGridTab("Column Privileges", _dgvColPrivs));

        page.Controls.Add(innerTabs);
        page.Controls.Add(explorerHint);
        page.Controls.Add(top);
        return page;
    }

    private void BuildGrantSystemPanel()
    {
        var layout = SimpleTwoColumnLayout();
        AddField(layout, 0, "System privilege", _cboGrantSystemPriv);
        _grantSystemPanel.Controls.Add(layout);
    }

    private void BuildGrantRolePanel()
    {
        var layout = SimpleTwoColumnLayout();
        AddField(layout, 0, "Granted role", _cboGrantRoleName);
        _grantRolePanel.Controls.Add(layout);
    }

    private void BuildGrantObjectPanel()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16, 8, 16, 8),
            ColumnCount = 4
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        AddField(layout, 0, "Owner", _txtGrantOwner, 0, 1);
        AddField(layout, 0, "Object name", _cboGrantObjectName, 2, 3);
        AddField(layout, 1, "Object type", _cboGrantObjectType, 0, 1);
        AddField(layout, 1, "Privilege", _cboGrantObjectPriv, 2, 3);

        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(new Label
        {
            Text = "Chọn cột (chỉ áp dụng cho SELECT/UPDATE trên TABLE/VIEW)",
            AutoSize = true,
            Anchor = AnchorStyles.Left
        }, 0, 2);
        layout.Controls.Add(_clbGrantColumns, 1, 2);
        layout.SetColumnSpan(_clbGrantColumns, 3);

        _grantObjectPanel.Controls.Add(layout);
    }

    private void BuildRevokeSystemPanel()
    {
        var layout = SimpleTwoColumnLayout();
        AddField(layout, 0, "System privilege", _cboRevokeSystemPriv);
        _revokeSystemPanel.Controls.Add(layout);
    }

    private void BuildRevokeRolePanel()
    {
        var layout = SimpleTwoColumnLayout();
        AddField(layout, 0, "Granted role", _cboRevokeRoleName);
        _revokeRolePanel.Controls.Add(layout);
    }

    private void BuildRevokeObjectPanel()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16, 8, 16, 8),
            ColumnCount = 4
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        AddField(layout, 0, "Owner", _txtRevokeOwner, 0, 1);
        AddField(layout, 0, "Object name", _cboRevokeObjectName, 2, 3);
        AddField(layout, 1, "Object type", _cboRevokeObjectType, 0, 1);
        AddField(layout, 1, "Privilege", _cboRevokeObjectPriv, 2, 3);

        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(new Label
        {
            Text = "Chọn cột (nếu thu hồi quyền SELECT/UPDATE mức cột)",
            AutoSize = true,
            Anchor = AnchorStyles.Left
        }, 0, 2);
        layout.Controls.Add(_clbRevokeColumns, 1, 2);
        layout.SetColumnSpan(_clbRevokeColumns, 3);

        _revokeObjectPanel.Controls.Add(layout);
    }

    private void LoadAll()
    {
        try
        {
            RefreshLists();
            LoadDashboard();
            ToggleGrantPanels();
            ToggleRevokePanels();
        }
        catch (Exception ex)
        {
            AdminUiHelper.ShowError(ex, "Khởi động thất bại");
        }
    }

    private void LoadDashboard()
    {
        try
        {
            _dgvDashboard.DataSource = _service.GetDatabaseInfo();
        }
        catch (Exception ex)
        {
            AdminUiHelper.ShowError(ex);
        }
    }

    private void LoadUsers()
    {
        try
        {
            _dgvUsers.DataSource = _service.GetUsers(_txtUserSearch.Text);
        }
        catch (Exception ex)
        {
            AdminUiHelper.ShowError(ex);
        }
    }

    private void LoadRoles()
    {
        try
        {
            _dgvRoles.DataSource = _service.GetRoles(_txtRoleSearch.Text);
        }
        catch (Exception ex)
        {
            AdminUiHelper.ShowError(ex);
        }
    }

    private void LoadObjects()
    {
        try
        {
            _dgvObjects.DataSource = _service.GetManagedObjects(_txtObjectOwnerFilter.Text);
        }
        catch (Exception ex)
        {
            AdminUiHelper.ShowError(ex);
        }
    }

    private void RefreshLists()
    {
        LoadUsers();
        LoadRoles();
        LoadObjects();

        RefreshPrincipalCombos(_cboGrantPrincipalType, _cboGrantPrincipal);
        RefreshPrincipalCombos(_cboRevokePrincipalType, _cboRevokePrincipal);
        RefreshPrincipalCombos(_cboExplorerPrincipalType, _cboExplorerPrincipal);

        FillCombo(_cboGrantRoleName, _service.GetRoleNames());
        FillCombo(_cboRevokeRoleName, _service.GetRoleNames());

        ToggleGrantObjectPrivilegeSource();
        ToggleRevokeObjectPrivilegeSource();
    }

    private void CreateUser()
    {
        using var dialog = new UserDialogForm("Tạo user");
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            _service.CreateUser(dialog.Username, dialog.PasswordValue, dialog.DefaultTablespace, dialog.TemporaryTablespace);
            AdminUiHelper.ShowInfo("Tạo user thành công.");
            RefreshLists();
        }
        catch (Exception ex)
        {
            AdminUiHelper.ShowError(ex);
        }
    }

    private void AlterUserPassword()
    {
        var username = GetSelectedCellValue(_dgvUsers, "USERNAME");
        if (string.IsNullOrWhiteSpace(username))
        {
            AdminUiHelper.ShowInfo("Hãy chọn một user trước.");
            return;
        }

        using var dialog = new UserDialogForm("Đổi mật khẩu user", true, username);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            _service.AlterUserPassword(dialog.Username, dialog.PasswordValue);
            AdminUiHelper.ShowInfo("Đổi mật khẩu thành công.");
            LoadUsers();
        }
        catch (Exception ex)
        {
            AdminUiHelper.ShowError(ex);
        }
    }

     private void AlterUserDefaultTablespace()
    {
        var username = GetSelectedCellValue(_dgvUsers, "USERNAME");
        if (string.IsNullOrWhiteSpace(username))
        {
            AdminUiHelper.ShowInfo("Hãy chọn một user trước.");
            return;
        }

        var current = GetSelectedCellValue(_dgvUsers, "DEFAULT_TABLESPACE");

        var dt = _service.GetTablespaces();

        var list = dt.Rows.Cast<DataRow>()
            .Select(r => Convert.ToString(r["TABLESPACE_NAME"]) ?? "")
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        using var dlg = new SelectValueForm("Chọn Default Tablespace", list, current);

        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            _service.AlterUserDefaultTablespace(username, dlg.SelectedValue);
            LoadUsers();
            AdminUiHelper.ShowInfo("Đổi Default Tablespace thành công.");
        }
        catch (Exception ex)
        {
            AdminUiHelper.ShowError(ex);
        }
    }
    private void AlterUserTemporaryTablespace()
    {
        var username = GetSelectedCellValue(_dgvUsers, "USERNAME");
        if (string.IsNullOrWhiteSpace(username))
        {
            AdminUiHelper.ShowInfo("Hãy chọn một user trước.");
            return;
        }

        var current = GetSelectedCellValue(_dgvUsers, "TEMPORARY_TABLESPACE");

        var dt = _service.GetTablespaces();

        var list = dt.Rows.Cast<DataRow>()
            .Select(r => Convert.ToString(r["TABLESPACE_NAME"]) ?? "")
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        using var dlg = new SelectValueForm("Chọn Temporary Tablespace", list, current);

        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            _service.AlterUserTemporaryTablespace(username, dlg.SelectedValue);
            LoadUsers();
            AdminUiHelper.ShowInfo("Đổi Temporary Tablespace thành công.");
        }
        catch (Exception ex)
        {
            AdminUiHelper.ShowError(ex);
        }
    }
    private void AlterUserProfile()
    {
        var username = GetSelectedCellValue(_dgvUsers, "USERNAME");
        if (string.IsNullOrWhiteSpace(username))
        {
            AdminUiHelper.ShowInfo("Hãy chọn một user trước.");
            return;
        }

        var current = GetSelectedCellValue(_dgvUsers, "PROFILE");

        var dt = _service.GetProfiles();

        var list = dt.Rows.Cast<DataRow>()
            .Select(r => Convert.ToString(r["PROFILE"]) ?? "")
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList();

        using var dlg = new SelectValueForm("Chọn Profile", list, current);

        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            _service.AlterUserProfile(username, dlg.SelectedValue);
            LoadUsers();
            AdminUiHelper.ShowInfo("Đổi Profile thành công.");
        }
        catch (Exception ex)
        {
            AdminUiHelper.ShowError(ex);
        }
    }

    private void ToggleSelectedUserLock()
    {
        var username = GetSelectedCellValue(_dgvUsers, "USERNAME");
        var accountStatus = GetSelectedCellValue(_dgvUsers, "ACCOUNT_STATUS");

        if (string.IsNullOrWhiteSpace(username))
        {
            AdminUiHelper.ShowInfo("Hãy chọn một user trước.");
            return;
        }

        var shouldLock = !(accountStatus?.Contains("LOCKED", StringComparison.OrdinalIgnoreCase) ?? false);

        try
        {
            _service.LockUser(username, shouldLock);
            AdminUiHelper.ShowInfo(shouldLock ? "Đã khóa user." : "Đã mở khóa user.");
            LoadUsers();
        }
        catch (Exception ex)
        {
            AdminUiHelper.ShowError(ex);
        }
    }

    private void DropSelectedUser()
    {
        var username = GetSelectedCellValue(_dgvUsers, "USERNAME");
        if (string.IsNullOrWhiteSpace(username))
        {
            AdminUiHelper.ShowInfo("Hãy chọn một user trước.");
            return;
        }

        if (!AdminUiHelper.Confirm($"Bạn có chắc muốn DROP USER {username} CASCADE không?"))
            return;

        try
        {
            _service.DropUser(username, cascade: true);
            AdminUiHelper.ShowInfo("Đã drop user.");
            RefreshLists();
        }
        catch (Exception ex)
        {
            AdminUiHelper.ShowError(ex);
        }
    }

    private void CreateRole()
    {
        using var dialog = new RoleDialogForm("Tạo role");
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            _service.CreateRole(dialog.RoleName, dialog.PasswordValue);
            AdminUiHelper.ShowInfo("Tạo role thành công.");
            RefreshLists();
        }
        catch (Exception ex)
        {
            AdminUiHelper.ShowError(ex);
        }
    }

    private void AlterRolePassword()
    {
        var roleName = GetSelectedCellValue(_dgvRoles, "ROLE");
        if (string.IsNullOrWhiteSpace(roleName))
        {
            AdminUiHelper.ShowInfo("Hãy chọn một role trước.");
            return;
        }

        using var dialog = new RoleDialogForm("Sửa role", true, roleName);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            _service.AlterRolePassword(dialog.RoleName, dialog.PasswordValue);
            AdminUiHelper.ShowInfo("Cập nhật role thành công.");
            LoadRoles();
        }
        catch (Exception ex)
        {
            AdminUiHelper.ShowError(ex);
        }
    }

    private void DropSelectedRole()
    {
        var roleName = GetSelectedCellValue(_dgvRoles, "ROLE");
        if (string.IsNullOrWhiteSpace(roleName))
        {
            AdminUiHelper.ShowInfo("Hãy chọn một role trước.");
            return;
        }

        if (!AdminUiHelper.Confirm($"Bạn có chắc muốn DROP ROLE {roleName} không?"))
            return;

        try
        {
            _service.DropRole(roleName);
            AdminUiHelper.ShowInfo("Đã drop role.");
            RefreshLists();
        }
        catch (Exception ex)
        {
            AdminUiHelper.ShowError(ex);
        }
    }

    private void ExecuteGrant()
    {
        try
        {
            var principal = _cboGrantPrincipal.SelectedItem?.ToString() ?? "";
            var mode = _cboGrantMode.SelectedItem?.ToString() ?? "";
            var delegable = _chkGrantDelegable.Checked;

            switch (mode)
            {
                case "SYSTEM PRIVILEGE":
                    _service.GrantSystemPrivilege(principal, _cboGrantSystemPriv.SelectedItem?.ToString() ?? "", withAdminOption: false);
                    break;

                case "ROLE":
                    _service.GrantRole(_cboGrantRoleName.SelectedItem?.ToString() ?? "", principal, withAdminOption: false);
                    break;

                case "OBJECT PRIVILEGE":
                    _service.GrantObjectPrivilege(
                        principal,
                        _txtGrantOwner.Text,
                        _cboGrantObjectName.SelectedItem?.ToString() ?? "",
                        _cboGrantObjectType.SelectedItem?.ToString() ?? "",
                        _cboGrantObjectPriv.SelectedItem?.ToString() ?? "",
                        GetCheckedColumns(_clbGrantColumns),
                        delegable);
                    break;
            }

            SelectExplorerPrincipal(_cboGrantPrincipalType.SelectedItem?.ToString(), principal);

            var grantCols = GetCheckedColumns(_clbGrantColumns).ToList();
            var selectViaView = mode == "OBJECT PRIVILEGE"
                && string.Equals(_cboGrantObjectPriv.SelectedItem?.ToString(), "SELECT", StringComparison.OrdinalIgnoreCase)
                && grantCols.Count > 0;

            if (selectViaView)
            {
                AdminUiHelper.ShowInfo(
                    "GRANT thành công (VPD). Các cột không được phép trả về NULL. " +
                    "Xem tab Tra cứu quyền → Column Privileges (SOURCE = VPD).");
            }
            else
            {
                AdminUiHelper.ShowInfo("GRANT thành công.");
            }

            LoadPrivilegeExplorer();
        }
        catch (Exception ex)
        {
            AdminUiHelper.ShowError(ex);
        }
    }

    private void ExecuteRevoke()
    {
        try
        {
            var principal = _cboRevokePrincipal.SelectedItem?.ToString() ?? "";
            var mode = _cboRevokeMode.SelectedItem?.ToString() ?? "";

            switch (mode)
            {
                case "SYSTEM PRIVILEGE":
                    _service.RevokeSystemPrivilege(principal, _cboRevokeSystemPriv.SelectedItem?.ToString() ?? "");
                    break;

                case "ROLE":
                    _service.RevokeRole(_cboRevokeRoleName.SelectedItem?.ToString() ?? "", principal);
                    break;

                case "OBJECT PRIVILEGE":
                    _service.RevokeObjectPrivilege(
                        principal,
                        _txtRevokeOwner.Text,
                        _cboRevokeObjectName.SelectedItem?.ToString() ?? "",
                        _cboRevokeObjectType.SelectedItem?.ToString() ?? "",
                        _cboRevokeObjectPriv.SelectedItem?.ToString() ?? "",
                        GetCheckedColumns(_clbRevokeColumns));
                    break;
            }

            SelectExplorerPrincipal(_cboRevokePrincipalType.SelectedItem?.ToString(), principal);

            AdminUiHelper.ShowInfo("REVOKE thành công.");
            LoadPrivilegeExplorer();
        }
        catch (Exception ex)
        {
            AdminUiHelper.ShowError(ex);
        }
    }

    private void SelectExplorerPrincipal(string? principalType, string? principal)
    {
        if (string.IsNullOrWhiteSpace(principal))
            return;

        if (!string.IsNullOrWhiteSpace(principalType))
        {
            var ti = _cboExplorerPrincipalType.Items.IndexOf(principalType);
            if (ti >= 0)
                _cboExplorerPrincipalType.SelectedIndex = ti;
        }

        RefreshPrincipalCombos(_cboExplorerPrincipalType, _cboExplorerPrincipal);
        var pi = _cboExplorerPrincipal.Items.IndexOf(principal);
        if (pi >= 0)
            _cboExplorerPrincipal.SelectedIndex = pi;
    }

    private void LoadPrivilegeExplorer()
    {
        try
        {
            var principal = _cboExplorerPrincipal.SelectedItem?.ToString() ?? "";
            if (string.IsNullOrWhiteSpace(principal))
            {
                AdminUiHelper.ShowInfo("Hãy chọn principal để xem quyền.");
                return;
            }

            _dgvSysPrivs.DataSource = _service.GetPrincipalSystemPrivileges(principal);
            _dgvRolePrivs.DataSource = _service.GetPrincipalRoleGrants(principal);
            _dgvObjPrivs.DataSource = _service.GetPrincipalObjectPrivileges(principal);
            _dgvColPrivs.DataSource = _service.GetPrincipalColumnPrivileges(principal);
        }
        catch (Exception ex)
        {
            AdminUiHelper.ShowError(ex);
        }
    }

    private void RefreshPrincipalCombos(ComboBox typeCombo, ComboBox targetCombo)
    {
        try
        {
            var type = typeCombo.SelectedItem?.ToString();
            var values = type == "ROLE" ? _service.GetRoleNames() : _service.GetUserNames();
            FillCombo(targetCombo, values);
        }
        catch (Exception ex)
        {
            AdminUiHelper.ShowError(ex);
        }
    }

    private void ToggleGrantPanels()
    {
        var mode = _cboGrantMode.SelectedItem?.ToString();
        _grantSystemPanel.Visible = mode == "SYSTEM PRIVILEGE";
        _grantRolePanel.Visible = mode == "ROLE";
        _grantObjectPanel.Visible = mode == "OBJECT PRIVILEGE";

        _chkGrantDelegable.Visible = mode == "OBJECT PRIVILEGE";
        if (mode != "OBJECT PRIVILEGE") _chkGrantDelegable.Checked = false;
    }

    private void ToggleRevokePanels()
    {
        _revokeSystemPanel.Visible = _cboRevokeMode.SelectedItem?.ToString() == "SYSTEM PRIVILEGE";
        _revokeRolePanel.Visible = _cboRevokeMode.SelectedItem?.ToString() == "ROLE";
        _revokeObjectPanel.Visible = _cboRevokeMode.SelectedItem?.ToString() == "OBJECT PRIVILEGE";
    }

    private void ToggleGrantColumnChooser()
    {
        var privilege = _cboGrantObjectPriv.SelectedItem?.ToString();
        var objectType = _cboGrantObjectType.SelectedItem?.ToString();
        _clbGrantColumns.Enabled = (objectType is "TABLE" or "VIEW") && (privilege is "SELECT" or "UPDATE");
        if (!_clbGrantColumns.Enabled)
        {
            _clbGrantColumns.Items.Clear();
            ClearChecks(_clbGrantColumns);
            return;
        }

        LoadColumns(_txtGrantOwner.Text, _cboGrantObjectName.SelectedItem?.ToString() ?? "", _clbGrantColumns);
    }

    private void ToggleRevokeColumnChooser()
    {
        var privilege = _cboRevokeObjectPriv.SelectedItem?.ToString();
        var objectType = _cboRevokeObjectType.SelectedItem?.ToString();
        _clbRevokeColumns.Enabled = (objectType is "TABLE" or "VIEW") && (privilege is "SELECT" or "UPDATE");
        if (!_clbRevokeColumns.Enabled)
        {
            _clbRevokeColumns.Items.Clear();
            ClearChecks(_clbRevokeColumns);
            return;
        }

        LoadColumns(_txtRevokeOwner.Text, _cboRevokeObjectName.SelectedItem?.ToString() ?? "", _clbRevokeColumns);
    }

    private void ToggleGrantObjectPrivilegeSource()
    {
        var objectType = _cboGrantObjectType.SelectedItem?.ToString();
        FillCombo(_cboGrantObjectPriv,
            objectType is "PROCEDURE" or "FUNCTION"
                ? OracleAdminService.ObjectPrivilegesForProgramUnit
                : OracleAdminService.ObjectPrivilegesForTableOrView);
        RefreshGrantObjectNames();
        ToggleGrantColumnChooser();
    }

    private void ToggleRevokeObjectPrivilegeSource()
    {
        var objectType = _cboRevokeObjectType.SelectedItem?.ToString();
        FillCombo(_cboRevokeObjectPriv,
            objectType is "PROCEDURE" or "FUNCTION"
                ? OracleAdminService.ObjectPrivilegesForProgramUnit
                : OracleAdminService.ObjectPrivilegesForTableOrView);
        RefreshRevokeObjectNames();
        ToggleRevokeColumnChooser();
    }

    private void RefreshGrantObjectNames()
    {
        FillCombo(_cboGrantObjectName, GetObjectNames(_txtGrantOwner.Text, _cboGrantObjectType.SelectedItem?.ToString()));
    }

    private void RefreshRevokeObjectNames()
    {
        FillCombo(_cboRevokeObjectName, GetObjectNames(_txtRevokeOwner.Text, _cboRevokeObjectType.SelectedItem?.ToString()));
    }

    private IEnumerable<string> GetObjectNames(string owner, string? objectType)
    {
        if (string.IsNullOrWhiteSpace(owner))
        {
            return [];
        }

        try
        {
            var table = _service.GetManagedObjects(owner);
            if (!table.Columns.Contains("OBJECT_NAME"))
            {
                return [];
            }

            return table.Rows.Cast<DataRow>()
                .Where(row => string.IsNullOrWhiteSpace(objectType)
                    || string.Equals(row["OBJECT_TYPE"]?.ToString(), objectType, StringComparison.OrdinalIgnoreCase))
                .Select(row => row["OBJECT_NAME"]?.ToString() ?? string.Empty)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name)
                .ToList();
        }
        catch
        {
            return [];
        }
    }

    private void LoadColumns(string owner, string objectName, CheckedListBox target)
    {
        if (string.IsNullOrWhiteSpace(owner) || string.IsNullOrWhiteSpace(objectName))
        {
            target.Items.Clear();
            return;
        }

        try
        {
            var columns = _service.GetColumns(owner, objectName);
            target.Items.Clear();
            foreach (var column in columns)
            {
                target.Items.Add(column, false);
            }
        }
        catch (Exception ex)
        {
            AdminUiHelper.ShowError(ex);
        }
    }

    private static IEnumerable<string> GetCheckedColumns(CheckedListBox listBox)
    {
        return listBox.CheckedItems.Cast<object>().Select(x => x.ToString() ?? "");
    }

    private static void FillCombo(ComboBox combo, IEnumerable<string> values)
    {
        var list = values.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        combo.BeginUpdate();
        combo.Items.Clear();
        combo.Items.AddRange(list.Cast<object>().ToArray());
        if (combo.Items.Count > 0)
        {
            combo.SelectedIndex = 0;
        }
        combo.EndUpdate();
    }

    private static string? GetSelectedCellValue(DataGridView grid, string columnName)
    {
        if (grid.CurrentRow?.DataBoundItem is DataRowView rowView &&
            rowView.Row.Table.Columns.Contains(columnName))
        {
            return rowView.Row[columnName]?.ToString();
        }

        return null;
    }

    private static DataGridView CreateGrid()
    {
        return new DataGridView
        {
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            MultiSelect = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            RowHeadersVisible = false,
            BackgroundColor = Color.White
        };
    }

    private static TabPage CreateGridTab(string title, DataGridView grid)
    {
        var page = new TabPage(title);
        grid.Dock = DockStyle.Fill;
        page.Controls.Add(grid);
        return page;
    }

    private static TableLayoutPanel SimpleTwoColumnLayout()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16, 8, 16, 8),
            ColumnCount = 2
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        return layout;
    }

    private static void AddField(TableLayoutPanel layout, int rowIndex, string label, Control control, int labelColumn = 0, int valueColumn = 1)
    {
        while (layout.RowStyles.Count <= rowIndex)
        {
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        }

        layout.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left }, labelColumn, rowIndex);
        control.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        layout.Controls.Add(control, valueColumn, rowIndex);
    }

    private static void AddHeaderField(TableLayoutPanel layout, int rowIndex, string leftLabel, Control leftControl, string rightLabel, Control rightControl)
    {
        while (layout.RowStyles.Count <= rowIndex)
        {
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        }

        layout.Controls.Add(new Label { Text = leftLabel, AutoSize = true, Anchor = AnchorStyles.Left }, 0, rowIndex);
        leftControl.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        layout.Controls.Add(leftControl, 1, rowIndex);

        if (!string.IsNullOrWhiteSpace(rightLabel))
        {
            layout.Controls.Add(new Label { Text = rightLabel, AutoSize = true, Anchor = AnchorStyles.Left }, 2, rowIndex);
            rightControl.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            layout.Controls.Add(rightControl, 3, rowIndex);
        }
    }

    private static void ClearChecks(CheckedListBox listBox)
    {
        for (var i = 0; i < listBox.Items.Count; i++)
        {
            listBox.SetItemChecked(i, false);
        }
    }
}
