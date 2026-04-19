using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BindingListThreadPitfallDemo;

public sealed class MainForm : Form
{
    private readonly BindingList<DemoRow> _rows = new();
    private readonly BindingSource _bindingSource = new();
    private int _nextNo;

    private readonly DataGridView _grid = new();
    private readonly Button _badButton = new();
    private readonly Button _goodButton = new();
    private readonly Button _clearButton = new();
    private readonly TextBox _output = new();
    private readonly Label _title = new();
    private readonly Label _hint = new();

    public MainForm()
    {
        Text = "BindingList 跨线程绑定坑演示";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(980, 640);

        BuildLayout();
        BindData();
    }

    private void BuildLayout()
    {
        _title.AutoSize = true;
        _title.Font = new Font(Font.FontFamily, 15F, FontStyle.Bold);
        _title.Text = "BindingList<T> 并不会自动切回 UI 线程";

        _hint.AutoSize = true;
        _hint.MaximumSize = new Size(900, 0);
        _hint.Text = "先点“错误示例”：后台线程直接 _rows.Add(...)，BindingList 会在后台线程触发 ListChanged，"
            + "已绑定的 DataGridView 会被迫在错误线程刷新，常见异常是“从不是创建控件的线程访问它”。"
            + "再点“正确示例”：先回到 UI 线程，再修改 BindingList。";

        _badButton.AutoSize = true;
        _badButton.Text = "错误示例：后台线程直接 Add";
        _badButton.Click += BadButton_Click;

        _goodButton.AutoSize = true;
        _goodButton.Text = "正确示例：Invoke 后再 Add";
        _goodButton.Click += GoodButton_Click;

        _clearButton.AutoSize = true;
        _clearButton.Text = "清空";
        _clearButton.Click += (_, _) =>
        {
            _rows.Clear();
            _output.Clear();
            _nextNo = 0;
        };

        _grid.Dock = DockStyle.Fill;
        _grid.AutoGenerateColumns = false;
        _grid.AllowUserToAddRows = false;
        _grid.ReadOnly = true;
        _grid.RowHeadersVisible = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "序号",
            DataPropertyName = nameof(DemoRow.No),
            Width = 80
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "消息",
            DataPropertyName = nameof(DemoRow.Message),
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "创建线程",
            DataPropertyName = nameof(DemoRow.CreatedOnThreadId),
            Width = 110
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "创建时间",
            DataPropertyName = nameof(DemoRow.CreatedAt),
            Width = 170,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "HH:mm:ss.fff" }
        });

        _output.Dock = DockStyle.Fill;
        _output.Multiline = true;
        _output.ReadOnly = true;
        _output.ScrollBars = ScrollBars.Vertical;
        _output.Font = new Font("Consolas", 10F);

        var buttons = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Dock = DockStyle.Top,
            Padding = new Padding(0, 8, 0, 8)
        };
        buttons.Controls.AddRange(new Control[] { _badButton, _goodButton, _clearButton });

        var top = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = 1,
            Dock = DockStyle.Top,
            Padding = new Padding(12)
        };
        top.Controls.Add(_title);
        top.Controls.Add(_hint);
        top.Controls.Add(buttons);

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal
        };
        split.Panel1.Controls.Add(_grid);
        split.Panel2.Controls.Add(_output);

        Controls.Add(split);
        Controls.Add(top);
    }

    private void BindData()
    {
        _bindingSource.DataSource = _rows;
        _grid.DataSource = _bindingSource;

        Log($"UI 线程 ID: {Environment.CurrentManagedThreadId}");
        Log("BindingList 已绑定到 DataGridView。");
    }

    private async void BadButton_Click(object? sender, EventArgs e)
    {
        _badButton.Enabled = false;
        Log("启动错误示例：Task.Run 后直接修改 BindingList。");

        try
        {
            await Task.Run(() =>
            {
                Thread.Sleep(300);

                var row = CreateRow("这是从后台线程直接 Add 的数据");
                LogFromAnyThread($"后台线程 ID: {Environment.CurrentManagedThreadId}");

                // Pitfall: BindingList raises ListChanged on this background thread.
                // The bound DataGridView then tries to refresh itself on the wrong thread.
                _rows.Add(row);
            });
        }
        catch (Exception ex)
        {
            Log("捕获到异常：");
            Log(ex.GetType().FullName ?? ex.GetType().Name);
            Log(ex.Message);

            MessageBox.Show(
                this,
                ex.Message,
                "BindingList 跨线程更新异常",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            _badButton.Enabled = true;
        }
    }

    private async void GoodButton_Click(object? sender, EventArgs e)
    {
        _goodButton.Enabled = false;
        Log("启动正确示例：后台线程准备数据，回到 UI 线程后再修改 BindingList。");

        try
        {
            var row = await Task.Run(() =>
            {
                Thread.Sleep(300);
                LogFromAnyThread($"后台线程 ID: {Environment.CurrentManagedThreadId}");
                return CreateRow("这是先切回 UI 线程后 Add 的数据");
            });

            AddRowOnUiThread(row);
            Log("添加成功：ListChanged 在 UI 线程触发，DataGridView 可以安全刷新。");
        }
        finally
        {
            _goodButton.Enabled = true;
        }
    }

    private DemoRow CreateRow(string message)
    {
        return new DemoRow
        {
            No = Interlocked.Increment(ref _nextNo),
            Message = message,
            CreatedOnThreadId = Environment.CurrentManagedThreadId,
            CreatedAt = DateTime.Now
        };
    }

    private void AddRowOnUiThread(DemoRow row)
    {
        if (InvokeRequired)
        {
            BeginInvoke((Action)(() => _rows.Add(row)));
            return;
        }

        _rows.Add(row);
    }

    private void Log(string message)
    {
        _output.AppendText($"[{DateTime.Now:HH:mm:ss.fff}] {message}{Environment.NewLine}");
    }

    private void LogFromAnyThread(string message)
    {
        if (InvokeRequired)
        {
            BeginInvoke((Action)(() => Log(message)));
            return;
        }

        Log(message);
    }
}
