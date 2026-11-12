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

namespace ProbeDemo
{
    /// <summary>频次探针：Hit() 计数，满窗口自动打印速率。线程安全。</summary>
    public sealed class RateProbe
    {
        private readonly string _name;
        private readonly int _windowMs;
        private readonly object _gate = new object();
        private long _count;
        private long _windowStart;

        public RateProbe(string name, int windowMs = 5000)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("探针名称不能为空。", nameof(name));
            if (windowMs <= 0)
                throw new ArgumentOutOfRangeException(nameof(windowMs), "统计窗口必须大于 0。");

            _name = name;
            _windowMs = windowMs;
            _windowStart = Stopwatch.GetTimestamp();
        }

        /// <summary>事件发生时调用一次。Release（未定义 PROBE）下整个方法被剥除。</summary>
        [Conditional("PROBE")]
        public void Hit()
        {
            long count;
            double elapsedMs;
            lock (_gate)
            {
                _count++;
                long now = Stopwatch.GetTimestamp();
                elapsedMs = (now - _windowStart) * 1000.0 / Stopwatch.Frequency;
                if (elapsedMs < _windowMs)
                    return;

                count = _count;
                _count = 0;
                _windowStart = now;
            }

            double rate = count / (elapsedMs / 1000.0);
            Trace.WriteLine($"[PROBE {DateTime.Now:HH:mm:ss.fff}] [RATE:{_name}] " +
                            $"近 {elapsedMs / 1000.0:F1}s 共 {count} 次（{rate:F1} 次/秒）");
        }
    }
}
