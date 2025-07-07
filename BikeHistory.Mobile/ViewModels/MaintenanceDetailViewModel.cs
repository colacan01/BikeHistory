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
            Debug.WriteLine($"MaintenanceDetailViewModel - ApplyQueryAttributes ȣ���");
            
            if (query.TryGetValue("id", out var idValue))
            {
                MaintenanceId = Uri.UnescapeDataString(idValue.ToString() ?? string.Empty);
                Debug.WriteLine($"MaintenanceDetailViewModel - MaintenanceId ������: {MaintenanceId}");
                LoadMaintenanceCommand.Execute(null);
            }
            else
            {
                Debug.WriteLine("MaintenanceDetailViewModel - id �Ķ���͸� ã�� �� ����");
                ErrorMessage = "�������� ID�� �������� �ʾҽ��ϴ�.";
            }
        }

        [RelayCommand]
        private async Task LoadMaintenance()
        {
            if (string.IsNullOrEmpty(MaintenanceId) || IsBusy)
                return;

            try
            {
                Debug.WriteLine($"MaintenanceDetailViewModel - LoadMaintenance ����: {MaintenanceId}");
                IsBusy = true;
                ErrorMessage = string.Empty;

                Maintenance = await _maintenanceService.GetMaintenanceById(MaintenanceId);
                
                if (Maintenance != null)
                {
                    Debug.WriteLine($"MaintenanceDetailViewModel - ���� ���� �ε� �Ϸ�: {Maintenance.Id}");
                    
                    // �������� �� ��ȸ �α�
                    await _activityLogger.LogActionAsync("ViewMaintenanceDetail", new Dictionary<string, string>
                    {
                        { "maintenanceId", MaintenanceId },
                        { "bikeId", Maintenance.BikeFrameId.ToString() }
                    });
                }
                else
                {
                    Debug.WriteLine("MaintenanceDetailViewModel - ���� ������ ã�� �� ����");
                    ErrorMessage = "�������� ������ ã�� �� �����ϴ�.";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MaintenanceDetailViewModel - ���� �� �ε� ����: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                ErrorMessage = "�������� ������ �ҷ����µ� �����߽��ϴ�.";
            }
            finally
            {
                IsBusy = false;
                Debug.WriteLine($"MaintenanceDetailViewModel - LoadMaintenance �Ϸ�");
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
                Debug.WriteLine("MaintenanceDetailViewModel - GoBack ȣ���");
                
                if (Maintenance != null)
                {
                    Debug.WriteLine($"MaintenanceDetailViewModel - ������ �� �������� �̵�: {Maintenance.BikeFrameId}");
                    // ���������� ������ �ش� �������� ���� �̷� �������� �̵�
                    await Shell.Current.GoToAsync($"bikes/maintenancehistory?id={Maintenance.BikeFrameId}");
                }
                else
                {
                    Debug.WriteLine("MaintenanceDetailViewModel - �Ϲ����� �ڷ� ����");
                    // �������� ������ ���� ��� �Ϲ����� �ڷ� ����
                    await Shell.Current.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MaintenanceDetailViewModel - �ڷ� ���� ����: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                
                // ���� �߻� �� Ȩ���� �̵�
                try
                {
                    await Shell.Current.GoToAsync("///bikes");
                }
                catch (Exception ex2)
                {
                    Debug.WriteLine($"MaintenanceDetailViewModel - Ȩ���� �̵��� ����: {ex2.Message}");
                }
            }
        }
    }
}