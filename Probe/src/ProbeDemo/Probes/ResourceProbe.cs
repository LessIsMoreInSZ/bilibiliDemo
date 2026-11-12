/*----------------------------------------------------------------
// 示例：资源探针 ResourceProbe
//
// 回答的问题："内存/句柄/线程是不是在涨？涨速多少？GC 在干活吗？"
// 能力：一个时间点采样并打印 一组资源指标，靠多次采样对比斜率。
//
// 场景：程序跑一晚内存涨了几百 MB（泄漏嫌疑）；TCP 一直断开重连
//       但句柄数只增不减（socket 没 Dispose）；GPU 拍照后线程数暴涨。
//       不要只采样一次，"泄漏"的定义是斜率，不是绝对值。
//----------------------------------------------------------------*/
using System;
using System.Diagnostics;

namespace ProbeDemo
{
    /// <summary>资源采样探针。Sample 是 [Conditional] 的，Release 下整条剥除。</summary>
    public static class ResourceProbe
    {
        /// <summary>打印一次当前进程资源快照。多次调用对比斜率才是目的。</summary>
        [Conditional("PROBE")]
        public static void Sample(string name)
        {
            using (var p = Process.GetCurrentProcess())
            {
                p.Refresh();   // 让句柄/线程计数取到当前值

                Trace.WriteLine(
                    $"[PROBE {DateTime.Now:HH:mm:ss.fff}] [RES:{name}] " +
                    $"内存={p.WorkingSet64 / 1024 / 1024}MB " +
                    $"私有提交={p.PrivateMemorySize64 / 1024 / 1024}MB " +
                    $"句柄={p.HandleCount} " +
                    $"线程={p.Threads.Count} " +
                    $"GC(Gen0/1/2)={GC.CollectionCount(0)}/{GC.CollectionCount(1)}/{GC.CollectionCount(2)} " +
                    $"托管堆≈{GC.GetTotalMemory(false) / 1024}KB");
            }
        }
    }
}
