/*----------------------------------------------------------------
// 探针技术演示入口（调度器）
//
// 每个 Demo 独立成文件：Demos/DemoNN_探针名.cs，与 Probes/ 里的
// 探针类一一对应。想单独看某个探针的用法，直接打开对应 Demo 文件；
// 想单独运行某个 Demo，在下面的清单里注释掉其余条目即可。
//
// Demo 01~06 基础类：条件/作用域/回环帧/状态机/频次资源/快照对比
// Demo 07~12 细分领域（讲解见《04-细分领域探针.md》）
//   07 线程锁  08 网络  09 文件IO  10 GC  11 异常流  12 派发延迟
//
// 运行：dotnet run --project src/ProbeDemo
// 发布：dotnet publish -c Release  观察输出里 [PROBE] 全部消失
//----------------------------------------------------------------*/
using System;
using ProbeDemo.Demos;

class Program
{
    static void Main()
    {
        // Trace 默认无监听器，控制台看不到输出；先挂监听器再跑
        DemoHost.SetupTrace();

        (string Title, Action Run)[] demos =
        {
            (Demo01_ConditionalProbe.Title,  Demo01_ConditionalProbe.Run),
            (Demo02_ProbeScope.Title,        Demo02_ProbeScope.Run),
            (Demo03_FakeModbusSlave.Title,   Demo03_FakeModbusSlave.Run),
            (Demo04_StateMachineProbe.Title, Demo04_StateMachineProbe.Run),
            (Demo05_RateAndResourceProbe.Title, Demo05_RateAndResourceProbe.Run),
            (Demo06_SnapshotProbe.Title,     Demo06_SnapshotProbe.Run),
            (Demo07_ThreadingProbe.Title,    Demo07_ThreadingProbe.Run),
            (Demo08_SocketProbe.Title,       Demo08_SocketProbe.Run),
            (Demo09_FileIoProbe.Title,       Demo09_FileIoProbe.Run),
            (Demo10_GcProbe.Title,           Demo10_GcProbe.Run),
            (Demo11_ExceptionProbe.Title,    Demo11_ExceptionProbe.Run),
            (Demo12_PumpProbe.Title,         Demo12_PumpProbe.Run),
        };

        foreach ((string title, Action run) in demos)
        {
            Console.WriteLine($"=== {title} ===");
            run();
            Console.WriteLine();
        }

        Console.WriteLine("演示结束。上面凡带 [PROBE] 前缀的行，Release 发布构建中全部不存在。");
    }
}
