using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using UiProbeDemo.Probes;

namespace UiProbeDemo
{
    /// <summary>
    /// 后台探针仪表盘:三张可视化的宿主。
    /// 数据源在 Probes/UiProbes.Backend.cs(纯采样/对帧);本文件只做绘制。
    /// 铁律:绘制层自己也是观测者 —— 统一 DispatcherTimer 低频批量重绘,
    /// 不做逐点实时刷,免得"观测行为"反过来制造卡顿污染被观测对象。
    /// </summary>
    public partial class DashWindow : Window
    {
        private readonly Random _rng = new();
        private readonly DemoStateMachine _sm = new();

        public DashWindow()
        {
            InitializeComponent();
            ProbeLog.BindSecondary(LogDash);
            BuildStateMachineDiagram();
            ProbeLog.Write("仪表盘已打开:资源斜率/状态机走线/Modbus 对帧 三张图待命", false);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            StopResTimer();
            StopSmTimer();
            StopMbTimer();
            ProbeLog.UnbindSecondary(LogDash);
        }

        private static Brush Br(byte r, byte g, byte b) => new SolidColorBrush(Color.FromRgb(r, g, b));

        // ==================== Tab① 资源斜率 ====================
        private DispatcherTimer _resTimer;
        private readonly List<double> _mem = new();
        private readonly List<double> _hnd = new();
        private readonly List<double> _thr = new();
        private readonly List<FileStream> _leakyStreams = new();
        private readonly List<byte[]> _ballast = new();

        private void BtnSample_Click(object sender, RoutedEventArgs e)
        {
            if (_resTimer != null) { StopResTimer(); return; }
            ResourceSeriesProbe.Sampled += OnResourceSampled;
            _resTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            _resTimer.Tick += (s, e2) => ResourceSeriesProbe.SampleNow();
            _resTimer.Start();
            ProbeLog.Announce("资源斜率探针:开始 500ms/点 自观测本进程");
        }

        private void StopResTimer()
        {
            if (_resTimer == null) return;
            _resTimer.Stop();
            _resTimer = null;
            ResourceSeriesProbe.Sampled -= OnResourceSampled;   // 探针摘除必须 -=
        }

        private void OnResourceSampled(ResourceSample s)
        {
            _mem.Add(s.WorkingSetMb); _hnd.Add(s.Handles); _thr.Add(s.Threads);
            Trim(_mem); Trim(_hnd); Trim(_thr);
            ValMem.Text = $"内存 {s.WorkingSetMb:F1} MB";
            ValHnd.Text = $"句柄 {s.Handles}";
            ValThr.Text = $"线程 {s.Threads}";
            ValGc.Text = $"GC 计数 {s.Gen0}/{s.Gen1}/{s.Gen2}";
            ChartRes.Children.Clear();
            DrawSeries(ChartRes, _mem, Brushes.DodgerBlue);
            DrawSeries(ChartRes, _hnd, Brushes.Orange);
            DrawSeries(ChartRes, _thr, Brushes.MediumSpringGreen);
        }

        private static void Trim(List<double> xs) { while (xs.Count > 170) xs.RemoveAt(0); }

        private static void DrawSeries(Canvas cv, List<double> ys, Brush stroke)
        {
            if (ys.Count < 2) return;
            var min = ys.Min();
            var max = ys.Max();
            if (max - min < 1e-9) max = min + 1;
            var w = cv.Width - 10;
            var h = cv.Height - 8;
            var pts = new PointCollection();
            for (var i = 0; i < ys.Count; i++)
            {
                var x = 2 + i * (w - 4) / (ys.Count - 1);
                var y = 4 + (h - 12) * (1 - (ys[i] - min) / (max - min));
                pts.Add(new Point(x, y));
            }
            cv.Children.Add(new Polyline { Points = pts, Stroke = stroke, StrokeThickness = 1.6 });
            var last = pts[^1];
            var tag = new TextBlock
            {
                Text = $"{ys[^1]:F1}",
                Foreground = stroke,
                FontSize = 11,
                Background = new SolidColorBrush(Color.FromArgb(120, 0x12, 0x12, 0x1C)),
            };
            Canvas.SetLeft(tag, Math.Min(Math.Max(last.X - 46, 4), w - 120));
            Canvas.SetTop(tag, Math.Max(last.Y - 18, 2));
            cv.Children.Add(tag);
        }

