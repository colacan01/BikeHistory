using System.Text.Json.Serialization;

namespace BikeHistory.Mobile.Models.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MaintenanceType
    {
        Maintenance, // ����
        Repair,      // ����
        Custom,      // Ŀ����
        Self         // �ڰ� ����
    }
}