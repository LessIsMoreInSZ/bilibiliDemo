using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int pid;
        public MainWindow()
        {
            InitializeComponent();

            //ProcessStartInfo startInfo = new ProcessStartInfo("notepad.exe");
            ProcessStartInfo startInfo = new ProcessStartInfo("C:\\Program Files\\xxx.exe"
                , "F:\\xxx.vps");
        
            try
            {
                // 启动进程
                using (Process process = Process.Start(startInfo))
                {
                    // 等待进程启动
                    process.WaitForInputIdle();

                    // 获取并打印进程ID
                    pid = process.Id;
                    Console.WriteLine($"进程ID: {pid}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发生错误: {ex.Message}");
            }

            Thread.Sleep(21000);
            //IntPtr hw = Win32API.FindWindow("Chrome_WidgetWin_0", "迅雷");   //第一参数是类名，第二个是窗口标题
            IntPtr hw = Win32API.FindWindow(null, "无标题 - 记事本");   //第一参数是类名，第二个是窗口标题

            Win32API.MoveWindow(hw, 0, 0, pannel_exe.Width, pannel_exe.Height, true);
            Win32API.SetParent(hw, pannel_exe.Handle);

           //Thread.Sleep(20000);

           IReadOnlyList<WindowInfo>  lst = WindowEnumerator.FindAll();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            //int pid = 12345; // 假设这是你要结束的进程的PID
            try
            {
                Process process = Process.GetProcessById(pid);
                process.Kill();
                Console.WriteLine("进程已被结束。");
            }
            catch (ArgumentException)
            {
                Console.WriteLine("无法找到PID为 " + pid + " 的进程。");
            }
        }
    }

    /// <summary>
    /// 包含枚举当前用户空间下所有窗口的方法。
    /// </summary>
    public class WindowEnumerator
    {
        //private delegate bool WndEnumProc(IntPtr hWnd, int lParam);

        //[DllImport("user32")]
        //private static extern bool EnumWindows(WndEnumProc lpEnumFunc, int lParam);

        //[DllImport("user32")]
        //private static extern IntPtr GetParent(IntPtr hWnd);

        //[DllImport("user32")]
        //private static extern bool IsWindowVisible(IntPtr hWnd);

        //[DllImport("user32")]
        //private static extern int GetWindowText(IntPtr hWnd, StringBuilder lptrString, int nMaxCount);

        //[DllImport("user32")]
        //private static extern int GetClassName(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        //[DllImport("user32")]
        //private static extern bool GetWindowRect(IntPtr hWnd, ref LPRECT rect);

        //[StructLayout(LayoutKind.Sequential)]
        //private readonly struct LPRECT
        //{
        //    public readonly int Left;
        //    public readonly int Top;
        //    public readonly int Right;
        //    public readonly int Bottom;

        //}


        /// <summary>
        /// 查找当前用户空间下所有符合条件的窗口。如果不指定条件，将仅查找可见窗口。
        /// </summary>
        /// <param name="match">过滤窗口的条件。如果设置为 null，将仅查找可见窗口。</param>
        /// <returns>找到的所有窗口信息。</returns>
        public static IReadOnlyList<WindowInfo> FindAll(Predicate<WindowInfo> match = null)
        {
            var windowList = new List<WindowInfo>();
            EnumWindows(OnWindowEnum, 0);
            return windowList.FindAll(match ?? DefaultPredicate);
            bool OnWindowEnum(IntPtr hWnd, int lparam)
            {
                // 仅查找顶层窗口。
                if (GetParent(hWnd) == IntPtr.Zero)
                {
                    // 获取窗口类名。
                    var lpString = new StringBuilder(512);
                    GetClassName(hWnd, lpString, lpString.Capacity);
                    var className = lpString.ToString();

                    // 获取窗口标题。
                    var lptrString = new StringBuilder(512);
                    GetWindowText(hWnd, lptrString, lptrString.Capacity);
                    var title = lptrString.ToString().Trim();

                    // 获取窗口可见性。
                    var isVisible = IsWindowVisible(hWnd);

                    // 获取窗口位置和尺寸。
                    LPRECT rect = default;
                    GetWindowRect(hWnd, ref rect);
                    var bounds = new System.Drawing.Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);

                    // 添加到已找到的窗口列表。
                    windowList.Add(new WindowInfo(hWnd, className, title, isVisible, bounds));

                }

                return true;
            }

        }

        /// <summary>
        /// 默认的查找窗口的过滤条件。可见 + 非最小化 + 包含窗口标题。
        /// </summary>
        private static readonly Predicate<WindowInfo> DefaultPredicate = x => x.IsVisible && !x.IsMinimized && x.Title.Length > 0;
        private delegate bool WndEnumProc(IntPtr hWnd, int lParam);

        [DllImport("user32")]
        private static extern bool EnumWindows(WndEnumProc lpEnumFunc, int lParam);

        [DllImport("user32")]
        private static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("user32")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lptrString, int nMaxCount);

        [DllImport("user32")]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32")]
        private static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

        [DllImport("user32")]
        private static extern bool GetWindowRect(IntPtr hWnd, ref LPRECT rect);

        [StructLayout(LayoutKind.Sequential)]
        private readonly struct LPRECT
        {
            public readonly int Left;
            public readonly int Top;
            public readonly int Right;
            public readonly int Bottom;
        }
    }

    /// <summary>
    /// 获取 Win32 窗口的一些基本信息。
    /// </summary>
    public readonly struct WindowInfo
    {
        public WindowInfo(IntPtr hWnd, string className, string title, bool isVisible, System.Drawing.Rectangle bounds) : this()
        {
            Hwnd = hWnd;
            ClassName = className;
            Title = title;
            IsVisible = isVisible;
            Bounds = bounds;
        }

        /// <summary>
        /// 获取窗口句柄。
        /// </summary>
        public IntPtr Hwnd { get; }

        /// <summary>
        /// 获取窗口类名。
        /// </summary>
        public string ClassName { get; }

        /// <summary>
        /// 获取窗口标题。
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// 获取当前窗口是否可见。
        /// </summary>
        public bool IsVisible { get; }

        /// <summary>
        /// 获取窗口当前的位置和尺寸。
        /// </summary>
        public System.Drawing.Rectangle Bounds { get; }

        /// <summary>
        /// 获取窗口当前是否是最小化的。
        /// </summary>
        public bool IsMinimized => Bounds.Left == -32000 && Bounds.Top == -32000;

    }
}