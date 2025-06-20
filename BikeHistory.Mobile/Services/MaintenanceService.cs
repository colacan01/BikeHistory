using BikeHistory.Mobile.Models;
using System.Net.Http.Json;

namespace BikeHistory.Mobile.Services
{
    public class MaintenanceService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public MaintenanceService()
        {
            _httpClient = new HttpClient();
            _baseUrl = Constants.BaseApiUrl;
        }

        // 특정 자전거의 유지보수 기록 가져오기
        public async Task<List<Maintenance>> GetMaintenancesByBikeId(int bikeId)
        {
            try
            {
                var token = await SecureStorage.GetAsync(Constants.AuthTokenKey);
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.GetFromJsonAsync<List<Maintenance>>($"{_baseUrl}/maintenances/bike/{bikeId}");
                return response ?? new List<Maintenance>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"유지보수 내역 로드 오류: {ex.Message}");
                return new List<Maintenance>();
            }
        }

        // 특정 유지보수 상세 가져오기
        public async Task<Maintenance?> GetMaintenanceById(string id)
        {
            try
            {
                var token = await SecureStorage.GetAsync(Constants.AuthTokenKey);
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                return await _httpClient.GetFromJsonAsync<Maintenance>($"{_baseUrl}/maintenances/{id}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"유지보수 상세 로드 오류: {ex.Message}");
                return null;
            }
        }
    }
}