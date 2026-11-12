/*----------------------------------------------------------------
// Demo 12：派发延迟探针 PumpProbe
//
// 覆盖探针：Probes/PumpProbe.cs —— 派发延迟测量与常驻哨兵
// 回答的问题："界面/回调为什么时卡时不卡？"（队列积压 = 卡顿真凶）
//
// 要点：占满线程池再量延迟，曲线 0ms → 897ms → 0ms——抬头点即积压起点。
//       哨兵与 UI 消息泵同构；量真 UI 线程把同样的探测放到
//       SynchronizationContext.Post 上即可。
//----------------------------------------------------------------*/
using System;
using System.Threading;

namespace ProbeDemo.Demos
{
    /// <summary>Demo 12：派发延迟探针（队列积压 = 卡顿真凶）。</summary>
    public static class Demo12_PumpProbe
    {
        public const string Title = "Demo 12：派发延迟探针（队列积压 = 卡顿真凶）";

        public static void Run()
        {
            using (PumpProbe.SentinelHandle sentinel = PumpProbe.Sentinel("调度哨兵", intervalMs: 150, warnMs: 50))
            {
                PumpProbe.ProbeDispatch("空闲时");

                // 制造积压：占满线程池（现实等价物：一堆回调在 Sleep / UI 线程忙长任务）
                int busy = Environment.ProcessorCount * 2;
                for (int i = 0; i < busy; i++)
                    ThreadPool.QueueUserWorkItem(_ => Thread.Sleep(1200));
                Thread.Sleep(300);

                PumpProbe.ProbeDispatch("积压时");
                PumpProbe.ProbeDispatch("积压时");

                Thread.Sleep(1500);        // 等哨兵记录到恢复
                PumpProbe.ProbeDispatch("恢复后");
                Thread.Sleep(400);
            }
            Console.WriteLine("(延迟曲线：空闲低 → 积压时飙升 → 恢复后回落，抬头点即积压起点)");
        }
    }
}
