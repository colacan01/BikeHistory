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

        private BikeFrame? bike;
        public BikeFrame? Bike
        {
            get => bike;
            set => SetProperty(ref bike, value);
        }

        private string newOwnerId;
        public string NewOwnerId 
        { 
            get => newOwnerId; 
            set => SetProperty(ref newOwnerId, value); 
        }

        private string notes;
        public string Notes 
        { 
            get => notes; 
            set => SetProperty(ref notes, value); 
        }

        private bool isBusy;
        public bool IsBusy
        {
            get => isBusy;
            set => SetProperty(ref isBusy, value);
        }

        private bool isLoading;
        public bool IsLoading
        {
            get => isLoading;
            set => SetProperty(ref isLoading, value);
        }

        private string errorMessage;
        public string ErrorMessage
        { 
            get => errorMessage; 
            set => SetProperty(ref errorMessage, value); 
        }

        private string successMessage;
        public string SuccessMessage
        {
            get => successMessage;
            set => SetProperty(ref successMessage, value);
        }

        private int bikeId;
        public int BikeId
        {
            get => bikeId; 
            set => SetProperty(ref bikeId, value);
        }

        public BikeTransferViewModel(BikeService bikeService, AuthService authService)
        {
            _bikeService = bikeService;
            _authService = authService;

            // null�� ������� �ʴ� �ʵ� �ʱ�ȭ
            newOwnerId = string.Empty;
            notes = string.Empty;
            errorMessage = string.Empty;
            successMessage = string.Empty;
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