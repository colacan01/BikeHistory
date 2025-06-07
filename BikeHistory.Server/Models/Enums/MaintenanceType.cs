using System.Text.Json.Serialization;

namespace BikeHistory.Server.Models.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MaintenanceType
    {
        Maintenance,  // ����
        Repair,       // ����
        Custom,       // Ŀ����
        Self          // self
    }
}