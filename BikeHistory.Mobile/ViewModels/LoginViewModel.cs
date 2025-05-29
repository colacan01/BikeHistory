using BikeHistory.Mobile.Models;
using BikeHistory.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IntelliJ.Lang.Annotations;
using System.Diagnostics;

namespace BikeHistory.Mobile.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly AuthService _authService;

        [ObservableProperty]
        private string email;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private string errorMessage;

        public LoginViewModel(AuthService authService)
        {
            _authService = authService;
        }

        [RelayCommand]
        public async Task Login()
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "이메일과 비밀번호를 입력해주세요.";
                return;
            }

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                var request = new LoginRequest
                {
                    Email = Email,
                    Password = Password
                };

                var response = await _authService.Login(request);

                // 로그인 성공시 메인 페이지로 이동
                await Shell.Current.GoToAsync("///bikes");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"로그인 오류: {ex.Message}");
                ErrorMessage = "로그인에 실패했습니다. 이메일과 비밀번호를 확인해주세요.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task GoToRegister()
        {
            await Shell.Current.GoToAsync("register");
        }
    }
}