using BikeHistory.Mobile.Models;
using BikeHistory.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace BikeHistory.Mobile.ViewModels
{
    [QueryProperty(nameof(BikeId), "id")]
    public partial class BikeTransferViewModel : ObservableObject, IQueryAttributable
    {
        private readonly BikeService _bikeService;
        private readonly AuthService _authService;

        [ObservableProperty]
        private BikeFrame bike;

        [ObservableProperty]
        private string newOwnerId;

        [ObservableProperty]
        private string notes;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string errorMessage;

        [ObservableProperty]
        private string successMessage;

        [ObservableProperty]
        private int bikeId;

        public BikeTransferViewModel(BikeService bikeService, AuthService authService)
        {
            _bikeService = bikeService;
            _authService = authService;
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
            if (BikeId <= 0 || IsLoading)
                return;

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                SuccessMessage = string.Empty;

                Bike = await _bikeService.GetBikeById(BikeId);
                
                // ���� ����ڰ� �������� ���������� Ȯ��
                bool isOwner = Bike.CurrentOwnerId == _authService.CurrentUser?.Id;
                
                if (!isOwner)
                {
                    ErrorMessage = "�ڽ��� �����Ÿ� �������� ������ �� �ֽ��ϴ�.";
                    await Shell.Current.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"������ ���� �ε� ����: {ex.Message}");
                ErrorMessage = "������ ������ �ҷ����µ� �����߽��ϴ�.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task TransferOwnership()
        {
            if (IsBusy)
                return;

            // ��ȿ�� �˻�
            if (string.IsNullOrEmpty(NewOwnerId))
            {
                ErrorMessage = "�� �������� ID�� �Է����ּ���.";
                return;
            }

            if (NewOwnerId == _authService.CurrentUser?.Id)
            {
                ErrorMessage = "�ڽſ��� �������� ������ �� �����ϴ�.";
                return;
            }

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;
                SuccessMessage = string.Empty;

                var request = new OwnershipTransferRequest
                {
                    NewOwnerId = NewOwnerId,
                    Notes = Notes
                };

                var success = await _bikeService.TransferBike(BikeId, request);

                if (success)
                {
                    SuccessMessage = "�������� ���������� �����Ǿ����ϴ�.";
                    
                    // ��� �� ���� �������� �̵�
                    await Task.Delay(2000);
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    ErrorMessage = "������ ������ �����߽��ϴ�.";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"������ ���� ����: {ex.Message}");
                ErrorMessage = "������ ���� �� ������ �߻��߽��ϴ�.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task Cancel()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}