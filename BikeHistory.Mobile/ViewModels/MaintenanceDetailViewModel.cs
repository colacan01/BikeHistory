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
                    // �������� �� ��ȸ �α�
                    await _activityLogger.LogActionAsync("ViewMaintenanceDetail", new Dictionary<string, string>
                    {
                        { "maintenanceId", MaintenanceId },
                        { "bikeId", Maintenance.BikeFrameId.ToString() }
                    });
                }
                else
                {
                    ErrorMessage = "�������� ������ ã�� �� �����ϴ�.";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"�������� �� �ε� ����: {ex.Message}");
                ErrorMessage = "�������� ������ �ҷ����µ� �����߽��ϴ�.";
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
                    // �������� �׸��� ���� ������ �� �������� ���� �̵�
                    await Shell.Current.GoToAsync($"///bikes/detail?id={Maintenance.BikeFrameId}");
                }
                else
                {
                    // �������� ������ ���� ��� �Ϲ����� �ڷ� ����
                    await Shell.Current.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"���� �������� �̵� �� ���� �߻�: {ex.Message}");
                // ���� �߻� �� Ȩ���� �̵�
                await Shell.Current.GoToAsync("//bikes");
            }
        }
    }
}