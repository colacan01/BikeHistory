using BikeHistory.Server.Models;
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
    public class BikeFramesController : ControllerBase
    {
        private readonly BikeService _bikeService;

        public BikeFramesController(BikeService bikeService)
        {
            _bikeService = bikeService;
        }

        // GET: api/BikeFrames
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BikeFrame>>> GetBikeFrames()
        {
            // Check if user is admin, if not, only return their bikes
            if (User.IsInRole("Admin"))
            {
                var allBikes = await _bikeService.GetAllBikeFramesAsync();
                return Ok(allBikes);
            }
            else
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId != null)
                {
                    var userBikes = await _bikeService.GetBikeFramesByOwnerIdAsync(userId);
                    return Ok(userBikes);
                }
                return Unauthorized();
            }
        }

        // GET: api/BikeFrames/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BikeFrame>> GetBikeFrame(int id)
        {
            var bikeFrame = await _bikeService.GetBikeFrameByIdAsync(id);

            if (bikeFrame == null)
            {
                return NotFound();
            }

            // Check if user is authorized to view this bike
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (bikeFrame.CurrentOwnerId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return Ok(bikeFrame);
        }

        // GET: api/BikeFrames/framenumber/ABC123
        [HttpGet("framenumber/{frameNumber}")]
        public async Task<ActionResult<BikeFrame>> GetBikeFrameByFrameNumber(string frameNumber)
        {
            var bikeFrame = await _bikeService.GetBikeFrameByFrameNumberAsync(frameNumber);

            if (bikeFrame == null)
            {
                return NotFound();
            }

            return Ok(bikeFrame);
        }

        // POST: api/BikeFrames
        [HttpPost]
        public async Task<ActionResult<BikeFrame>> RegisterBikeFrame(BikeFrameRegisterDto registerDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var bikeFrame = new BikeFrame
            {
                FrameNumber = registerDto.FrameNumber,
                ManufacturerId = registerDto.ManufacturerId,
                BrandId = registerDto.BrandId,
                BikeTypeId = registerDto.BikeTypeId,
                Model = registerDto.Model,
                ManufactureYear = registerDto.ManufactureYear,
                Color = registerDto.Color,
                CurrentOwnerId = userId,
                RegisteredDate = DateTime.UtcNow
            };

            try
            {
                var registeredBike = await _bikeService.RegisterBikeFrameAsync(bikeFrame);
                return CreatedAtAction(nameof(GetBikeFrame), new { id = registeredBike.Id }, registeredBike);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/BikeFrames/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBikeFrame(int id, BikeFrameUpdateDto updateDto)
        {
            var bikeFrame = await _bikeService.GetBikeFrameByIdAsync(id);

            if (bikeFrame == null)
            {
                return NotFound();
            }

            // Check if user is authorized to update this bike
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (bikeFrame.CurrentOwnerId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            // Update properties
            bikeFrame.ManufacturerId = updateDto.ManufacturerId;
            bikeFrame.BrandId = updateDto.BrandId;
            bikeFrame.BikeTypeId = updateDto.BikeTypeId;
            bikeFrame.Model = updateDto.Model;
            bikeFrame.ManufactureYear = updateDto.ManufactureYear;
            bikeFrame.Color = updateDto.Color;

            try
            {
                await _bikeService.UpdateBikeFrameAsync(bikeFrame);
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Failed to update bike frame" });
            }
        }

        // POST: api/BikeFrames/5/transfer
        [HttpPost("{id}/transfer")]
        public async Task<IActionResult> TransferOwnership(int id, OwnershipTransferDto transferDto)
        {
            var bikeFrame = await _bikeService.GetBikeFrameByIdAsync(id);

            if (bikeFrame == null)
            {
                return NotFound();
            }

            // Check if user is authorized to transfer this bike
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (bikeFrame.CurrentOwnerId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            try
            {
                var ownershipRecord = await _bikeService.TransferOwnershipAsync(id, transferDto.NewOwnerId, transferDto.Notes);
                return Ok(ownershipRecord);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/BikeFrames/5/history
        [HttpGet("{id}/history")]
        public async Task<ActionResult<IEnumerable<OwnershipRecord>>> GetOwnershipHistory(int id)
        {
            var bikeFrame = await _bikeService.GetBikeFrameByIdAsync(id);

            if (bikeFrame == null)
            {
                return NotFound();
            }

            // Check if user is authorized to view this bike's history
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (bikeFrame.CurrentOwnerId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var history = await _bikeService.GetOwnershipHistoryAsync(id);
            return Ok(history);
        }
    }

    public class BikeFrameRegisterDto
    {
        [Required]
        public string FrameNumber { get; set; } = string.Empty;

        [Required]
        public int ManufacturerId { get; set; }

        [Required]
        public int BrandId { get; set; }

        [Required]
        public int BikeTypeId { get; set; }

        public string? Model { get; set; }

        public int? ManufactureYear { get; set; }

        public string? Color { get; set; }
    }

    public class BikeFrameUpdateDto
    {
        [Required]
        public int ManufacturerId { get; set; }

        [Required]
        public int BrandId { get; set; }

        [Required]
        public int BikeTypeId { get; set; }

        public string? Model { get; set; }

        public int? ManufactureYear { get; set; }

        public string? Color { get; set; }
    }

    public class OwnershipTransferDto
    {
        [Required]
        public string NewOwnerId { get; set; } = string.Empty;

        public string? Notes { get; set; }
    }
}