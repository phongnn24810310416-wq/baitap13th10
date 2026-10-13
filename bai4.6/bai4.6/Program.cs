using bai4._6;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace Bai46_QuanLySinhVien;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new FormStudents());
    }
}

public class Student
{
    public string StudentId { get; set; } = "";
    public string FullName { get; set; } = "";
    public int Age { get; set; }
    public string Gender { get; set; } = "Nam";
    public string AvatarPath { get; set; } = "";
}

public class FormStudents : Form
{
    private readonly BindingList<Student> students = new();
    private readonly BindingSource source = new();
    private readonly DataGridView grid = new();
    private readonly TextBox txtId = new();
    private readonly TextBox txtName = new();
    private readonly TextBox txtAge = new();
    private readonly ComboBox cboGender = new();
    private readonly TextBox txtAvatar = new();
    private readonly PictureBox picAvatar = new();
    private readonly Button btnChoose = new();
    private readonly Button btnAdd = new();
    private readonly Button btnUpdate = new();
    private readonly Button btnDelete = new();
    private readonly Button btnCsv = new();

    public FormStudents()
    {
        Text = "Bài 4.6 - Quản lý sinh viên";
        StartPosition = FormStartPosition.CenterScreen;
        Width = 1050; Height = 650; MinimumSize = new Size(900, 560);
        BuildUi();
        source.DataSource = students;
        grid.DataSource = source;
        BindInputs();
        grid.SelectionChanged += (_, _) => ShowAvatarForCurrent();
        source.CurrentChanged += (_, _) => ShowAvatarForCurrent();
        students.Add(new Student { StudentId = "SV001", FullName = "Nguyễn Văn An", Age = 20, Gender = "Nam" });
        students.Add(new Student { StudentId = "SV002", FullName = "Trần Thị Bình", Age = 19, Gender = "Nữ" });
        source.ResetBindings(false);
        if (grid.Rows.Count > 0) grid.Rows[0].Selected = true;
        ShowAvatarForCurrent();
    }

