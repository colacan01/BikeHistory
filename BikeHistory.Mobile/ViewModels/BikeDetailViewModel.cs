using BikeHistory.Mobile.Models;
using BikeHistory.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace BikeHistory.Mobile.ViewModels
{
    [QueryProperty(nameof(BikeId), "id")]
    public partial class BikeDetailViewModel : ObservableObject, IQueryAttributable
    {
        private readonly BikeService _bikeService;
        private readonly AuthService _authService;

        [ObservableProperty]
        private BikeFrame bike;

        [ObservableProperty]
        private ObservableCollection<OwnershipRecord> ownershipHistory;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private bool isHistoryBusy;

        [ObservableProperty]
        private string errorMessage;

        [ObservableProperty]
        private int bikeId;

        [ObservableProperty]
        private bool isOwner;

        public BikeDetailViewModel(BikeService bikeService, AuthService authService)
        {
            _bikeService = bikeService;
            _authService = authService;
            OwnershipHistory = new ObservableCollection<OwnershipRecord>();
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("id"))
            {
                BikeId = Convert.ToInt32(query["id"]);
                LoadBikeCommand.Execute(null);
            }
        }

        [RelayCommand]
        private async Task LoadBike()
        {
            if (BikeId <= 0 || IsBusy)
                return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                Bike = await _bikeService.GetBikeById(BikeId);
                
                // 현재 사용자가 자전거 소유자인지 확인
                IsOwner = Bike.CurrentOwnerId == _authService.CurrentUser?.Id;

                // 소유권 이력 로드
                await LoadOwnershipHistory();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"자전거 상세 정보 로드 오류: {ex.Message}");
                ErrorMessage = "자전거 정보를 불러오는데 실패했습니다.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task LoadOwnershipHistory()
        {
            if (BikeId <= 0 || IsHistoryBusy)
                return;

            try
            {
                IsHistoryBusy = true;

                var history = await _bikeService.GetBikeHistory(BikeId);
                
                OwnershipHistory.Clear();
                foreach (var record in history)
                {
                    OwnershipHistory.Add(record);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"소유권 이력 로드 오류: {ex.Message}");
            }
            finally
            {
                IsHistoryBusy = false;
            }
        }

        [RelayCommand]
        private async Task TransferOwnership()
        {
            if (Bike == null || !IsOwner)
                return;

            await Shell.Current.GoToAsync($"///bikes/transfer?id={BikeId}");
        }

        [RelayCommand]
        private async Task GoBack()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}