using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace BikeHistory.Server.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        
        // Navigation property for owned bikes
        public virtual ICollection<BikeFrame> OwnedBikes { get; set; } = new List<BikeFrame>();
        
        // Navigation property for ownership history
        public virtual ICollection<OwnershipRecord> OwnershipHistory { get; set; } = new List<OwnershipRecord>();
    }
}