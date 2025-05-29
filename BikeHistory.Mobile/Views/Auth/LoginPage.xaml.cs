using BikeHistory.Mobile.ViewModels;

namespace BikeHistory.Mobile.Views.Auth
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage(LoginViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}