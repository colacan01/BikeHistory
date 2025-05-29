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

        [ObservableProperty]
        private string frameNumber;

        [ObservableProperty]
        private string model;

        [ObservableProperty]
        private string color;

        [ObservableProperty]
        private int? manufactureYear;

        [ObservableProperty]
        private ObservableCollection<Manufacturer> manufacturers;

        [ObservableProperty]
        private ObservableCollection<Brand> brands;

        [ObservableProperty]
        private ObservableCollection<BikeType> bikeTypes;

        [ObservableProperty]
        private Manufacturer selectedManufacturer;

        [ObservableProperty]
        private Brand selectedBrand;

        [ObservableProperty]
        private BikeType selectedBikeType;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string errorMessage;

        [ObservableProperty]
        private string successMessage;

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
                var manufacturerList = await _catalogService.GetManufacturers();
                Manufacturers.Clear();
                foreach (var manufacturer in manufacturerList)
                {
                    Manufacturers.Add(manufacturer);
                }

                // �귣�� �ε�
                var brandList = await _catalogService.GetBrands();
                Brands.Clear();
                foreach (var brand in brandList)
                {
                    Brands.Add(brand);
                }

                // ������ Ÿ�� �ε�
                var bikeTypeList = await _catalogService.GetBikeTypes();
                BikeTypes.Clear();
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