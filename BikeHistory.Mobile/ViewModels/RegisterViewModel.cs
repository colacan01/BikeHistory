using BikeHistory.Mobile.Models;
using BikeHistory.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace BikeHistory.Mobile.ViewModels
{
    public partial class RegisterViewModel : ObservableObject
    {
        private readonly AuthService _authService;

        private string? email;
        public string? Email
        {
            get => email;
            set => SetProperty(ref email, value);
        }

        private string? password;
        public string? Password
        {
            get => password;
            set => SetProperty(ref password, value);
        }

        private string? confirmPassword;
        public string? ConfirmPassword
        {
            get => confirmPassword;
            set => SetProperty(ref confirmPassword, value);
        }

        private string? firstName;
        public string? FirstName
        {
            get => firstName;
            set => SetProperty(ref firstName, value);
        }

        private string? lastName;
        public string? LastName
        {
            get => lastName;
            set => SetProperty(ref lastName, value);
        }

        private bool isBusy;
        public bool IsBusy
        {
            get => isBusy;
            set => SetProperty(ref isBusy, value);
        }

        private string? errorMessage;
        public string? ErrorMessage
        {
            get => errorMessage;
            set => SetProperty(ref errorMessage, value);
        }
        private string? successMessage;
        public string? SuccessMessage
        {
            get => successMessage;
            set => SetProperty(ref successMessage, value);
        }
        public RegisterViewModel(AuthService authService)
        {
            _authService = authService;
        }

        [RelayCommand]
        public async Task Register()
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password) ||
                string.IsNullOrEmpty(ConfirmPassword) || string.IsNullOrEmpty(FirstName) ||
                string.IsNullOrEmpty(LastName))
            {
                ErrorMessage = "모든 필드를 입력해주세요.";
                return;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "비밀번호와 비밀번호 확인이 일치하지 않습니다.";
                return;
            }

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;
                SuccessMessage = string.Empty;

                var request = new RegisterRequest
                {
                    Email = Email,
                    Password = Password,
                    ConfirmPassword = ConfirmPassword,
                    FirstName = FirstName,
                    LastName = LastName
                };

                var success = await _authService.Register(request);

                if (success)
                {
                    SuccessMessage = "회원가입이 완료되었습니다. 로그인 페이지로 이동합니다.";

                    // 잠시 후 로그인 페이지로 이동
                    await Task.Delay(2000);
                    await Shell.Current.GoToAsync("///login");
                }
                else
                {
                    ErrorMessage = "회원가입 중 오류가 발생했습니다.";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"회원가입 오류: {ex.Message}");
                ErrorMessage = "회원가입에 실패했습니다. 입력 정보를 확인해주세요.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task GoToLogin()
        {
            await Shell.Current.GoToAsync("///login");
        }
    }
}