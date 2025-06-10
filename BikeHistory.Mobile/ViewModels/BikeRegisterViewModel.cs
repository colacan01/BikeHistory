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

                // 제조사 로드
                if (Manufacturers == null)
                    Manufacturers = new ObservableCollection<Manufacturer>();
                Manufacturers.Clear();
                var manufacturerList = await _catalogService.GetManufacturers();
                foreach (var manufacturer in manufacturerList)
                {
                    Manufacturers.Add(manufacturer);
                }

                // 브랜드 로드
                if (Brands == null)
                    Brands = new ObservableCollection<Brand>();
                Brands.Clear();
                var brandList = await _catalogService.GetBrands();
                foreach (var brand in brandList)
                {
                    Brands.Add(brand);
                }

                // 자전거 타입 로드
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