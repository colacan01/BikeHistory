using System.Text.Json.Serialization;

namespace BikeHistory.Mobile.Models
{
    public class UserActivityLog
    {
        [JsonPropertyName("userId")]
        public string? UserId { get; set; }

        [JsonPropertyName("userName")]
        public string? UserName { get; set; }

        [JsonPropertyName("pageUrl")]
        public string? PageUrl { get; set; }

        [JsonPropertyName("previousPageUrl")]
        public string? PreviousPageUrl { get; set; }

        [JsonPropertyName("actionType")]
        public string? ActionType { get; set; }

        [JsonPropertyName("additionalData")]
        public Dictionary<string, string>? AdditionalData { get; set; }
    }
}