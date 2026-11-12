/*----------------------------------------------------------------
// 示例：异常流探针 ExceptionProbe
//
// 回答的问题："程序里到底在发生什么异常？——包括那些被 catch 吞掉、
//              连日志都不留的。"
//
// 能力：
//   Install 挂上 AppDomain.CurrentDomain.FirstChanceException：
//   CLR 抛出"任何"异常的第一时间都会经过这里——不管后面有没有人 catch。
//   按异常类型聚合计数，Report 时输出排行。
//
// 最重要的用法：某功能"静默失效"很久没日志，把 Install 挂上跑一会儿，
//   往往发现它其实每秒都在 int.Parse失败/SocketException，
//   只不过全被空 catch 吞了。异常统计就是吞异常行为的照妖镜。
//
// 注意：FirstChanceException 在热路径上每次异常都进回调，
//       统计用 ConcurrentDictionary + Interlocked，别在里面做重活。
//----------------------------------------------------------------*/
using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace ProbeDemo
{
    /// <summary>异常流探针：FirstChanceException 按类型聚合统计。静态类。</summary>
    public static class ExceptionProbe
    {
        private static readonly ConcurrentDictionary<string, long> Counts = new ConcurrentDictionary<string, long>();
        private static bool _installed;

        /// <summary>挂上异常照妖镜（幂等）。Release 未定义 PROBE 时不会挂载，零影响。</summary>
        [Conditional("PROBE")]
        public static void Install()
        {
            if (_installed) return;
            _installed = true;
            AppDomain.CurrentDomain.FirstChanceException += (s, e) =>
            {
                string key = e.Exception.GetType().Name;
                Counts.AddOrUpdate(key, 1, (k, v) => v + 1);
            };
            Write($"{Now} [EXC] 异常照妖镜已挂上（此后所有异常——含被 catch 吞掉的——都会被统计）");
        }

        /// <summary>输出异常类型排行。</summary>
        [Conditional("PROBE")]
        public static void Report(string tag)
        {
            Write($"{Now} [EXC:{tag}] 累计异常统计（含被吞掉的）：");
            foreach (var kv in Counts)
                Write($"{Now} [EXC:{tag}]   {kv.Key} x {kv.Value}");
            if (Counts.IsEmpty)
                Write($"{Now} [EXC:{tag}]   （期间无任何异常发生）");
        }

        /// <summary>清零重新计数（分阶段观测用）。</summary>
        [Conditional("PROBE")]
        public static void Reset(string tag)
        {
            Counts.Clear();
            Write($"{Now} [EXC:{tag}] 计数已清零，开始新阶段的观测");
        }

        [Conditional("PROBE")]
        private static void Write(string line)
        {
            Trace.WriteLine(line);
        }

        private static string Now => "[PROBE] " + DateTime.Now.ToString("HH:mm:ss.fff");
    }
}
