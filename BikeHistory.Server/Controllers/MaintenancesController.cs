using BikeHistory.Server.Models;
using BikeHistory.Server.Models.Enums;
using BikeHistory.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BikeHistory.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MaintenancesController : ControllerBase
    {
        private readonly MaintenanceService _maintenanceService;

        public MaintenancesController(MaintenanceService maintenanceService)
        {
            _maintenanceService = maintenanceService;
        }

        // GET: api/Maintenances
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Maintenance>>> GetMaintenances(
            [FromQuery] string? ownerId = null,
            [FromQuery] string? storeId = null,
            [FromQuery] int? bikeFrameId = null)
        {
            // ���� Ȯ��
            bool isAdmin = User.IsInRole("Admin");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            // ���� ����
            try
            {
                // Admin ������� ���
                if (isAdmin)
                {
                    // Ư�� ������ ID�� ���͸�
                    if (bikeFrameId.HasValue)
                    {
                        var bikeMaintenances = await _maintenanceService.GetMaintenancesByBikeFrameIdAsync(bikeFrameId.Value);
                        return Ok(bikeMaintenances);
                    }
                    // Ư�� ������ ID�� ���͸�
                    else if (!string.IsNullOrEmpty(ownerId))
                    {
                        var ownerMaintenances = await _maintenanceService.GetMaintenancesByOwnerIdAsync(ownerId);
                        return Ok(ownerMaintenances);
                    }
                    // Ư�� ���� ID�� ���͸�
                    else if (!string.IsNullOrEmpty(storeId))
                    {
                        var storeMaintenances = await _maintenanceService.GetMaintenancesByStoreIdAsync(storeId);
                        return Ok(storeMaintenances);
                    }
                    // ��� �������� ���
                    else
                    {
                        var allMaintenances = await _maintenanceService.GetAllMaintenancesAsync();
                        return Ok(allMaintenances);
                    }
                }
                // �Ϲ� ������� ��� �ڽ��� �������� ��� �Ǵ� �ڽ��� �������� ó���� ��ϸ� �� �� ����
                else
                {
                    // �������� ��ϵ� ������� ���
                    if (bikeFrameId.HasValue)
                    {
                        // ������ ������ �Ǵ� ���� ���������� Ȯ���ؾ� ��
                        var bikeMaintenances = await _maintenanceService.GetMaintenancesByBikeFrameIdAsync(bikeFrameId.Value);
                        var filteredMaintenances = bikeMaintenances.Where(m => m.OwnerId == userId || m.StoreId == userId).ToList();
                        return Ok(filteredMaintenances);
                    }
                    else
                    {
                        // �ڽ��� �����ſ� ���� �������� ��� �Ǵ� �ڽ��� �������� ó���� ���
                        var userMaintenances = await _maintenanceService.GetMaintenancesByOwnerIdAsync(userId);
                        var storeMaintenances = await _maintenanceService.GetMaintenancesByStoreIdAsync(userId);
                        
                        // �� ��� ��ġ��
                        var combinedMaintenances = userMaintenances.Union(storeMaintenances).ToList();
                        return Ok(combinedMaintenances);
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET: api/Maintenances/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Maintenance>> GetMaintenance(Guid id)
        {
            var maintenance = await _maintenanceService.GetMaintenanceByIdAsync(id);

            if (maintenance == null)
            {
                return NotFound();
            }

            // ���� Ȯ��: ������, ������, �Ǵ� ���� �����ڸ� �� �� ����
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (maintenance.OwnerId != userId && 
                maintenance.StoreId != userId && 
                !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return maintenance;
        }

        // POST: api/Maintenances
        [HttpPost]
        public async Task<ActionResult<Maintenance>> CreateMaintenance(MaintenanceCreateDto createDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            try
            {
                var maintenance = new Maintenance
                {
                    Id = Guid.NewGuid(),
                    MaintenanceDate = createDto.MaintenanceDate,
                    MaintenanceType = createDto.MaintenanceType,
                    StoreId = createDto.StoreId,
                    OwnerId = createDto.OwnerId,
                    BikeFrameId = createDto.BikeFrameId,
                    PaymentMethod = createDto.PaymentMethod,
                    Notes = createDto.Notes
                };

                // �� �׸� ����
                var details = createDto.Details.Select(d => new MaintenanceDetail
                {
                    LaborCost = d.LaborCost,
                    PartPrice = d.PartPrice,
                    PartName = d.PartName,
                    PartSpecification = d.PartSpecification
                }).ToList();

                var createdMaintenance = await _maintenanceService.CreateMaintenanceAsync(maintenance, details);
                return CreatedAtAction(nameof(GetMaintenance), new { id = createdMaintenance.Id }, createdMaintenance);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/Maintenances/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMaintenance(Guid id, MaintenanceUpdateDto updateDto)
        {
            if (id != updateDto.Id)
            {
                return BadRequest("ID�� ��ġ���� �ʽ��ϴ�");
            }

            var maintenance = await _maintenanceService.GetMaintenanceByIdAsync(id);
            if (maintenance == null)
            {
                return NotFound();
            }

            // ���� Ȯ��: ������ �Ǵ� ���� �����ڸ� ���� ����
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (maintenance.StoreId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            try
            {
                // ������ �Ӽ� ������Ʈ
                maintenance.MaintenanceDate = updateDto.MaintenanceDate;
                maintenance.MaintenanceType = updateDto.MaintenanceType;
                maintenance.PaymentMethod = updateDto.PaymentMethod;
                maintenance.Notes = updateDto.Notes;

                // �� �׸� ����
                var details = updateDto.Details.Select(d => new MaintenanceDetail
                {
                    LaborCost = d.LaborCost,
                    PartPrice = d.PartPrice,
                    PartName = d.PartName,
                    PartSpecification = d.PartSpecification
                }).ToList();

                await _maintenanceService.UpdateMaintenanceAsync(maintenance, details);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // DELETE: api/Maintenances/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMaintenance(Guid id)
        {
            var maintenance = await _maintenanceService.GetMaintenanceByIdAsync(id);
            if (maintenance == null)
            {
                return NotFound();
            }

            // ���� Ȯ��: ������ �Ǵ� ���� �����ڸ� ���� ����
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (maintenance.StoreId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            try
            {
                await _maintenanceService.DeleteMaintenanceAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
        
        // GET: api/Maintenances/bike/5
        [HttpGet("bike/{bikeFrameId}")]
        public async Task<ActionResult<IEnumerable<Maintenance>>> GetMaintenancesByBikeId(int bikeFrameId)
        {
            // ���� Ȯ�� �ʿ� (�� �κ��� BikeFramesController�� ������ ����)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            try
            {
                var maintenances = await _maintenanceService.GetMaintenancesByBikeFrameIdAsync(bikeFrameId);
                
                // �����ڰ� �ƴ� ��� �ڽŰ� ���õ� ���� ��ϸ� �� �� ����
                if (!User.IsInRole("Admin"))
                {
                    maintenances = maintenances.Where(m => 
                        m.OwnerId == userId || m.StoreId == userId).ToList();
                }
                
                return Ok(maintenances);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }

    // DTO Ŭ������
    public class MaintenanceCreateDto
    {
        [Required]
        public DateTime MaintenanceDate { get; set; } = DateTime.UtcNow;
        
        [Required]
        public MaintenanceType MaintenanceType { get; set; }
        
        [Required]
        public string StoreId { get; set; } = string.Empty;
        
        [Required]
        public string OwnerId { get; set; } = string.Empty;
        
        [Required]
        public int BikeFrameId { get; set; }
        
        [Required]
        public PaymentMethod PaymentMethod { get; set; }
        
        public string? Notes { get; set; }
        
        [Required]
        public List<MaintenanceDetailDto> Details { get; set; } = new List<MaintenanceDetailDto>();
    }

    public class MaintenanceUpdateDto
    {
        [Required]
        public Guid Id { get; set; }
        
        [Required]
        public DateTime MaintenanceDate { get; set; }
        
        [Required]
        public MaintenanceType MaintenanceType { get; set; }
        
        [Required]
        public PaymentMethod PaymentMethod { get; set; }
        
        public string? Notes { get; set; }
        
        [Required]
        public List<MaintenanceDetailDto> Details { get; set; } = new List<MaintenanceDetailDto>();
    }

    public class MaintenanceDetailDto
    {
        [Required]
        public decimal LaborCost { get; set; }
        
        [Required]
        public decimal PartPrice { get; set; }
        
        public string? PartName { get; set; }
        
        public string? PartSpecification { get; set; }
    }
}