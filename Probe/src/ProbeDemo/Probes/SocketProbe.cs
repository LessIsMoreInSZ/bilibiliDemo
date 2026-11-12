/*----------------------------------------------------------------
// 示例：网络通信探针 SocketProbe
//
// 回答的问题："这条 TCP 通道到底通没通？卡在连接、发出去还是收回来？
//              收发了多少字节？帧是不是被'拆开'读了好几次？"
//
// 能力：
//   1) 连接计时：Connect 包住 TcpClient.Connect，区分"连不上（网络问题）"
//      和"连上了但读不到数据（对端卡了）"——两类故障的处理方式完全不同。
//   2) ProbeStream 包装：对 NetworkStream 套一层 Stream，逐次读/写打点：
//      字节数、累计流量、首次读到数据（首字节延迟）、一帧被 Read 拆散的次数。
//
// 说明：ProbeStream 是"包一层"的探针，本身有微小转发开销，适合边界通道
//       （上位机与下位机、客户端与服务端之间的长连接）；高频小包路径
//       关掉打点（logEach=false）只留字节计数，开销可忽略。
//
// 场景：设备"升级到一半断流"、偶发 SocketException、怀疑对端没回数据。
//----------------------------------------------------------------*/
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ProbeDemo
{
    /// <summary>连接 + 流量探针：连接计时（静态）与字节级 Stream 包装（实例）。</summary>
    public static class SocketProbe
    {
        /// <summary>带打点的连接：成功打印握手耗时，失败打印目标与异常类型后原样抛出。</summary>
        public static async Task<TcpClient> Connect(string name, string host, int port, int timeoutMs = 5000)
        {
            if (timeoutMs <= 0)
                throw new ArgumentOutOfRangeException(nameof(timeoutMs), "连接超时必须大于 0。");

            var client = new TcpClient();
            var watch = Stopwatch.StartNew();
            try
            {
                using (var timeout = new CancellationTokenSource(timeoutMs))
                {
                    try
                    {
                        await client.ConnectAsync(host, port, timeout.Token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException ex) when (timeout.IsCancellationRequested)
                    {
                        throw new TimeoutException($"{name} 连接 {host}:{port} 超过 {timeoutMs}ms 未完成", ex);
                    }
                }

                Write($"{Now} [NET:{name}] 已连接 {host}:{port}（握手 {watch.ElapsedMilliseconds}ms，本机端口 {((System.Net.IPEndPoint)client.Client.LocalEndPoint).Port}）");
                return client;
            }
            catch (Exception ex)
            {
                Write($"{Now} [NET:{name}] 连接失败 {host}:{port} —— {ex.GetType().Name}: {ex.Message}（耗时 {watch.ElapsedMilliseconds}ms）");
                client.Dispose();
                throw;
            }
        }

        /// <summary>给流套上探针包装。logEach=false 时只累计计数不逐次打点。</summary>
        public static ProbeStream Wrap(Stream inner, string name, bool logEach = true)
        {
            if (inner == null) throw new ArgumentNullException(nameof(inner));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("探针名称不能为空。", nameof(name));
            return new ProbeStream(inner, name, logEach);
        }

        [Conditional("PROBE")]
        private static void Write(string line)
        {
            Trace.WriteLine(line);
        }

        private static string Now => "[PROBE] " + DateTime.Now.ToString("HH:mm:ss.fff");
    }

    /// <summary>字节级流量包装：同步/异步 Read/Write 计数与打点。</summary>
    public sealed class ProbeStream : Stream
    {
        private readonly Stream _inner;
        private readonly string _name;
        private readonly bool _logEach;

        private long _txBytes, _rxBytes, _txOps, _rxOps;
        private readonly Stopwatch _watch = Stopwatch.StartNew();
        private int _firstReadLogged;

        internal ProbeStream(Stream inner, string name, bool logEach)
        {
            _inner = inner;
            _name = name;
            _logEach = logEach;
        }

        public override bool CanRead => _inner.CanRead;
        public override bool CanSeek => _inner.CanSeek;
        public override bool CanWrite => _inner.CanWrite;
        public override long Length => _inner.Length;
        public override long Position { get => _inner.Position; set => _inner.Position = value; }
        public override void Flush() { _inner.Flush(); }
        public override Task FlushAsync(CancellationToken cancellationToken) => _inner.FlushAsync(cancellationToken);
        public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);
        public override void SetLength(long value) => _inner.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count)
        {
            _inner.Write(buffer, offset, count);
            OnWrite(count);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return WriteAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
        }

        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            await _inner.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
            OnWrite(buffer.Length);
        }

        private void OnWrite(int count)
        {
            long total = Interlocked.Add(ref _txBytes, count);
            long ops = Interlocked.Increment(ref _txOps);
            if (_logEach)
                WriteProbe($"{Now} [NET:{_name}] TX +{count}B（累计 {total}B / 第 {ops} 次写）");
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken ct)
        {
            return await ReadAsync(buffer.AsMemory(offset, count), ct).ConfigureAwait(false);
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            int got = await _inner.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            OnRead(got);
            return got;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int got = _inner.Read(buffer, offset, count);
            OnRead(got);
            return got;
        }

        private void OnRead(int got)
        {
            long total = Interlocked.Add(ref _rxBytes, got);
            long ops = Interlocked.Increment(ref _rxOps);
            if (got > 0 && Interlocked.Exchange(ref _firstReadLogged, 1) == 0)
            {
                WriteProbe($"{Now} [NET:{_name}] 首次收到数据（首字节延迟 {_watch.ElapsedMilliseconds}ms，本次 +{got}B）");
            }
            if (_logEach)
                WriteProbe($"{Now} [NET:{_name}] RX +{got}B（累计 {total}B / 第 {ops} 次读）");
        }

        /// <summary>快照：本地观察一次收发全貌（对帧次数多=一帧被拆成多次 Read，典型的粘包/半包证据）。</summary>
        public void Snapshot(string tag)
        {
            long rxBytes = Interlocked.Read(ref _rxBytes);
            long rxOps = Interlocked.Read(ref _rxOps);
            long txBytes = Interlocked.Read(ref _txBytes);
            long txOps = Interlocked.Read(ref _txOps);
            WriteProbe($"{Now} [NET:{_name}] 快照[{tag}] RX {rxBytes}B/{rxOps}次 · TX {txBytes}B/{txOps}次" +
                       (rxOps > 0 ? $" · 平均每次读 {rxBytes / rxOps}B" : ""));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                WriteProbe($"{Now} [NET:{_name}] 关闭（存活 {_watch.ElapsedMilliseconds}ms，RX {Interlocked.Read(ref _rxBytes)}B / TX {Interlocked.Read(ref _txBytes)}B）");
                _inner.Dispose();
            }
            base.Dispose(disposing);
        }

        [Conditional("PROBE")]
        private void WriteProbe(string line)
        {
            Trace.WriteLine(line);
        }

        private static string Now => "[PROBE] " + DateTime.Now.ToString("HH:mm:ss.fff");
    }
}
