using bai4._7;
using System.ComponentModel;

namespace Bai47_MDI;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new FormMain());
    }
}

public class FormMain : Form
{
    private readonly ToolStripStatusLabel statusUser = new("Người dùng: Admin");
    private readonly ToolStripStatusLabel statusMessage = new("Sẵn sàng");
    private readonly ToolStripStatusLabel statusClock = new();
    public FormMain()
    {
        Text = "Bài 4.7 - Hệ thống quản lý MDI";
        IsMdiContainer = true;
        WindowState = FormWindowState.Maximized;
        var menu = new MenuStrip();
        var system = new ToolStripMenuItem("Hệ thống");
        system.DropDownItems.Add("Đăng xuất", null, (_, _) => { UpdateStatus("Đã đăng xuất"); MessageBox.Show("Bạn có thể bổ sung màn hình đăng nhập tại đây."); });
        system.DropDownItems.Add("Thoát", null, (_, _) => Close());
        var catalog = new ToolStripMenuItem("Danh mục");
        catalog.DropDownItems.Add("Quản lý Sinh viên", null, (_, _) => OpenChild<FormStudent>());
        catalog.DropDownItems.Add("Quản lý Sản phẩm", null, (_, _) => OpenChild<FormProduct>());
        var windows = new ToolStripMenuItem("Cửa sổ");
        windows.DropDownItems.Add("Cascade", null, (_, _) => LayoutMdi(MdiLayout.Cascade));
        windows.DropDownItems.Add("Tile Horizontal", null, (_, _) => LayoutMdi(MdiLayout.TileHorizontal));
        windows.DropDownItems.Add("Tile Vertical", null, (_, _) => LayoutMdi(MdiLayout.TileVertical));
        menu.Items.AddRange(new ToolStripItem[] { system, catalog, windows });
        MainMenuStrip = menu; Controls.Add(menu);

        var tool = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden };
        tool.Items.Add("Sinh viên", null, (_, _) => OpenChild<FormStudent>());
        tool.Items.Add("Sản phẩm", null, (_, _) => OpenChild<FormProduct>());
        Controls.Add(tool);

        var status = new StatusStrip();
        status.Items.Add(statusUser); status.Items.Add(new ToolStripStatusLabel { Spring = true }); status.Items.Add(statusMessage); status.Items.Add(new ToolStripStatusLabel { Spring = true }); status.Items.Add(statusClock);
        Controls.Add(status);
        var timer = new System.Windows.Forms.Timer { Interval = 1000 };
        timer.Tick += (_, _) => statusClock.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        timer.Start();
        MainMenuStrip.BringToFront();
    }

    public void UpdateStatus(string message) => statusMessage.Text = message;

    private void OpenChild<T>() where T : Form, new()
    {
        foreach (Form child in MdiChildren)
        {
            if (child is T)
            {
                if (child.WindowState == FormWindowState.Minimized) child.WindowState = FormWindowState.Normal;
                child.Activate();
                return;
            }
        }
        var form = new T { MdiParent = this };
        form.FormClosed += (_, _) => UpdateStatus($"Đã đóng {form.Text}");
        form.Show();
        UpdateStatus($"Đã mở {form.Text}");
    }
}

public class FormStudent : Form
{
    private readonly DataGridView grid = new();
    private readonly BindingSource source = new();
    private readonly BindingList<StudentRow> data = new();
    public FormStudent()
    {
        Text = "Quản lý Sinh viên"; Width = 700; Height = 420;
        data.Add(new("SV001", "Nguyễn Văn An", 20));
        data.Add(new("SV002", "Trần Thị Bình", 19));
        source.DataSource = data;
        grid.Dock = DockStyle.Fill; grid.DataSource = source; grid.ReadOnly = true;
        grid.AllowUserToAddRows = false; grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        Controls.Add(grid);
        var context = new ContextMenuStrip();
        context.Items.Add("Xóa dòng", null, (_, _) => DeleteRow());
        context.Items.Add("Làm mới dữ liệu", null, (_, _) => { source.ResetBindings(false); Notify("Đã làm mới danh sách sinh viên"); });
        context.Items.Add("Xem chi tiết", null, (_, _) => ShowDetails());
        grid.ContextMenuStrip = context;
        grid.MouseDown += (_, e) => { if (e.Button == MouseButtons.Right) { var hit = grid.HitTest(e.X, e.Y); if (hit.RowIndex >= 0) { grid.ClearSelection(); grid.Rows[hit.RowIndex].Selected = true; source.Position = hit.RowIndex; } } };
        var bottom = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 42 };
        var add = new Button { Text = "Thêm mẫu", AutoSize = true };
        add.Click += (_, _) => { data.Add(new StudentRow("SV" + (data.Count + 1).ToString("000"), "Sinh viên mới", 18)); Notify("Đã thêm sinh viên"); };
        var delete = new Button { Text = "Xóa dòng", AutoSize = true }; delete.Click += (_, _) => DeleteRow();
        bottom.Controls.Add(add); bottom.Controls.Add(delete); Controls.Add(bottom);
    }
    private void Notify(string message) { if (MdiParent is FormMain main) main.UpdateStatus(message); }
    private void DeleteRow()
    {
        if (source.Current is StudentRow row) { data.Remove(row); Notify("Đã xóa sinh viên"); }
    }
    private void ShowDetails()
    {
        if (source.Current is StudentRow row) MessageBox.Show($"Mã SV: {row.StudentId}\nHọ tên: {row.FullName}\nTuổi: {row.Age}", "Chi tiết sinh viên");
    }
}
public record StudentRow(string StudentId, string FullName, int Age);

public class FormProduct : Form
{
    public FormProduct()
    {
        Text = "Quản lý Sản phẩm"; Width = 550; Height = 300;
        var grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
        grid.DataSource = new BindingList<ProductRow>
        {
            new("SP001", "Bàn phím", 450000),
            new("SP002", "Chuột máy tính", 250000),
            new("SP003", "Tai nghe", 600000)
        };
        Controls.Add(grid);
    }
}
public record ProductRow(string ProductId, string ProductName, decimal Price);