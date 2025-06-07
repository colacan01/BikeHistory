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
            // 권한 확인
            bool isAdmin = User.IsInRole("Admin");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            // 쿼리 수행
            try
            {
                // Admin 사용자인 경우
                if (isAdmin)
                {
                    // 특정 자전거 ID로 필터링
                    if (bikeFrameId.HasValue)
                    {
                        var bikeMaintenances = await _maintenanceService.GetMaintenancesByBikeFrameIdAsync(bikeFrameId.Value);
                        return Ok(bikeMaintenances);
                    }
                    // 특정 소유자 ID로 필터링
                    else if (!string.IsNullOrEmpty(ownerId))
                    {
                        var ownerMaintenances = await _maintenanceService.GetMaintenancesByOwnerIdAsync(ownerId);
                        return Ok(ownerMaintenances);
                    }
                    // 특정 상점 ID로 필터링
                    else if (!string.IsNullOrEmpty(storeId))
                    {
                        var storeMaintenances = await _maintenanceService.GetMaintenancesByStoreIdAsync(storeId);
                        return Ok(storeMaintenances);
                    }
                    // 모든 유지보수 기록
                    else
                    {
                        var allMaintenances = await _maintenanceService.GetAllMaintenancesAsync();
                        return Ok(allMaintenances);
                    }
                }
                // 일반 사용자인 경우 자신의 유지보수 기록 또는 자신의 상점에서 처리한 기록만 볼 수 있음
                else
                {
                    // 상점으로 등록된 사용자인 경우
                    if (bikeFrameId.HasValue)
                    {
                        // 자전거 소유자 또는 상점 소유자인지 확인해야 함
                        var bikeMaintenances = await _maintenanceService.GetMaintenancesByBikeFrameIdAsync(bikeFrameId.Value);
                        var filteredMaintenances = bikeMaintenances.Where(m => m.OwnerId == userId || m.StoreId == userId).ToList();
                        return Ok(filteredMaintenances);
                    }
                    else
                    {
                        // 자신의 자전거에 대한 유지보수 기록 또는 자신의 상점에서 처리한 기록
                        var userMaintenances = await _maintenanceService.GetMaintenancesByOwnerIdAsync(userId);
                        var storeMaintenances = await _maintenanceService.GetMaintenancesByStoreIdAsync(userId);
                        
                        // 두 목록 합치기
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

            // 권한 확인: 관리자, 소유자, 또는 상점 소유자만 볼 수 있음
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

                // 상세 항목 생성
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
                return BadRequest("ID가 일치하지 않습니다");
            }

            var maintenance = await _maintenanceService.GetMaintenanceByIdAsync(id);
            if (maintenance == null)
            {
                return NotFound();
            }

            // 권한 확인: 관리자 또는 상점 소유자만 수정 가능
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (maintenance.StoreId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            try
            {
                // 수정할 속성 업데이트
                maintenance.MaintenanceDate = updateDto.MaintenanceDate;
                maintenance.MaintenanceType = updateDto.MaintenanceType;
                maintenance.PaymentMethod = updateDto.PaymentMethod;
                maintenance.Notes = updateDto.Notes;

                // 상세 항목 생성
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

            // 권한 확인: 관리자 또는 상점 소유자만 삭제 가능
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
            // 권한 확인 필요 (이 부분은 BikeFramesController의 로직을 참조)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            try
            {
                var maintenances = await _maintenanceService.GetMaintenancesByBikeFrameIdAsync(bikeFrameId);
                
                // 관리자가 아닌 경우 자신과 관련된 정비 기록만 볼 수 있음
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

    // DTO 클래스들
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