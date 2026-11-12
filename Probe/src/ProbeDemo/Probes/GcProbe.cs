/*----------------------------------------------------------------
// 示例：内存/GC 探针 GcProbe
//
// 回答的问题："内存是在涨还是在抖？"——涨（只升不降）是泄漏；
//              抖（升了又降）是正常的分配回收节奏。答案就在
//              GC 代际计数和托管堆水位的变化里，不需要任务管理器猜。
//
// 能力：
//   Sample 打一行"体检单"：Gen0/1/2 收集次数、托管堆水位。
//   Under 包住一段压测：输出代际增量 + 完整回收后的驻留量——
//   泄漏的黄金判据是"回收后还驻留多少"，而不是内存曲线涨没涨。
//
// 冤案提示：内存曲线图会冤枉人——看到内存涨未必是泄漏，可能只是 GC
//   还没觉得有必要回收。让 Sample() 在压力前后各打一次，用代际计数说话。
//----------------------------------------------------------------*/
using System;
using System.Diagnostics;
using System.Threading;

namespace ProbeDemo
{
    /// <summary>GC/内存探针：代际收集增量 + 托管堆水位采样。静态类。</summary>
    public static class GcProbe
    {
        /// <summary>打一行 GC 体检单。tag 用来对齐"哪个业务节点前后"。</summary>
        [Conditional("PROBE")]
        public static void Sample(string tag)
        {
            Trace.WriteLine($"{Now} [GC:{tag}] Gen0={GC.CollectionCount(0)} Gen1={GC.CollectionCount(1)} " +
                            $"Gen2={GC.CollectionCount(2)} · 托管堆 {GC.GetTotalMemory(false) / 1024}KB · " +
                            $" HighMem负载={GC.GetGCMemoryInfo().HighMemoryLoadThresholdBytes / (1024 * 1024 * 1024)}GB 阈值");
        }

        /// <summary>强制完整回收后再采样（只用于诊断打点，业务代码里别这么干）。</summary>
        [Conditional("PROBE")]
        public static void SampleAfterFullCollect(string tag)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            Sample(tag + "（完整回收后）");
        }

        /// <summary>
        /// 压测函数外壳：跑一段代码，输出代际增量 + 完整回收后的驻留量。
        /// 泄漏的黄金判据是"回收后还驻留多少"，代际增量只是过程参考——
        /// 分配太快时对象来不及死就会被晋升，Gen2 增量并不等于泄漏。
        /// </summary>
        [Conditional("PROBE")]
        public static void Under(string tag, Action workload)
        {
            int g0 = GC.CollectionCount(0), g1 = GC.CollectionCount(1), g2 = GC.CollectionCount(2);
            long heapBefore = GC.GetTotalMemory(true);
            var watch = Stopwatch.StartNew();

            workload();

            int d0 = GC.CollectionCount(0) - g0, d1 = GC.CollectionCount(1) - g1, d2 = GC.CollectionCount(2) - g2;
            long deltaKb = (GC.GetTotalMemory(false) - heapBefore) / 1024;
            long retainedKb = (GC.GetTotalMemory(true) - heapBefore) / 1024;   // 完整回收后再量：这就是"赖着不走"的部分
            string verdict = retainedKb > 1024
                ? $"回收后仍驻留 {retainedKb:+#,#;-#,#;0}KB —— 有对象被引用攥住（或 LOH 大对象），重点排查 static/事件/缓存/留存集合"
                : $"回收后基本归零 —— 均为短命分配，节奏健康";
            Trace.WriteLine($"{Now} [GC:{tag}] 压测 {watch.ElapsedMilliseconds}ms：Gen0 +{d0} Gen1 +{d1} Gen2 +{d2}，" +
                            $"堆峰值变化 {deltaKb:+#,#;-#,#;0}KB —— {verdict}");
        }

        private static string Now => "[PROBE] " + DateTime.Now.ToString("HH:mm:ss.fff");
    }
}
