using System.Diagnostics;
using System.Windows.Controls;

namespace UiProbeDemo.Probes
{
    /// <summary>
    /// 界面版输出通道:Trace 的屏幕替身。探针与业务共用一个日志面板。
    /// Write(msg, true) 的打点带 [PROBE] 序号;仪表盘窗口打开时兼任第二块屏幕。
    /// </summary>
    public static class ProbeLog
    {
        private static TextBox _sink;
        private static TextBox _second;
        private static int _seq;

        public static void Bind(TextBox sink) => _sink = sink;

        /// <summary>仪表盘等副屏:主窗口与子窗口同时看到同一份事件流。</summary>
        public static void BindSecondary(TextBox sink) => _second = sink;

        public static void UnbindSecondary(TextBox sink)
        {
            if (ReferenceEquals(_second, sink)) _second = null;
        }

        public static void Write(string msg, bool isProbe = false)
        {
            var line = isProbe ? $"[PROBE #{++_seq:D3}] {msg}" : msg;
            Append(_sink, line);
            Append(_second, line);
        }

        private static void Append(TextBox tb, string line)
        {
            if (tb == null) return;
            tb.Dispatcher.BeginInvoke(() =>
            {
                tb.AppendText(line + Environment.NewLine);
                tb.ScrollToEnd();
            });
        }

        /// <summary>探针生命周期播报(启动/停止)——挂 [PROBE],Release 同样剥除。</summary>
        [Conditional("PROBE")]
        public static void Announce(string msg) => Write(msg, true);
    }
}
