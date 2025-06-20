using BikeHistory.Mobile.Models;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace BikeHistory.Mobile.Services
{
    public class ActivityLoggerService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthService _authService;
        private string _lastPage = string.Empty;

        public ActivityLoggerService(AuthService authService)
        {
            _httpClient = new HttpClient
            {
                DefaultRequestHeaders =
                {
                    Authorization = authService.CurrentUser?.Token != null
                        ? new AuthenticationHeaderValue("Bearer", authService.CurrentUser.Token)
                        : null
                }
            };
            _authService = authService;
        }

        public async Task LogNavigationAsync(string currentPage)
        {
            if (_authService.IsLoggedIn && _authService.CurrentUser != null)
            {
                var log = new UserActivityLog
                {
                    UserId = _authService.CurrentUser.Id,
                    UserName = _authService.CurrentUser.Email,
                    PageUrl = currentPage,
                    PreviousPageUrl = _lastPage,
                    ActionType = "[Mobile] Navigation"
                };

                await SendLogAsync(log);
                _lastPage = currentPage;
            }
        }

        public async Task LogActionAsync(string actionType, Dictionary<string, string>? additionalData = null)
        {
            if (_authService.IsLoggedIn && _authService.CurrentUser != null)
            {
                var log = new UserActivityLog
                {
                    UserId = _authService.CurrentUser.Id,
                    UserName = _authService.CurrentUser.Email,
                    PageUrl = Shell.Current.CurrentState.Location.ToString(),
                    PreviousPageUrl = _lastPage,
                    ActionType = "[Mobile] " + actionType,
                    AdditionalData = additionalData
                };

                await SendLogAsync(log);
            }
        }

        private async Task SendLogAsync(UserActivityLog log)
        {
            try
            {
                // ��ū�� �ִٸ� ���� ��� ����
                if (_authService.CurrentUser?.Token != null)
                {
                    _httpClient.DefaultRequestHeaders.Authorization = 
                        new AuthenticationHeaderValue("Bearer", _authService.CurrentUser.Token);
                }

                await _httpClient.PostAsJsonAsync($"{Constants.BaseApiUrl}/useractivity", log);
            }
            catch (Exception ex)
            {
                // �α� ���д� �� ��ɿ� ������ ���� �ʵ��� ������ ó��
                Debug.WriteLine($"�α� ���� ����: {ex.Message}");
            }
        }
    }
}