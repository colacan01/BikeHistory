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
                ErrorMessage = "�̸��ϰ� ��й�ȣ�� �Է����ּ���.";
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

                // �α��� ������ ���� �������� �̵�
                await Shell.Current.GoToAsync("///bikes");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"�α��� ����: {ex.Message}");
                ErrorMessage = "�α��ο� �����߽��ϴ�. �̸��ϰ� ��й�ȣ�� Ȯ�����ּ���.";
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