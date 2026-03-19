using System.Drawing;
using System.Windows.Forms;

namespace CrashTest.Unified;

public sealed class CrashSelectorForm : Form
{
    /// <summary>
    /// 初始化崩溃选择窗体并构建界面。
    /// </summary>
    public CrashSelectorForm()
    {
        InitializeComponents();
    }

    /// <summary>
    /// 创建窗体布局、崩溃按钮和底部提示信息。
    /// </summary>
    private void InitializeComponents()
    {
        Text = "CrashTest Unified - 选择崩溃类型";
        Size = new Size(860, 640);
        MinimumSize = new Size(760, 520);
        StartPosition = FormStartPosition.CenterScreen;

        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(12)
        };
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));

        var title = new Label
        {
            Text = "WinForms 闪退测试套件",
            Font = new Font("Microsoft YaHei UI", 16, FontStyle.Bold),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        };
        mainLayout.Controls.Add(title, 0, 0);

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            Padding = new Padding(4)
        };

        foreach (var (key, info) in CrashTrigger.GetAvailableCrashes())
        {
            var button = new Button
            {
                Text = $"{key.ToUpperInvariant()}\r\n{info.Name}\r\n{info.Description}",
                Size = new Size(250, 96),
                Margin = new Padding(6),
                Font = new Font("Microsoft YaHei UI", 9),
                BackColor = GetColorForCrashType(key),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                UseVisualStyleBackColor = false
            };

            button.FlatAppearance.BorderSize = 0;
            button.Click += (_, _) =>
            {
                var exitCodeText = info.ExitCode.HasValue
                    ? $"0x{info.ExitCode.Value:X8}"
                    : "未固定 / 依运行时行为决定";

                var result = MessageBox.Show(
                    $"确定要触发 [{key.ToUpperInvariant()}] {info.Name} ?\n\n" +
                    $"描述: {info.Description}\n" +
                    $"预期退出码: {exitCodeText}\n\n" +
                    "这会让当前进程异常退出或卡死。",
                    "确认崩溃测试",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    CrashTrigger.Execute(key);
                }
            };

            var toolTip = new ToolTip();
            toolTip.SetToolTip(button, $"命令行参数: {key}");
            buttonPanel.Controls.Add(button);
        }

        mainLayout.Controls.Add(buttonPanel, 0, 1);

        var infoLabel = new Label
        {
            Text = "也可以通过命令行参数触发，例如: CrashTest.Unified.exe av",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.DimGray
        };
        mainLayout.Controls.Add(infoLabel, 0, 2);

        Controls.Add(mainLayout);
    }

    /// <summary>
    /// 为不同崩溃类型返回固定颜色，便于界面上区分。
    /// </summary>
    /// <param name="key">崩溃类型缩写。</param>
    /// <returns>对应按钮颜色。</returns>
    private static Color GetColorForCrashType(string key) => key switch
    {
        "av" => Color.Firebrick,
        "so" => Color.DarkOrange,
        "hc" => Color.MediumPurple,
        "dz" => Color.RoyalBlue,
        "nr" => Color.Teal,
        "oom" => Color.SeaGreen,
        "dl" => Color.Goldenrod,
        "md" => Color.DeepPink,
        "ue" => Color.SlateBlue,
        "ta" => Color.IndianRed,
        "se" => Color.SteelBlue,
        "pf" => Color.MidnightBlue,
        "sf" => Color.DarkRed,
        "io" => Color.DarkSlateGray,
        "gc" => Color.ForestGreen,
        "jit" => Color.SaddleBrown,
        "tc" => Color.DarkCyan,
        "hl" => Color.DarkOliveGreen,
        "com" => Color.DarkMagenta,
        _ => Color.Gray
    };
}
