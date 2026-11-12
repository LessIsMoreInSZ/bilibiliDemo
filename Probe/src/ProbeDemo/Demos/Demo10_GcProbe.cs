/*----------------------------------------------------------------
// Demo 10：GC/内存探针 GcProbe
//
// 覆盖探针：Probes/GcProbe.cs —— 代际增量 + 回收后驻留判定
// 回答的问题："内存是在涨还是在抖？这段代码会不会造成泄漏？"
//
// 要点：泄漏的黄金判据是"完整回收后还驻留多少"，不是 Gen2 增量——
//       分配太快时对象来不及死就被晋升。压测 A 特意用 60KB 小块
//       （低于 85KB 的 LOH 阈值）让对比干净。
//----------------------------------------------------------------*/
using System;
using System.Collections.Generic;

namespace ProbeDemo.Demos
{
    /// <summary>Demo 10：GC 探针（代际增量判泄漏）。</summary>
    public static class Demo10_GcProbe
    {
        public const string Title = "Demo 10：GC 探针（代际增量判泄漏）";

        public static void Run()
        {
            GcProbe.Sample("压测前");

            // 压测 A：分配 48MB 且全部立刻丢掉。注意用 60KB 小块（低于 85KB 的
            // LOH 阈值）——超过阈值的"大对象"直接进 Gen2（LOH），会把对比弄脏
            GcProbe.Under("短命对象 48MB（小块，立刻丢）", () =>
            {
                for (int i = 0; i < 800; i++) { var junk = new byte[60 * 1024]; junk[0] = 1; }
            });

            // 压测 B：分配 48MB 且存进存活列表——大对象直接进 LOH/Gen2，且被引用攥住
            List<byte[]> keep = new List<byte[]>();
            GcProbe.Under("长命对象 48MB（留存引用）", () =>
            {
                for (int i = 0; i < 48; i++) keep.Add(new byte[1024 * 1024]);
            });

            GcProbe.SampleAfterFullCollect("压测后");
            Console.WriteLine("(对比两段 verdict：回收后还驻留多少 = 泄漏与否的分水岭)");
        }
    }
}
