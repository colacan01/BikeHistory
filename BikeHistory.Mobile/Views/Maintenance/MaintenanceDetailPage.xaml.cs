using BikeHistory.Mobile.ViewModels;

namespace BikeHistory.Mobile.Views.Maintenance;

public partial class MaintenanceDetailPage : ContentPage
{
    public MaintenanceDetailPage(MaintenanceDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}