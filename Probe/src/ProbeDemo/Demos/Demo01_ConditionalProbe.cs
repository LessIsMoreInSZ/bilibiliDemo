/*----------------------------------------------------------------
// Demo 01：条件探针 ConditionalProbe（Demo01 = src/ProbeDemo/Demos/Demo01_ConditionalProbe.cs）
//
// 覆盖探针：Probes/ConditionalProbe.cs —— 零开销条件打点（[Conditional("PROBE")]）
// 回答的问题："这一帧到底长什么样？"（Debug 留底，Release 剥除）
//
// 要点：帧打点是网络排障的最基础动作；ConditionalProbe 按 12B 两行排版
//       输出，逐字节与交互图对得上，比人肉数十六进制快得多。
//----------------------------------------------------------------*/
using System;

namespace ProbeDemo.Demos
{
    /// <summary>Demo 01：条件探针（ConditionalProbe）。</summary>
    public static class Demo01_ConditionalProbe
    {
        public const string Title = "Demo 1：条件探针（[Conditional(\"PROBE\")]）";

        public static void Run()
        {
            var frame = new byte[] { 0x00, 0x01, 0x00, 0x00, 0x00, 0x06, 0x01, 0x03, 0x00, 0x00, 0x00, 0x02 };
            ConditionalProbe.Frame("TX", frame);
            ConditionalProbe.Log("按 MBAP 逐字段核对：事务号=0001 协议号=0000 长度=0006 从站=01 功能码=03 起始=0000 数量=0002");
            Console.WriteLine("（业务照常执行，帧已在 [PROBE] 留底）");
        }
    }
}
