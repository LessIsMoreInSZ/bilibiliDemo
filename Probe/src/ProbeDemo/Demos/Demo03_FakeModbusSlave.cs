/*----------------------------------------------------------------
// Demo 03：回环帧探针 FakeModbusSlaveProbe
//
// 覆盖探针：Probes/FakeModbusSlave.cs —— TcpListener 伪从站，逐字段核帧
// 回答的问题："主站发出去的帧/收回来的应答，每个字段是否符合交互图？"
//
// 要点：TCP 流式读必须"凑读"（ReadExactAsync）——半包/粘包不是 bug，
//       是 TCP 的本性行为；伪从站把 RX/TX 全部留痕，对帧一目了然。
//----------------------------------------------------------------*/
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ProbeDemo.Demos
{
    /// <summary>Demo 03：回环帧探针（伪从站 + 最小主站对帧）。</summary>
    public static class Demo03_FakeModbusSlave
    {
        public const string Title = "Demo 3：回环帧探针（伪从站 + 最小主站）";

        public static void Run()
        {
            Inner().GetAwaiter().GetResult();
        }

        private static async Task Inner()
        {
            const int port = 5502;
            var slave = new FakeModbusSlaveProbe(port);
            slave.Start();
            Thread.Sleep(200);       // 等监听就绪

            using (TcpClient client = new TcpClient())
            {
                await client.ConnectAsync("127.0.0.1", port);
                NetworkStream stream = client.GetStream();

                // 组一帧标准 MBAP 读保持寄存器请求（0x03，从站 1，起始 0，数量 2）
                byte[] request =
                {
                    0x00, 0x01,       // 事务号
                    0x00, 0x00,       // 协议号
                    0x00, 0x06,       // 长度 = 单元标识(1) + PDU(5)
                    0x01,             // 单元标识
                    0x03,             // 功能码：读保持寄存器
                    0x00, 0x00,       // 起始地址
                    0x00, 0x02        // 数量
                };

                Console.WriteLine("主站 TX: " + DemoHost.Hex(request));
                await stream.WriteAsync(request, 0, request.Length);

                // 读应答：先 7 字节 MBAP 头，再按长度取 PDU（TCP 流式必须凑读）
                var header = new byte[7];
                await DemoHost.ReadExactAsync(stream, header, 7);
                int pduLen = ((header[4] << 8) | header[5]) - 1;
                var pdu = new byte[pduLen];
                await DemoHost.ReadExactAsync(stream, pdu, pduLen);

                Console.WriteLine("主站 RX: " + DemoHost.Hex(header) + DemoHost.Hex(pdu));
                Console.WriteLine($"解析：寄存器值 = 0x{pdu[2]:X2}{pdu[3]:X2}, 0x{pdu[4]:X2}{pdu[5]:X2}");
            }

            Thread.Sleep(300);       // 给伪从站打印留时间
        }
    }
}
