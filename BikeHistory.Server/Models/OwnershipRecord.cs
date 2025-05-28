using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BikeHistory.Server.Models
{
    public class OwnershipRecord
    {
        [Key]
        [JsonPropertyName("id")]
        public int Id { get; set; }
        
        [Required]
        [JsonPropertyName("bikeFrameId")]
        public int BikeFrameId { get; set; }
        
        [JsonPropertyName("bikeFrame")]
        public BikeFrame BikeFrame { get; set; } = null!;
        
        [Required]
        [JsonPropertyName("previousOwnerId")]
        public string PreviousOwnerId { get; set; } = string.Empty;
        [JsonPropertyName("previousOwnerName")]
        public ApplicationUser PreviousOwner { get; set; } = null!;
        
        [Required]
        [JsonPropertyName("newOwnerId")]
        public string NewOwnerId { get; set; } = string.Empty;
        [JsonPropertyName("newOwnerName")]
        public ApplicationUser NewOwner { get; set; } = null!;
        
        [Required]
        [JsonPropertyName("transferDate")]
        public DateTime TransferDate { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("notes")]
        public string? Notes { get; set; }
    }
}