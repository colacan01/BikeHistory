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
            
            // Set the bottom sheet container after the page is loaded
            await Task.Delay(100); // Ensure layout is complete
            _viewModel.SetBottomSheetContainer(BikeMenuBottomSheet);
            
            await _viewModel.OnAppearing();
        }
    }
}