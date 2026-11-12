using System.Windows;
using UiProbeDemo.Probes;

namespace UiProbeDemo
{
    /// <summary>探针装配点:全局异常捕获(四路)在此安装。</summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            UiDiagnosticsProbes.InstallExceptionProbes(this);   // §8 全局异常四路
            base.OnStartup(e);
        }
    }
}
