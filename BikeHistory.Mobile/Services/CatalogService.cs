using BikeHistory.Mobile.Models;
using System.Net.Http.Json;

namespace BikeHistory.Mobile.Services
{
    public class CatalogService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public CatalogService()
        {
            _httpClient = new HttpClient();
            _baseUrl = Constants.BaseApiUrl;

            // 토큰 확인 및 설정
            var token = SecureStorage.GetAsync(Constants.AuthTokenKey).Result;
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<List<Manufacturer>> GetManufacturers()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/catalog/manufacturers");
            response.EnsureSuccessStatusCode();

            var manufacturers = await response.Content.ReadFromJsonAsync<List<Manufacturer>>();
            return manufacturers ?? new List<Manufacturer>();
        }

        public async Task<List<Brand>> GetBrands()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/catalog/brands");
            response.EnsureSuccessStatusCode();

            var brands = await response.Content.ReadFromJsonAsync<List<Brand>>();
            return brands ?? new List<Brand>();
        }

        public async Task<List<BikeType>> GetBikeTypes()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/catalog/biketypes");
            response.EnsureSuccessStatusCode();

            var bikeTypes = await response.Content.ReadFromJsonAsync<List<BikeType>>();
            return bikeTypes ?? new List<BikeType>();
        }
    }
}