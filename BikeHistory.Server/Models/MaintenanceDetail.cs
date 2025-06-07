using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BikeHistory.Server.Models
{
    public class MaintenanceDetail
    {
        [Required]
        [JsonPropertyName("maintenanceId")]
        public Guid MaintenanceId { get; set; }
        
        [Key]
        [Required]
        [JsonPropertyName("seq")]
        public int Seq { get; set; }
        
        [Required]
        [JsonPropertyName("laborCost")]
        public decimal LaborCost { get; set; }
        
        [Required]
        [JsonPropertyName("partPrice")]
        public decimal PartPrice { get; set; }
        
        [JsonPropertyName("partName")]
        public string? PartName { get; set; }
        
        [JsonPropertyName("partSpecification")]
        public string? PartSpecification { get; set; }
        
        // Navigation property
        [ForeignKey("MaintenanceId")]
        [JsonPropertyName("maintenance")]
        public virtual Maintenance Maintenance { get; set; } = null!;
    }
}