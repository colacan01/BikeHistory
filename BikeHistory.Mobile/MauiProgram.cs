using BikeHistory.Mobile.Services;
using BikeHistory.Mobile.ViewModels;
using BikeHistory.Mobile.Views.Auth;
using BikeHistory.Mobile.Views.Bikes;
using BikeHistory.Mobile.Views.Maintenance;

namespace BikeHistory.Mobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // 서비스 등록
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<BikeService>();
            builder.Services.AddSingleton<CatalogService>();
            builder.Services.AddSingleton<ActivityLoggerService>();
            builder.Services.AddTransient<MaintenanceService>();

            // 뷰모델 등록
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<RegisterViewModel>();
            builder.Services.AddTransient<BikeListViewModel>();
            builder.Services.AddTransient<BikeDetailViewModel>();
            builder.Services.AddTransient<BikeRegisterViewModel>();
            builder.Services.AddTransient<ProfileViewModel>();
            builder.Services.AddTransient<BikeTransferViewModel>(); // 추가
            builder.Services.AddTransient<MaintenanceDetailViewModel>();

            // 페이지 등록
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<BikeListPage>();
            builder.Services.AddTransient<BikeDetailPage>();
            builder.Services.AddTransient<BikeRegisterPage>();
            builder.Services.AddTransient<ProfilePage>();
            builder.Services.AddTransient<BikeTransferPage>(); // 추가
            builder.Services.AddTransient<MaintenanceDetailPage>();
            
            return builder.Build();
        }
    }
}