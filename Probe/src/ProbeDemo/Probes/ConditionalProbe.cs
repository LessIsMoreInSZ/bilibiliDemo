/*----------------------------------------------------------------
// 示例：零开销条件探针 ConditionalProbe
// 核心：System.Diagnostics.ConditionalAttribute —— 标记的方法体
// 只在定义了对应编译符号时编入。Release 发布不带 PROBE 符号时，
// 所有 Probe.Log(...) 调用点连同方法体被编译器剥除，运行零开销，
// 调用参数（含字符串拼接）也不会执行。
// 
// 调试构建（植入探针）:   csc /define:PROBE ...
// 发布构建（自动剥除）:   csc /optimize ...
//----------------------------------------------------------------*/
using System;
using System.Diagnostics;

namespace ProbeDemo
{
    public static class ConditionalProbe
    {
        public static bool Enabled =
#if PROBE
            true;
#else
            false;
#endif

        [Conditional("PROBE")]
        public static void Log(string message)
        {
            Trace.WriteLine($"[PROBE {DateTime.Now:HH:mm:ss.fff}] {message}");
        }

        [Conditional("PROBE")]
        public static void Frame(string direction, byte[] data)
        {
            Trace.WriteLine($"[PROBE {Direction(direction)} {data.Length}B] {ToHex(data)}");
        }

        private static string Direction(string direction)
        {
            if (string.Equals(direction, "TX", StringComparison.OrdinalIgnoreCase)) return "发 >>>";
            if (string.Equals(direction, "RX", StringComparison.OrdinalIgnoreCase)) return "收 <<<";
            return direction ?? "未知方向";
        }

        private static string ToHex(byte[] data)
        {
            if (data == null) return "(null)";
            var sb = new System.Text.StringBuilder(data.Length * 3);
            for (int i = 0; i < data.Length; i++)
            {
                if (i > 0) sb.Append(' ');
                sb.Append(data[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }

    // ---------------- 使用演示 ----------------
    public class Demo
    {
        public void Transceive(int slave, byte[] frame)
        {
            // Release 下这一行连 $"..." 拼接都不会执行
            ConditionalProbe.Frame("TX", frame);

            byte[] response = ModbusLike(slave, frame);

            ConditionalProbe.Frame("RX", response);
            ConditionalProbe.Log($"从站 {slave} 本次往返 {frame.Length + response.Length} 字节");
        }

        private byte[] ModbusLike(int slave, byte[] req)
        {
            // 示意：回显一个应答帧（地址+功能码+CRC 占位）
            var resp = new byte[] { (byte)slave, req[1], 0x02, 0x12, 0x34, 0xA1, 0x53 };
            return resp;
        }
    }
}
