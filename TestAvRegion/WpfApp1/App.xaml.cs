using Prism;
using Prism.Ioc;
using Prism.Regions;
using Prism.Unity;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using WpfApp1.ViewModels;
using WpfApp1.Views;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            //捕获全局异常
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<TestRegionAView, TestRegionAViewModel>();
            containerRegistry.RegisterForNavigation<MainWindow, MainViewModel>();

        }

        protected override void ConfigureRegionAdapterMappings(RegionAdapterMappings regionAdapterMappings)
        {
            base.ConfigureRegionAdapterMappings(regionAdapterMappings);
            regionAdapterMappings.RegisterMapping(typeof(StackPanel),Container.Resolve<StackPanelRegionAdapter>());
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = e.ExceptionObject as Exception;
            if (exception == null)
            {
                return;
            }
            //_logger.Error(exception.Message);
        }
    }

}
