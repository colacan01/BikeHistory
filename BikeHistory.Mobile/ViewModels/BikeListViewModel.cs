using BikeHistory.Mobile.Models;
using BikeHistory.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace BikeHistory.Mobile.ViewModels
{
    public partial class BikeListViewModel : ObservableObject
    {
        private readonly BikeService _bikeService;
        private readonly AuthService _authService;
        private Grid? _bottomSheetContainer;

        public void SetBottomSheetContainer(Grid container)
        {
            _bottomSheetContainer = container;
        }

        private ObservableCollection<BikeFrame> _bikes = new ObservableCollection<BikeFrame>();
        public ObservableCollection<BikeFrame> Bikes
        {
            get => _bikes;
            set => SetProperty(ref _bikes, value);
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private string? _errorMessage;
        public string? ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        private bool _isProfileMenuVisible;
        public bool IsProfileMenuVisible
        {
            get => _isProfileMenuVisible;
            set => SetProperty(ref _isProfileMenuVisible, value);
        }

        private bool _isBikeMenuVisible;
        public bool IsBikeMenuVisible
        {
            get => _isBikeMenuVisible;
            set => SetProperty(ref _isBikeMenuVisible, value);
        }

        private BikeFrame? _selectedBike;
        public BikeFrame? SelectedBike
        {
            get => _selectedBike;
            set => SetProperty(ref _selectedBike, value);
        }

        public BikeListViewModel(BikeService bikeService, AuthService authService)
        {
            _bikeService = bikeService;
            _authService = authService;
            _bikes = new ObservableCollection<BikeFrame>(); // Fixed ambiguity by directly assigning to the private field
        }

        [RelayCommand]
        private async Task LoadBikes()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                var bikeList = await _bikeService.GetBikes();

                Bikes.Clear();
                foreach (var bike in bikeList)
                {
                    Bikes.Add(bike);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"자전거 목록 로드 실패: {ex.Message}");
                ErrorMessage = "자전거 목록을 불러오는데 실패했습니다.";
            }
            finally
            {
                IsBusy = false;
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        private async Task AddNewBike()
        {
            try
            {
                await Shell.Current.GoToAsync("///bikes/register");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"네비게이션 실패: {ex.Message}");
                ErrorMessage = "자전거 등록 페이지로 이동할 수 없습니다.";
            }
        }

        [RelayCommand]
        private async Task BikeSelected(BikeFrame bike)
        {
            if (bike == null)
                return;

            try
            {
                await Shell.Current.GoToAsync($"///bikes/detail?id={bike.Id}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"네비게이션 실패: {ex.Message}");
                ErrorMessage = "자전거 상세 페이지로 이동할 수 없습니다.";
            }
        }

        [RelayCommand]
        private async Task Refresh()
        {
            IsRefreshing = true;
            await LoadBikes();
        }

        [RelayCommand]
        private async Task Logout()
        {
            await _authService.Logout();
        }

        [RelayCommand]
        private void ShowProfileMenu()
        {
            IsProfileMenuVisible = !IsProfileMenuVisible;
            if (IsProfileMenuVisible)
            {
                IsBikeMenuVisible = false; // Close bike menu if open
            }
        }

        [RelayCommand]
        private async Task ShowBikeMenu(BikeFrame bike)
        {
            if (bike == null) return;
            
            SelectedBike = bike;
            
            if (_bottomSheetContainer != null)
            {
                // Close profile menu first
                IsProfileMenuVisible = false;
                
                // Set up initial position
                _bottomSheetContainer.TranslationY = _bottomSheetContainer.Height > 0 ? _bottomSheetContainer.Height : 400;
                
                // Show the container and animate
                IsBikeMenuVisible = true;
                
                // Small delay to ensure the container is rendered
                await Task.Delay(10);
                
                // Slide up animation
                await _bottomSheetContainer.TranslateTo(0, 0, 100, Easing.CubicOut);
            }
            else
            {
                IsBikeMenuVisible = true;
                IsProfileMenuVisible = false;
            }
        }

        [RelayCommand]
        private async Task HideMenus()
        {
            IsProfileMenuVisible = false;
            
            if (IsBikeMenuVisible && _bottomSheetContainer != null)
            {
                // Slide down animation
                var targetY = _bottomSheetContainer.Height > 0 ? _bottomSheetContainer.Height : 400;
                await _bottomSheetContainer.TranslateTo(0, targetY, 100, Easing.CubicIn);
                
                // Hide the container and reset position
                IsBikeMenuVisible = false;
                _bottomSheetContainer.TranslationY = 0;
            }
            else
            {
                IsBikeMenuVisible = false;
            }
        }

        [RelayCommand]
        private async Task NavigateToProfile()
        {
            await HideMenus();
            try
            {
                await Shell.Current.GoToAsync("///profile");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"네비게이션 실패: {ex.Message}");
                ErrorMessage = "프로필 페이지로 이동할 수 없습니다.";
            }
        }

        [RelayCommand]
        private async Task GenerateQRCode()
        {
            if (SelectedBike == null) return;
            
            await HideMenus();
            try
            {
                // QR 코드 생성 기능 구현
                await Application.Current.MainPage.DisplayAlert("QR 코드", $"{SelectedBike.Manufacturer.Name} {SelectedBike.Model}의 QR 코드를 생성합니다.", "확인");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"QR 코드 생성 실패: {ex.Message}");
                ErrorMessage = "QR 코드를 생성할 수 없습니다.";
            }
        }

        [RelayCommand]
        private async Task EditBikeInfo()
        {
            if (SelectedBike == null) return;
            
            await HideMenus();
            try
            {
                await Shell.Current.GoToAsync($"///bikes/detail?id={SelectedBike.Id}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"네비게이션 실패: {ex.Message}");
                ErrorMessage = "자전거 정보 수정 페이지로 이동할 수 없습니다.";
            }
        }

        [RelayCommand]
        private async Task AddMaintenanceRecord()
        {
            if (SelectedBike == null) return;
            
            await HideMenus();
            try
            {
                await Shell.Current.GoToAsync($"///bikes/maintenance?bikeId={SelectedBike.Id}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"네비게이션 실패: {ex.Message}");
                ErrorMessage = "정비 이력 페이지로 이동할 수 없습니다.";
            }
        }

        [RelayCommand]
        private async Task TransferOwnership()
        {
            if (SelectedBike == null) return;
            
            await HideMenus();
            try
            {
                await Shell.Current.GoToAsync($"///bikes/transfer?id={SelectedBike.Id}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"네비게이션 실패: {ex.Message}");
                ErrorMessage = "소유권 이전 페이지로 이동할 수 없습니다.";
            }
        }

        public async Task OnAppearing()
        {
            await LoadBikes();
        }
    }
}