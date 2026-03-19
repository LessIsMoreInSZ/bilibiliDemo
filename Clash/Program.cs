using System;
using System.Windows.Forms;

namespace CrashTest.Unified;

internal static class Program
{
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
