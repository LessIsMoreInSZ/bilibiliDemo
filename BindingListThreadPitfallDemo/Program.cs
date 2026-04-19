using System.Windows.Forms;

namespace BindingListThreadPitfallDemo;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        // Make the cross-thread access fail loudly for teaching/demo purposes.
        Control.CheckForIllegalCrossThreadCalls = true;

        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}
