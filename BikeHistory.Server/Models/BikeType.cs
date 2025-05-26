using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BikeHistory.Server.Models
{
    public class BikeType
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        // Navigation property for bike frames
        public virtual ICollection<BikeFrame> BikeFrames { get; set; } = new List<BikeFrame>();
    }
}