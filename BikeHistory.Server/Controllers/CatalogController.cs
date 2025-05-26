using BikeHistory.Server.Models;
using BikeHistory.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BikeHistory.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatalogController : ControllerBase
    {
        private readonly CatalogService _catalogService;

        public CatalogController(CatalogService catalogService)
        {
            _catalogService = catalogService;
        }

        #region Manufacturers

        // GET: api/Catalog/manufacturers
        [HttpGet("manufacturers")]
        public async Task<ActionResult<IEnumerable<Manufacturer>>> GetManufacturers()
        {
            var manufacturers = await _catalogService.GetAllManufacturersAsync();
            return Ok(manufacturers);
        }

        // GET: api/Catalog/manufacturers/5
        [HttpGet("manufacturers/{id}")]
        public async Task<ActionResult<Manufacturer>> GetManufacturer(int id)
        {
            var manufacturer = await _catalogService.GetManufacturerByIdAsync(id);

            if (manufacturer == null)
            {
                return NotFound();
            }

            return Ok(manufacturer);
        }

        // POST: api/Catalog/manufacturers
        [HttpPost("manufacturers")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Manufacturer>> CreateManufacturer(Manufacturer manufacturer)
        {
            var createdManufacturer = await _catalogService.CreateManufacturerAsync(manufacturer);
            return CreatedAtAction(nameof(GetManufacturer), new { id = createdManufacturer.Id }, createdManufacturer);
        }

        // PUT: api/Catalog/manufacturers/5
        [HttpPut("manufacturers/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateManufacturer(int id, Manufacturer manufacturer)
        {
            if (id != manufacturer.Id)
            {
                return BadRequest();
            }

            var existingManufacturer = await _catalogService.GetManufacturerByIdAsync(id);
            if (existingManufacturer == null)
            {
                return NotFound();
            }

            await _catalogService.UpdateManufacturerAsync(manufacturer);
            return NoContent();
        }

        // DELETE: api/Catalog/manufacturers/5
        [HttpDelete("manufacturers/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteManufacturer(int id)
        {
            var success = await _catalogService.DeleteManufacturerAsync(id);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        #endregion

        #region Brands

        // GET: api/Catalog/brands
        [HttpGet("brands")]
        public async Task<ActionResult<IEnumerable<Brand>>> GetBrands()
        {
            var brands = await _catalogService.GetAllBrandsAsync();
            return Ok(brands);
        }

        // GET: api/Catalog/manufacturers/5/brands
        [HttpGet("manufacturers/{manufacturerId}/brands")]
        public async Task<ActionResult<IEnumerable<Brand>>> GetBrandsByManufacturer(int manufacturerId)
        {
            var brands = await _catalogService.GetBrandsByManufacturerAsync(manufacturerId);
            return Ok(brands);
        }

        // GET: api/Catalog/brands/5
        [HttpGet("brands/{id}")]
        public async Task<ActionResult<Brand>> GetBrand(int id)
        {
            var brand = await _catalogService.GetBrandByIdAsync(id);

            if (brand == null)
            {
                return NotFound();
            }

            return Ok(brand);
        }

        // POST: api/Catalog/brands
        [HttpPost("brands")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Brand>> CreateBrand(Brand brand)
        {
            var createdBrand = await _catalogService.CreateBrandAsync(brand);
            return CreatedAtAction(nameof(GetBrand), new { id = createdBrand.Id }, createdBrand);
        }

        // PUT: api/Catalog/brands/5
        [HttpPut("brands/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateBrand(int id, Brand brand)
        {
            if (id != brand.Id)
            {
                return BadRequest();
            }

            var existingBrand = await _catalogService.GetBrandByIdAsync(id);
            if (existingBrand == null)
            {
                return NotFound();
            }

            await _catalogService.UpdateBrandAsync(brand);
            return NoContent();
        }

        // DELETE: api/Catalog/brands/5
        [HttpDelete("brands/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBrand(int id)
        {
            var success = await _catalogService.DeleteBrandAsync(id);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        #endregion

        #region BikeTypes

        // GET: api/Catalog/biketypes
        [HttpGet("biketypes")]
        public async Task<ActionResult<IEnumerable<BikeType>>> GetBikeTypes()
        {
            var bikeTypes = await _catalogService.GetAllBikeTypesAsync();
            return Ok(bikeTypes);
        }

        // GET: api/Catalog/biketypes/5
        [HttpGet("biketypes/{id}")]
        public async Task<ActionResult<BikeType>> GetBikeType(int id)
        {
            var bikeType = await _catalogService.GetBikeTypeByIdAsync(id);

            if (bikeType == null)
            {
                return NotFound();
            }

            return Ok(bikeType);
        }

        // POST: api/Catalog/biketypes
        [HttpPost("biketypes")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BikeType>> CreateBikeType(BikeType bikeType)
        {
            var createdBikeType = await _catalogService.CreateBikeTypeAsync(bikeType);
            return CreatedAtAction(nameof(GetBikeType), new { id = createdBikeType.Id }, createdBikeType);
        }

        // PUT: api/Catalog/biketypes/5
        [HttpPut("biketypes/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateBikeType(int id, BikeType bikeType)
        {
            if (id != bikeType.Id)
            {
                return BadRequest();
            }

            var existingBikeType = await _catalogService.GetBikeTypeByIdAsync(id);
            if (existingBikeType == null)
            {
                return NotFound();
            }

            await _catalogService.UpdateBikeTypeAsync(bikeType);
            return NoContent();
        }

        // DELETE: api/Catalog/biketypes/5
        [HttpDelete("biketypes/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBikeType(int id)
        {
            var success = await _catalogService.DeleteBikeTypeAsync(id);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        #endregion
    }
}