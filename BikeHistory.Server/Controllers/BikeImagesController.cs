using BikeHistory.Server.Data;
using BikeHistory.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Security.Claims;
using System.Text.Json;

namespace BikeHistory.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BikeImagesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<BikeImagesController> _logger;

        // 허용된 이미지 파일 확장자
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
        
        // 허용된 MIME 타입
        private readonly string[] _allowedMimeTypes = { 
            "image/jpeg", "image/png", "image/gif", "image/bmp", "image/webp" 
        };
        
        // 최대 파일 크기 (10MB)
        private const long _maxFileSize = 10 * 1024 * 1024;
        
        // 사용자당 최대 이미지 수
        private const int _maxImagesPerBike = 20;

        public BikeImagesController(
            ApplicationDbContext context, 
            IWebHostEnvironment environment,
            ILogger<BikeImagesController> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        // GET: api/BikeImages/bike/{bikeFrameId}
        [HttpGet("bike/{bikeFrameId}")]
        public async Task<ActionResult<IEnumerable<BikeImage>>> GetBikeImages(int bikeFrameId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // 사용자가 해당 자전거의 소유자인지 확인 (또는 Admin인지)
            var bikeFrame = await _context.BikeFrames
                .FirstOrDefaultAsync(b => b.Id == bikeFrameId);

            if (bikeFrame == null)
            {
                return NotFound("자전거를 찾을 수 없습니다.");
            }

            if (bikeFrame.CurrentOwnerId != userId && !User.IsInRole("Admin"))
            {
                return Forbid("해당 자전거의 이미지에 접근할 권한이 없습니다.");
            }

            var images = await _context.BikeImages
                .Where(bi => bi.BikeFrameId == bikeFrameId && !bi.IsDeleted)
                .OrderByDescending(bi => bi.IsPrimary)
                .ThenByDescending(bi => bi.UploadedDate)
                .ToListAsync();

            return Ok(images);
        }

        // GET: api/BikeImages/serve/{id} - New endpoint for serving images
        [HttpGet("serve/{id}")]
        [AllowAnonymous] // Allow anonymous access for image display
        public async Task<IActionResult> ServeImage(int id)
        {
            var image = await _context.BikeImages
                .Include(bi => bi.BikeFrame)
                .FirstOrDefaultAsync(bi => bi.Id == id && !bi.IsDeleted);

            if (image == null)
            {
                return NotFound();
            }

            var fullPath = Path.Combine(_environment.ContentRootPath, "wwwroot", image.FilePath);
            if (!System.IO.File.Exists(fullPath))
            {
                _logger.LogWarning("Image file not found: {FilePath}", fullPath);
                return NotFound();
            }

            try
            {
                var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
                
                // Set cache headers for better performance
                Response.Headers.Append("Cache-Control", "public, max-age=31536000"); // 1 year
                Response.Headers.Append("ETag", $"\"{image.Id}-{image.FileSize}\"");
                
                return File(fileBytes, image.ContentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error serving image {ImageId}", id);
                return StatusCode(500, "Error serving image");
            }
        }

        // GET: api/BikeImages/thumbnail/{id} - New endpoint for serving thumbnails
        [HttpGet("thumbnail/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> ServeThumbnail(int id, int width = 300, int height = 300)
        {
            var image = await _context.BikeImages
                .Include(bi => bi.BikeFrame)
                .FirstOrDefaultAsync(bi => bi.Id == id && !bi.IsDeleted);

            if (image == null)
            {
                return NotFound();
            }

            var fullPath = Path.Combine(_environment.ContentRootPath, "wwwroot", image.FilePath);
            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound();
            }

            try
            {
                // Check for cached thumbnail
                var thumbnailDir = Path.Combine(_environment.ContentRootPath, "wwwroot", "uploads", "thumbnails");
                if (!Directory.Exists(thumbnailDir))
                {
                    Directory.CreateDirectory(thumbnailDir);
                }

                var thumbnailPath = Path.Combine(thumbnailDir, $"{id}_{width}x{height}.jpg");
                
                if (!System.IO.File.Exists(thumbnailPath))
                {
                    // Generate thumbnail
                    await GenerateThumbnailAsync(fullPath, thumbnailPath, width, height);
                }

                var thumbnailBytes = await System.IO.File.ReadAllBytesAsync(thumbnailPath);
                
                // Set cache headers
                Response.Headers.Append("Cache-Control", "public, max-age=31536000");
                Response.Headers.Append("ETag", $"\"{image.Id}-thumb-{width}x{height}\"");
                
                return File(thumbnailBytes, "image/jpeg");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating thumbnail for image {ImageId}", id);
                // Fallback to original image
                return await ServeImage(id);
            }
        }

        // POST: api/BikeImages/upload/{bikeFrameId}
        [HttpPost("upload/{bikeFrameId}")]
        public async Task<ActionResult<object>> UploadImages(int bikeFrameId, [FromForm] IFormFileCollection files)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // 자전거 프레임 존재 여부 및 권한 확인
            var bikeFrame = await _context.BikeFrames
                .FirstOrDefaultAsync(b => b.Id == bikeFrameId);

            if (bikeFrame == null)
            {
                return NotFound("자전거를 찾을 수 없습니다.");
            }

            if (bikeFrame.CurrentOwnerId != userId && !User.IsInRole("Admin"))
            {
                return Forbid("해당 자전거에 이미지를 업로드할 권한이 없습니다.");
            }

            // 현재 이미지 수 확인
            var currentImageCount = await _context.BikeImages
                .CountAsync(bi => bi.BikeFrameId == bikeFrameId && !bi.IsDeleted);

            if (currentImageCount + files.Count > _maxImagesPerBike)
            {
                return BadRequest($"자전거당 최대 {_maxImagesPerBike}개의 이미지만 업로드할 수 있습니다.");
            }

            var uploadedImages = new List<BikeImage>();
            var errors = new List<string>();

            // 업로드 디렉토리 생성
            var uploadDir = Path.Combine(_environment.ContentRootPath, "wwwroot", "uploads", "bike-images");
            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
            }

            foreach (var file in files)
            {
                try
                {
                    // 파일 유효성 검사
                    var validation = ValidateFile(file);
                    if (!validation.IsValid)
                    {
                        errors.Add($"{file.FileName}: {validation.ErrorMessage}");
                        continue;
                    }

                    // 고유한 파일명 생성
                    var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    var uniqueFileName = $"{bikeFrameId}_{Guid.NewGuid()}{fileExtension}";
                    var filePath = Path.Combine(uploadDir, uniqueFileName);

                    // 파일 저장
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // 이미지 최적화 (JPG로 변환 및 크기 조정)
                    await OptimizeImageAsync(filePath);

                    // 데이터베이스에 이미지 정보 저장
                    var bikeImage = new BikeImage
                    {
                        BikeFrameId = bikeFrameId,
                        FileName = uniqueFileName,
                        OriginalFileName = file.FileName,
                        FilePath = Path.Combine("uploads", "bike-images", uniqueFileName),
                        ContentType = file.ContentType,
                        FileSize = file.Length,
                        IsPrimary = currentImageCount == 0 && uploadedImages.Count == 0, // 첫 번째 이미지를 기본으로 설정
                        UploadedBy = userId,
                        UploadedDate = DateTime.UtcNow
                    };

                    _context.BikeImages.Add(bikeImage);
                    uploadedImages.Add(bikeImage);

                    _logger.LogInformation("이미지 업로드 성공: {FileName} by {UserId}", file.FileName, userId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "이미지 업로드 실패: {FileName} by {UserId}", file.FileName, userId);
                    errors.Add($"{file.FileName}: 업로드 중 오류가 발생했습니다.");
                }
            }

            if (uploadedImages.Count != 0)
            {
                await _context.SaveChangesAsync();
            }

            var response = new
            {
                uploadedImages = uploadedImages,
                errors = errors,
                totalUploaded = uploadedImages.Count,
                totalErrors = errors.Count
            };

            return Ok(response);
        }

        // PUT: api/BikeImages/{id}/set-primary
        [HttpPut("{id}/set-primary")]
        public async Task<IActionResult> SetPrimaryImage(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var image = await _context.BikeImages
                .Include(bi => bi.BikeFrame)
                .FirstOrDefaultAsync(bi => bi.Id == id && !bi.IsDeleted);

            if (image?.BikeFrame == null)
            {
                return NotFound("이미지를 찾을 수 없습니다.");
            }

            if (image.BikeFrame.CurrentOwnerId != userId && !User.IsInRole("Admin"))
            {
                return Forbid("해당 이미지를 수정할 권한이 없습니다.");
            }

            // 기존 대표 이미지 해제
            var existingPrimary = await _context.BikeImages
                .FirstOrDefaultAsync(bi => bi.BikeFrameId == image.BikeFrameId && bi.IsPrimary && !bi.IsDeleted);

            if (existingPrimary != null)
            {
                existingPrimary.IsPrimary = false;
            }

            // 새 대표 이미지 설정
            image.IsPrimary = true;

            await _context.SaveChangesAsync();

            return Ok(new { message = "대표 이미지가 변경되었습니다." });
        }

        // DELETE: api/BikeImages/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var image = await _context.BikeImages
                .Include(bi => bi.BikeFrame)
                .FirstOrDefaultAsync(bi => bi.Id == id && !bi.IsDeleted);

            if (image?.BikeFrame == null)
            {
                return NotFound("이미지를 찾을 수 없습니다.");
            }

            if (image.BikeFrame.CurrentOwnerId != userId && !User.IsInRole("Admin"))
            {
                return Forbid("해당 이미지를 삭제할 권한이 없습니다.");
            }

            // 소프트 삭제
            image.IsDeleted = true;
            image.DeletedDate = DateTime.UtcNow;

            // 대표 이미지였다면 다른 이미지를 대표로 설정
            if (image.IsPrimary)
            {
                var newPrimary = await _context.BikeImages
                    .Where(bi => bi.BikeFrameId == image.BikeFrameId && !bi.IsDeleted && bi.Id != id)
                    .OrderBy(bi => bi.UploadedDate)
                    .FirstOrDefaultAsync();

                if (newPrimary != null)
                {
                    newPrimary.IsPrimary = true;
                }
            }

            await _context.SaveChangesAsync();

            // 실제 파일 삭제
            try
            {
                var fullPath = Path.Combine(_environment.ContentRootPath, "wwwroot", image.FilePath);
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }

                // 썸네일 파일들도 삭제
                var thumbnailDir = Path.Combine(_environment.ContentRootPath, "wwwroot", "uploads", "thumbnails");
                if (Directory.Exists(thumbnailDir))
                {
                    var thumbnailFiles = Directory.GetFiles(thumbnailDir, $"{id}_*.jpg");
                    foreach (var thumbnailFile in thumbnailFiles)
                    {
                        System.IO.File.Delete(thumbnailFile);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "파일 삭제 실패: {FilePath}", image.FilePath);
            }

            return Ok(new { message = "이미지가 삭제되었습니다." });
        }

        // GET: api/BikeImages/{id}/download
        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadImage(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var image = await _context.BikeImages
                .Include(bi => bi.BikeFrame)
                .FirstOrDefaultAsync(bi => bi.Id == id && !bi.IsDeleted);

            if (image?.BikeFrame == null)
            {
                return NotFound("이미지를 찾을 수 없습니다.");
            }

            if (image.BikeFrame.CurrentOwnerId != userId && !User.IsInRole("Admin"))
            {
                return Forbid("해당 이미지에 접근할 권한이 없습니다.");
            }

            var fullPath = Path.Combine(_environment.ContentRootPath, "wwwroot", image.FilePath);
            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound("이미지 파일을 찾을 수 없습니다.");
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
            return File(fileBytes, image.ContentType, image.OriginalFileName);
        }

        private static (bool IsValid, string ErrorMessage) ValidateFile(IFormFile file)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
            var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/bmp", "image/webp" };
            const long maxFileSize = 10 * 1024 * 1024;

            if (file == null || file.Length == 0)
            {
                return (false, "파일이 선택되지 않았습니다.");
            }

            if (file.Length > maxFileSize)
            {
                return (false, $"파일 크기가 최대 허용 크기({maxFileSize / 1024 / 1024}MB)를 초과합니다.");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                return (false, $"허용되지 않는 파일 형식입니다. 허용된 형식: {string.Join(", ", allowedExtensions)}");
            }

            if (!allowedMimeTypes.Contains(file.ContentType))
            {
                return (false, $"허용되지 않는 파일 형식입니다. MIME 타입: {file.ContentType}");
            }

            return (true, string.Empty);
        }

        private async Task OptimizeImageAsync(string filePath)
        {
            try
            {
                using var originalImage = Image.FromFile(filePath);
                
                // 이미지 크기가 너무 크면 리사이즈 (최대 1920x1920)
                if (originalImage.Width > 1920 || originalImage.Height > 1920)
                {
                    using var resizedImage = ResizeImage(originalImage, 1920, 1920);
                    resizedImage.Save(filePath, ImageFormat.Jpeg);
                }
                else
                {
                    // 크기는 적절하지만 JPG로 변환 (품질 최적화)
                    originalImage.Save(filePath, ImageFormat.Jpeg);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "이미지 최적화 실패: {FilePath}", filePath);
                // 최적화 실패해도 원본 파일은 유지
            }
        }

        private static async Task GenerateThumbnailAsync(string originalPath, string thumbnailPath, int width, int height)
        {
            await Task.Run(() =>
            {
                using var originalImage = Image.FromFile(originalPath);
                using var thumbnail = ResizeImage(originalImage, width, height);
                thumbnail.Save(thumbnailPath, ImageFormat.Jpeg);
            });
        }

        private static Image ResizeImage(Image originalImage, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / originalImage.Width;
            var ratioY = (double)maxHeight / originalImage.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(originalImage.Width * ratio);
            var newHeight = (int)(originalImage.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);
            using (var graphics = Graphics.FromImage(newImage))
            {
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.DrawImage(originalImage, 0, 0, newWidth, newHeight);
            }

            return newImage;
        }
    }
}
