using System.Diagnostics;

namespace UiProbeDemo.Probes
{
    /// <summary>
    /// 后台类探针的数据源层:探针只负责 采样/捕获/解析(纯观测),
    /// 不负责画、不自己起线程;谁消费谁驱动(仪表盘用 DispatcherTimer 轮询)。
    /// </summary>
    public sealed class ResourceSample
    {
        public DateTime At;
        public double WorkingSetMb;
        public int Handles;
        public int Threads;
        public int Gen0, Gen1, Gen2;
    }

    // ==================== 资源斜率探针(自观测本进程) ====================
    public static class ResourceSeriesProbe
    {
        public static event Action<ResourceSample> Sampled;

        public static void SampleNow()
        {
            using var p = Process.GetCurrentProcess();
            var s = new ResourceSample
            {
                At = DateTime.Now,
                WorkingSetMb = p.WorkingSet64 / 1048576.0,
                Handles = p.HandleCount,
                Threads = p.Threads.Count,
                Gen0 = GC.CollectionCount(0),
                Gen1 = GC.CollectionCount(1),
                Gen2 = GC.CollectionCount(2),
            };
            Sampled?.Invoke(s);
            Report(s);
        }

        [Conditional("PROBE")]
        private static void Report(ResourceSample s) =>
            ProbeLog.Write($"资源采样: 内存 {s.WorkingSetMb:F1}MB / 句柄 {s.Handles} / 线程 {s.Threads} / GC {s.Gen0}/{s.Gen1}/{s.Gen2}", true);
    }

    // ==================== 状态机(演示业务) + 状态流探针 ====================
    public class DemoStateMachine
    {
        public const string Idle = "待机", Feed = "喂料", Heat = "加热", Hold = "保压", Open = "开模", Take = "取件";

        private static readonly string[] Cycle = { Idle, Feed, Heat, Hold, Open, Take, Idle };
        private static readonly (string From, string To)[] Legals =
        {
            (Idle, Feed), (Feed, Heat), (Heat, Hold), (Hold, Open), (Open, Take), (Take, Idle),
        };

        private int _idx;
        public string State => Cycle[_idx];
        public IReadOnlyList<(string From, string To)> LegalEdges => Legals;

        public event Action<string, string> Stepped;   // (from,to) 正常走线:业务事件
        public event Action<string, string> Illegal;   // 野事件引发的非法迁移:探针捕获

        /// <summary>业务:走一步正常工艺循环。</summary>
        public void StepNext()
        {
            var from = State;
            _idx = (_idx + 1) % Cycle.Length;
            Stepped?.Invoke(from, State);
            SteppedReport(from, State);
        }

        /// <summary>业务:模拟跨状态乱序回调(野事件),不改变状态,应被探针逮住。</summary>
        public void StepIllegal()
        {
            var from = State;
            var to = Cycle[Random.Shared.Next(Cycle.Length)];
            if (to == from || Legals.Any(l => l.From == from && l.To == to)) return;
            Illegal?.Invoke(from, to);
            IllegalReport(from, to);
        }

        [Conditional("PROBE")]
        private static void SteppedReport(string f, string t) =>
            ProbeLog.Write($"状态机走线: {f} → {t} (合法迁移)", true);

        [Conditional("PROBE")]
        private static void IllegalReport(string f, string t) =>
            ProbeLog.Write($"非法迁移捕获: {f} → {t} —— 野事件必须打回并留证", true);
    }

    // ==================== 伪从站字节流(演示业务) + 对帧分解探针 ====================
    public sealed class DecodedFrame
    {
        public byte[] Raw;
        public ushort Tx, Len, Addr, Count;
        public byte Proto, Unit, Func;
        public string Error;                       // null = 健康帧
        public bool IsError => Error != null;
    }

    public static class ModbusFrameProbe
    {
        private static ushort _tx;

        /// <summary>业务:造一帧示例报文,偶发坏帧(协议号错/长度字段错/截断半帧)。</summary>
        public static byte[] MakeSampleFrame(Random rng)
        {
            ushort tx = ++_tx;
            ushort addr = (ushort)rng.Next(0, 64);
            ushort cnt = (ushort)rng.Next(1, 10);
            var raw = new byte[12];
            raw[0] = (byte)(tx >> 8); raw[1] = (byte)tx;
            raw[2] = 0; raw[3] = 0;
            raw[4] = 0; raw[5] = 6;
            raw[6] = 1; raw[7] = rng.Next(2) == 1 ? (byte)0x06 : (byte)0x03;
            raw[8] = (byte)(addr >> 8); raw[9] = (byte)addr;
            raw[10] = (byte)(cnt >> 8); raw[11] = (byte)cnt;
            switch (rng.Next(0, 9))
            {
                case 0: raw[2] = 1; break;                  // 协议号坏
                case 1: raw[4] = 0; raw[5] = 9; break;      // 长度字段与实际不符
                case 2: return raw[..9];                    // 截断半帧
            }
            return raw;
        }

        /// <summary>探针:按 MBAP 逐段对帧,坏段定罪。</summary>
        public static DecodedFrame Decode(byte[] raw)
        {
            var d = new DecodedFrame { Raw = raw };
            if (raw == null || raw.Length < 8) { d.Error = "帧长不足 8 字节(粘包/半帧)"; return d; }
            d.Tx = (ushort)((raw[0] << 8) | raw[1]);
            d.Proto = raw[2];
            d.Len = (ushort)((raw[4] << 8) | raw[5]);
            d.Unit = raw[6];
            d.Func = raw[7];
            if (d.Proto != 0) { d.Error = $"协议号 {d.Proto} 应为 0"; return d; }
            if (d.Len + 6 != raw.Length) { d.Error = $"长度字段 {d.Len}+6 与实际 {raw.Length} 字节不符"; return d; }
            if (raw.Length < 12) { d.Error = "PDU 截断,地址/数量缺失"; return d; }
            d.Addr = (ushort)((raw[8] << 8) | raw[9]);
            d.Count = (ushort)((raw[10] << 8) | raw[11]);
            FrameReport(d);
            return d;
        }

        [Conditional("PROBE")]
        private static void FrameReport(DecodedFrame d) =>
            ProbeLog.Write(d.IsError
                ? $"对帧异常: {d.Error} | 原始 {ToHex(d.Raw)}"
                : $"对帧 OK: 事务 {d.Tx} 功能码 0x{d.Func:X2} 地址 {d.Addr} 数量 {d.Count}", true);

        private static string ToHex(byte[] b) =>
            b.Length == 0 ? "(空)" : string.Join(" ", b.Take(16).Select(x => x.ToString("X2")));
    }
}
