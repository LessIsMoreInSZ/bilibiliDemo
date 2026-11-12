/*----------------------------------------------------------------
// 示例：频次/速率探针 RateProbe
//
// 回答的问题："这个事件多久发生一次？是风暴还是漏发？"
// 能力：滑动窗口计数，每满一个窗口自动打印 次数/速率。
//
// 场景：报警每秒刷几百条（报警风暴）？心跳丢了（应该每秒 1 次却 5 秒没来）？
//       按钮连击导致命令重复执行？——先看频率的真实数字再下结论。
//----------------------------------------------------------------*/
using System;
using System.Diagnostics;
using System.Threading;

namespace ProbeDemo
{
    /// <summary>频次探针：Hit() 计数，满窗口自动打印速率。线程安全。</summary>
    public sealed class RateProbe
    {
        private readonly string _name;
        private readonly int _windowMs;

        private long _count;
        private long _windowStartTicks = Stopwatch.GetTimestamp();

        public RateProbe(string name, int windowMs = 5000)
        {
            _name = name;
            _windowMs = windowMs;
        }

        /// <summary>事件发生时调用一次。Release（未定义 PROBE）下整个方法被剥除。</summary>
        [Conditional("PROBE")]
        public void Hit()
        {
            Interlocked.Increment(ref _count);
            double elapsedMs = (Stopwatch.GetTimestamp() - _windowStartTicks) * 1000.0 / Stopwatch.Frequency;
            if (elapsedMs >= _windowMs)
            {
                Trace.WriteLine($"[PROBE {DateTime.Now:HH:mm:ss.fff}] [RATE:{_name}] " +
                                $"近 {_windowMs / 1000.0:F1}s 共 {Interlocked.Read(ref _count)} 次 " +
                                $"（{Interlocked.Read(ref _count) / (_windowMs / 1000.0):F1} 次/秒）");
                // 窗口翻转：从下一次 Hit 起重新计窗（速率以调用间隔为粒度）
                Interlocked.Exchange(ref _windowStartTicks, Stopwatch.GetTimestamp());
                Interlocked.Exchange(ref _count, 0);
            }
        }
    }
}
