using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BikeHistory.Server.Models
{
    public class Brand
    {
        [Key]
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        [Required]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("manufacturerId")]
        public int? ManufacturerId { get; set; }

        [JsonPropertyName("manufacturer")]
        public Manufacturer? Manufacturer { get; set; }

        [JsonPropertyName("bikeFrames")]
        // Navigation property for bike frames
        public virtual ICollection<BikeFrame> BikeFrames { get; set; } = new List<BikeFrame>();
    }
}