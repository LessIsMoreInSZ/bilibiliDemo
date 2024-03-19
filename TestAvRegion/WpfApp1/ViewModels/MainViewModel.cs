using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WpfApp1.Views;

namespace WpfApp1.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;

        public MainViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            _regionManager.Regions["StackPanelRegion"].RequestNavigate("TestRegionAView", new NavigationParameters());


        }

        public ICommand LoadCommand
        {
            get => new DelegateCommand(() =>
            {
                NavigationParameters keyValuePairs = new NavigationParameters();
                //_regionManager.Regions["StackPanelRegion"].RequestNavigate("TestRegionAView", keyValuePairs);
            });
        }

        public ICommand ClickCommand
        {
            get => new DelegateCommand(() =>
            {
                NavigationParameters keyValuePairs = new NavigationParameters();
                _regionManager.Regions["StackPanelRegion"].RequestNavigate("TestRegionAView", keyValuePairs);
            });
        }
    }
}
