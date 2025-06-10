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

        public BikeDetailViewModel(BikeService bikeService, AuthService authService)
        {
            _bikeService = bikeService;
            _authService = authService;
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
                
                // ���� ����ڰ� ������ ���������� Ȯ��
                IsOwner = Bike.CurrentOwnerId == _authService.CurrentUser?.Id;

                // ������ �̷� �ε�
                await LoadOwnershipHistory();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"������ �� ���� �ε� ����: {ex.Message}");
                ErrorMessage = "������ ������ �ҷ����µ� �����߽��ϴ�.";
            }
            finally
            {
                IsBusy = false;
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

                // OwnershipHistory�� null���� Ȯ�� �� �ʱ�ȭ
                OwnershipHistory ??= new ObservableCollection<OwnershipRecord>();
                OwnershipHistory.Clear();
                foreach (var record in history)
                {
                    OwnershipHistory.Add(record);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"������ �̷� �ε� ����: {ex.Message}");
            }
            finally
            {
                IsHistoryBusy = false;
            }
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