        private void BtnLeakHandle_Click(object sender, RoutedEventArgs e)
        {
            var path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            for (var i = 0; i < 60 && _leakyStreams.Count < 600; i++)
                _leakyStreams.Add(File.OpenRead(path));
            ProbeLog.Write($"业务:又打开 60 个文件句柄不关(累计持有 {_leakyStreams.Count} 个)", false);
        }

        private void BtnLeakMem_Click(object sender, RoutedEventArgs e)
        {
            _ballast.Add(new byte[30 * 1024 * 1024]);
            ProbeLog.Write($"业务:挂起一个 30MB 数组不释放(累计 {_ballast.Count} 个)", false);
        }

        private void BtnRelease_Click(object sender, RoutedEventArgs e)
        {
            foreach (var fs in _leakyStreams) fs.Dispose();
            _leakyStreams.Clear();
            _ballast.Clear();
            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            ProbeLog.Write("业务:句柄与内存全部释放 —— 回头看曲线是否应声回落", false);
        }

        // ==================== Tab② 状态机走线 ====================
        private DispatcherTimer _smTimer;
        private readonly Dictionary<string, (Border Box, TextBlock Tbx)> _nodes = new();
        private readonly Dictionary<string, Point> _pos = new();
        private readonly Dictionary<string, Line> _edges = new();
        private int _smSteps;

        private void BuildStateMachineDiagram()
        {
            var layout = new (string Name, double X, double Y)[]
            {
                (DemoStateMachine.Idle, 110, 205), (DemoStateMachine.Feed, 300, 70),
                (DemoStateMachine.Heat, 540, 70),  (DemoStateMachine.Hold, 730, 205),
                (DemoStateMachine.Open, 540, 360), (DemoStateMachine.Take, 300, 360),
            };
            foreach (var l in layout) _pos[l.Name] = new Point(l.X, l.Y);
            foreach (var (from, to) in _sm.LegalEdges)
            {
                var a = _pos[from];
                var b = _pos[to];
                var ln = new Line
                {
                    X1 = a.X + 42, Y1 = a.Y + 17, X2 = b.X + 42, Y2 = b.Y + 17,
                    Stroke = Br(0x4A, 0x4A, 0x68), StrokeThickness = 2,
                };
                _edges[from + ">" + to] = ln;
                ChartSm.Children.Add(ln);
            }
            foreach (var (name, x, y) in layout)
            {
                var tbx = new TextBlock
                {
                    Text = name, Foreground = Brushes.White, FontSize = 12.5,
                    HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center,
                };
                var box = new Border
                {
                    Width = 84, Height = 34, CornerRadius = new CornerRadius(17),
                    Background = Br(0x2F, 0x2F, 0x44), BorderBrush = Br(0x5A, 0x5A, 0x7A),
                    BorderThickness = new Thickness(1), Child = tbx,
                };
                Canvas.SetLeft(box, x); Canvas.SetTop(box, y);
                ChartSm.Children.Add(box);
                _nodes[name] = (box, tbx);
            }
            var cap = new TextBlock
            {
                Text = "工艺循环:待机 → 喂料 → 加热 → 保压 → 开模 → 取件 → 待机",
                Foreground = Br(0x8F, 0x8F, 0xA8), FontSize = 12,
            };
            Canvas.SetLeft(cap, 280); Canvas.SetTop(cap, 444);
            ChartSm.Children.Add(cap);
        }

