/*----------------------------------------------------------------
// Demo 08：网络探针 SocketProbe
//
// 覆盖探针：Probes/SocketProbe.cs —— 连接计时 + ProbeStream 字节级流量包装
// 回答的问题："这条 TCP 通道卡在哪一段？收发了多少字节？"
//
// 要点：区分"连不上（握手失败/超时）"和"连上了收不到（首字节延迟）"；
//       一次写 12B 却分两次 Read——粘包/半包的直接证据。
//----------------------------------------------------------------*/
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProbeDemo.Demos
{
    /// <summary>Demo 08：网络探针（连接计时 + 字节级流量）。</summary>
    public static class Demo08_SocketProbe
    {
        public const string Title = "Demo 8：网络探针（连接计时 + 字节级流量）";

        public static void Run()
        {
            Inner().GetAwaiter().GetResult();
        }

        private static async Task Inner()
        {
            // 本地 echo 服务端：收多少回多少（充当"下位机"角色）
            const int port = 5510;
            TcpListener listener = new TcpListener(IPAddress.Loopback, port);
            listener.Start();
            Task serverTask = Task.Run(async () =>
            {
                using (TcpClient s = await listener.AcceptTcpClientAsync())
                using (NetworkStream ns = s.GetStream())
                {
                    var buf = new byte[4096];
                    int n;
                    while ((n = await ns.ReadAsync(buf, 0, buf.Length)) > 0)
                        await ns.WriteAsync(buf, 0, n);         // echo
                }
            });

            using (TcpClient client = await SocketProbe.Connect("升级通道", "127.0.0.1", port))
            using (ProbeStream probed = SocketProbe.Wrap(client.GetStream(), "升级通道", logEach: true))
            {
                for (int i = 1; i <= 3; i++)
                {
                    byte[] payload = Encoding.ASCII.GetBytes($"FW-PACKET-{i:D2}");
                    await probed.WriteAsync(payload, 0, payload.Length);

                    var rx = new byte[payload.Length];
                    await DemoHost.ReadExactAsync(probed, rx, rx.Length);
                    Thread.Sleep(80);   // 模拟逐包节奏
                }
                probed.Snapshot("结束");
            }
            listener.Stop();
            Console.WriteLine("(连接握手耗时、首字节延迟、每包字节数、总流量都已留痕)");
        }
    }
}
