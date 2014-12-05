using System.ComponentModel.Composition;
using Cafeteria.Wpf.ViewModels;
using MahApps.Metro.Controls;

namespace Cafeteria.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [Export]
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        [Import]
        public MainViewModel MainViewModel
        {
            set { DataContext = value; }
        }
    }
}
