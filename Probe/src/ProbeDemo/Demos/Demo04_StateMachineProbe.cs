/*----------------------------------------------------------------
// Demo 04：状态机探针 StateMachineProbe
//
// 覆盖探针：Probes/StateMachineProbe.cs —— 状态流证明 + 非法迁移捕获
// 回答的问题："流程实际怎么走的？每步停了多久？有没有走过不该走的路？"
//
// 要点：把"业务允许的状态迁移表"交给探针，探针只做两件事——
//       ①记录实际序列与每态滞留时长 ②发现非法迁移就当场点名。
//       DumpHistory 输出的完整状态流，是"流程类 bug"的铁证。
//----------------------------------------------------------------*/
using System;
using System.Threading;

namespace ProbeDemo.Demos
{
    /// <summary>Demo 04：状态机探针（状态流证明 + 非法迁移捕获）。</summary>
    public static class Demo04_StateMachineProbe
    {
        public const string Title = "Demo 4：状态机探针（状态流证明 + 非法迁移捕获）";

        enum DeviceState { Idle, Loading, Running, Paused, Error, Done }

        public static void Run()
        {
            // 合法迁移表（按业务定义；这里示例一套常见的流程约束）
            bool IsLegal(DeviceState from, DeviceState to)
            {
                switch (from)
                {
                    case DeviceState.Idle:    return to == DeviceState.Loading;
                    case DeviceState.Loading: return to == DeviceState.Running || to == DeviceState.Error;
                    case DeviceState.Running: return to == DeviceState.Paused || to == DeviceState.Done || to == DeviceState.Error;
                    case DeviceState.Paused:  return to == DeviceState.Running || to == DeviceState.Error;
                    case DeviceState.Error:   return to == DeviceState.Idle;
                    default: return false;
                }
            }

            StateMachineProbe<DeviceState> probe = new StateMachineProbe<DeviceState>("主流程", IsLegal);

            // 一段正常流程：Idle -> Loading -> Running
            probe.OnEnter(DeviceState.Idle);
            Thread.Sleep(120);       // 模拟各步骤的真实耗时
            probe.OnEnter(DeviceState.Loading);
            Thread.Sleep(230);
            probe.OnEnter(DeviceState.Running);
            Thread.Sleep(450);
            // 模拟 bug：跳过中间关系直接回 Idle（非法！）
            probe.OnEnter(DeviceState.Idle);

            // 事后导出完整状态流（实际项目里一般 Dump 进日志文件）
#if PROBE
            Console.WriteLine(probe.DumpHistory());
#endif
        }
    }
}
