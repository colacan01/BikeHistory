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

        [ObservableProperty]
        private ObservableCollection<BikeFrame> bikes;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private string? errorMessage;

        [ObservableProperty]
        private bool isRefreshing;

        public BikeListViewModel(BikeService bikeService, AuthService authService)
        {
            _bikeService = bikeService;
            _authService = authService;
            //Bikes = new ObservableCollection<BikeFrame>();
            Bikes = [];
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
                Debug.WriteLine($"자전거 목록 로드 오류: {ex.Message}");
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
                Debug.WriteLine($"네비게이션 오류: {ex.Message}");
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
                Debug.WriteLine($"네비게이션 오류: {ex.Message}");
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

        public async Task OnAppearing()
        {
            await LoadBikes();
        }
    }
}