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

                // 제조사 로드
                var manufacturerList = await _catalogService.GetManufacturers();
                Manufacturers.Clear();
                foreach (var manufacturer in manufacturerList)
                {
                    Manufacturers.Add(manufacturer);
                }

                // 브랜드 로드
                var brandList = await _catalogService.GetBrands();
                Brands.Clear();
                foreach (var brand in brandList)
                {
                    Brands.Add(brand);
                }

                // 자전거 타입 로드
                var bikeTypeList = await _catalogService.GetBikeTypes();
                BikeTypes.Clear();
                foreach (var bikeType in bikeTypeList)
                {
                    BikeTypes.Add(bikeType);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"카탈로그 데이터 로드 오류: {ex.Message}");
                ErrorMessage = "카탈로그 데이터를 불러오는데 실패했습니다.";
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

            // 유효성 검사
            if (string.IsNullOrEmpty(FrameNumber))
            {
                ErrorMessage = "프레임 번호를 입력해주세요.";
                return;
            }

            if (SelectedManufacturer == null)
            {
                ErrorMessage = "제조사를 선택해주세요.";
                return;
            }

            if (SelectedBrand == null)
            {
                ErrorMessage = "브랜드를 선택해주세요.";
                return;
            }

            if (SelectedBikeType == null)
            {
                ErrorMessage = "자전거 타입을 선택해주세요.";
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

                SuccessMessage = "자전거가 성공적으로 등록되었습니다.";
                
                // 성공 메시지 표시 후 이전 페이지로 이동
                await Task.Delay(2000);
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"자전거 등록 오류: {ex.Message}");
                ErrorMessage = "자전거 등록에 실패했습니다.";
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