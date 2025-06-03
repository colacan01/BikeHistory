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

        // ��� ���� ��� ��������
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BikeServiceRecord>>> GetServiceRecords()
        {
            return Ok(await _serviceRecordService.GetAllServiceRecordsAsync());
        }

        // Ư�� ���� ��� ��������
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

        // �����ź� ���� ��� ��������
        [HttpGet("bike/{bikeFrameId}")]
        public async Task<ActionResult<IEnumerable<BikeServiceRecord>>> GetServiceRecordsByBike(int bikeFrameId)
        {
            return Ok(await _serviceRecordService.GetServiceRecordsByBikeAsync(bikeFrameId));
        }

        // ����Һ� ���� ��� ��������
        [HttpGet("shop/{shopId}")]
        public async Task<ActionResult<IEnumerable<BikeServiceRecord>>> GetServiceRecordsByShop(string shopId)
        {
            return Ok(await _serviceRecordService.GetServiceRecordsByShopAsync(shopId));
        }

        // ���� ��� �����ϱ�
        [HttpPost]
        public async Task<ActionResult<BikeServiceRecord>> CreateServiceRecord(BikeServiceRecord serviceRecord)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // ������ �������� ������ �����ϴ��� Ȯ��
            if (!await _serviceRecordService.BikeFrameExistsAsync(serviceRecord.BikeFrameId))
            {
                return BadRequest("������ ������ �������� �������� �ʽ��ϴ�.");
            }

            var createdRecord = await _serviceRecordService.CreateServiceRecordAsync(serviceRecord);
            return CreatedAtAction(nameof(GetServiceRecord), new { id = createdRecord.Id }, createdRecord);
        }

        // ���� ��� ������Ʈ
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateServiceRecord(int id, BikeServiceRecord serviceRecord)
        {
            if (!await _serviceRecordService.BikeFrameExistsAsync(serviceRecord.BikeFrameId))
            {
                return BadRequest("������ ������ �������� �������� �ʽ��ϴ�.");
            }

            if (!await _serviceRecordService.UpdateServiceRecordAsync(id, serviceRecord))
            {
                if (await _serviceRecordService.GetServiceRecordByIdAsync(id) == null)
                {
                    return NotFound();
                }
                return BadRequest("���� ��� ������Ʈ �� ������ �߻��߽��ϴ�.");
            }

            return NoContent();
        }

        // ���� ��� ����
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // ������ �����ڸ� �����ϵ��� ����
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