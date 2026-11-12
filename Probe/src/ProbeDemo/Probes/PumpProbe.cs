/*----------------------------------------------------------------
// 示例：派发延迟探针 PumpProbe
//
// 回答的问题："界面/回调为什么时卡时不卡？"——很多上位机的"偶发卡顿"
//              本质是同一类病：UI 线程或线程池积压，任务从"被派发"
//              到"真正执行"隔了太久。这个探针量出这个间隔的真实数字。
//
// 能力：
//   1) ProbeDispatch：立刻给自己派一个回调，量"派发→执行"的当天延迟。
//   2) Sentinel：后台每 interval 打一拍延迟，超过阈值标红告警，
//      输出一条延迟曲线——曲线抬头的时刻就是开始积压的时刻。
//
// 原理与 UI 消息泵同构：向队列投递"你是几点几分派我的？"消息，
// 执行时一算就得到积压程度。放到 SynchronizationContext.Post 上就是
// 纯正的 UI 线程泵探针；本示例用线程池做等价演示。
//
// 说明：Sentinel 是常驻观测，建议只在排查期间挂载（对照下述 Demo 12）。
//----------------------------------------------------------------*/
using System;
using System.Diagnostics;
using System.Threading;

namespace ProbeDemo
{
    /// <summary>派发延迟探针：一次性测量 + 常驻延迟哨兵。静态类。</summary>
    public static class PumpProbe
    {
        /// <summary>单次测量：从本次调用到回调真正执行，延迟多少毫秒。</summary>
        [Conditional("PROBE")]
        public static void ProbeDispatch(string tag)
        {
            long postedTicks = Stopwatch.GetTimestamp();
            ThreadPool.QueueUserWorkItem(_ =>
            {
                double delayMs = (Stopwatch.GetTimestamp() - postedTicks) * 1000.0 / Stopwatch.Frequency;
                string level = delayMs >= 500 ? " !! 严重积压" : delayMs >= 100 ? " —— 偏高" : "";
                Trace.WriteLine($"{Now} [PUMP:{tag}] 派发延迟 {delayMs:F0}ms{level}");
            });
        }

        /// <summary>
        /// 常驻哨兵：每隔 intervalMs 量一次派发延迟，超过 warnMs 标记告警。
        /// 返回的句柄 Dispose 后停止观测。
        /// </summary>
        public static SentinelHandle Sentinel(string name, int intervalMs = 200, double warnMs = 100)
        {
            var handle = new SentinelHandle(name, intervalMs, warnMs);
            Write($"{Now} [PUMP:{name}] 哨兵上线（每 {intervalMs}ms 一拍，告警阈值 {warnMs:F0}ms）");
            return handle;
        }

        /// <summary>哨兵句柄：保活即观测，Dispose 即停。</summary>
        public sealed class SentinelHandle : IDisposable
        {
            private readonly string _name;
            private readonly Timer _timer;
            private readonly double _warnMs;
            private int _tick;

            internal SentinelHandle(string name, int intervalMs, double warnMs)
            {
                _name = name;
                _warnMs = warnMs;
                _timer = new Timer(Tick, null, intervalMs, intervalMs);
            }

            private void Tick(object state)
            {
                int n = Interlocked.Increment(ref _tick);
                long posted = Stopwatch.GetTimestamp();
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    double delayMs = (Stopwatch.GetTimestamp() - posted) * 1000.0 / Stopwatch.Frequency;
                    Write($"{Now} [PUMP:{_name}] #{n} 派发延迟 {delayMs:F0}ms{(delayMs >= _warnMs ? "  <-- 超阈值，队列在积压" : "")}");
                });
            }

            public void Dispose()
            {
                _timer.Dispose();
                Write($"{Now} [PUMP:{_name}] 哨兵下线");
            }
        }

        [Conditional("PROBE")]
        private static void Write(string line)
        {
            Trace.WriteLine(line);
        }

        private static string Now => "[PROBE] " + DateTime.Now.ToString("HH:mm:ss.fff");
    }
}
