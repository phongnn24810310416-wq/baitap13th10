using bai4._8;
using System.ComponentModel;

namespace Bai48_AsyncProgress;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new FormMain());
    }
}

public class Student
{
    public string StudentId { get; set; } = "";
    public string FullName { get; set; } = "";
    public int Age { get; set; }
    public string Gender { get; set; } = "";
}

public class FormMain : Form
{
    private readonly DataGridView grid = new();
    private readonly ProgressBar progressBar = new();
    private readonly Label lblPercent = new();
    private readonly Button btnLoad = new();
    private readonly ToolStripStatusLabel status = new("Sẵn sàng");
    private readonly BindingSource source = new();
    private readonly BindingList<Student> students = new();

    public FormMain()
    {
        Text = "Bài 4.8 - Nạp dữ liệu bất đồng bộ";
        Width = 850; Height = 550; StartPosition = FormStartPosition.CenterScreen;
        var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(10) };
        btnLoad.Text = "Nạp dữ liệu"; btnLoad.AutoSize = true;
        btnLoad.Click += async (_, _) => await LoadStudentsAsync();
        top.Controls.Add(btnLoad);
        progressBar.Width = 420; progressBar.Height = 24; progressBar.Minimum = 0; progressBar.Maximum = 100;
        top.Controls.Add(progressBar);
        lblPercent.Text = "0%"; lblPercent.AutoSize = true; lblPercent.Padding = new Padding(0, 8, 0, 0);
        top.Controls.Add(lblPercent);
        Controls.Add(top);
        grid.Dock = DockStyle.Fill; grid.ReadOnly = true; grid.AllowUserToAddRows = false;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        source.DataSource = students; grid.DataSource = source; Controls.Add(grid);
        var strip = new StatusStrip(); strip.Items.Add(status); Controls.Add(strip);
        // Dữ liệu mẫu để người dùng thấy giao diện trước khi nhấn Nạp dữ liệu.
        students.Add(new Student { StudentId = "SV000", FullName = "Nhấn Nạp dữ liệu để bắt đầu", Age = 18, Gender = "Nam" });
    }

    private async Task LoadStudentsAsync()
    {
        btnLoad.Enabled = false;
        progressBar.Value = 0;
        lblPercent.Text = "0%";
        status.Text = "Đang nạp dữ liệu...";
        try
        {
            var progress = new Progress<int>(percent =>
            {
                int safe = Math.Max(0, Math.Min(100, percent));
                progressBar.Value = safe;
                lblPercent.Text = safe + "%";
                status.Text = $"Đang nạp dữ liệu: {safe}%";
            });

            List<Student> loaded = await LoadDataInBackgroundAsync(progress);
            students.Clear();
            foreach (var student in loaded) students.Add(student);
            source.ResetBindings(false);
            progressBar.Value = 100;
            lblPercent.Text = "100%";
            status.Text = $"Hoàn tất: đã nạp {loaded.Count} sinh viên.";
            MessageBox.Show($"Nạp dữ liệu thành công: {loaded.Count} sinh viên.", "Thông báo");
        }
        catch (Exception ex)
        {
            status.Text = "Nạp dữ liệu thất bại.";
            MessageBox.Show("Có lỗi khi nạp dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnLoad.Enabled = true;
        }
    }

    private static Task<List<Student>> LoadDataInBackgroundAsync(IProgress<int> progress)
    {
        return Task.Run(async () =>
        {
            var result = new List<Student>();
            const int total = 100;
            for (int i = 1; i <= total; i++)
            {
                await Task.Delay(50); // Giả lập đọc dữ liệu chậm từ CSDL hoặc file.
                result.Add(new Student
                {
                    StudentId = $"SV{i:000}",
                    FullName = $"Sinh viên {i:000}",
                    Age = 18 + (i % 8),
                    Gender = i % 2 == 0 ? "Nam" : "Nữ"
                });
                progress.Report(i); // Progress<T> tự chuyển callback về UI thread.
            }
            return result;
        });
    }
}