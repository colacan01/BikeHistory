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
            Debug.WriteLine($"MaintenanceHistoryViewModel - ApplyQueryAttributes ȣ���");
            
            if (query.TryGetValue("id", out var idValue))
            {
                BikeId = Convert.ToInt32(idValue);
                Debug.WriteLine($"MaintenanceHistoryViewModel - BikeId ������: {BikeId}");
                _ = LoadMaintenanceHistory(BikeId);
            }
            else
            {
                Debug.WriteLine("MaintenanceHistoryViewModel - id �Ķ���͸� ã�� �� ����");
            }
        }

        public async Task LoadMaintenanceHistory(int bikeId)
        {
            try
            {
                Debug.WriteLine($"MaintenanceHistoryViewModel - LoadMaintenanceHistory ����: {bikeId}");
                IsLoading = true;
                ErrorMessage = string.Empty;

                // Load bike info
                BikeInfo = await _bikeService.GetBikeById(bikeId);
                Debug.WriteLine($"MaintenanceHistoryViewModel - ������ ���� �ε� �Ϸ�: {BikeInfo?.FrameNumber}");

                // Load maintenance history
                var history = await _maintenanceService.GetMaintenancesByBikeId(bikeId);
                Debug.WriteLine($"MaintenanceHistoryViewModel - ���� �̷� �ε� �Ϸ�: {history.Count}��");
                
                MaintenanceHistory.Clear();
                foreach (var record in history.OrderByDescending(x => x.MaintenanceDate))
                {
                    MaintenanceHistory.Add(record);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MaintenanceHistoryViewModel - �ε� ����: {ex.Message}");
                _logger.LogError(ex, "Failed to load maintenance history for bike {BikeId}", bikeId);
                ErrorMessage = "���� �̷��� �ҷ����µ� �����߽��ϴ�. �ٽ� �õ����ּ���.";
            }
            finally
            {
                IsLoading = false;
                Debug.WriteLine($"MaintenanceHistoryViewModel - LoadMaintenanceHistory �Ϸ�");
            }
        }

        private async Task ViewMaintenanceDetail(Maintenance? maintenance)
        {
            if (maintenance == null) 
            {
                Debug.WriteLine("MaintenanceHistoryViewModel - maintenance�� null�Դϴ�");
                return;
            }

            try
            {
                Debug.WriteLine($"MaintenanceHistoryViewModel - ViewMaintenanceDetail ȣ���: {maintenance.Id}");
                Debug.WriteLine($"MaintenanceHistoryViewModel - �׺���̼� �õ�: bikes/maintenance?id={maintenance.Id}");
                
                // ��� ��η� ���� �õ�
                await Shell.Current.GoToAsync($"bikes/maintenance?id={maintenance.Id}");
                Debug.WriteLine("MaintenanceHistoryViewModel - ���� �� ������ �׺���̼� ����");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MaintenanceHistoryViewModel - ViewMaintenanceDetail ����: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                
                // ���� ��η� ��õ�
                try
                {
                    Debug.WriteLine("MaintenanceHistoryViewModel - ���� ��η� ��õ�");
                    await Shell.Current.GoToAsync($"///bikes/maintenance?id={maintenance.Id}");
                    Debug.WriteLine("MaintenanceHistoryViewModel - ���� ��� �׺���̼� ����");
                }
                catch (Exception ex2)
                {
                    Debug.WriteLine($"MaintenanceHistoryViewModel - ���� ��ε� ����: {ex2.Message}");
                    _logger.LogError(ex2, "Failed to navigate to maintenance detail for maintenance {MaintenanceId}", maintenance.Id);
                    ErrorMessage = "���� �� ������ �ҷ����µ� �����߽��ϴ�.";
                }
            }
        }

        private async Task GoBackAsync()
        {
            try
            {
                Debug.WriteLine("MaintenanceHistoryViewModel - GoBack ȣ���");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MaintenanceHistoryViewModel - GoBack ����: {ex.Message}");
            }
        }
    }
}