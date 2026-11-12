/*----------------------------------------------------------------
// 实战示例：伪从站回环探针（TcpListener 伪造 Modbus TCP 从站）
// 用途：不接真实设备，在 127.0.0.1 上观测主站的每一帧 MBAP 报文，
//       逐字段解析打印；对 0x03 读保持寄存器回一个固定应答。
//
// 复现 Mitsubishi.MotorTest 案例 1：把被测的 ModbusTcpMaster 链进来，
// 连 http://127.0.0.1:5502，主站发什么这里就看到什么——
// 组帧 bug（少 1 字节 / 长度字段错）在十六进制流下无所遁形。
//
// 运行：任一 C# 工程 Program.Main 里 new FakeModbusSlaveProbe(5502).Start();
//----------------------------------------------------------------*/
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ProbeDemo
{
    public sealed class FakeModbusSlaveProbe
    {
        private readonly int _port;
        public FakeModbusSlaveProbe(int port) { _port = port; }

        public void Start()
        {
            var listener = new TcpListener(IPAddress.Loopback, _port);
            listener.Start();
            Console.WriteLine($"[SLAVE] 伪从站监听 127.0.0.1:{_port}，等待主站连接...");
            listener.AcceptTcpClientAsync().ContinueWith(t => Serve(t.Result));
        }

        private async void Serve(TcpClient client)
        {
            Console.WriteLine($"[SLAVE] 主站已连接 {client.Client.RemoteEndPoint}");
            var stream = client.GetStream();
            var header = new byte[7]; // MBAP：事务号2+协议号2+长度2+单元标识1

            try
            {
                while (true)
                {
                    // ---- 探针核心：完整读下一个 PDU 并原样打印 ----
                    if (!await ReadExactAsync(stream, header, 7)) break;
                    int mbapLength = (header[4] << 8) | header[5];
                    if (mbapLength < 2 || mbapLength > 254)
                    {
                        Console.WriteLine($"[SLAVE] !! MBAP 长度无效: {mbapLength}（合法范围 2..254）");
                        break;
                    }

                    int pduLen = mbapLength - 1;  // 长度字段包含单元标识，减 1 得到 PDU 长度
                    var pdu = new byte[pduLen];
                    if (!await ReadExactAsync(stream, pdu, pduLen)) break;

                    ushort transaction = (ushort)((header[0] << 8) | header[1]);
                    ushort protocol   = (ushort)((header[2] << 8) | header[3]);
                    byte   unit       = header[6];

                    Console.WriteLine("[SLAVE] RX 完整帧: " + Hex(header) + Hex(pdu));
                    if (protocol != 0)
                    {
                        Console.WriteLine("[SLAVE] !! 协议号非 0 —— 主站组帧错位");
                        continue;
                    }

                    Console.WriteLine($"[SLAVE] 解析: 事务号={transaction} 协议号={protocol}(应=0) " +
                                      $"长度={mbapLength} 单元={unit} 功能码=0x{pdu[0]:X2}");

                    // ---- 对 0x03 回一个合法应答（读 2 个寄存器）----
                    if (pdu[0] == 0x03)
                    {
                        if (pduLen != 5)
                        {
                            Console.WriteLine($"[SLAVE] !! 0x03 请求 PDU 长度应为 5，实际为 {pduLen}");
                            continue;
                        }

                        var resp = new byte[] { header[0], header[1], 0x00, 0x00, 0x00, 0x07, unit,
                                                0x03, 0x04, 0x12, 0x34, 0x56, 0x78 };
                        await stream.WriteAsync(resp, 0, resp.Length);
                        Console.WriteLine("[SLAVE] TX 应答: " + Hex(resp));
                    }
                    else
                    {
                        Console.WriteLine($"[SLAVE] !! 暂不支持功能码 0x{pdu[0]:X2}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[SLAVE] 探针异常（不要吞，要打出来）: " + ex.Message);
            }
            finally
            {
                Console.WriteLine("[SLAVE] 连接结束");
            }
        }

        // TCP 是流式协议：必须凑满 n 字节，Read 一次不够
        private static async System.Threading.Tasks.Task<bool> ReadExactAsync(NetworkStream s, byte[] buf, int n)
        {
            int got = 0;
            while (got < n)
            {
                int r = await s.ReadAsync(buf, got, n - got);
                if (r == 0) return false;
                got += r;
            }
            return true;
        }

        private static string Hex(byte[] data)
        {
            var sb = new StringBuilder(data.Length * 3);
            foreach (byte b in data) sb.Append(b.ToString("X2")).Append(' ');
            return sb.ToString();
        }
    }
}
