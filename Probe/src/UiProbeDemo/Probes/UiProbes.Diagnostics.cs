using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace UiProbeDemo.Probes
{
    /// <summary>
    /// §2 绑定错误探针:TraceListener 挂进 PresentationTraceSources,门口截获。
    /// §8 全局异常四路:Dispatcher / Task / AppDomain / async void 包装器。
    /// §9 可视化树快照:VisualTreeHelper 递归打印 + 类型计数。
    /// </summary>
    public static class UiDiagnosticsProbes
    {
        // ==================== §2 绑定错误 ====================

        private sealed class BindingTraceListener : TraceListener
        {
            public override void Write(string message) => Report(message);
            public override void WriteLine(string message) => Report(message);

            [Conditional("PROBE")]
            private static void Report(string message) =>
                ProbeLog.Write($"绑定错误: {Clamp(message)}", true);
        }

        private static string Clamp(string m)
        {
            m = m.Replace(Environment.NewLine, " ").Trim();
            return m.Length > 180 ? m[..180] + "…" : m;
        }

        public static void StartBindingProbe()
        {
            PresentationTraceSources.Refresh();
            var src = PresentationTraceSources.DataBindingSource;
            src.Listeners.Add(new BindingTraceListener());
            src.Switch.Level = SourceLevels.Warning;   // 只收 Warning 以上,避免噪音
            ProbeLog.Announce("绑定错误探针:已安装(Warning 以上进日志,原样透传给原生输出)");
        }

        public static void StopBindingProbe()
        {
            var src = PresentationTraceSources.DataBindingSource;
            foreach (var l in src.Listeners.OfType<BindingTraceListener>().ToArray())
                src.Listeners.Remove(l);
            src.Switch.Level = SourceLevels.Off;
            ProbeLog.Announce("绑定错误探针:已摘除");
        }

        // ==================== §8 全局异常四路 ====================

        public static void InstallExceptionProbes(Application app)
        {
            app.DispatcherUnhandledException += (s, e) =>
            {
                ReportUnhandled("UI 线程未处理异常", e.Exception);
                e.Handled = true;    // 采证期:先吞掉保活窗口再逐帧复现
            };
            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                ReportUnhandled("Task 未观察异常", e.Exception);
                e.SetObserved();
            };
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                ReportUnhandled("AppDomain 终结异常", e.ExceptionObject as Exception);

            ProbeLog.Announce("全局异常探针:已安装三路(第四路见 SafeAsync 包装器)");
        }

        [Conditional("PROBE")]
        private static void ReportUnhandled(string where, Exception ex)
        {
            if (ex == null) { ProbeLog.Write($"{where}: (无法转成 Exception)", true); return; }
            ProbeLog.Write($"{where}: {ex.GetType().Name}: {ex.Message}", true);
            var first = ex.StackTrace?.Split('\r', '\n').FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));
            if (first != null) ProbeLog.Write($"  首帧: {first.Trim()}", true);
        }

        /// <summary>第四路:async void 包装器——async void 事件处理器必须经它下坠。</summary>
        public static async void SafeAsync(Action action, string tag = "业务")
        {
            try { action(); }
            catch (Exception ex) { ReportSafeFailure(tag, ex); }
        }

        [Conditional("PROBE")]
        private static void ReportSafeFailure(string tag, Exception ex) =>
            ProbeLog.Write($"async void 包装器[{tag}]: {ex.GetType().Name}: {ex.Message}", true);

        // ==================== §9 可视化树快照 ====================

        public static void DumpVisualTree(DependencyObject root, string tag = "快照")
        {
            var sb = new StringBuilder();
            var counts = new SortedDictionary<string, int>();
            Walk(root, 0, sb, counts);
            ProbeLog.Write($"可视化树[{tag}]:共 {counts.Values.Sum()} 个节点,类型分布: " +
                           string.Join("  ", counts.Select(kv => $"{kv.Key}×{kv.Value}")), true);
            ProbeLog.Write(sb.ToString().TrimEnd(), true);
        }

        private static void Walk(DependencyObject node, int depth, StringBuilder sb, SortedDictionary<string, int> counts)
        {
            if (depth > 7) { return; }
            var name = node.GetType().Name;
            counts[name] = counts.ContainsKey(name) ? counts[name] + 1 : 1;
            sb.Append(' ', depth * 2).Append(name);
            if (node is FrameworkElement fe && !string.IsNullOrEmpty(fe.Name))
                sb.Append($" {fe.Name}");
            sb.AppendLine();
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(node); i++)
                Walk(VisualTreeHelper.GetChild(node, i), depth + 1, sb, counts);
        }
    }
}
