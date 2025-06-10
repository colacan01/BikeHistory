using BikeHistory.Mobile.Models;
using BikeHistory.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace BikeHistory.Mobile.ViewModels
{
    public partial class ProfileViewModel : ObservableObject
    {
        private readonly AuthService _authService;

        [ObservableProperty]
        private string email;

        [ObservableProperty]
        private string firstName;

        [ObservableProperty]
        private string lastName;

        [ObservableProperty]
        private List<string> roles;

        [ObservableProperty]
        private string currentPassword;

        [ObservableProperty]
        private string newPassword;

        [ObservableProperty]
        private string confirmNewPassword;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string errorMessage;

        [ObservableProperty]
        private string successMessage;

        [ObservableProperty]
        private bool isPasswordChangeVisible;

        public ProfileViewModel(AuthService authService)
        {
            _authService = authService;
            //Roles = new List<string>();
            Roles = [];
            IsPasswordChangeVisible = false;
        }

        [RelayCommand]
        private async Task LoadProfile()
        {
            if (IsLoading)
                return;

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                SuccessMessage = string.Empty;

                var profile = await _authService.GetProfile();
                
                Email = profile.Email;
                FirstName = profile.FirstName ?? string.Empty;
                LastName = profile.LastName ?? string.Empty;
                Roles = profile.Roles ?? [];        // roles�� ���ٸ�, �� ����Ʈ�� �ʱ�ȭ
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"������ �ε� ����: {ex.Message}");
                ErrorMessage = "������ ������ �ҷ����µ� �����߽��ϴ�.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task UpdateProfile()
        {
            if (IsBusy)
                return;

            // ��ȿ�� �˻�
            if (string.IsNullOrEmpty(FirstName) || string.IsNullOrEmpty(LastName))
            {
                ErrorMessage = "�̸��� ���� �Է����ּ���.";
                return;
            }

            // ��й�ȣ ���� ��ȿ�� �˻�
            if (IsPasswordChangeVisible)
            {
                if (string.IsNullOrEmpty(CurrentPassword))
                {
                    ErrorMessage = "���� ��й�ȣ�� �Է����ּ���.";
                    return;
                }

                if (string.IsNullOrEmpty(NewPassword) || string.IsNullOrEmpty(ConfirmNewPassword))
                {
                    ErrorMessage = "�� ��й�ȣ�� Ȯ���� �Է����ּ���.";
                    return;
                }

                if (NewPassword != ConfirmNewPassword)
                {
                    ErrorMessage = "�� ��й�ȣ�� Ȯ���� ��ġ���� �ʽ��ϴ�.";
                    return;
                }
            }

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;
                SuccessMessage = string.Empty;

                var request = new UpdateProfileRequest
                {
                    FirstName = FirstName,
                    LastName = LastName
                };

                // ��й�ȣ ���� ��û�� ���
                if (IsPasswordChangeVisible && !string.IsNullOrEmpty(NewPassword))
                {
                    request.CurrentPassword = CurrentPassword;
                    request.NewPassword = NewPassword;
                    request.ConfirmNewPassword = ConfirmNewPassword;
                }

                var response = await _authService.UpdateProfile(request);

                SuccessMessage = "�������� ���������� ������Ʈ�Ǿ����ϴ�.";

                // ��й�ȣ �ʵ� �ʱ�ȭ
                CurrentPassword = string.Empty;
                NewPassword = string.Empty;
                ConfirmNewPassword = string.Empty;
                IsPasswordChangeVisible = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"������ ������Ʈ ����: {ex.Message}");
                ErrorMessage = "������ ������Ʈ�� �����߽��ϴ�.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void TogglePasswordChange()
        {
            IsPasswordChangeVisible = !IsPasswordChangeVisible;
        }

        [RelayCommand]
        private async Task Logout()
        {
            await _authService.Logout();
        }

        public async Task OnAppearing()
        {
            await LoadProfile();
        }
    }
}