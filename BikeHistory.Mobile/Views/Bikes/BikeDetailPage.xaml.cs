using BikeHistory.Mobile.ViewModels;

namespace BikeHistory.Mobile.Views.Bikes
{
    public partial class BikeDetailPage : ContentPage
    {
        public BikeDetailPage(BikeDetailViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}