/*----------------------------------------------------------------
// Demo 06：快照对比探针 SnapshotProbe
//
// 覆盖探针：Probes/SnapshotProbe.cs —— 对象状态快照与 diff
// 回答的问题："这个对象的配置，在哪一步之后被谁改掉了？"
//
// 要点：先 Dump 快照，跑完可疑代码段后 Compare——diff 直接点名
//       "哪个字段、从什么改成了什么"。排查"配置漂移"类 bug 的标准打法。
//----------------------------------------------------------------*/
using System;

namespace ProbeDemo.Demos
{
    /// <summary>Demo 06：快照对比探针（谁改了我的配置）。</summary>
    public static class Demo06_SnapshotProbe
    {
        public const string Title = "Demo 6：快照对比探针（谁改了我的配置）";

        class AmpConfig
        {
            public string PortName { get; set; } = "COM3";
            public int BaudRate { get; set; } = 9600;
            public bool AutoReconnect { get; set; } = true;
            public int TimeoutMs { get; set; } = 3000;
        }

        public static void Run()
        {
            AmpConfig config = new AmpConfig();

            string before = SnapshotProbe.Dump(config);   // 快照 1：改动前

            // 模拟"某个隐藏分支"偷偷改了配置——实际排查时通常不知道是哪段代码
            config.BaudRate = 19200;
            config.TimeoutMs = 800;

            SnapshotProbe.Compare("AmpConfig", before, config);   // diff 自动点名

            Console.WriteLine("(diff 直接给出改动字段：BaudRate 9600->19200、TimeoutMs 3000->800)");
        }
    }
}
