using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BikeHistory.Mobile.Models;
using System.Net.Http.Json;

namespace BikeHistory.Mobile.Services
{
    public class BikeService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public BikeService()
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

        public async Task<List<BikeFrame>> GetBikes()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/BikeFrames");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<BikeFrame>>();
        }

        public async Task<BikeFrame> GetBikeById(int id)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/BikeFrames/{id}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<BikeFrame>();
        }

        public async Task<BikeFrame> RegisterBike(BikeFrameRegisterRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/BikeFrames", request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<BikeFrame>();
        }

        public async Task<BikeFrame> UpdateBike(int id, BikeFrameRegisterRequest request)
        {
            var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}/BikeFrames/{id}", request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<BikeFrame>();
        }

        public async Task<bool> TransferBike(int id, OwnershipTransferRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/BikeFrames/{id}/transfer", request);
            return response.IsSuccessStatusCode;
        }

        public async Task<List<OwnershipRecord>> GetBikeHistory(int id)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/BikeFrames/{id}/history");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<OwnershipRecord>>();
        }

        // 토큰이 변경될 때 호출되는 메서드
        public void UpdateAuthToken(string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }
    }
}
