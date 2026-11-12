using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using UiProbeDemo.Probes;

namespace UiProbeDemo
{
    /// <summary>
    /// 演示台主窗口:左侧是「制造 bug 的业务代码」,右侧日志面板显示探针打点。
    /// 本文件只含业务触发逻辑;全部观测打点都在 Probes/ 层,且挂 [Conditional("PROBE")]。
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DemoViewModel _vm = new();
        private DispatcherTimer _jitterTimer;
        private bool _jitterPhase;
        private int _leakNo;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _vm;
            ProbeLog.Bind(LogBox);
            Loaded += (s, e) =>
            {
                ProbeLog.Write("演示台就绪:依次点 ①~⑩ 制造 bug / 打开仪表盘,看右侧黄色行回放现场;", false);
                AnnounceStartup("心跳 / 绑定错误 / 布局 / 全局异常四路探针已随窗口启动(本行 Release 不出现)。");
                UiCoreProbes.StartHeartbeat();
                UiDiagnosticsProbes.StartBindingProbe();
                UiChangeProbes.WatchLayout(RectJitter);
            };
        }

        [Conditional("PROBE")]
        private void AnnounceStartup(string msg) => ProbeLog.Announce(msg);

        // ==================== ① UI 卡顿 ====================
        private void Btn1_Click(object sender, RoutedEventArgs e)
        {
            ProbeLog.Write("业务:UI 线程开始同步执行 800ms 重操作…");
            Thread.Sleep(800);   // 真实项目里是同步 IO / 大循环 / 少写一个 await
            ProbeLog.Write("业务:重操作结束");
        }

        // ==================== ② 绑定错误 ====================
        private void Btn2_Click(object sender, RoutedEventArgs e)
        {
            var warn = new TextBlock { Foreground = Brushes.Salmon, FontSize = 12, Margin = new Thickness(0, 2, 0, 2) };
            warn.SetBinding(TextBlock.TextProperty, new Binding("NoSuchProperty") { Source = _vm });
            PanelBroken.Children.Add(warn);
        }

        // ==================== ③ 事件泄漏 ====================
        private void Btn3_Click(object sender, RoutedEventArgs e)
        {
            _leakNo++;
            var unsubscribed = _leakNo % 2 == 0;
            ProbeLog.Write($"业务:第 {_leakNo} 轮订阅静态事件({(unsubscribed ? "用完即退订" : "不退订")}),随后触发一次广播");
            UiChangeProbes.RunLeakDemo(unsubscribed);
            UiChangeProbes.SignalSource.Raise();
        }

        // ==================== ④ 布局抖动 ====================
        private void Btn4_Click(object sender, RoutedEventArgs e)
        {
            if (_jitterTimer != null)
            {
                _jitterTimer.Stop();
                _jitterTimer = null;
                RectJitter.Width = 60;
                RectJitter.Height = 35;
                return;
            }
            _jitterTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(30) };
            _jitterTimer.Tick += (s2, e2) =>
            {
                _jitterPhase = !_jitterPhase;
                RectJitter.Width = _jitterPhase ? 92 : 60;
                RectJitter.Height = _jitterPhase ? 54 : 35;
            };
            _jitterTimer.Start();
        }

        // ==================== ⑤~⑩ 触发各类探针 / 仪表盘 ====================
        private void Btn5_Click(object sender, RoutedEventArgs e) => UiCoreProbes.MeasureDispatchLatency(40);
        private void Btn6_Click(object sender, RoutedEventArgs e) => UiCoreProbes.StartFps(10);
        private void Btn7_Click(object sender, RoutedEventArgs e) => UiChangeProbes.ToggleValueTrace(_vm, "滑条");

        private void Btn8_Click(object sender, RoutedEventArgs e) =>
            UiDiagnosticsProbes.SafeAsync(async () =>
            {
                await Task.Yield();
                BusinessRisky();
            }, "订单同步");

        private void BusinessRisky() => throw new InvalidOperationException("模拟:上游返回 null 引发的空引用");

        private void Btn9_Click(object sender, RoutedEventArgs e) =>
            UiDiagnosticsProbes.DumpVisualTree(this, "MainWindow");

        private void Btn10_Click(object sender, RoutedEventArgs e) =>
            new DashWindow { Owner = this }.Show();

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            if (_jitterTimer != null) { _jitterTimer.Stop(); _jitterTimer = null; }
            UiCoreProbes.StopFps();
            UiChangeProbes.UnwatchLayout();
            UiChangeProbes.SignalSource.Reset();
            UiDiagnosticsProbes.StopBindingProbe();
            UiDiagnosticsProbes.StopExceptionProbes();
            ProbeLog.Write("全部探针已停止;业务照常运行", false);
        }
    }
}
