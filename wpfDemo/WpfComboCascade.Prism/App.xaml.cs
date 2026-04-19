using System.Windows;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Mvvm;
using WpfComboCascade.PrismDemo.ViewModels;

namespace WpfComboCascade.PrismDemo;

public partial class App : PrismApplication
{
    protected override Window CreateShell()
    {
        return Container.Resolve<MainWindow>();
    }

    protected override void ConfigureViewModelLocator()
    {
        base.ConfigureViewModelLocator();
        ViewModelLocationProvider.Register<MainWindow, MainWindowViewModel>();
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
    }
}
