using BikeHistory.Mobile.ViewModels;

namespace BikeHistory.Mobile.Views.Bikes
{
    public partial class BikeTransferPage : ContentPage
    {
        private readonly BikeTransferViewModel _viewModel;

        public BikeTransferPage(BikeTransferViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }
    }
}