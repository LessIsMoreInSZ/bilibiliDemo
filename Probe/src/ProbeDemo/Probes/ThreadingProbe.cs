/*----------------------------------------------------------------
// 示例：线程/锁探针 ThreadingProbe
//
// 回答的问题："这把锁被持有多久？卡顿是等锁还是真在干活？
//              线程池是不是已经饿到开始排队了？"
//
// 能力：
//   1) 锁持有计时：LockBegin/LockEnd 包住 lock 段，超过阈值点名告警。
//      采用 ref Stopwatch 模式——Release（未定义 PROBE）下两个调用被剥除，
//      watch 恒为 null，连 Stopwatch 对象都不分配，真正零开销。
//   2) 线程池采样：工作线程/队列积压/IO 线程一眼看出"线程池饥饿"。
//
// 场景：UI 偶发冻结、多个工位动作莫名串行、Task 全在排队但 CPU 很闲
//       ——先怀疑锁和线程池，让数字说话。
//----------------------------------------------------------------*/
using System;
using System.Diagnostics;
using System.Threading;

namespace ProbeDemo
{
    /// <summary>线程/锁探针：锁持有计时 + 线程池饥饿采样。静态类，各方法线程安全。</summary>
    public static class ThreadingProbe
    {
        // ---------- 锁持有计时（ref Stopwatch 模式）----------

        /// <summary>进入 lock 前调用。用法：Stopwatch w = null; LockBegin("配方锁", ref w); lock(...){...; LockEnd("配方锁", w);}</summary>
        [Conditional("PROBE")]
        public static void LockBegin(string name, ref Stopwatch watch)
        {
            watch = Stopwatch.StartNew();
        }

        /// <summary>退出 lock 后调用。超过 slowMs 视为"长持锁"，直接点名。</summary>
        [Conditional("PROBE")]
        public static void LockEnd(string name, Stopwatch watch, long slowMs = 50)
        {
            if (watch == null) return;   // Release 下恒为 null（防御，正常走不到）
            watch.Stop();
            long ms = watch.ElapsedMilliseconds;
            if (ms >= slowMs)
                Trace.WriteLine($"{Now} [LOCK:{name}] 持有 {ms}ms —— 超过阈值 {slowMs}ms，持锁过久，会阻塞其他线程");
            else
                Trace.WriteLine($"{Now} [LOCK:{name}] 持有 {ms}ms");
        }

        // ---------- 线程池饥饿采样 ----------

        /// <summary>打印线程池水位：可用工作线程 / 队列积压数 / IO 线程。队列持续 >0 即"饥饿"信号。</summary>
        [Conditional("PROBE")]
        public static void ThreadPoolSample(string tag)
        {
            ThreadPool.GetAvailableThreads(out int workerAvail, out int ioAvail);
            ThreadPool.GetMaxThreads(out int workerMax, out int ioMax);
            ThreadPool.GetMinThreads(out int workerMin, out _);

            string warning = "";
            if (ThreadPool.PendingWorkItemCount > 0)
                warning = $" —— 队列积压 {ThreadPool.PendingWorkItemCount}，新任务在排队（饥饿征兆）";

            Trace.WriteLine($"{Now} [TP:{tag}] 工作线程 {ThreadPool.ThreadCount}（可用 {workerAvail}/{workerMax}，最小 {workerMin}）、" +
                            $"IO 线程可用 {ioAvail}/{ioMax}、待处理 {ThreadPool.PendingWorkItemCount}{warning}");
        }

        private static string Now => "[PROBE] " + DateTime.Now.ToString("HH:mm:ss.fff");
    }
}
