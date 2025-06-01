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
        private readonly AuthService _authService; // 추가: AuthService 의존성

        public BikeService(AuthService authService) // 생성자 의존성 주입
        {
            _httpClient = new HttpClient();
            _baseUrl = Constants.BaseApiUrl;
            _authService = authService;

            // 초기 토큰 설정
            UpdateAuthToken(_authService.CurrentUser?.Token);

            // AuthService의 상태 변경 이벤트 구독
            _authService.AuthenticationStateChanged += OnAuthenticationStateChanged;
        }

        public async Task<List<BikeFrame>> GetBikes()
        {
            // 요청 직전에 토큰 확인 및 업데이트 (추가)
            UpdateAuthToken(_authService.CurrentUser?.Token);

            var response = await _httpClient.GetAsync($"{_baseUrl}/BikeFrames");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<BikeFrame>>();
        }

        public async Task<BikeFrame> GetBikeById(int id)
        {
            // 요청 직전에 토큰 확인 및 업데이트 (추가)
            UpdateAuthToken(_authService.CurrentUser?.Token);

            var response = await _httpClient.GetAsync($"{_baseUrl}/BikeFrames/{id}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<BikeFrame>();
        }

        public async Task<BikeFrame> RegisterBike(BikeFrameRegisterRequest request)
        {
            // 요청 직전에 토큰 확인 및 업데이트 (추가)
            UpdateAuthToken(_authService.CurrentUser?.Token);

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


        private void OnAuthenticationStateChanged()
        {
            System.Diagnostics.Debug.WriteLine("BikeService: 인증 상태 변경 감지됨");
            // 인증 상태 변경 시 토큰 업데이트
            UpdateAuthToken(_authService.CurrentUser?.Token);
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
