using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BikeHistory.Server.Models
{
    public class UserActivityLog
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("userId")]
        public string? UserId { get; set; }

        [JsonPropertyName("userName")]
        public string? UserName { get; set; }

        [JsonPropertyName("ipAddress")]
        public string? IpAddress { get; set; }

        [JsonPropertyName("pageUrl")]
        public string? PageUrl { get; set; }

        [JsonPropertyName("previousPageUrl")]
        public string? PreviousPageUrl { get; set; }

        [JsonPropertyName("userAgent")]
        public string? UserAgent { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("actionType")]
        public string? ActionType { get; set; } // Navigation, Login, Logout 등

        [JsonPropertyName("additionalDataJson")]
        public string? AdditionalDataJson { get; set; }

        // JSON 데이터 접근을 위한 비영속 속성
        [NotMapped] // EF Core가 이 속성을 데이터베이스에 매핑하지 않도록 표시
        public Dictionary<string, string>? AdditionalData
        {
            get => string.IsNullOrEmpty(AdditionalDataJson)
                ? null
                : JsonSerializer.Deserialize<Dictionary<string, string>>(AdditionalDataJson);
            set => AdditionalDataJson = value != null
                ? JsonSerializer.Serialize(value)
                : null;
        }
    }
}