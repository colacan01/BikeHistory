using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BikeHistory.Server.Models
{
    public class UserImage
    {
        [Key]
        [JsonPropertyName("id")]
        public int Id { get; set; }
        
        [Required]
        [JsonPropertyName("userId")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("user")]
        public ApplicationUser User { get; set; } = null!;
        
        [Required]
        [JsonPropertyName("fileName")]
        public string FileName { get; set; } = string.Empty;
        
        [Required]
        [JsonPropertyName("originalFileName")]
        public string OriginalFileName { get; set; } = string.Empty;
        
        [Required]
        [JsonPropertyName("filePath")]
        public string FilePath { get; set; } = string.Empty;
        
        [JsonPropertyName("contentType")]
        public string ContentType { get; set; } = string.Empty;
        
        [JsonPropertyName("fileSize")]
        public long FileSize { get; set; }
        
        [JsonPropertyName("isProfileImage")]
        public bool IsProfileImage { get; set; } = false;
        
        [JsonPropertyName("uploadedDate")]
        public DateTime UploadedDate { get; set; } = DateTime.UtcNow;
        
        [JsonPropertyName("description")]
        public string? Description { get; set; }
        
        [JsonPropertyName("isDeleted")]
        public bool IsDeleted { get; set; } = false;
        
        [JsonPropertyName("deletedDate")]
        public DateTime? DeletedDate { get; set; }
    }
}
