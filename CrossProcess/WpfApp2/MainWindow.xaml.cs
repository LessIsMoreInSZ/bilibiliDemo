using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp2
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //IntPtr hw = Win32API.FindWindow("Chrome_WidgetWin_0", "迅雷");   //第一参数是类名，第二个是窗口标题
            IntPtr hw = Win32API.FindWindow(null, "无标题 - 记事本");   //第一参数是类名，第二个是窗口标题
            Win32API.MoveWindow(hw, 0, 0, pannel_exe.Width, pannel_exe.Height, true);
            Win32API.SetParent(hw, pannel_exe.Handle);
        }
    }
}
