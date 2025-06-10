using BikeHistory.Mobile.Models;
using BikeHistory.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace BikeHistory.Mobile.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly AuthService _authService;

        private string? email;
        public string? Email
        {
            get => email;
            set => SetProperty(ref email, value?.Trim().ToLowerInvariant()); // �̸��� �ҹ��� �� ���� ����
        }

        private string? password;
        public string? Password
        {
            get => password;
            set => SetProperty(ref password, value?.Trim()); // ��й�ȣ ���� ����
        }
        
        private bool? isBusy;
        public bool? IsBusy
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