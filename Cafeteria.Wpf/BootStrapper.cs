using System.ComponentModel.Composition.Hosting;
using System.Windows;
using Cafeteria.Services.Contracts;
using Microsoft.Practices.Prism.MefExtensions;

namespace Cafeteria.Wpf
{
    class BootStrapper : MefBootstrapper
    {
        protected override DependencyObject CreateShell()
        {
            return Container.GetExportedValue<MainWindow>();
        }

        protected override void ConfigureAggregateCatalog()
        {
            AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(BootStrapper).Assembly));
            AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(ITableReservationService).Assembly));
            base.ConfigureAggregateCatalog();
        }

        protected override void InitializeShell()
        {
            Application.Current.MainWindow = (MainWindow)(Shell);
            Application.Current.MainWindow.Show();
        }
    }
}
