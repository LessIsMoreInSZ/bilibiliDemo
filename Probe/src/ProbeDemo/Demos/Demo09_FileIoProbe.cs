/*----------------------------------------------------------------
// Demo 09：文件 IO 探针 FileIoProbe
//
// 覆盖探针：Probes/FileIoProbe.cs —— SHA256 内容指纹 + FileSystemWatcher 变更哨兵
// 回答的问题："这个文件是不是被人改了？什么时候、被谁改的？"
//
// 要点：Measure 已改为不挂 Conditional（业务必须照常执行，只有输出被剥除）
//       ——业务代码放进 Conditional 委托会被 Release 连业务一起剥掉（铁律）。
//----------------------------------------------------------------*/
using System;
using System.IO;
using System.Threading;

namespace ProbeDemo.Demos
{
    /// <summary>Demo 09：文件 IO 探针（内容指纹 + 变更哨兵）。</summary>
    public static class Demo09_FileIoProbe
    {
        public const string Title = "Demo 9：文件 IO 探针（内容指纹 + 变更哨兵）";

        public static void Run()
        {
            string dir = Path.Combine(Path.GetTempPath(), "ProbeDemo_FileIo");
            Directory.CreateDirectory(dir);
            string cfg = Path.Combine(dir, "app.cfg");
            string bak = Path.Combine(dir, "app.cfg.bak");

            // 变更哨兵:盯住目录，谁动了文件立刻留痕
            using (FileSystemWatcher fsw = FileIoProbe.Watch("配置目录", dir))
            {
                FileIoProbe.Measure("写入参数文件", () =>
                    File.WriteAllText(cfg, "ip=192.168.168.200\nport=502\n"));
                Thread.Sleep(120);                     // 哨兵是异步事件，留点时间落地

                File.Copy(cfg, bak, overwrite: true);
                FileIoProbe.Fingerprint("写入后", cfg);
                FileIoProbe.Fingerprint("备份", bak);
                FileIoProbe.Compare("备份核对", cfg, bak);

                // 模拟"半夜谁来动了我的配置"：外部把内容改掉
                File.WriteAllText(cfg, "ip=192.168.168.210\nport=502\n");
                Thread.Sleep(200);
                FileIoProbe.Fingerprint("可疑改动后", cfg);
                FileIoProbe.Compare("与备份对照", cfg, bak);   // 直接点名"内容不同（被改动）"
            }
            Console.WriteLine("(哨兵捕获了 Created/Changed 事件，指纹对比直接点名内容被改)");
            Directory.Delete(dir, recursive: true);
        }
    }
}
