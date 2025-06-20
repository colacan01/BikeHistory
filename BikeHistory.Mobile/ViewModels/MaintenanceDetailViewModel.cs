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

        public MaintenanceDetailViewModel(MaintenanceService maintenanceService, ActivityLoggerService activityLogger)
        {
            _maintenanceService = maintenanceService;
            _activityLogger = activityLogger;
            maintenanceId = string.Empty;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("id", out var idValue))
            {
                MaintenanceId = Uri.UnescapeDataString(idValue.ToString() ?? string.Empty);
                LoadMaintenanceCommand.Execute(null);
            }
        }

        [RelayCommand]
        private async Task LoadMaintenance()
        {
            if (string.IsNullOrEmpty(MaintenanceId) || IsBusy)
                return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                Maintenance = await _maintenanceService.GetMaintenanceById(MaintenanceId);
                
                if (Maintenance != null)
                {
                    // 유지보수 상세 조회 로그
                    await _activityLogger.LogActionAsync("ViewMaintenanceDetail", new Dictionary<string, string>
                    {
                        { "maintenanceId", MaintenanceId },
                        { "bikeId", Maintenance.BikeFrameId.ToString() }
                    });
                }
                else
                {
                    ErrorMessage = "유지보수 정보를 찾을 수 없습니다.";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"유지보수 상세 로드 오류: {ex.Message}");
                ErrorMessage = "유지보수 정보를 불러오는데 실패했습니다.";
            }
            finally
            {
                IsBusy = false;
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
                if (Maintenance != null)
                {
                    // 유지보수 항목이 속한 자전거 상세 페이지로 직접 이동
                    await Shell.Current.GoToAsync($"///bikes/detail?id={Maintenance.BikeFrameId}");
                }
                else
                {
                    // 유지보수 정보가 없는 경우 일반적인 뒤로 가기
                    await Shell.Current.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"이전 페이지로 이동 중 오류 발생: {ex.Message}");
                // 오류 발생 시 홈으로 이동
                await Shell.Current.GoToAsync("//bikes");
            }
        }
    }
}