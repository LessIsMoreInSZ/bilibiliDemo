/*----------------------------------------------------------------
// Demo 02：作用域探针 ProbeScope
//
// 覆盖探针：Probes/ProbeScope.cs —— using/委托两种作用域耗时装裹
// 回答的问题："这一步花了多久？退出是正常还是异常？"
//
// 要点：Run() 委托形式在异常时自动标记再原样抛出——探针不改变异常语义
//       （不动语义原则），日志却已经记下了"异常退出 + 耗时"。
//----------------------------------------------------------------*/
using System;

namespace ProbeDemo.Demos
{
    /// <summary>Demo 02：作用域耗时探针（ProbeScope）。</summary>
    public static class Demo02_ProbeScope
    {
        public const string Title = "Demo 2：作用域耗时探针（含异常路径）";

        public static void Run()
        {
            ProbeScope.Run("正常路径：模拟加载配方", () =>
                System.Threading.Thread.Sleep(150));   // 用 Sleep 模拟放大耗时

            try
            {
                ProbeScope.Run("异常路径：模拟读点失败", () =>
                {
                    throw new InvalidOperationException("模拟：点位 PI_001 读取超时");
                });
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("(异常已按预期向上抛出——探针不吞异常，但已自动记录)");
            }
        }
    }
}
