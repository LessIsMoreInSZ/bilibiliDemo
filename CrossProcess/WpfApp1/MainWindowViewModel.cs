using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace WpfApp1
{
    public class MainWindowViewModel: BindableBase
    {
        public ICommand TestCommand
        {
            get => new DelegateCommand(() =>
            {
                //MessageBox.Show("666");
            });
        }
    }
}
