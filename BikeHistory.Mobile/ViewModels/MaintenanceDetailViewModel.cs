using BikeHistory.Mobile.Models;
using BikeHistory.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace BikeHistory.Mobile.ViewModels
{
    [QueryProperty(nameof(MaintenanceId), "id")]
    public partial class MaintenanceDetailViewModel : ObservableObject, IQueryAttributable
    {
        private readonly MaintenanceService _maintenanceService;
        private readonly ActivityLoggerService _activityLogger;

        private Maintenance? maintenance;
        public Maintenance? Maintenance
        {
            get => maintenance;
            set => SetProperty(ref maintenance, value);
        }

        private bool isBusy;
        public bool IsBusy
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

        private string maintenanceId;
        public string MaintenanceId
        {
            get => maintenanceId;
            set => SetProperty(ref maintenanceId, value);
        }

        public MaintenanceDetailViewModel()
        {
            _maintenanceService = null!;
            _activityLogger = null!;
            maintenanceId = string.Empty;
        }

        public MaintenanceDetailViewModel(MaintenanceService maintenanceService, ActivityLoggerService activityLogger)
        {
            _maintenanceService = maintenanceService;
            _activityLogger = activityLogger;
            maintenanceId = string.Empty;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            Debug.WriteLine($"MaintenanceDetailViewModel - ApplyQueryAttributes 호출됨");
            
            if (query.TryGetValue("id", out var idValue))
            {
                MaintenanceId = Uri.UnescapeDataString(idValue.ToString() ?? string.Empty);
                Debug.WriteLine($"MaintenanceDetailViewModel - MaintenanceId 설정됨: {MaintenanceId}");
                LoadMaintenanceCommand.Execute(null);
            }
            else
            {
                Debug.WriteLine("MaintenanceDetailViewModel - id 파라미터를 찾을 수 없음");
                ErrorMessage = "유지보수 ID가 제공되지 않았습니다.";
            }
        }

        [RelayCommand]
        private async Task LoadMaintenance()
        {
            if (string.IsNullOrEmpty(MaintenanceId) || IsBusy)
                return;

            try
            {
                Debug.WriteLine($"MaintenanceDetailViewModel - LoadMaintenance 시작: {MaintenanceId}");
                IsBusy = true;
                ErrorMessage = string.Empty;

                Maintenance = await _maintenanceService.GetMaintenanceById(MaintenanceId);
                
                if (Maintenance != null)
                {
                    Debug.WriteLine($"MaintenanceDetailViewModel - 정비 정보 로드 완료: {Maintenance.Id}");
                    
                    // 유지보수 상세 조회 로그
                    await _activityLogger.LogActionAsync("ViewMaintenanceDetail", new Dictionary<string, string>
                    {
                        { "maintenanceId", MaintenanceId },
                        { "bikeId", Maintenance.BikeFrameId.ToString() }
                    });
                }
                else
                {
                    Debug.WriteLine("MaintenanceDetailViewModel - 정비 정보를 찾을 수 없음");
                    ErrorMessage = "유지보수 정보를 찾을 수 없습니다.";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MaintenanceDetailViewModel - 정비 상세 로드 실패: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                ErrorMessage = "유지보수 정보를 불러오는데 실패했습니다.";
            }
            finally
            {
                IsBusy = false;
                Debug.WriteLine($"MaintenanceDetailViewModel - LoadMaintenance 완료");
            }
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadMaintenance();
        }

        [RelayCommand]
        private async Task GoBack()
        {
            try
            {
                Debug.WriteLine("MaintenanceDetailViewModel - GoBack 호출됨");
                
                if (Maintenance != null)
                {
                    Debug.WriteLine($"MaintenanceDetailViewModel - 자전거 상세 페이지로 이동: {Maintenance.BikeFrameId}");
                    // 유지보수가 있으면 해당 자전거의 정비 이력 페이지로 이동
                    await Shell.Current.GoToAsync($"bikes/maintenancehistory?id={Maintenance.BikeFrameId}");
                }
                else
                {
                    Debug.WriteLine("MaintenanceDetailViewModel - 일반적인 뒤로 가기");
                    // 유지보수 정보가 없을 경우 일반적인 뒤로 가기
                    await Shell.Current.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MaintenanceDetailViewModel - 뒤로 가기 실패: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                
                // 오류 발생 시 홈으로 이동
                try
                {
                    await Shell.Current.GoToAsync("///bikes");
                }
                catch (Exception ex2)
                {
                    Debug.WriteLine($"MaintenanceDetailViewModel - 홈으로 이동도 실패: {ex2.Message}");
                }
            }
        }
    }
}