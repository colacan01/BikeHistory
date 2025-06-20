using BikeHistory.Mobile.Services;

namespace BikeHistory.Mobile
{
    public partial class App : Application
    {
        private readonly AuthService _authService;
        private readonly ActivityLoggerService _activityLogger;

        public App(AuthService authService, ActivityLoggerService activityLogger)
        {
            InitializeComponent();

            _authService = authService;
            _activityLogger = activityLogger;
            MainPage = new AppShell(_authService, _activityLogger);

            // 인증 상태에 따라 시작 페이지 설정
            CheckAuthState();

            // 인증 상태 변경 이벤트 구독
            _authService.AuthenticationStateChanged += OnAuthenticationStateChanged;
        }

        private void CheckAuthState()
        {
            if (_authService.IsLoggedIn)
            {
                // 로그인된 경우 메인 페이지로
                Shell.Current.GoToAsync("///bikes");
            }
            else
            {
                // 로그인되지 않은 경우 로그인 페이지로
                Shell.Current.GoToAsync("///login");
            }
        }

        private void OnAuthenticationStateChanged()
        {
            // 인증 상태가 변경되면 확인
            CheckAuthState();
        }

        protected override void OnSleep()
        {
            // 앱이 백그라운드로 가면 호출됨
        }

        protected override void OnResume()
        {
            // 앱이 다시 포그라운드로 오면 호출됨
        }
    }
}