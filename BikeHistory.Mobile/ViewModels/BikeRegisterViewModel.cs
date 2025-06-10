using BikeHistory.Mobile.Models;
using BikeHistory.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace BikeHistory.Mobile.ViewModels
{
    public partial class BikeRegisterViewModel : ObservableObject
    {
        private readonly BikeService _bikeService;
        private readonly CatalogService _catalogService;

        private string? frameNumber;
        public string? FrameNumber
        {
            get => frameNumber;
            set => SetProperty(ref frameNumber, value);
        }

        private string? model;
        public string? Model
        {
            get => model;
            set => SetProperty(ref model, value);
        }

        private string? color;
        public string? Color
        {
            get => color;
            set => SetProperty(ref color, value);
        }

        private int? manufactureYear;
        public int? ManufactureYear
        {
            get => manufactureYear;
            set => SetProperty(ref manufactureYear, value);
        }
        
        private ObservableCollection<Manufacturer>? manufacturers;
        public ObservableCollection<Manufacturer>? Manufacturers
        {
            get => manufacturers;
            set => SetProperty(ref manufacturers, value);
        }
        
        private ObservableCollection<Brand>? brands;
        public ObservableCollection<Brand>? Brands
        {
            get => brands;
            set => SetProperty(ref brands, value);
        }
        
        private ObservableCollection<BikeType>? bikeTypes;
        public ObservableCollection<BikeType>? BikeTypes
        {
            get => bikeTypes;
            set => SetProperty(ref bikeTypes, value);
        }
        
        private Manufacturer? selectedManufacturer;
        public Manufacturer? SelectedManufacturer
        {
            get => selectedManufacturer;
            set => SetProperty(ref selectedManufacturer, value);
        }
        
        private Brand? selectedBrand;
        public Brand? SelectedBrand
        {
            get => selectedBrand;
            set => SetProperty(ref selectedBrand, value);
        }
        
        private BikeType? selectedBikeType;
        public BikeType? SelectedBikeType
        {
            get => selectedBikeType;
            set => SetProperty(ref selectedBikeType, value);
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
        
        private string? errorMessage;
        public string? ErrorMessage
        {
            get => errorMessage;
            set => SetProperty(ref errorMessage, value);
        }
        
        private string? successMessage;
        public string? SuccessMessage
        {
            get => successMessage;
            set => SetProperty(ref successMessage, value);
        }
        public BikeRegisterViewModel(BikeService bikeService, CatalogService catalogService)
        {
            _bikeService = bikeService;
            _catalogService = catalogService;

            Manufacturers = new ObservableCollection<Manufacturer>();
            Brands = new ObservableCollection<Brand>();
            BikeTypes = new ObservableCollection<BikeType>();
        }

        [RelayCommand]
        private async Task LoadCatalogData()
        {
            if (IsLoading)
                return;

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                // ������ �ε�
                if (Manufacturers == null)
                    Manufacturers = new ObservableCollection<Manufacturer>();
                Manufacturers.Clear();
                var manufacturerList = await _catalogService.GetManufacturers();
                foreach (var manufacturer in manufacturerList)
                {
                    Manufacturers.Add(manufacturer);
                }

                // �귣�� �ε�
                if (Brands == null)
                    Brands = new ObservableCollection<Brand>();
                Brands.Clear();
                var brandList = await _catalogService.GetBrands();
                foreach (var brand in brandList)
                {
                    Brands.Add(brand);
                }

                // ������ Ÿ�� �ε�
                if (BikeTypes == null)
                    BikeTypes = new ObservableCollection<BikeType>();
                BikeTypes.Clear();
                var bikeTypeList = await _catalogService.GetBikeTypes();
                foreach (var bikeType in bikeTypeList)
                {
                    BikeTypes.Add(bikeType);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"īŻ�α� ������ �ε� ����: {ex.Message}");
                ErrorMessage = "īŻ�α� �����͸� �ҷ����µ� �����߽��ϴ�.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task RegisterBike()
        {
            if (IsBusy)
                return;

            // ��ȿ�� �˻�
            if (string.IsNullOrEmpty(FrameNumber))
            {
                ErrorMessage = "������ ��ȣ�� �Է����ּ���.";
                return;
            }

            if (SelectedManufacturer == null)
            {
                ErrorMessage = "�����縦 �������ּ���.";
                return;
            }

            if (SelectedBrand == null)
            {
                ErrorMessage = "�귣�带 �������ּ���.";
                return;
            }

            if (SelectedBikeType == null)
            {
                ErrorMessage = "������ Ÿ���� �������ּ���.";
                return;
            }

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;
                SuccessMessage = string.Empty;

                var request = new BikeFrameRegisterRequest
                {
                    FrameNumber = FrameNumber,
                    ManufacturerId = SelectedManufacturer.Id,
                    BrandId = SelectedBrand.Id,
                    BikeTypeId = SelectedBikeType.Id,
                    Model = Model,
                    ManufactureYear = ManufactureYear,
                    Color = Color
                };

                var result = await _bikeService.RegisterBike(request);

                SuccessMessage = "�����Ű� ���������� ��ϵǾ����ϴ�.";
                
                // ���� �޽��� ǥ�� �� ���� �������� �̵�
                await Task.Delay(2000);
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"������ ��� ����: {ex.Message}");
                ErrorMessage = "������ ��Ͽ� �����߽��ϴ�.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task GoBack()
        {
            await Shell.Current.GoToAsync("..");
        }

        public async Task OnAppearing()
        {
            await LoadCatalogData();
        }
    }
}