        private void BtnSmRun_Click(object sender, RoutedEventArgs e)
        {
            if (_smTimer != null) { StopSmTimer(); ((Button)sender).Content = "开始走线"; return; }
            _sm.Stepped += OnSmStepped;
            _sm.Illegal += OnSmIllegal;
            _smTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1100) };
            _smTimer.Tick += (s, e2) =>
            {
                _smSteps++;
                if (_smSteps % 7 == 0) _sm.StepIllegal();   // 定期放一次野事件,考验探针
                else _sm.StepNext();
            };
            _smTimer.Start();
            ((Button)sender).Content = "停止走线";
            ProbeLog.Announce("状态机演示已启动(1100ms/步,每 7 步注入一次野事件)");
        }

        private void StopSmTimer()
        {
            if (_smTimer == null) return;
            _smTimer.Stop();
            _smTimer = null;
            _sm.Stepped -= OnSmStepped;   // 探针摘除必须 -=
            _sm.Illegal -= OnSmIllegal;
        }

        private void OnSmStepped(string from, string to)
        {
            SetActive(to);
            if (_edges.TryGetValue(from + ">" + to, out var ln))
                FlashEdge(ln, Brushes.LightGreen, Br(0x4A, 0x4A, 0x68), 420);
        }

        private void OnSmIllegal(string from, string to)
        {
            if (!_pos.TryGetValue(from, out var a) || !_pos.TryGetValue(to, out var b)) return;
            var wild = new Line
            {
                X1 = a.X + 42, Y1 = a.Y + 17, X2 = b.X + 42, Y2 = b.Y + 17,
                Stroke = Brushes.Red, StrokeThickness = 3,
                StrokeDashArray = new System.Windows.Media.DoubleCollection { 2, 2 },
            };
            ChartSm.Children.Add(wild);
            var t = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1200) };
            t.Tick += (s, e2) => { ChartSm.Children.Remove(wild); ((DispatcherTimer)s).Stop(); };
            t.Start();
        }

        private void SetActive(string state)
        {
            foreach (var kv in _nodes)
            {
                var on = kv.Key == state;
                kv.Value.Box.Background = on ? Br(0x2F, 0x8D, 0x5C) : Br(0x2F, 0x2F, 0x44);
            }
        }

        private static void FlashEdge(Line ln, Brush flash, Brush rest, int ms)
        {
            ln.Stroke = flash;
            ln.StrokeThickness = 3;
            var t = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(ms) };
            t.Tick += (s, e2) => { ln.Stroke = rest; ln.StrokeThickness = 2; ((DispatcherTimer)s).Stop(); };
            t.Start();
        }

        private void BtnSmIllegal_Click(object sender, RoutedEventArgs e) => _sm.StepIllegal();

        // ==================== Tab③ Modbus 对帧分解 ====================
        private DispatcherTimer _mbTimer;
        private int _frameRow;
        private static readonly (string Name, int Bytes, byte R, byte G, byte B)[] Segs =
        {
            ("事务号", 2, 0x37, 0x63, 0xA8), ("协议号", 1, 0x6B, 0x4F, 0xA3), ("长度", 2, 0x2E, 0x8B, 0x77),
            ("单元", 1, 0x9C, 0x7A, 0x3C), ("功能码", 1, 0x8B, 0x4F, 0x7A), ("地址", 2, 0x4F, 0x6D, 0x2F),
            ("数量", 2, 0x3C, 0x9C, 0x8A),
        };

        private void BtnMbRun_Click(object sender, RoutedEventArgs e)
        {
            if (_mbTimer != null)
            {
                _mbTimer.Stop(); _mbTimer = null;
                ((Button)sender).Content = "开始收流";
                return;
            }
            _mbTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(350) };
            _mbTimer.Tick += (s, e2) => ReceiveOneFrame();
            _mbTimer.Start();
            ((Button)sender).Content = "停止收流";
            ProbeLog.Announce("Modbus 收流已启动(350ms/帧,约 1/3 概率坏帧,坏段红底黄字)");
        }

        private void StopMbTimer() { _mbTimer?.Stop(); _mbTimer = null; }

        private void ReceiveOneFrame()
        {
            var d = ModbusFrameProbe.Decode(ModbusFrameProbe.MakeSampleFrame(_rng));
            UpdateCurrentPanel(d);
            AppendFrameRow(d);
        }

        private void UpdateCurrentPanel(DecodedFrame d)
        {
            FrameSegs.Children.Clear();
            var culprit = CulpritIndex(d);
            for (var i = 0; i < Segs.Length; i++)
            {
                var seg = Segs[i];
                var bad = culprit == i;
                var box = new Border
                {
                    Margin = new Thickness(0, 0, 3, 0),
                    Padding = new Thickness(6, 2, 6, 2),
                    CornerRadius = new CornerRadius(3),
                    Background = new SolidColorBrush(Color.FromRgb(seg.R, seg.G, seg.B)),
                    Opacity = bad ? 1.0 : 0.8,
                };
                var sp = new StackPanel();
                sp.Children.Add(new TextBlock { Text = seg.Name, FontSize = 10, Foreground = Brushes.White, HorizontalAlignment = HorizontalAlignment.Center });
                sp.Children.Add(new TextBlock
                {
                    Text = SegValue(d, i), FontSize = 12.5, FontWeight = FontWeights.Bold,
                    Foreground = bad ? Brushes.Yellow : Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Center,
                });
                box.Child = sp;
                FrameSegs.Children.Add(box);
            }
            TxtError.Text = d.IsError ? "坏帧定罪: " + d.Error : "健康帧";
            TxtError.Foreground = d.IsError ? Brushes.OrangeRed : Brushes.LightGreen;
        }

        private void AppendFrameRow(DecodedFrame d)
        {
            _frameRow++;
            var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(3, 1, 0, 0) };
            var culprit = CulpritIndex(d);
            for (var i = 0; i < Segs.Length; i++)
            {
                var seg = Segs[i];
                var bad = culprit == i;
                row.Children.Add(new Border
                {
                    Width = 12 + seg.Bytes * 30,
                    Margin = new Thickness(0, 0, 2, 0),
                    CornerRadius = new CornerRadius(2),
                    Background = new SolidColorBrush(Color.FromRgb(seg.R, seg.G, seg.B)),
                    Opacity = bad ? 1.0 : 0.55,
                    Child = new TextBlock
                    {
                        Text = SegValue(d, i), FontSize = 11,
                        Foreground = bad ? Brushes.Yellow : Brushes.White,
                        HorizontalAlignment = HorizontalAlignment.Center,
                    },
                });
            }
            row.Children.Add(new TextBlock
            {
                Text = d.IsError ? "✗ " + d.Error : "✓ 事务 " + d.Tx,
                FontSize = 11, Foreground = d.IsError ? Brushes.OrangeRed : Brushes.LightGreen,
                VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(8, 0, 0, 0),
            });
            var cell = new Border
            {
                Width = 872, Height = 22, CornerRadius = new CornerRadius(3),
                Background = d.IsError ? Br(0x4A, 0x1F, 0x1F) : Br(0x1C, 0x24, 0x30),
                Child = row,
            };
            Canvas.SetLeft(cell, 4);
            Canvas.SetTop(cell, 4 + (_frameRow - 1) % 15 * 23);
            ChartMb.Children.Add(cell);
            while (ChartMb.Children.Count > 40) ChartMb.Children.RemoveAt(0);
        }

        private static int? CulpritIndex(DecodedFrame d)
        {
            if (d.Error == null) return null;
            if (d.Error.Contains("协议号")) return 1;
            if (d.Error.Contains("长度")) return 2;
            return 6;   // 截断/缺数据:定罪在尾段
        }

        private static string SegValue(DecodedFrame d, int i)
        {
            return i switch
            {
                0 => d.Tx.ToString(),
                1 => d.Proto.ToString(),
                2 => d.Len.ToString(),
                3 => d.Unit.ToString(),
                4 => "0x" + d.Func.ToString("X2"),
                5 => d.Addr.ToString(),
                _ => d.Count.ToString(),
            };
        }
    }
}
