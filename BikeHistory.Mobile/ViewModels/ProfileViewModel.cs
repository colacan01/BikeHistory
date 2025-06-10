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

        private string? email;
        public string? Email
        {
            get => email;
            set => SetProperty(ref email, value);
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

        private List<string>? roles;
        public List<string>? Roles
        {
            get => roles;
            set => SetProperty(ref roles, value);
        }

        private string? currentPassword;
        public string? CurrentPassword
        {
            get => currentPassword;
            set => SetProperty(ref currentPassword, value);
        }

        private string? newPassword;
        public string? NewPassword
        {
            get => newPassword;
            set => SetProperty(ref newPassword, value);
        }

        private string? confirmNewPassword;
        public string? ConfirmNewPassword
        {
            get => confirmNewPassword;
            set => SetProperty(ref confirmNewPassword, value);
        }

        private bool isBusy;
        public bool IsBusy
        {
            get => isBusy;
            set => SetProperty(ref isBusy, value);
        }

        private bool isLoading;
        public bool IsLoading
        {
            get => isLoading;
            set => SetProperty(ref isLoading, value);
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

        private bool isPasswordChangeVisible;
        public bool IsPasswordChangeVisible
        {
            get => isPasswordChangeVisible;
            set => SetProperty(ref isPasswordChangeVisible, value);
        }

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
                Roles = profile.Roles ?? [];        // roles가 없다면, 빈 리스트로 초기화
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"프로필 로드 오류: {ex.Message}");
                ErrorMessage = "프로필 정보를 불러오는데 실패했습니다.";
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

            // 유효성 검사
            if (string.IsNullOrEmpty(FirstName) || string.IsNullOrEmpty(LastName))
            {
                ErrorMessage = "이름과 성을 입력해주세요.";
                return;
            }

            // 비밀번호 변경 유효성 검사
            if (IsPasswordChangeVisible)
            {
                if (string.IsNullOrEmpty(CurrentPassword))
                {
                    ErrorMessage = "현재 비밀번호를 입력해주세요.";
                    return;
                }

                if (string.IsNullOrEmpty(NewPassword) || string.IsNullOrEmpty(ConfirmNewPassword))
                {
                    ErrorMessage = "새 비밀번호와 확인을 입력해주세요.";
                    return;
                }

                if (NewPassword != ConfirmNewPassword)
                {
                    ErrorMessage = "새 비밀번호와 확인이 일치하지 않습니다.";
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

                // 비밀번호 변경 요청인 경우
                if (IsPasswordChangeVisible && !string.IsNullOrEmpty(NewPassword))
                {
                    request.CurrentPassword = CurrentPassword;
                    request.NewPassword = NewPassword;
                    request.ConfirmNewPassword = ConfirmNewPassword;
                }

                var response = await _authService.UpdateProfile(request);

                SuccessMessage = "프로필이 성공적으로 업데이트되었습니다.";

                // 비밀번호 필드 초기화
                CurrentPassword = string.Empty;
                NewPassword = string.Empty;
                ConfirmNewPassword = string.Empty;
                IsPasswordChangeVisible = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"프로필 업데이트 오류: {ex.Message}");
                ErrorMessage = "프로필 업데이트에 실패했습니다.";
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