using System;
using System.Windows.Forms;

namespace CrashTest.Unified;

internal static class Program
{
    /// <summary>
    /// 程序入口；有命令行参数时直接执行对应崩溃触发器，否则启动图形界面。
    /// </summary>
    /// <param name="args">启动参数，第一个参数可指定崩溃类型缩写。</param>
    [STAThread]
    private static void Main(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        if (args.Length > 0)
        {
            CrashTrigger.Execute(args[0]);
            return;
        }

        Application.Run(new CrashSelectorForm());
    }
}
