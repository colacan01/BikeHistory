using BikeHistory.Mobile.ViewModels;

namespace BikeHistory.Mobile.Views.Auth
{
    public partial class RegisterPage : ContentPage
    {
        public RegisterPage(RegisterViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}