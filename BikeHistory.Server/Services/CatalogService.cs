using BikeHistory.Server.Data;
using BikeHistory.Server.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BikeHistory.Server.Services
{
    public class CatalogService
    {
        private readonly ApplicationDbContext _context;

        public CatalogService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Manufacturer methods
        public async Task<List<Manufacturer>> GetAllManufacturersAsync()
        {
            return await _context.Manufacturers.ToListAsync();
        }

        public async Task<Manufacturer?> GetManufacturerByIdAsync(int id)
        {
            return await _context.Manufacturers.FindAsync(id);
        }

        public async Task<Manufacturer> CreateManufacturerAsync(Manufacturer manufacturer)
        {
            _context.Manufacturers.Add(manufacturer);
            await _context.SaveChangesAsync();
            return manufacturer;
        }

        public async Task<Manufacturer> UpdateManufacturerAsync(Manufacturer manufacturer)
        {

            // 엔티티가 이미 컨텍스트에서 추적 중인지 확인
            var local = _context.Set<Manufacturer>()
                              .Local
                              .FirstOrDefault(m => m.Id == manufacturer.Id);

            // 기존 추적 중인 엔티티가 있으면 분리
            if (local != null)
            {
                _context.Entry(local).State = EntityState.Detached;
            }

            _context.Entry(manufacturer).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return manufacturer;
        }

        public async Task<bool> DeleteManufacturerAsync(int id)
        {
            var manufacturer = await _context.Manufacturers.FindAsync(id);
            if (manufacturer == null)
                return false;

            _context.Manufacturers.Remove(manufacturer);
            await _context.SaveChangesAsync();
            return true;
        }

        // Brand methods
        public async Task<List<Brand>> GetAllBrandsAsync()
        {
            return await _context.Brands
                .Include(b => b.Manufacturer)
                .ToListAsync();
        }

        public async Task<List<Brand>> GetBrandsByManufacturerAsync(int manufacturerId)
        {
            return await _context.Brands
                .Where(b => b.ManufacturerId == manufacturerId)
                .ToListAsync();
        }

        public async Task<Brand?> GetBrandByIdAsync(int id)
        {
            return await _context.Brands
                .Include(b => b.Manufacturer)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<Brand> CreateBrandAsync(Brand brand)
        {
            _context.Brands.Add(brand);
            await _context.SaveChangesAsync();
            return brand;
        }

        public async Task<Brand> UpdateBrandAsync(Brand brand)
        {
            // 엔티티가 이미 컨텍스트에서 추적 중인지 확인
            var local = _context.Set<Brand>()
                              .Local
                              .FirstOrDefault(b => b.Id == brand.Id);

            // 기존 추적 중인 엔티티가 있으면 분리
            if (local != null)
            {
                _context.Entry(local).State = EntityState.Detached;
            }

            _context.Entry(brand).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return brand;
        }

        public async Task<bool> DeleteBrandAsync(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
                return false;

            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();
            return true;
        }

        // BikeType methods
        public async Task<List<BikeType>> GetAllBikeTypesAsync()
        {
            return await _context.BikeTypes.ToListAsync();
        }

        public async Task<BikeType?> GetBikeTypeByIdAsync(int id)
        {
            return await _context.BikeTypes.FindAsync(id);
        }

        public async Task<BikeType> CreateBikeTypeAsync(BikeType bikeType)
        {
            _context.BikeTypes.Add(bikeType);
            await _context.SaveChangesAsync();
            return bikeType;
        }

        public async Task<BikeType> UpdateBikeTypeAsync(BikeType bikeType)
        {
            // 엔티티가 이미 컨텍스트에서 추적 중인지 확인
            var local = _context.Set<BikeType>()
                              .Local
                              .FirstOrDefault(bt => bt.Id == bikeType.Id);

            // 기존 추적 중인 엔티티가 있으면 분리
            if (local != null)
            {
                _context.Entry(local).State = EntityState.Detached;
            }

            _context.Entry(bikeType).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return bikeType;
        }

        public async Task<bool> DeleteBikeTypeAsync(int id)
        {
            var bikeType = await _context.BikeTypes.FindAsync(id);
            if (bikeType == null)
                return false;

            _context.BikeTypes.Remove(bikeType);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}