    private void BuildUi()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(10) };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
        Controls.Add(root);

        grid.Dock = DockStyle.Fill;
        grid.ReadOnly = true;
        grid.AllowUserToAddRows = false;
        grid.MultiSelect = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.RowHeadersVisible = false;
        root.Controls.Add(grid, 0, 0);

        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 9, Padding = new Padding(12, 0, 0, 0) };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 66));
        for (int i = 0; i < 5; i++) panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 155));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.Controls.Add(panel, 1, 0);

        AddField(panel, "Mã sinh viên", txtId, 0);
        AddField(panel, "Họ và tên", txtName, 1);
        AddField(panel, "Tuổi", txtAge, 2);
        cboGender.DropDownStyle = ComboBoxStyle.DropDownList;
        cboGender.Items.AddRange(new object[] { "Nam", "Nữ" });
        cboGender.SelectedIndex = 0;
        AddField(panel, "Giới tính", cboGender, 3);
        txtAvatar.ReadOnly = true;
        AddField(panel, "Đường dẫn ảnh", txtAvatar, 4);

        picAvatar.Dock = DockStyle.Fill;
        picAvatar.BorderStyle = BorderStyle.FixedSingle;
        picAvatar.SizeMode = PictureBoxSizeMode.Zoom;
        panel.Controls.Add(new Label { Text = "Avatar", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 5);
        panel.Controls.Add(picAvatar, 1, 5);

        btnChoose.Text = "Chọn ảnh...";
        btnChoose.Dock = DockStyle.Fill;
        btnChoose.Click += ChooseAvatar;
        panel.Controls.Add(btnChoose, 1, 6);

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = true };
        btnAdd.Text = "Thêm"; btnUpdate.Text = "Cập nhật"; btnDelete.Text = "Xóa"; btnCsv.Text = "Xuất CSV";
        foreach (var b in new[] { btnAdd, btnUpdate, btnDelete, btnCsv }) { b.AutoSize = true; b.Height = 32; buttons.Controls.Add(b); }
        panel.Controls.Add(new Label { Text = "Chức năng", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 7);
        panel.Controls.Add(buttons, 1, 7);
        btnAdd.Click += AddStudent;
        btnUpdate.Click += UpdateStudent;
        btnDelete.Click += DeleteStudent;
        btnCsv.Click += ExportCsv;
    }

    private static void AddField(TableLayoutPanel panel, string label, Control control, int row)
    {
        panel.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, row);
        control.Dock = DockStyle.Fill;
        panel.Controls.Add(control, 1, row);
    }

    private void BindInputs()
    {
        txtId.DataBindings.Add("Text", source, nameof(Student.StudentId), true, DataSourceUpdateMode.OnPropertyChanged);
        txtName.DataBindings.Add("Text", source, nameof(Student.FullName), true, DataSourceUpdateMode.OnPropertyChanged);
        txtAge.DataBindings.Add("Text", source, nameof(Student.Age), true, DataSourceUpdateMode.OnPropertyChanged);
        cboGender.DataBindings.Add("SelectedItem", source, nameof(Student.Gender), true, DataSourceUpdateMode.OnPropertyChanged);
        txtAvatar.DataBindings.Add("Text", source, nameof(Student.AvatarPath), true, DataSourceUpdateMode.OnPropertyChanged);
    }

    private Student? CurrentStudent => source.Current as Student;

    private void AddStudent(object? sender, EventArgs e)
    {
        if (!TryReadInputs(out var student)) return;
        if (students.Any(s => s.StudentId.Equals(student.StudentId, StringComparison.OrdinalIgnoreCase)))
        { MessageBox.Show("Mã sinh viên đã tồn tại."); return; }
        students.Add(student);
        source.Position = students.Count - 1;
        source.ResetBindings(false);
        ShowAvatarForCurrent();
        MessageBox.Show("Đã thêm sinh viên.");
    }

    private bool TryReadInputs(out Student student)
    {
        student = new Student();
        string id = txtId.Text.Trim(), name = txtName.Text.Trim();
        if (id.Length == 0 || name.Length == 0)
        { MessageBox.Show("Vui lòng nhập mã sinh viên và họ tên."); return false; }
        if (!int.TryParse(txtAge.Text.Trim(), out int age) || age < 1 || age > 120)
        { MessageBox.Show("Tuổi phải là số nguyên từ 1 đến 120."); return false; }
        student = new Student { StudentId = id, FullName = name, Age = age, Gender = cboGender.Text, AvatarPath = txtAvatar.Text };
        return true;
    }

    private void UpdateStudent(object? sender, EventArgs e)
    {
        var current = CurrentStudent;
        if (current == null) { MessageBox.Show("Hãy chọn sinh viên cần cập nhật."); return; }
        if (!TryReadInputs(out var edited)) return;
        if (students.Any(s => !ReferenceEquals(s, current) && s.StudentId.Equals(edited.StudentId, StringComparison.OrdinalIgnoreCase)))
        { MessageBox.Show("Mã sinh viên đã tồn tại."); return; }
        current.StudentId = edited.StudentId; current.FullName = edited.FullName;
        current.Age = edited.Age; current.Gender = edited.Gender; current.AvatarPath = edited.AvatarPath;
        source.ResetCurrentItem();
        ShowAvatarForCurrent();
        MessageBox.Show("Đã cập nhật sinh viên.");
    }

    private void DeleteStudent(object? sender, EventArgs e)
    {
        var current = CurrentStudent;
        if (current == null) return;
        if (MessageBox.Show($"Xóa sinh viên {current.FullName}?", "Xác nhận", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
        int oldPosition = source.Position;
        students.Remove(current);
        if (students.Count > 0) source.Position = Math.Min(oldPosition, students.Count - 1);
        source.ResetBindings(false);
        ShowAvatarForCurrent();
    }

    private void ChooseAvatar(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog { Title = "Chọn ảnh đại diện", Filter = "Tệp hình ảnh|*.jpg;*.jpeg;*.png;*.bmp|Tất cả tệp|*.*" };
        if (dialog.ShowDialog() != DialogResult.OK) return;
        txtAvatar.Text = dialog.FileName;
        if (CurrentStudent != null) CurrentStudent.AvatarPath = dialog.FileName;
        LoadAvatar(dialog.FileName);
        source.ResetCurrentItem();
    }

    private void ShowAvatarForCurrent() => LoadAvatar(CurrentStudent?.AvatarPath ?? "");

    private void LoadAvatar(string path)
    {
        Image? old = picAvatar.Image;
        picAvatar.Image = null;
        old?.Dispose();
        if (File.Exists(path))
        {
            try { using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite); picAvatar.Image = Image.FromStream(stream).Clone() as Image; }
            catch { picAvatar.Image = null; }
        }
    }

    private void ExportCsv(object? sender, EventArgs e)
    {
        using var dialog = new SaveFileDialog { Title = "Xuất danh sách sinh viên", Filter = "CSV (*.csv)|*.csv", FileName = "DanhSachSinhVien.csv", DefaultExt = "csv", AddExtension = true };
        if (dialog.ShowDialog() != DialogResult.OK) return;
        try
        {
            using var writer = new StreamWriter(dialog.FileName, false, new UTF8Encoding(true));
            writer.WriteLine("MaSV,HoTen,Tuoi,GioiTinh,AvatarPath");
            foreach (var s in students)
                writer.WriteLine(string.Join(",", new[] { s.StudentId, s.FullName, s.Age.ToString(CultureInfo.InvariantCulture), s.Gender, s.AvatarPath }.Select(EscapeCsv)));
            MessageBox.Show("Xuất CSV thành công.");
        }
        catch (Exception ex) { MessageBox.Show("Không thể xuất file: " + ex.Message); }
    }

    private static string EscapeCsv(string value) => "\"" + (value ?? "").Replace("\"", "\"\"") + "\"";
}