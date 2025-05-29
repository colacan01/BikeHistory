using BikeHistory.Mobile.ViewModels;

namespace BikeHistory.Mobile.Views.Bikes
{
    public partial class BikeListPage : ContentPage
    {
        private readonly BikeListViewModel _viewModel;

        public BikeListPage(BikeListViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.OnAppearing();
        }
    }
}