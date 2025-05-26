using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BikeHistory.Server.Models
{
    public class BikeFrame
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string FrameNumber { get; set; } = string.Empty;
        
        [Required]
        public int ManufacturerId { get; set; }
        public Manufacturer Manufacturer { get; set; } = null!;
        
        [Required]
        public int BrandId { get; set; }
        public Brand Brand { get; set; } = null!;
        
        [Required]
        public int BikeTypeId { get; set; }
        public BikeType BikeType { get; set; } = null!;
        
        public string? Model { get; set; }
        
        public int? ManufactureYear { get; set; }
        
        public string? Color { get; set; }
        
        [Required]
        public string CurrentOwnerId { get; set; } = string.Empty;
        public ApplicationUser CurrentOwner { get; set; } = null!;
        
        public DateTime RegisteredDate { get; set; } = DateTime.UtcNow;
        
        // Navigation property for ownership history
        public virtual ICollection<OwnershipRecord> OwnershipHistory { get; set; } = new List<OwnershipRecord>();
    }
}