using BikeHistory.Mobile.Services;
using BikeHistory.Mobile.Views.Auth;
using BikeHistory.Mobile.Views.Bikes;
using BikeHistory.Mobile.Views.Maintenance;
using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace BikeHistory.Mobile
{
    public partial class AppShell : Shell, INotifyPropertyChanged
    {
        private readonly AuthService _authService;
        private readonly ActivityLoggerService _activityLogger;
        private string _userFullName = string.Empty;

        public string FullName
        {
            get => _userFullName;
            set
            {
                if (_userFullName != value)
                {
                    _userFullName = value;
                    OnPropertyChanged(nameof(FullName));
                }
            }
        }

        public AppShell(AuthService authService, ActivityLoggerService activityLogger)
        {
            InitializeComponent();

            _authService = authService;
            _activityLogger = activityLogger;

            // 네비게이션 이벤트 구독
            Navigated += OnShellNavigated;

            // 인증 상태 변경 이벤트 구독
            _authService.AuthenticationStateChanged += OnAuthenticationStateChanged;

            UpdateUserInfo();

            // 상대 경로로 라우트 등록
            Routing.RegisterRoute("bikes/register", typeof(BikeRegisterPage));
            Routing.RegisterRoute("bikes/detail", typeof(BikeDetailPage));
            Routing.RegisterRoute("bikes/transfer", typeof(BikeTransferPage));
            Routing.RegisterRoute("bikes/maintenance", typeof(MaintenanceDetailPage));
            Routing.RegisterRoute("login", typeof(LoginPage));
            Routing.RegisterRoute("register", typeof(RegisterPage));
            Routing.RegisterRoute("profile", typeof(ProfilePage));
        }

        private async void OnShellNavigated(object? sender, ShellNavigatedEventArgs e)
        {
            // 페이지 이동 로깅
            await _activityLogger.LogNavigationAsync(e.Current.Location.ToString());
        }

        private void OnAuthenticationStateChanged()
        {
            UpdateUserInfo();
        }

        private void UpdateUserInfo()
        {
            if (_authService.IsLoggedIn && _authService.CurrentUser != null)
            {
                FullName = $"{_authService.CurrentUser.FirstName} {_authService.CurrentUser.LastName}";
            }
            else
            {
                FullName = string.Empty;
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            UpdateUserInfo();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Navigated -= OnShellNavigated;
            _authService.AuthenticationStateChanged -= OnAuthenticationStateChanged;
        }

        // 'new' 키워드를 사용하여 상속된 멤버를 명시적으로 숨김
        public new event PropertyChangedEventHandler? PropertyChanged;

        protected new void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}