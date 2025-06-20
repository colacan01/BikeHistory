using System.Text.Json.Serialization;

namespace BikeHistory.Mobile.Models.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MaintenanceType
    {
        Maintenance, // 정비
        Repair,      // 수리
        Custom,      // 커스텀
        Self         // 자가 수리
    }
}