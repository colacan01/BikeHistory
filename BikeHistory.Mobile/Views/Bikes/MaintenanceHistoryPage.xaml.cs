using BikeHistory.Mobile.ViewModels;

namespace BikeHistory.Mobile.Views.Bikes
{
    public partial class MaintenanceHistoryPage : ContentPage
    {
        public MaintenanceHistoryPage(MaintenanceHistoryViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}