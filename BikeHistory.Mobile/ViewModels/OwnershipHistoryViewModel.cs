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
    public partial class OwnershipHistoryViewModel : ObservableObject, IQueryAttributable
    {
        private readonly BikeService _bikeService;
        private readonly ILogger<OwnershipHistoryViewModel> _logger;

        private BikeFrame? _bikeInfo;
        private ObservableCollection<OwnershipRecord> _ownershipHistory = new();
        private bool _isLoading;
        private string? _errorMessage;
        private int _bikeId;

        public BikeFrame? BikeInfo
        {
            get => _bikeInfo;
            set => SetProperty(ref _bikeInfo, value);
        }

        public ObservableCollection<OwnershipRecord> OwnershipHistory
        {
            get => _ownershipHistory;
            set => SetProperty(ref _ownershipHistory, value);
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

        public OwnershipHistoryViewModel(BikeService bikeService, ILogger<OwnershipHistoryViewModel> logger)
        {
            _bikeService = bikeService;
            _logger = logger;
            
            GoBackCommand = new AsyncRelayCommand(GoBackAsync);
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            Debug.WriteLine($"OwnershipHistoryViewModel - ApplyQueryAttributes ȣ���");
            
            if (query.TryGetValue("id", out var idValue))
            {
                BikeId = Convert.ToInt32(idValue);
                Debug.WriteLine($"OwnershipHistoryViewModel - BikeId ������: {BikeId}");
                _ = LoadOwnershipHistory(BikeId);
            }
            else
            {
                Debug.WriteLine("OwnershipHistoryViewModel - id �Ķ���͸� ã�� �� ����");
            }
        }

        public async Task LoadOwnershipHistory(int bikeId)
        {
            try
            {
                Debug.WriteLine($"OwnershipHistoryViewModel - LoadOwnershipHistory ����: {bikeId}");
                IsLoading = true;
                ErrorMessage = string.Empty;

                // Load bike info
                BikeInfo = await _bikeService.GetBikeById(bikeId);
                Debug.WriteLine($"OwnershipHistoryViewModel - ������ ���� �ε� �Ϸ�: {BikeInfo?.FrameNumber}");

                // Load ownership history
                var history = await _bikeService.GetBikeHistory(bikeId);
                Debug.WriteLine($"OwnershipHistoryViewModel - ������ �̷� �ε� �Ϸ�: {history.Count}��");
                
                OwnershipHistory.Clear();
                foreach (var record in history.OrderByDescending(x => x.TransferDate))
                {
                    OwnershipHistory.Add(record);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OwnershipHistoryViewModel - �ε� ����: {ex.Message}");
                _logger.LogError(ex, "Failed to load ownership history for bike {BikeId}", bikeId);
                ErrorMessage = "������ �̷��� �ҷ����µ� �����߽��ϴ�. �ٽ� �õ����ּ���.";
            }
            finally
            {
                IsLoading = false;
                Debug.WriteLine($"OwnershipHistoryViewModel - LoadOwnershipHistory �Ϸ�");
            }
        }

        private async Task GoBackAsync()
        {
            try
            {
                Debug.WriteLine("OwnershipHistoryViewModel - GoBack ȣ���");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OwnershipHistoryViewModel - GoBack ����: {ex.Message}");
            }
        }
    }
}