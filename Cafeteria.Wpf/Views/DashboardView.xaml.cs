using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Cafeteria.Wpf.ViewModels;
using nGantt.GanttChart;
using nGantt.PeriodSplitter;

namespace Cafeteria.Wpf.Views
{
    /// <summary>
    /// Interaction logic for DashboardView.xaml
    /// </summary>
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
            DataContextChanged += DashboardView_DataContextChanged;
        }

        private string FormatHour(Period period)
        {
            return period.Start.Hour.ToString();
        }

        void DashboardView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var vm = DataContext as DashboardViewModel;

            if (vm == null) return;

            DateTime minDate = vm.MinDate;
            DateTime maxDate = vm.MaxDate;

            GanttControl.ClearGantt();
            GanttControl.Initialize(minDate, maxDate);


            var tables = vm.Tables;
            var reservations = vm.Reservations;

            var rowgroup = GanttControl.CreateGanttRowGroup();

            foreach (var table in tables)
            {
                var row = GanttControl.CreateGanttRow(rowgroup, string.Format("Table {0} ({1})", table.TableId, table.MaxOccupancy));

                var rs = reservations.Where(x => x.TableNumber == table.TableId).ToList();

                foreach (var r in rs)
                {
                    GanttControl.AddGanttTask(row, new GanttTask { Start = r.FromTime, End = r.ToTime, Name = string.Format("{0} ({1})", r.ReservedFor, r.NumberOfPeople), TaskProgressVisibility = System.Windows.Visibility.Hidden });
                }
            }

            var gridLineHourTimeLine = GanttControl.CreateTimeLine(new PeriodHourSplitter(minDate, maxDate), FormatHour);
            GanttControl.SetGridLinesTimeline(gridLineHourTimeLine);
        }
    }
}
