using System.Windows;

namespace Cafeteria.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var bootStrapper = new BootStrapper();
            bootStrapper.Run();
            base.OnStartup(e);
        }
    }
}
