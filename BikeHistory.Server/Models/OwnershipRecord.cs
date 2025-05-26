using System;
using System.ComponentModel.DataAnnotations;

namespace BikeHistory.Server.Models
{
    public class OwnershipRecord
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int BikeFrameId { get; set; }
        public BikeFrame BikeFrame { get; set; } = null!;
        
        [Required]
        public string PreviousOwnerId { get; set; } = string.Empty;
        public ApplicationUser PreviousOwner { get; set; } = null!;
        
        [Required]
        public string NewOwnerId { get; set; } = string.Empty;
        public ApplicationUser NewOwner { get; set; } = null!;
        
        [Required]
        public DateTime TransferDate { get; set; } = DateTime.UtcNow;
        
        public string? Notes { get; set; }
    }
}