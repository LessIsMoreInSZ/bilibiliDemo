using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace UiProbeDemo.Probes
{
    /// <summary>
    /// §1 UI 心跳:心跳间隔 = UI 线程卡顿时长(200ms 心跳,&gt;500ms 报告)。
    /// §6 渲染帧率:CompositionTarget.Rendering 计帧,每秒采样一次。
    /// §5 派发延迟:投递时刻 vs 执行时刻的差值,换算成帧数看积压。
    /// </summary>
    public static class UiCoreProbes
    {
        private static readonly Stopwatch Sw = Stopwatch.StartNew();

        // ==================== §1 UI 心跳 ====================

        private static DispatcherTimer _hb;
        private static long _lastMs;
        private static int _jankCount;

        public static void StartHeartbeat()
        {
            if (_hb != null) return;
            _hb = new DispatcherTimer(DispatcherPriority.Render) { Interval = TimeSpan.FromMilliseconds(200) };
            _lastMs = Sw.ElapsedMilliseconds;
            _hb.Tick += (s, e) =>
            {
                var now = Sw.ElapsedMilliseconds;
                var gap = now - _lastMs;
                _lastMs = now;
                HeartbeatTick(gap);          // 打点挂在 [Conditional] 上,Release 剥除
            };
            _hb.Start();
            ProbeLog.Announce("心跳探针:已启动(Render 优先级,200ms/次,&gt;500ms 判卡顿)");
        }

        [Conditional("PROBE")]
        private static void HeartbeatTick(long gap)
        {
            if (gap > 500)
            {
                _jankCount++;
                ProbeLog.Write($"UI 线程疑似卡顿:心跳间隔 {gap}ms(阈值 500ms,第 {_jankCount} 次)", true);
            }
        }

        // ==================== §6 渲染帧率 ====================

        private static int _fpsFrames;
        private static long _fpsLastMs;
        private static long _fpsMaxGap;
        private static DispatcherTimer _fpsSampler;
        private static int _seconds;
        private static int _budget;

        public static void StartFps(int seconds = 10)
        {
            if (_fpsSampler != null) return;
            _fpsFrames = 0; _seconds = 0; _budget = seconds;
            _fpsLastMs = Sw.ElapsedMilliseconds; _fpsMaxGap = 0;
            CompositionTarget.Rendering += FpsFrame;
            _fpsSampler = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _fpsSampler.Tick += (s, e) => FpsSample();
            _fpsSampler.Start();
            ProbeLog.Announce($"FPS 探针:开始采样 {seconds}s(静止界面帧数低属正常,看它如何被卡顿拉垮)");
        }

        private static void FpsFrame(object sender, EventArgs e)
        {
            var now = Sw.ElapsedMilliseconds;
            var gap = now - _fpsLastMs;
            if (gap > _fpsMaxGap) _fpsMaxGap = gap;
            _fpsLastMs = now;
            _fpsFrames++;
        }

        [Conditional("PROBE")]
        private static void FpsSample()
        {
            _seconds++;
            ProbeLog.Write($"FPS 第 {_seconds}/{_budget}s:帧数 {_fpsFrames},最长帧间隔 {_fpsMaxGap}ms", true);
            _fpsFrames = 0; _fpsMaxGap = 0;
            if (_seconds >= _budget) StopFps();
        }

        public static void StopFps()
        {
            if (_fpsSampler == null) return;
            CompositionTarget.Rendering -= FpsFrame;   // 铁律 2:探针摘除必须 -=
            _fpsSampler.Stop();
            _fpsSampler = null;
            ProbeLog.Announce("FPS 探针:已停止,Rendering 订阅已解除");
        }

        // ==================== §5 派发延迟 ====================

        public static void MeasureDispatchLatency(int count)
        {
            ProbeLog.Announce($"派发延迟探针:向 Dispatcher 投递 {count} 个重活(每个约 30ms),观测排队时间");
            for (var i = 0; i < count; i++)
            {
                var queuedAt = Sw.ElapsedMilliseconds;   // 闭包变量必须在投递前取
                var idx = i;
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    var waited = Sw.ElapsedMilliseconds - queuedAt;
                    DispatchExecuted(idx, count, waited);
                    Thread.Sleep(30);                    // 业务模拟重活,勿放进探针
                });
            }
        }

        [Conditional("PROBE")]
        private static void DispatchExecuted(int idx, int total, long waited)
        {
            var frames = waited / 17.0;
            if (idx % 10 == 0 || waited > 200)
                ProbeLog.Write($"派发 #{idx}/{total}:排队 {waited}ms ≈ {frames:F1} 帧{(waited > 200 ? "  ← 队列积压!" : "")}", true);
        }
    }
}
