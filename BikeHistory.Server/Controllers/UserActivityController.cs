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
                // IP �ּ� �� ��Ÿ ���� ����
                log.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                log.UserAgent = HttpContext.Request.Headers[HeaderNames.UserAgent]; // ������ �κ�

                await _logger.LogUserActivityAsync(log);
                return Ok();
            }
            catch (Exception ex)
            {
                // �α� �����ص� ����� ���迡 ���� ������ ������ �α׷� ����
                Console.WriteLine($"Error logging user activity: {ex.Message}");
                return StatusCode(500);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            // ������ �ε����� 1���� �����ϴ��� Ȯ��
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            // ���� ����Ͽ� ����¡�� �α� ��������
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
            // ���� �α׸� ���ų� �����ڸ� Ÿ���� �α׸� �� �� �ֵ��� ���� üũ
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            // ����¡�� ����� �α� ��������
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