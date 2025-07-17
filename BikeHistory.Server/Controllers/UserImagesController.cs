using BikeHistory.Server.Data;
using BikeHistory.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace BikeHistory.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserImagesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<UserImagesController> _logger;

        // 허용된 이미지 파일 확장자
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
        
        // 허용된 MIME 타입
        private readonly string[] _allowedMimeTypes = { 
            "image/jpeg", "image/png", "image/gif", "image/bmp", "image/webp" 
        };
        
        // 최대 파일 크기 (5MB)
        private const long _maxFileSize = 5 * 1024 * 1024;
        
        // 사용자당 최대 이미지 수
        private const int _maxImagesPerUser = 10;

        public UserImagesController(
            ApplicationDbContext context, 
            IWebHostEnvironment environment,
            ILogger<UserImagesController> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        // GET: api/UserImages
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Models.UserImage>>> GetUserImages()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var images = await _context.UserImages
                .Where(ui => ui.UserId == userId && !ui.IsDeleted)
                .OrderByDescending(ui => ui.IsProfileImage)
                .ThenByDescending(ui => ui.UploadedDate)
                .ToListAsync();

            return Ok(images);
        }

        // GET: api/UserImages/serve/{id}
        [HttpGet("serve/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> ServeImage(int id)
        {
            var image = await _context.UserImages
                .FirstOrDefaultAsync(ui => ui.Id == id && !ui.IsDeleted);

            if (image == null)
            {
                return NotFound();
            }

            var fullPath = Path.Combine(_environment.ContentRootPath, "wwwroot", image.FilePath);
            if (!System.IO.File.Exists(fullPath))
            {
                _logger.LogWarning("User image file not found: {FilePath}", fullPath);
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
                _logger.LogError(ex, "Error serving user image {ImageId}", id);
                return StatusCode(500, "Error serving image");
            }
        }

        // GET: api/UserImages/thumbnail/{id}
        [HttpGet("thumbnail/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> ServeThumbnail(int id, int width = 150, int height = 150)
        {
            var image = await _context.UserImages
                .FirstOrDefaultAsync(ui => ui.Id == id && !ui.IsDeleted);

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
                var thumbnailDir = Path.Combine(_environment.ContentRootPath, "wwwroot", "uploads", "user-thumbnails");
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
                _logger.LogError(ex, "Error generating thumbnail for user image {ImageId}", id);
                // Fallback to original image
                return await ServeImage(id);
            }
        }

        // POST: api/UserImages/upload
        [HttpPost("upload")]
        public async Task<ActionResult<object>> UploadImages([FromForm] IFormFileCollection files)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // 현재 이미지 수 확인
            var currentImageCount = await _context.UserImages
                .CountAsync(ui => ui.UserId == userId && !ui.IsDeleted);

            if (currentImageCount + files.Count > _maxImagesPerUser)
            {
                return BadRequest($"사용자당 최대 {_maxImagesPerUser}개의 이미지만 업로드할 수 있습니다.");
            }

            var uploadedImages = new List<Models.UserImage>();
            var errors = new List<string>();

            // 업로드 디렉토리 생성
            var uploadDir = Path.Combine(_environment.ContentRootPath, "wwwroot", "uploads", "user-images");
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
                    var uniqueFileName = $"{userId}_{Guid.NewGuid()}{fileExtension}";
                    var filePath = Path.Combine(uploadDir, uniqueFileName);

                    // 파일 저장
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // 이미지 최적화 (JPG로 변환 및 크기 조정)
                    await OptimizeImageAsync(filePath);

                    // 데이터베이스에 이미지 정보 저장
                    var userImage = new Models.UserImage
                    {
                        UserId = userId,
                        FileName = uniqueFileName,
                        OriginalFileName = file.FileName,
                        FilePath = Path.Combine("uploads", "user-images", uniqueFileName),
                        ContentType = file.ContentType,
                        FileSize = file.Length,
                        IsProfileImage = currentImageCount == 0 && uploadedImages.Count == 0, // 첫 번째 이미지를 기본 프로필로 설정
                        UploadedDate = DateTime.UtcNow
                    };

                    _context.UserImages.Add(userImage);
                    uploadedImages.Add(userImage);

                    _logger.LogInformation("사용자 이미지 업로드 성공: {FileName} by {UserId}", file.FileName, userId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "사용자 이미지 업로드 실패: {FileName} by {UserId}", file.FileName, userId);
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

        // PUT: api/UserImages/{id}/set-profile
        [HttpPut("{id}/set-profile")]
        public async Task<IActionResult> SetProfileImage(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var image = await _context.UserImages
                .FirstOrDefaultAsync(ui => ui.Id == id && ui.UserId == userId && !ui.IsDeleted);

            if (image == null)
            {
                return NotFound("이미지를 찾을 수 없습니다.");
            }

            // 기존 프로필 이미지 해제
            var existingProfile = await _context.UserImages
                .FirstOrDefaultAsync(ui => ui.UserId == userId && ui.IsProfileImage && !ui.IsDeleted);

            if (existingProfile != null)
            {
                existingProfile.IsProfileImage = false;
            }

            // 새 프로필 이미지 설정
            image.IsProfileImage = true;

            await _context.SaveChangesAsync();

            return Ok(new { message = "프로필 이미지가 변경되었습니다." });
        }

        // DELETE: api/UserImages/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var image = await _context.UserImages
                .FirstOrDefaultAsync(ui => ui.Id == id && ui.UserId == userId && !ui.IsDeleted);

            if (image == null)
            {
                return NotFound("이미지를 찾을 수 없습니다.");
            }

            // 소프트 삭제
            image.IsDeleted = true;
            image.DeletedDate = DateTime.UtcNow;

            // 프로필 이미지였다면 다른 이미지를 프로필로 설정
            if (image.IsProfileImage)
            {
                var newProfile = await _context.UserImages
                    .Where(ui => ui.UserId == userId && !ui.IsDeleted && ui.Id != id)
                    .OrderBy(ui => ui.UploadedDate)
                    .FirstOrDefaultAsync();

                if (newProfile != null)
                {
                    newProfile.IsProfileImage = true;
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
                var thumbnailDir = Path.Combine(_environment.ContentRootPath, "wwwroot", "uploads", "user-thumbnails");
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

        private static (bool IsValid, string ErrorMessage) ValidateFile(IFormFile file)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
            var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/bmp", "image/webp" };
            const long maxFileSize = 5 * 1024 * 1024; // 5MB

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
                using var image = await Image.LoadAsync(filePath);
                
                // 이미지 크기가 너무 크면 리사이즈 (최대 800x800)
                if (image.Width > 800 || image.Height > 800)
                {
                    var size = ResizeKeepAspectRatio(image.Width, image.Height, 800, 800);
                    image.Mutate(x => x.Resize(size.Width, size.Height));
                }

                // JPG 형식으로 저장 (고품질)
                var encoder = new JpegEncoder
                {
                    Quality = 85
                };
                
                await image.SaveAsync(filePath, encoder);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "이미지 최적화 실패: {FilePath}", filePath);
                // 최적화 실패해도 원본 파일은 유지
            }
        }

        private static async Task GenerateThumbnailAsync(string originalPath, string thumbnailPath, int width, int height)
        {
            try
            {
                using var image = await Image.LoadAsync(originalPath);
                var size = ResizeKeepAspectRatio(image.Width, image.Height, width, height);
                
                image.Mutate(x => x.Resize(size.Width, size.Height));
                
                var encoder = new JpegEncoder
                {
                    Quality = 80
                };
                
                await image.SaveAsync(thumbnailPath, encoder);
            }
            catch (Exception ex)
            {
                // 썸네일 생성 실패 시 로그만 기록하고 예외는 다시 던져서 상위에서 처리
                throw new InvalidOperationException($"썸네일 생성 실패: {originalPath}", ex);
            }
        }

        private static (int Width, int Height) ResizeKeepAspectRatio(int originalWidth, int originalHeight, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / originalWidth;
            var ratioY = (double)maxHeight / originalHeight;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(originalWidth * ratio);
            var newHeight = (int)(originalHeight * ratio);

            return (newWidth, newHeight);
        }
    }
}
