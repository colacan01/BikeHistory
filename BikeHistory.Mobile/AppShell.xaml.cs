using BikeHistory.Mobile.Views.Auth;
using BikeHistory.Mobile.Views.Bikes;

namespace BikeHistory.Mobile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // 라우팅 등록
            Routing.RegisterRoute("login", typeof(LoginPage));
            Routing.RegisterRoute("register", typeof(RegisterPage));
            Routing.RegisterRoute("profile", typeof(ProfilePage));
            Routing.RegisterRoute("bikes/register", typeof(BikeRegisterPage));
            Routing.RegisterRoute("bikes/detail", typeof(BikeDetailPage));
            Routing.RegisterRoute("bikes/transfer", typeof(BikeTransferPage));
        }
    }
}