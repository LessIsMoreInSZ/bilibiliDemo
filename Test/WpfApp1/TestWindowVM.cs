using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfApp1
{
    public class TestWindowVM:BindableBase
    {
        private readonly IRegionManager _regionManager;
        public TestWindowVM(IRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public ICommand DiaoCommand
        {
            get => new DelegateCommand(() =>
            {
                _regionManager.Regions["MainViewRegionName"].RequestNavigate("TestView");
            });
        }

        public ICommand QuDiaoCommand
        {
            get => new DelegateCommand(() =>
            {
                _regionManager.Regions["MainViewRegionName"].RemoveAll();
            });
        }
    }
}
