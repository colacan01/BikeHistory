using System.ComponentModel.DataAnnotations;

namespace BikeHistory.Server.Models
{
    public class ImageUploadDto
    {
        public int BikeFrameId { get; set; }
        public IFormFile[] Files { get; set; } = Array.Empty<IFormFile>();
        public string? Description { get; set; }
    }

    public class ImageUploadResult
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public DateTime UploadedDate { get; set; }
        public string? ErrorMessage { get; set; }
        public bool IsSuccess { get; set; }
    }

    public class SetPrimaryImageDto
    {
        [Required]
        public int ImageId { get; set; }
    }
}
