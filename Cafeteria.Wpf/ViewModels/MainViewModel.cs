using System.ComponentModel.Composition;

namespace Cafeteria.Wpf.ViewModels
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class MainViewModel
    {
        [Import]
        public ReservationsViewModel ReservationsViewModel { get; set; }
        
        [Import]
        public DashboardViewModel DashboardViewModel { get; set; }
    }
}
