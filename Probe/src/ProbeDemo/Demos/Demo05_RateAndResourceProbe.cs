/*----------------------------------------------------------------
// Demo 05：频次探针 + 资源探针（RateProbe / ResourceProbe）
//
// 覆盖探针：Probes/RateProbe.cs（滑动窗口速率）、Probes/ResourceProbe.cs（资源采样）
// 回答的问题："这个事件多久发生一次？内存/句柄/线程是在涨还是在抖？"
//
// 要点：心跳丢一拍，速率窗口会把 10 打成 9——频次问题要的是数字，
//       不是感觉；资源泄漏看的是两次采样的**斜率**，不是绝对值。
//----------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Threading;

namespace ProbeDemo.Demos
{
    /// <summary>Demo 05：频次探针 + 资源探针（速率与泄漏观测）。</summary>
    public static class Demo05_RateAndResourceProbe
    {
        public const string Title = "Demo 5：频次探针 + 资源探针（速率与泄漏观测）";

        public static void Run()
        {
            RateProbe heartBeat = new RateProbe("下位机心跳", windowMs: 900);

            ResourceProbe.Sample("压测前");

            // 模拟：心跳每 100ms 一次（=10 次/秒），略短于窗口的时间段内故意丢一拍
            for (int i = 0; i < 10; i++)
            {
                if (i != 6) heartBeat.Hit();           // 故意丢一拍，观察计数 9 而不是 10
                Thread.Sleep(100);
            }

            // 资源对比：故意留存 3 块 10MB 数组模拟"内存上涨"，看采样斜率
            List<byte[]> leak = new List<byte[]>();
            for (int i = 0; i < 3; i++) leak.Add(new byte[10 * 1024 * 1024]);

            ResourceProbe.Sample("压测后（故意留存 30MB）");
            Console.WriteLine("(泄漏的判断依据是两次采样的斜率，不是绝对值)");
        }
    }
}
