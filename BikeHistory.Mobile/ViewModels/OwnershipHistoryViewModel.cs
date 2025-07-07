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
            Debug.WriteLine($"OwnershipHistoryViewModel - ApplyQueryAttributes 호출됨");
            
            if (query.TryGetValue("id", out var idValue))
            {
                BikeId = Convert.ToInt32(idValue);
                Debug.WriteLine($"OwnershipHistoryViewModel - BikeId 설정됨: {BikeId}");
                _ = LoadOwnershipHistory(BikeId);
            }
            else
            {
                Debug.WriteLine("OwnershipHistoryViewModel - id 파라미터를 찾을 수 없음");
            }
        }

        public async Task LoadOwnershipHistory(int bikeId)
        {
            try
            {
                Debug.WriteLine($"OwnershipHistoryViewModel - LoadOwnershipHistory 시작: {bikeId}");
                IsLoading = true;
                ErrorMessage = string.Empty;

                // Load bike info
                BikeInfo = await _bikeService.GetBikeById(bikeId);
                Debug.WriteLine($"OwnershipHistoryViewModel - 자전거 정보 로드 완료: {BikeInfo?.FrameNumber}");

                // Load ownership history
                var history = await _bikeService.GetBikeHistory(bikeId);
                Debug.WriteLine($"OwnershipHistoryViewModel - 소유권 이력 로드 완료: {history.Count}개");
                
                OwnershipHistory.Clear();
                foreach (var record in history.OrderByDescending(x => x.TransferDate))
                {
                    OwnershipHistory.Add(record);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OwnershipHistoryViewModel - 로드 실패: {ex.Message}");
                _logger.LogError(ex, "Failed to load ownership history for bike {BikeId}", bikeId);
                ErrorMessage = "소유권 이력을 불러오는데 실패했습니다. 다시 시도해주세요.";
            }
            finally
            {
                IsLoading = false;
                Debug.WriteLine($"OwnershipHistoryViewModel - LoadOwnershipHistory 완료");
            }
        }

        private async Task GoBackAsync()
        {
            try
            {
                Debug.WriteLine("OwnershipHistoryViewModel - GoBack 호출됨");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OwnershipHistoryViewModel - GoBack 실패: {ex.Message}");
            }
        }
    }
}