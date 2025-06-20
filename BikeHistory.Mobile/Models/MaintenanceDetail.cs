using System.Text.Json.Serialization;

namespace BikeHistory.Mobile.Models
{
    public class MaintenanceDetail
    {
        [JsonPropertyName("maintenanceId")]
        public Guid MaintenanceId { get; set; }
        
        [JsonPropertyName("seq")]
        public int Seq { get; set; }
        
        [JsonPropertyName("laborCost")]
        public decimal LaborCost { get; set; }
        
        [JsonPropertyName("partPrice")]
        public decimal PartPrice { get; set; }
        
        [JsonPropertyName("partName")]
        public string? PartName { get; set; }
        
        [JsonPropertyName("partSpecification")]
        public string? PartSpecification { get; set; }

        [JsonIgnore]
        public decimal TotalCost => LaborCost + PartPrice;
    }
}