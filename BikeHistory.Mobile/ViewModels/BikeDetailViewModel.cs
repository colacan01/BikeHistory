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
        private readonly MaintenanceService _maintenanceService;
        private readonly ActivityLoggerService _activityLogger;

        private BikeFrame? bike;
        public BikeFrame? Bike
        {
            get => bike;
            set => SetProperty(ref bike, value);
        }

        private ObservableCollection<OwnershipRecord>? ownershipHistory;
        public ObservableCollection<OwnershipRecord>? OwnershipHistory 
        { 
            get => ownershipHistory; 
            private set => SetProperty(ref ownershipHistory, value);
        }

        private ObservableCollection<Maintenance>? maintenancHistory;
        public ObservableCollection<Maintenance>? MaintenancHistory
        {
            get => maintenancHistory;
            private set => SetProperty(ref maintenancHistory, value);
        }

        private bool isBusy;
        public bool IsBusy
        {
            get => isBusy;
            set => SetProperty(ref isBusy, value);
        }   

        private bool isHistoryBusy;
        public bool IsHistoryBusy
        {
            get => isHistoryBusy;
            set => SetProperty(ref isHistoryBusy, value);
        }

        private bool isMaintenanceBusy;
        public bool IsMaintenanceBusy
        {
            get => isMaintenanceBusy;
            set => SetProperty(ref isMaintenanceBusy, value);
        }

        private string? errorMessage;
        public string? ErrorMessage
        {
            get => errorMessage;
            set => SetProperty(ref errorMessage, value);
        }

        private int bikeId;
        public int BikeId
        {
            get => bikeId;
            set => SetProperty(ref bikeId, value);
        }

        private bool isOwner;
        public bool IsOwner
        {
            get => isOwner;
            set => SetProperty(ref isOwner, value);
        }

        public BikeDetailViewModel(BikeService bikeService, AuthService authService, MaintenanceService maintenanceService, ActivityLoggerService activityLogger)
        {
            _bikeService = bikeService;
            _authService = authService;
            _maintenanceService = maintenanceService;
            _activityLogger = activityLogger;

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

                // 유지보수 이력 로드
                await LoadMaintenanceHistory();

                // 자전거 상세 페이지 조회 로깅
                await _activityLogger.LogActionAsync("ViewBikeDetail", new Dictionary<string, string>
                {
                    { "bikeId", bikeId.ToString() }
                });
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
        private async Task LoadMaintenanceHistory()
        {
            if (BikeId <= 0 || IsMaintenanceBusy)
                return;

            try
            {
                IsMaintenanceBusy = true;

                var history = await _maintenanceService.GetMaintenancesByBikeId(BikeId);

                // MaintenanceHistory가 null인지 확인 후 초기화
                MaintenancHistory ??= new ObservableCollection<Maintenance>();
                MaintenancHistory.Clear();
                foreach (var record in history)
                {
                    MaintenancHistory.Add(record);
                }

                // 자전거 유지보수 내역 조회 로깅
                await _activityLogger.LogActionAsync("ViewMaintenanceHistoryAll", new Dictionary<string, string>
                {
                    { "bikeId", bikeId.ToString() }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"유지보수 이력 로드 오류: {ex.Message}");
            }
            finally
            {
                IsMaintenanceBusy = false;
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

                // OwnershipHistory가 null인지 확인 후 초기화
                OwnershipHistory ??= new ObservableCollection<OwnershipRecord>();
                OwnershipHistory.Clear();
                foreach (var record in history)
                {
                    OwnershipHistory.Add(record);
                }
                // 자전거 소유 내역 조회 로깅
                await _activityLogger.LogActionAsync("ViewOwnershipHistoryAll", new Dictionary<string, string>
                {
                    { "bikeId", bikeId.ToString() }
                });
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
        private async Task RefreshAsync()
        {
            await LoadBike();            
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