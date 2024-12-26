using Prism.Ioc;
using Prism.Modularity;
using Prism.Unity;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>

    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            //return Container.Resolve<MainWindow>();
            return Container.Resolve<TestWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<MainWindow, Data2CsvViewModel>();
            containerRegistry.RegisterForNavigation<TestWindow, TestWindowVM>();
        }

        protected override IModuleCatalog CreateModuleCatalog()
        {
            DirectoryModuleCatalog catelog = new DirectoryModuleCatalog();
            try
            {
                if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, "Functions")))
                {
                    Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "Functions"));
                }
                else
                {
                    catelog.ModulePath = Path.Combine(AppContext.BaseDirectory, "Functions");
                }
            }
            catch (Exception ex)
            {
                //_logger.Error("load dll fail," + ex.Message);
            }
            return catelog;
        }
    }

}
