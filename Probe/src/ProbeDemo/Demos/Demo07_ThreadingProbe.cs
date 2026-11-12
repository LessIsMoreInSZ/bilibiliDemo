/*----------------------------------------------------------------
// Demo 07：线程/锁探针 ThreadingProbe
//
// 覆盖探针：Probes/ThreadingProbe.cs —— 锁持有计时 + 线程池水位采样
// 回答的问题："这把锁被持有多久？线程池是不是已经饿到排队？"
//
// 要点：ref Stopwatch 模式（调用方先声明 w = null）保证 Release 下
//       连 Stopwatch 都不分配；ThreadPoolSample 的"待处理>0"是饥饿铁证。
//----------------------------------------------------------------*/
using System;
using System.Diagnostics;
using System.Threading;

namespace ProbeDemo.Demos
{
    /// <summary>Demo 07：线程/锁探针（锁持有 + 线程池饥饿）。</summary>
    public static class Demo07_ThreadingProbe
    {
        public const string Title = "Demo 7：线程/锁探针（锁持有 + 线程池饥饿）";

        public static void Run()
        {
            object locker = new object();

            // 场景 1：线程 A 持有锁 200ms 干"慢活"，B 卡在外面等
            var waitWatch = Stopwatch.StartNew();
            Thread t1 = new Thread(() =>
            {
                Thread.Sleep(100);                       // A 先干点别的
                lock (locker)
                {
                    Stopwatch w = null;
                    ThreadingProbe.LockBegin("配方文件锁", ref w);
                    Thread.Sleep(200);                   // 模拟持锁期间慢操作（真实项目里常见：锁内做 IO）
                    ThreadingProbe.LockEnd("配方文件锁", w, slowMs: 50);
                }
            });
            t1.Start();

            Thread.Sleep(180);                           // B 几乎同时到达锁前
            lock (locker) { /* B 只是想快速读一下 */ }
            waitWatch.Stop();

            t1.Join();
            Console.WriteLine($"(B 等锁实际等了 {waitWatch.ElapsedMilliseconds}ms——持锁方已由 [LOCK] 点名)");

            // 场景 2：线程池水位——喂一批短任务进去，看队列是否积压
            for (int i = 0; i < 20; i++)
            {
                ThreadPool.QueueUserWorkItem(_ => { Thread.Sleep(50); });
            }
            ThreadingProbe.ThreadPoolSample("投喂 20 个任务后");
        }
    }
}
