using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace BikeHistory.Server.Models
{
    public class ApplicationUser : IdentityUser
    {
        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string? LastName { get; set; }

        // Navigation property for owned bikes
        [JsonPropertyName("ownedBikes")]
        public virtual ICollection<BikeFrame> OwnedBikes { get; set; } = new List<BikeFrame>();

        // Navigation property for ownership history
        [JsonPropertyName("ownershipHistory")]
        public virtual ICollection<OwnershipRecord> OwnershipHistory { get; set; } = new List<OwnershipRecord>();
        
        // Navigation property for user images
        [JsonPropertyName("images")]
        public virtual ICollection<UserImage> Images { get; set; } = new List<UserImage>();
        
        // Computed property for profile image
        [JsonPropertyName("profileImage")]
        public UserImage? ProfileImage => Images?.FirstOrDefault(i => i.IsProfileImage && !i.IsDeleted);
    }
}
