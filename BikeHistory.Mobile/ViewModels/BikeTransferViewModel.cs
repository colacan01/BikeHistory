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

            // null을 허용하지 않는 필드 초기화
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
                
                // 현재 사용자가 자전거의 소유자인지 확인
                bool isOwner = Bike.CurrentOwnerId == _authService.CurrentUser?.Id;
                
                if (!isOwner)
                {
                    ErrorMessage = "자신의 자전거만 소유권을 이전할 수 있습니다.";
                    await Shell.Current.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"자전거 정보 로드 오류: {ex.Message}");
                ErrorMessage = "자전거 정보를 불러오는데 실패했습니다.";
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

            // 유효성 검사
            if (string.IsNullOrEmpty(NewOwnerId))
            {
                ErrorMessage = "새 소유자의 ID를 입력해주세요.";
                return;
            }

            if (NewOwnerId == _authService.CurrentUser?.Id)
            {
                ErrorMessage = "자신에게 소유권을 이전할 수 없습니다.";
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
                    SuccessMessage = "소유권이 성공적으로 이전되었습니다.";
                    
                    // 잠시 후 이전 페이지로 이동
                    await Task.Delay(2000);
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    ErrorMessage = "소유권 이전에 실패했습니다.";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"소유권 이전 오류: {ex.Message}");
                ErrorMessage = "소유권 이전 중 오류가 발생했습니다.";
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