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

            OwnershipHistory = [];
            MaintenancHistory = [];
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("id", out var idValue))
            {
                BikeId = Convert.ToInt32(idValue);
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

                Debug.WriteLine($"LoadBike 시작 - BikeId: {BikeId}");

                Bike = await _bikeService.GetBikeById(BikeId);
                
                Debug.WriteLine($"자전거 로드 완료: {Bike?.FrameNumber}");
                Debug.WriteLine($"현재 소유자: {Bike?.CurrentOwnerId}");
                Debug.WriteLine($"로그인한 사용자: {_authService.CurrentUser?.Id}");
                
                // 현재 사용자가 자전거 소유자인지 확인
                IsOwner = Bike.CurrentOwnerId == _authService.CurrentUser?.Id;
                
                Debug.WriteLine($"소유자 여부: {IsOwner}");

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
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
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

                MaintenancHistory ??= [];
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
                ErrorMessage = "자전거 유지보수 이력을 불러오는데 실패했습니다.";
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

                OwnershipHistory ??= [];
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
                ErrorMessage = "자전거 소유권 이력을 불러오는데 실패했습니다.";
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
            try
            {
                // 디버깅을 위한 로깅
                Debug.WriteLine($"TransferOwnership 호출됨 - BikeId: {BikeId}");
                Debug.WriteLine($"Bike null 여부: {Bike == null}");
                Debug.WriteLine($"IsOwner: {IsOwner}");
                Debug.WriteLine($"CurrentUser: {_authService.CurrentUser?.Email}");

                if (Bike == null)
                {
                    Debug.WriteLine("자전거 정보가 없습니다.");
                    ErrorMessage = "자전거 정보를 먼저 로드해주세요.";
                    return;
                }

                if (!IsOwner)
                {
                    Debug.WriteLine("소유자가 아닙니다.");
                    ErrorMessage = "자신의 자전거만 양도할 수 있습니다.";
                    return;
                }

                Debug.WriteLine($"네비게이션 시도: ///bikes/transfer?id={BikeId}");
                
                // 소유권 이전 페이지 조회 로깅
                await _activityLogger.LogActionAsync("NavigateToTransferPage", new Dictionary<string, string>
                {
                    { "bikeId", BikeId.ToString() }
                });

                // 절대 경로로 네비게이션 (다른 곳과 일치하도록)
                await Shell.Current.GoToAsync($"///bikes/transfer?id={BikeId}");
                
                Debug.WriteLine("네비게이션 성공");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"TransferOwnership 오류: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                ErrorMessage = $"소유권 이전 페이지로 이동 중 오류가 발생했습니다: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task TestNavigation()
        {
            try
            {
                Debug.WriteLine("테스트 네비게이션 시작");
                await Shell.Current.GoToAsync("///bikes");
                Debug.WriteLine("테스트 네비게이션 성공");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"테스트 네비게이션 실패: {ex.Message}");
                ErrorMessage = $"네비게이션 테스트 실패: {ex.Message}";
            }
        }

        [RelayCommand]
        private static async Task GoBack()
        {
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        private static async Task ViewMaintenanceDetail(object selectedItem)
        {
            if (selectedItem is Maintenance maintenance)
            {
                await Shell.Current.GoToAsync($"///bikes/maintenance?id={maintenance.Id}");
            }
        }
    }
}