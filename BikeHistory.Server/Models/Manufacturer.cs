using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BikeHistory.Server.Models
{
    public class Manufacturer
    {
        [Key]
        [JsonPropertyName("id")]
        public int Id { get; set; }
        
        [Required]
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("countryOfOrigin")]
        public string? CountryOfOrigin { get; set; }

        [JsonPropertyName("website")]
        public string? Website { get; set; }

        // Navigation property for bike frames
        [JsonPropertyName("bikeFrames")]
        public virtual ICollection<BikeFrame> BikeFrames { get; set; } = new List<BikeFrame>();

        // Navigation property for brands
        [JsonPropertyName("brands")]
        public virtual ICollection<Brand> Brands { get; set; } = new List<Brand>();
    }
}