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
        private readonly MaintenanceService _maintenanceService;
        private readonly ActivityLoggerService _activityLogger;

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

        private ObservableCollection<Maintenance>? maintenancHistory;
        public ObservableCollection<Maintenance>? MaintenancHistory
        {
            get => maintenancHistory;
            private set => SetProperty(ref maintenancHistory, value);
        }

        public ObservableCollection<OwnershipRecord> RecentOwnershipHistory
        {
            get
            {
                if (OwnershipHistory == null || OwnershipHistory.Count == 0)
                    return new ObservableCollection<OwnershipRecord>();

                return new ObservableCollection<OwnershipRecord>(
                    OwnershipHistory.OrderByDescending(x => x.TransferDate).Take(2)
                );
            }
        }

        public ObservableCollection<Maintenance> RecentMaintenanceHistory
        {
            get
            {
                if (MaintenancHistory == null || MaintenancHistory.Count == 0)
                    return new ObservableCollection<Maintenance>();

                return new ObservableCollection<Maintenance>(
                    MaintenancHistory.OrderByDescending(x => x.MaintenanceDate).Take(2)
                );
            }
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

        private bool isMaintenanceBusy;
        public bool IsMaintenanceBusy
        {
            get => isMaintenanceBusy;
            set => SetProperty(ref isMaintenanceBusy, value);
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

        public BikeDetailViewModel(BikeService bikeService, AuthService authService, MaintenanceService maintenanceService, ActivityLoggerService activityLogger)
        {
            _bikeService = bikeService;
            _authService = authService;
            _maintenanceService = maintenanceService;
            _activityLogger = activityLogger;

            OwnershipHistory = [];
            MaintenancHistory = [];
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("id", out var idValue))
            {
                BikeId = Convert.ToInt32(idValue);
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

                Debug.WriteLine($"LoadBike ���� - BikeId: {BikeId}");

                Bike = await _bikeService.GetBikeById(BikeId);
                
                Debug.WriteLine($"������ �ε� �Ϸ�: {Bike?.FrameNumber}");
                Debug.WriteLine($"���� ������: {Bike?.CurrentOwnerId}");
                Debug.WriteLine($"�α����� �����: {_authService.CurrentUser?.Id}");
                
                // ���� ����ڰ� ������ ���������� Ȯ��
                IsOwner = Bike?.CurrentOwnerId == _authService.CurrentUser?.Id;
                
                Debug.WriteLine($"������ ����: {IsOwner}");

                // ������ �̷� �ε�
                await LoadOwnershipHistory();

                // �������� �̷� �ε�
                await LoadMaintenanceHistory();

                // ������ �� ������ ��ȸ �α�
                await _activityLogger.LogActionAsync("ViewBikeDetail", new Dictionary<string, string>
                {
                    { "bikeId", bikeId.ToString() }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"������ �� ���� �ε� ����: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                ErrorMessage = "������ ������ �ҷ����µ� �����߽��ϴ�.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task LoadMaintenanceHistory()
        {
            if (BikeId <= 0 || IsMaintenanceBusy)
                return;

            try
            {
                IsMaintenanceBusy = true;

                var history = await _maintenanceService.GetMaintenancesByBikeId(BikeId);

                MaintenancHistory ??= [];
                MaintenancHistory.Clear();
                foreach (var record in history)
                {
                    MaintenancHistory.Add(record);
                }

                // Recent ������Ƽ ������Ʈ �˸�
                OnPropertyChanged(nameof(RecentMaintenanceHistory));

                // ������ �������� ���� ��ȸ �α�
                await _activityLogger.LogActionAsync("ViewMaintenanceHistoryAll", new Dictionary<string, string>
                {
                    { "bikeId", bikeId.ToString() }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"�������� �̷� �ε� ����: {ex.Message}");
                ErrorMessage = "������ �������� �̷��� �ҷ����µ� �����߽��ϴ�.";
            }
            finally
            {
                IsMaintenanceBusy = false;
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

                OwnershipHistory ??= [];
                OwnershipHistory.Clear();
                foreach (var record in history)
                {
                    OwnershipHistory.Add(record);
                }

                // Recent ������Ƽ ������Ʈ �˸�
                OnPropertyChanged(nameof(RecentOwnershipHistory));

                // ������ ���� ���� ��ȸ �α�
                await _activityLogger.LogActionAsync("ViewOwnershipHistoryAll", new Dictionary<string, string>
                {
                    { "bikeId", bikeId.ToString() }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"������ �̷� �ε� ����: {ex.Message}");
                ErrorMessage = "������ ������ �̷��� �ҷ����µ� �����߽��ϴ�.";
            }
            finally
            {
                IsHistoryBusy = false;
            }
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadBike();            
        }

        [RelayCommand]
        private async Task TransferOwnership()
        {
            try
            {
                // ������� ���� �α�
                Debug.WriteLine($"TransferOwnership ȣ��� - BikeId: {BikeId}");
                Debug.WriteLine($"Bike null ����: {Bike == null}");
                Debug.WriteLine($"IsOwner: {IsOwner}");
                Debug.WriteLine($"CurrentUser: {_authService.CurrentUser?.Email}");

                if (Bike == null)
                {
                    Debug.WriteLine("������ ������ �����ϴ�.");
                    ErrorMessage = "������ ������ ���� �ε����ּ���.";
                    return;
                }

                if (!IsOwner)
                {
                    Debug.WriteLine("�����ڰ� �ƴմϴ�.");
                    ErrorMessage = "�ڽ��� �����Ÿ� ������ �� �ֽ��ϴ�.";
                    return;
                }

                Debug.WriteLine($"�׺���̼� �õ�: ///bikes/transfer?id={BikeId}");
                
                // ������ ���� ������ ��ȸ �α�
                await _activityLogger.LogActionAsync("NavigateToTransferPage", new Dictionary<string, string>
                {
                    { "bikeId", BikeId.ToString() }
                });

                // ���� ��η� �׺���̼� (�ٸ� ���� ��ġ�ϵ���)
                await Shell.Current.GoToAsync($"///bikes/transfer?id={BikeId}");
                
                Debug.WriteLine("�׺���̼� �Ϸ�");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"TransferOwnership ����: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                ErrorMessage = $"������ ���� �������� �̵� �� ������ �߻��߽��ϴ�: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task ViewAllOwnershipHistory()
        {
            try
            {
                Debug.WriteLine($"ViewAllOwnershipHistory ȣ��� - BikeId: {BikeId}");
                
                if (BikeId <= 0)
                {
                    ErrorMessage = "������ ������ �ùٸ��� �ʽ��ϴ�.";
                    return;
                }

                await _activityLogger.LogActionAsync("NavigateToOwnershipHistoryPage", new Dictionary<string, string>
                {
                    { "bikeId", BikeId.ToString() }
                });

                // ���� ���������� ��� ��η� �׺���̼� 
                Debug.WriteLine($"�׺���̼� �õ�: bikes/ownershiphistory?id={BikeId}");
                await Shell.Current.GoToAsync($"bikes/ownershiphistory?id={BikeId}");
                Debug.WriteLine("������ �̷� ������ �׺���̼� �Ϸ�");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ViewAllOwnershipHistory ����: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                
                // �ٸ� ������� �õ�
                try
                {
                    Debug.WriteLine("���� ��η� ��õ�");
                    await Shell.Current.GoToAsync($"///bikes/ownershiphistory?id={BikeId}");
                    Debug.WriteLine("���� ��� �׺���̼� ����");
                }
                catch (Exception ex2)
                {
                    Debug.WriteLine($"���� ��ε� ����: {ex2.Message}");
                    ErrorMessage = $"������ �̷� �������� �̵��ϴµ� �����߽��ϴ�: {ex.Message}";
                }
            }
        }

        [RelayCommand]
        private async Task ViewAllMaintenanceHistory()
        {
            try
            {
                Debug.WriteLine($"ViewAllMaintenanceHistory ȣ��� - BikeId: {BikeId}");
                
                if (BikeId <= 0)
                {
                    ErrorMessage = "������ ������ �ùٸ��� �ʽ��ϴ�.";
                    return;
                }

                await _activityLogger.LogActionAsync("NavigateToMaintenanceHistoryPage", new Dictionary<string, string>
                {
                    { "bikeId", BikeId.ToString() }
                });

                // ���� ���������� ��� ��η� �׺���̼�
                Debug.WriteLine($"�׺���̼� �õ�: bikes/maintenancehistory?id={BikeId}");
                await Shell.Current.GoToAsync($"bikes/maintenancehistory?id={BikeId}");
                Debug.WriteLine("���� �̷� ������ �׺���̼� �Ϸ�");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ViewAllMaintenanceHistory ����: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                
                // �ٸ� ������� �õ�
                try
                {
                    Debug.WriteLine("���� ��η� ��õ�");
                    await Shell.Current.GoToAsync($"///bikes/maintenancehistory?id={BikeId}");
                    Debug.WriteLine("���� ��� �׺���̼� ����");
                }
                catch (Exception ex2)
                {
                    Debug.WriteLine($"���� ��ε� ����: {ex2.Message}");
                    ErrorMessage = $"���� �̷� �������� �̵��ϴµ� �����߽��ϴ�: {ex.Message}";
                }
            }
        }

        [RelayCommand]
        private async Task TestNavigation()
        {
            try
            {
                Debug.WriteLine("�׽�Ʈ �׺���̼� ����");
                await Shell.Current.GoToAsync("///bikes");
                Debug.WriteLine("�׽�Ʈ �׺���̼� �Ϸ�");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"�׽�Ʈ �׺���̼� ����: {ex.Message}");
                ErrorMessage = $"�׺���̼� �׽�Ʈ ����: {ex.Message}";
            }
        }

        [RelayCommand]
        private static async Task GoBack()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}