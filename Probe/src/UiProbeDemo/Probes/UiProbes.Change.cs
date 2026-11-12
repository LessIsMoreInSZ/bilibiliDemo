using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace UiProbeDemo.Probes
{
    /// <summary>
    /// §3 事件泄漏:WeakReference + 强制回收验尸(二分摘除法的数据来源)。
    /// §4 布局抖动:SizeChanged 监督 + 祖先链层数(约束环定位)。
    /// §7 值流追踪:INPC 侧记录,与界面动作对齐时间轴。
    /// </summary>
    public static class UiChangeProbes
    {
        private static readonly Stopwatch Sw = Stopwatch.StartNew();

        // ==================== §3 事件泄漏 ====================

        /// <summary>演示用静态事件源:静态事件的生命周期 = 进程生命周期。</summary>
        public static class SignalSource
        {
            public static event EventHandler Occurred;
            public static void Raise() => Occurred?.Invoke(null, EventArgs.Empty);
            public static void Reset() => Occurred = null;   // 演示善后,下轮重新开始
        }

        private sealed class Payload
        {
            public string Tag;
            public void OnOccurred(object s, EventArgs e) =>
                ProbeLog.Write($"[{Tag}] 负载对象收到信号 —— 它还挂在事件链上");
        }

        /// <summary>跑一次「订阅 → GC → 验尸」;unsubscribed=true 演示正确退订。</summary>
        public static void RunLeakDemo(bool unsubscribed)
        {
            var label = unsubscribed ? "退订版" : "不退订版";
            ProbeLog.Announce("事件泄漏演示 [" + label + "]:订阅静态事件后 GC,再看对象生死");
            var payload = new Payload { Tag = label };
            SignalSource.Occurred += payload.OnOccurred;
            if (unsubscribed) SignalSource.Occurred -= payload.OnOccurred;
            var wr = new WeakReference(payload);
            payload = null;
            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();   // 铁律:只在采证窗口 Collect
            LeakVerdict(label, wr.IsAlive);
        }

        [Conditional("PROBE")]
        private static void LeakVerdict(string label, bool alive)
        {
            var verdict = alive ? "仍存活 → 泄漏(事件链拽着它,链上谁忘了退订?)" : "已被回收 → 健康";
            ProbeLog.Write($"GC 验尸 [{label}]: {verdict}", true);
        }

        // ==================== §4 布局抖动 ====================

        private static FrameworkElement _watched;
        private static long _lastChangeMs = -1;
        private static int _changeCount;

        public static void WatchLayout(FrameworkElement el)
        {
            if (el == null) return;
            el.SizeChanged += OnWatchedSizeChanged;
            _watched = el;
            ProbeLog.Announce($"布局探针:已监督 {NameOf(el)},相邻变更间隔 <50ms 判抖动");
        }

        public static void UnwatchLayout()
        {
            if (_watched == null) return;
            var el = _watched;
            _watched = null;
            el.SizeChanged -= OnWatchedSizeChanged;     // 铁律 2:摘除必须 -=
            ProbeLog.Write($"布局探针:已摘除,共记录 {_changeCount} 次尺寸变更", true);
        }

        private static void OnWatchedSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var el = (FrameworkElement)sender;
            var now = Sw.ElapsedMilliseconds;
            var gap = _lastChangeMs < 0 ? -1 : (int)(now - _lastChangeMs);
            _lastChangeMs = now;
            _changeCount++;
            var hops = 0;
            var p = VisualTreeHelper.GetParent(el);
            while (p != null) { hops++; p = VisualTreeHelper.GetParent(p); }
            LayoutChanged(NameOf(el), e.PreviousSize.Width, e.PreviousSize.Height,
                          e.NewSize.Width, e.NewSize.Height, gap, hops, _changeCount);
        }

        [Conditional("PROBE")]
        private static void LayoutChanged(string name, double pw, double ph, double nw, double nh,
                                          int gapMs, int hops, int count)
        {
            var flag = gapMs >= 0 && gapMs < 50 ? "  ← 高频变更,疑约束环/抖动" : "";
            ProbeLog.Write($"尺寸变更#{count} [{name}]: {pw:F0}×{ph:F0} → {nw:F0}×{nh:F0}" +
                           $" (间隔 {gapMs}ms, 祖先 {hops} 层){flag}", true);
        }

        // ==================== §7 值流追踪 ====================

        private static INotifyPropertyChanged _tracedSource;
        private static PropertyChangedEventHandler _traceHandler;

        public static void ToggleValueTrace(INotifyPropertyChanged src, string cname)
        {
            if (_traceHandler != null)
            {
                _tracedSource.PropertyChanged -= _traceHandler;
                _tracedSource = null; _traceHandler = null;
                ProbeLog.Write($"值流探针:[{cname}] 已摘除", true);
                return;
            }
            var startedAt = Sw.ElapsedMilliseconds;
            _traceHandler = (s, e) => ValueChanged(cname, e.PropertyName, Sw.ElapsedMilliseconds - startedAt);
            _tracedSource = src;
            src.PropertyChanged += _traceHandler;
            ProbeLog.Announce($"值流探针:[{cname}] 已开启,拖动滑条看 INPC 全链路");
        }

        [Conditional("PROBE")]
        private static void ValueChanged(string cname, string prop, long sinceMs) =>
            ProbeLog.Write($"值流[{cname}] {prop} 触发 PropertyChanged (距开启 {sinceMs}ms)", true);

        private static string NameOf(FrameworkElement el) =>
            string.IsNullOrEmpty(el.Name) ? el.GetType().Name : el.Name;
    }
}
