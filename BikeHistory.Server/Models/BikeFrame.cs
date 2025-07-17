using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;

namespace BikeHistory.Server.Models
{
    public class BikeFrame
    {
        [Key]
        [JsonPropertyName("id")]
        public int Id { get; set; }
        
        [Required]
        [JsonPropertyName("frameNumber")]
        public string FrameNumber { get; set; } = string.Empty;
        
        [Required]
        [JsonPropertyName("manufacturerId")]
        public int ManufacturerId { get; set; }

        [JsonPropertyName("manufacturer")]
        public Manufacturer Manufacturer { get; set; } = null!;
        
        [Required]
        [JsonPropertyName("brandId")]
        public int BrandId { get; set; }

        [JsonPropertyName("brand")]
        public Brand Brand { get; set; } = null!;
        
        [Required]
        [JsonPropertyName("bikeTypeId")]
        public int BikeTypeId { get; set; }

        [JsonPropertyName("bikeType")]
        public BikeType BikeType { get; set; } = null!;

        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("manufactureYear")]
        public int? ManufactureYear { get; set; }

        [JsonPropertyName("color")]
        public string? Color { get; set; }
        
        [Required]
        [JsonPropertyName("currentOwnerId")]
        public string CurrentOwnerId { get; set; } = string.Empty;
        
        [JsonPropertyName("currentOwner")]
        public ApplicationUser CurrentOwner { get; set; } = null!;

        [JsonPropertyName("registeredDate")]
        public DateTime RegisteredDate { get; set; } = DateTime.UtcNow;

        // Navigation property for ownership history
        [JsonPropertyName("ownershipHistory")]
        public virtual ICollection<OwnershipRecord> OwnershipHistory { get; set; } = new List<OwnershipRecord>();
        
        // Navigation property for bike images
        [JsonPropertyName("images")]
        public virtual ICollection<BikeImage> Images { get; set; } = new List<BikeImage>();
        
        // Computed property for primary image
        [JsonPropertyName("primaryImage")]
        public BikeImage? PrimaryImage => Images?.FirstOrDefault(i => i.IsPrimary && !i.IsDeleted);
    }
}
