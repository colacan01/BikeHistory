using BikeHistory.Mobile.ViewModels;

namespace BikeHistory.Mobile.Views.Bikes
{
    public partial class BikeRegisterPage : ContentPage
    {
        private readonly BikeRegisterViewModel _viewModel;

        public BikeRegisterPage(BikeRegisterViewModel viewModel)
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