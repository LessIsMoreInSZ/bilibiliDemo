/*----------------------------------------------------------------
// Demo 11：异常流探针 ExceptionProbe
//
// 覆盖探针：Probes/ExceptionProbe.cs —— FirstChanceException 按类型统计
// 回答的问题："程序里到底在发生什么异常？——包括被 catch 吞掉的。"
//
// 要点：空 catch 是"静默失效"的头号帮凶；FirstChanceException 抓的是
//       抛出的第一现场，不改变异常去路（不动语义原则）。
//----------------------------------------------------------------*/
using System;

namespace ProbeDemo.Demos
{
    /// <summary>Demo 11：异常流探针（照出被吞掉的异常）。</summary>
    public static class Demo11_ExceptionProbe
    {
        public const string Title = "Demo 11：异常流探针（照出被吞掉的异常）";

        public static void Run()
        {
            ExceptionProbe.Install();
            ExceptionProbe.Reset("阶段1");

            // 场景：一段"静默失败"的解析循环，所有异常都被空 catch 吞掉
            int ok = 0;
            for (int i = 0; i < 300; i++)
            {
                try
                {
                    ok += int.Parse(i % 3 == 0 ? "abc" : i.ToString());   // 1/3 概率解析失败
                }
                catch { /* 实际项目里最常见的反模式：吞了 */ }
            }
            try { throw new InvalidOperationException("模拟：心跳丢失"); } catch { }
            try { throw new TimeoutException("模拟：读点超时"); } catch { }
            try { throw new TimeoutException("模拟：读点超时"); } catch { }

            ExceptionProbe.Report("阶段1");
            Console.WriteLine("(循环 300 次解析，1/3 概率失败——全部被空 catch 吞掉，" +
                              "业务表面风平浪静，但 [EXC] 照出了 100 次 FormatException + 3 次其他异常)");
        }
    }
}
