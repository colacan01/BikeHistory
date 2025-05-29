using BikeHistory.Mobile.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace BikeHistory.Mobile.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public User CurrentUser { get; private set; }
        public bool IsLoggedIn => CurrentUser != null;

        public event Action AuthenticationStateChanged;

        public AuthService()
        {
            _httpClient = new HttpClient();
            _baseUrl = Constants.BaseApiUrl;

            // 저장된 토큰이 있으면 사용자 정보 복원
            LoadUserFromToken();
        }

        private void LoadUserFromToken()
        {
            var token = SecureStorage.GetAsync(Constants.AuthTokenKey).Result;

            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(token);

                    CurrentUser = new User
                    {
                        Id = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid" || c.Type == "sub")?.Value,
                        Email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value,
                        FirstName = jwtToken.Claims.FirstOrDefault(c => c.Type == "given_name")?.Value,
                        LastName = jwtToken.Claims.FirstOrDefault(c => c.Type == "family_name")?.Value,
                        Token = token
                    };

                    // 토큰 헤더 설정
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
                catch
                {
                    // 토큰 파싱 실패시 제거
                    SecureStorage.Remove(Constants.AuthTokenKey);
                }
            }
        }

        public async Task<AuthResponse> Login(LoginRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/auth/login", request);
            response.EnsureSuccessStatusCode();

            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();

            // 토큰 저장 및 헤더 설정
            await SecureStorage.SetAsync(Constants.AuthTokenKey, authResponse.Token);
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResponse.Token);

            // 사용자 정보 설정
            CurrentUser = new User
            {
                Id = authResponse.UserId,
                Email = authResponse.Email,
                FirstName = authResponse.FirstName,
                LastName = authResponse.LastName,
                Token = authResponse.Token
            };

            AuthenticationStateChanged?.Invoke();

            return authResponse;
        }

        public async Task<bool> Register(RegisterRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/auth/register", request);
            return response.IsSuccessStatusCode;
        }

        public async Task Logout()
        {
            // 토큰 제거
            await SecureStorage.SetAsync(Constants.AuthTokenKey, string.Empty);
            _httpClient.DefaultRequestHeaders.Authorization = null;

            CurrentUser = null;
            AuthenticationStateChanged?.Invoke();
        }

        public async Task<ProfileResponse> GetProfile()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/auth/profile");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<ProfileResponse>();
        }

        public async Task<AuthResponse> UpdateProfile(UpdateProfileRequest request)
        {
            var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}/auth/profile", request);
            response.EnsureSuccessStatusCode();

            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();

            if (!string.IsNullOrEmpty(authResponse.Token))
            {
                await SecureStorage.SetAsync(Constants.AuthTokenKey, authResponse.Token);
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResponse.Token);

                CurrentUser.FirstName = authResponse.FirstName;
                CurrentUser.LastName = authResponse.LastName;
                CurrentUser.Token = authResponse.Token;

                AuthenticationStateChanged?.Invoke();
            }

            return authResponse;
        }

        public bool IsAdmin
        {
            get
            {
                if (CurrentUser?.Token == null) return false;

                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(CurrentUser.Token);

                    var roleClaim = jwtToken.Claims.FirstOrDefault(c =>
                        c.Type == "role" ||
                        c.Type == "roles" ||
                        c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");

                    if (roleClaim != null)
                    {
                        return roleClaim.Value == "Admin";
                    }

                    return false;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}