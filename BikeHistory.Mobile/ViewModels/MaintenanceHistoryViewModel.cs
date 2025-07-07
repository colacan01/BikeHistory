using System.Collections.ObjectModel;
using System.Windows.Input;
using BikeHistory.Mobile.Models;
using BikeHistory.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace BikeHistory.Mobile.ViewModels
{
    [QueryProperty(nameof(BikeId), "id")]
    public partial class MaintenanceHistoryViewModel : ObservableObject, IQueryAttributable
    {
        private readonly BikeService _bikeService;
        private readonly MaintenanceService _maintenanceService;
        private readonly ILogger<MaintenanceHistoryViewModel> _logger;

        private BikeFrame? _bikeInfo;
        private ObservableCollection<Maintenance> _maintenanceHistory = new();
        private bool _isLoading;
        private string? _errorMessage;
        private int _bikeId;

        public BikeFrame? BikeInfo
        {
            get => _bikeInfo;
            set => SetProperty(ref _bikeInfo, value);
        }

        public ObservableCollection<Maintenance> MaintenanceHistory
        {
            get => _maintenanceHistory;
            set => SetProperty(ref _maintenanceHistory, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string? ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public int BikeId
        {
            get => _bikeId;
            set => SetProperty(ref _bikeId, value);
        }

        public ICommand GoBackCommand { get; }
        public ICommand ViewMaintenanceDetailCommand { get; }

        public MaintenanceHistoryViewModel(
            BikeService bikeService, 
            MaintenanceService maintenanceService,
            ILogger<MaintenanceHistoryViewModel> logger)
        {
            _bikeService = bikeService;
            _maintenanceService = maintenanceService;
            _logger = logger;
            
            GoBackCommand = new AsyncRelayCommand(GoBackAsync);
            ViewMaintenanceDetailCommand = new AsyncRelayCommand<Maintenance>(ViewMaintenanceDetail);
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            Debug.WriteLine($"MaintenanceHistoryViewModel - ApplyQueryAttributes 호출됨");
            
            if (query.TryGetValue("id", out var idValue))
            {
                BikeId = Convert.ToInt32(idValue);
                Debug.WriteLine($"MaintenanceHistoryViewModel - BikeId 설정됨: {BikeId}");
                _ = LoadMaintenanceHistory(BikeId);
            }
            else
            {
                Debug.WriteLine("MaintenanceHistoryViewModel - id 파라미터를 찾을 수 없음");
            }
        }

        public async Task LoadMaintenanceHistory(int bikeId)
        {
            try
            {
                Debug.WriteLine($"MaintenanceHistoryViewModel - LoadMaintenanceHistory 시작: {bikeId}");
                IsLoading = true;
                ErrorMessage = string.Empty;

                // Load bike info
                BikeInfo = await _bikeService.GetBikeById(bikeId);
                Debug.WriteLine($"MaintenanceHistoryViewModel - 자전거 정보 로드 완료: {BikeInfo?.FrameNumber}");

                // Load maintenance history
                var history = await _maintenanceService.GetMaintenancesByBikeId(bikeId);
                Debug.WriteLine($"MaintenanceHistoryViewModel - 정비 이력 로드 완료: {history.Count}개");
                
                MaintenanceHistory.Clear();
                foreach (var record in history.OrderByDescending(x => x.MaintenanceDate))
                {
                    MaintenanceHistory.Add(record);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MaintenanceHistoryViewModel - 로드 실패: {ex.Message}");
                _logger.LogError(ex, "Failed to load maintenance history for bike {BikeId}", bikeId);
                ErrorMessage = "정비 이력을 불러오는데 실패했습니다. 다시 시도해주세요.";
            }
            finally
            {
                IsLoading = false;
                Debug.WriteLine($"MaintenanceHistoryViewModel - LoadMaintenanceHistory 완료");
            }
        }

        private async Task ViewMaintenanceDetail(Maintenance? maintenance)
        {
            if (maintenance == null) 
            {
                Debug.WriteLine("MaintenanceHistoryViewModel - maintenance가 null입니다");
                return;
            }

            try
            {
                Debug.WriteLine($"MaintenanceHistoryViewModel - ViewMaintenanceDetail 호출됨: {maintenance.Id}");
                Debug.WriteLine($"MaintenanceHistoryViewModel - 네비게이션 시도: bikes/maintenance?id={maintenance.Id}");
                
                // 상대 경로로 먼저 시도
                await Shell.Current.GoToAsync($"bikes/maintenance?id={maintenance.Id}");
                Debug.WriteLine("MaintenanceHistoryViewModel - 정비 상세 페이지 네비게이션 성공");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MaintenanceHistoryViewModel - ViewMaintenanceDetail 실패: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                
                // 절대 경로로 재시도
                try
                {
                    Debug.WriteLine("MaintenanceHistoryViewModel - 절대 경로로 재시도");
                    await Shell.Current.GoToAsync($"///bikes/maintenance?id={maintenance.Id}");
                    Debug.WriteLine("MaintenanceHistoryViewModel - 절대 경로 네비게이션 성공");
                }
                catch (Exception ex2)
                {
                    Debug.WriteLine($"MaintenanceHistoryViewModel - 절대 경로도 실패: {ex2.Message}");
                    _logger.LogError(ex2, "Failed to navigate to maintenance detail for maintenance {MaintenanceId}", maintenance.Id);
                    ErrorMessage = "정비 상세 정보를 불러오는데 실패했습니다.";
                }
            }
        }

        private async Task GoBackAsync()
        {
            try
            {
                Debug.WriteLine("MaintenanceHistoryViewModel - GoBack 호출됨");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MaintenanceHistoryViewModel - GoBack 실패: {ex.Message}");
            }
        }
    }
}