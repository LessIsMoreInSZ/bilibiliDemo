/*----------------------------------------------------------------
// 公共工具 DemoHost：所有 Demo 共用的装配与辅助方法
//
//   SetupTrace       —— 挂控制台监听器（Trace 默认无监听器，不挂看不到 [PROBE]）
//   ReadExactAsync   —— TCP 流式凑读（半包/粘包的标准应对）
//   Hex              —— 字节数组转十六进制排版
//----------------------------------------------------------------*/
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ProbeDemo.Demos
{
    /// <summary>Demo 公共工具集。</summary>
    public static class DemoHost
    {
        /// <summary>在 Main 最先调用：指向 Console.Out 的 Trace 监听器 + AutoFlush。</summary>
        public static void SetupTrace()
        {
            System.Diagnostics.Trace.Listeners.Add(new System.Diagnostics.TextWriterTraceListener(Console.Out));
            System.Diagnostics.Trace.AutoFlush = true;
        }

        /// <summary>凑读：直到读满 n 字节或对端关闭。TCP 流式读必须用它，不能假定一次 Read 读齐一帧。</summary>
        public static async Task ReadExactAsync(Stream s, byte[] buf, int n)
        {
            int got = 0;
            while (got < n)
            {
                int r = await s.ReadAsync(buf, got, n - got);
                if (r == 0) throw new IOException("对端关闭");
                got += r;
            }
        }

        /// <summary>字节数组转十六进制（每字节后跟一个空格）。</summary>
        public static string Hex(byte[] data)
        {
            var sb = new StringBuilder(data.Length * 3);
            foreach (byte b in data) sb.Append(b.ToString("X2")).Append(' ');
            return sb.ToString();
        }
    }
}
