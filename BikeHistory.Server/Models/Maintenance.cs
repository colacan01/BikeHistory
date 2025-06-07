using BikeHistory.Server.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BikeHistory.Server.Models
{
    public class Maintenance
    {
        [Key]
        [JsonPropertyName("id")]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        [JsonPropertyName("maintenanceDate")]
        public DateTime MaintenanceDate { get; set; } = DateTime.UtcNow;
        
        [Required]
        [JsonPropertyName("maintenanceType")]
        public MaintenanceType MaintenanceType { get; set; }
        
        [Required]
        [JsonPropertyName("storeId")]
        public string StoreId { get; set; } = string.Empty;
        
        [JsonPropertyName("store")]
        public ApplicationUser Store { get; set; } = null!;
        
        [Required]
        [JsonPropertyName("ownerId")]
        public string OwnerId { get; set; } = string.Empty;
        
        [JsonPropertyName("owner")]
        public ApplicationUser Owner { get; set; } = null!;
        
        [Required]
        [JsonPropertyName("bikeFrameId")]
        public int BikeFrameId { get; set; }
        
        [JsonPropertyName("bikeFrame")]
        public BikeFrame BikeFrame { get; set; } = null!;

        [JsonPropertyName("totalAmount")]
        [NotMapped] // DB에 저장되지 않는 계산 속성
        public decimal TotalAmount => MaintenanceDetails.Sum(d => d.LaborCost + d.PartPrice);

        [Required]
        [JsonPropertyName("paymentMethod")]
        public PaymentMethod PaymentMethod { get; set; }
        
        [JsonPropertyName("notes")]
        public string? Notes { get; set; }
        
        [Required]
        [JsonPropertyName("maintenanceDetails")]
        public virtual ICollection<MaintenanceDetail> MaintenanceDetails { get; set; } = new List<MaintenanceDetail>(); 
    }
}