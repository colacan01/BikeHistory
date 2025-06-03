using BikeHistory.Server.Models;
using BikeHistory.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BikeHistory.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Store")]
    public class BikeServiceController : ControllerBase
    {
        private readonly IBikeServiceRecordService _serviceRecordService;
        private readonly ILogger<BikeServiceController> _logger;

        public BikeServiceController(IBikeServiceRecordService serviceRecordService, ILogger<BikeServiceController> logger)
        {
            _serviceRecordService = serviceRecordService;
            _logger = logger;
        }

        // 모든 정비 기록 가져오기
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BikeServiceRecord>>> GetServiceRecords()
        {
            return Ok(await _serviceRecordService.GetAllServiceRecordsAsync());
        }

        // 특정 정비 기록 가져오기
        [HttpGet("{id}")]
        public async Task<ActionResult<BikeServiceRecord>> GetServiceRecord(int id)
        {
            var serviceRecord = await _serviceRecordService.GetServiceRecordByIdAsync(id);

            if (serviceRecord == null)
            {
                return NotFound();
            }

            return serviceRecord;
        }

        // 자전거별 정비 기록 가져오기
        [HttpGet("bike/{bikeFrameId}")]
        public async Task<ActionResult<IEnumerable<BikeServiceRecord>>> GetServiceRecordsByBike(int bikeFrameId)
        {
            return Ok(await _serviceRecordService.GetServiceRecordsByBikeAsync(bikeFrameId));
        }

        // 정비소별 정비 기록 가져오기
        [HttpGet("shop/{shopId}")]
        public async Task<ActionResult<IEnumerable<BikeServiceRecord>>> GetServiceRecordsByShop(string shopId)
        {
            return Ok(await _serviceRecordService.GetServiceRecordsByShopAsync(shopId));
        }

        // 정비 기록 생성하기
        [HttpPost]
        public async Task<ActionResult<BikeServiceRecord>> CreateServiceRecord(BikeServiceRecord serviceRecord)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 자전거 프레임이 실제로 존재하는지 확인
            if (!await _serviceRecordService.BikeFrameExistsAsync(serviceRecord.BikeFrameId))
            {
                return BadRequest("지정한 자전거 프레임이 존재하지 않습니다.");
            }

            var createdRecord = await _serviceRecordService.CreateServiceRecordAsync(serviceRecord);
            return CreatedAtAction(nameof(GetServiceRecord), new { id = createdRecord.Id }, createdRecord);
        }

        // 정비 기록 업데이트
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateServiceRecord(int id, BikeServiceRecord serviceRecord)
        {
            if (!await _serviceRecordService.BikeFrameExistsAsync(serviceRecord.BikeFrameId))
            {
                return BadRequest("지정한 자전거 프레임이 존재하지 않습니다.");
            }

            if (!await _serviceRecordService.UpdateServiceRecordAsync(id, serviceRecord))
            {
                if (await _serviceRecordService.GetServiceRecordByIdAsync(id) == null)
                {
                    return NotFound();
                }
                return BadRequest("정비 기록 업데이트 중 오류가 발생했습니다.");
            }

            return NoContent();
        }

        // 정비 기록 삭제
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // 삭제는 관리자만 가능하도록 제한
        public async Task<IActionResult> DeleteServiceRecord(int id)
        {
            if (!await _serviceRecordService.DeleteServiceRecordAsync(id))
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}