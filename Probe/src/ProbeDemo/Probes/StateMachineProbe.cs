/*----------------------------------------------------------------
// 示例：状态机探针 StateMachineProbe
//
// 回答的问题："设备流程到底按什么顺序走的状态？为什么卡住/跳状态？"
// 能力：①记录状态迁移序列（带每次滞留时长）；②非法迁移立即报警；
//      ③事后 Dump 完整历史，还原"卡死前最后走到哪"。
//
// 场景：自动流程"莫名停在第 4 步不动""直接从 Idle 跳到了 Done"——
//       打断点会把流程打死（对端超时），状态机探针不干扰时序。
//----------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ProbeDemo
{
    /// <summary>状态机探针：观测一个 TState 状态流。线程安全性：单线程使用。</summary>
    public sealed class StateMachineProbe<TState>
    {
        private readonly string _name;
        private readonly Func<TState, TState, bool> _isLegal;   // null = 不校验合法性
        private readonly Queue<string> _history = new Queue<string>(64);
        private readonly int _historyCapacity;

        private TState _last;
        private DateTime _lastAt = DateTime.Now;
        private bool _started;

        public StateMachineProbe(string name,
                                 Func<TState, TState, bool> isLegal = null,
                                 int historyCapacity = 64)
        {
            _name = name;
            _isLegal = isLegal;
            _historyCapacity = historyCapacity;
        }

        /// <summary>进入新状态时调用。滞留时长=上次进入到本次进入的间隔。</summary>
        public void OnEnter(TState state)
        {
            if (!_started)
            {
                _started = true;
                _last = state;
                _lastAt = DateTime.Now;
                Write($"{Now} [STATE:{_name}] 初始态 = {state}");
                return;
            }

            var now = DateTime.Now;
            double dwellMs = (now - _lastAt).TotalMilliseconds;

            Write($"{Now} [STATE:{_name}] {_last} -> {state}（{_last} 滞留 {dwellMs:F0}ms）");

            // 非法迁移不一定抛异常——很多流程只是"悄悄地不对了"，这正是要抓的
            if (_isLegal != null && !_isLegal(_last, state))
                Write($"{Now} [STATE:{_name}] !! 非法迁移：{_last} -> {state}");

            _last = state;
            _lastAt = now;
            _history.Enqueue($"{state} @ {now:HH:mm:ss.fff} (+{dwellMs:F0}ms)");
            while (_history.Count > _historyCapacity) _history.Dequeue();
        }

        /// <summary>导出完整状态流历史（用于事后还原"卡死前走过了哪些状态"）。</summary>
        public string DumpHistory()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"[PROBE] [STATE:{_name}] 历史序列：");
            int i = 0;
            foreach (var h in _history) sb.AppendLine($"[PROBE]   #{++i} {h}");
            return sb.ToString();
        }

        [Conditional("PROBE")]
        private static void Write(string line) => Trace.WriteLine(line);

        private static string Now => "[PROBE] " + DateTime.Now.ToString("HH:mm:ss.fff");
    }
}
