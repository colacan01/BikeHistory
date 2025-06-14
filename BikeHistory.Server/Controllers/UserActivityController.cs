using BikeHistory.Server.Models;
using BikeHistory.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.Security.Claims;

namespace BikeHistory.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserActivityController : ControllerBase
    {
        private readonly IUserActivityLogger _logger;

        public UserActivityController(IUserActivityLogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        public async Task<IActionResult> LogActivity(UserActivityLog log)
        {
            try
            {
                // IP 주소 및 기타 정보 설정
                log.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                log.UserAgent = HttpContext.Request.Headers[HeaderNames.UserAgent]; // 수정된 부분

                await _logger.LogUserActivityAsync(log);
                return Ok();
            }
            catch (Exception ex)
            {
                // 로깅 실패해도 사용자 경험에 영향 없도록 에러만 로그로 남김
                Console.WriteLine($"Error logging user activity: {ex.Message}");
                return StatusCode(500);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            // 페이지 인덱스가 1부터 시작하는지 확인
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            // 서비스 사용하여 페이징된 로그 가져오기
            var result = await _logger.GetAllActivityLogsPagedAsync(page, pageSize, startDate, endDate);

            var response = new
            {
                data = result.Logs,
                totalItems = result.TotalItems,
                totalPages = result.TotalPages,
                currentPage = page,
                pageSize = pageSize,
                hasNext = page < result.TotalPages,
                hasPrevious = page > 1
            };

            return Ok(response);
        }

        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetUserLogs(string userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            // 본인 로그만 보거나 관리자만 타인의 로그를 볼 수 있도록 권한 체크
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            // 페이징된 사용자 로그 가져오기
            var result = await _logger.GetUserActivityLogsPagedAsync(userId, page, pageSize, startDate, endDate);

            var response = new
            {
                data = result.Logs,
                totalItems = result.TotalItems,
                totalPages = result.TotalPages,
                currentPage = page,
                pageSize = pageSize,
                hasNext = page < result.TotalPages,
                hasPrevious = page > 1
            };

            return Ok(response);
        }
    }
}