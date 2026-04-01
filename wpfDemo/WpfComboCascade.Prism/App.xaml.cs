using System.Windows;
using Prism.DryIoc;
using Prism.Ioc;

namespace WpfComboCascade.PrismDemo;

public partial class App : PrismApplication
{
    protected override Window CreateShell()
    {
        return Container.Resolve<MainWindow>();
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
    }
}
