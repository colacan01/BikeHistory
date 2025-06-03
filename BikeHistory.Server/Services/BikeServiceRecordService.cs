using BikeHistory.Server.Data;
using BikeHistory.Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BikeHistory.Server.Services
{
    public class BikeServiceRecordService : IBikeServiceRecordService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BikeServiceRecordService> _logger;

        public BikeServiceRecordService(ApplicationDbContext context, ILogger<BikeServiceRecordService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<BikeServiceRecord>> GetAllServiceRecordsAsync()
        {
            return await _context.BikeServiceRecords
                .Include(b => b.BikeFrame)
                .Include(b => b.ServiceShop)
                .ToListAsync();
        }

        public async Task<BikeServiceRecord> GetServiceRecordByIdAsync(int id)
        {
            return await _context.BikeServiceRecords
                .Include(b => b.BikeFrame)
                .Include(b => b.ServiceShop)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<IEnumerable<BikeServiceRecord>> GetServiceRecordsByBikeAsync(int bikeFrameId)
        {
            return await _context.BikeServiceRecords
                .Where(s => s.BikeFrameId == bikeFrameId)
                .Include(s => s.ServiceShop)
                .OrderByDescending(s => s.ServiceDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<BikeServiceRecord>> GetServiceRecordsByShopAsync(string shopId)
        {
            return await _context.BikeServiceRecords
                .Where(s => s.ServiceShopId == shopId)
                .Include(s => s.BikeFrame)
                .OrderByDescending(s => s.ServiceDate)
                .ToListAsync();
        }

        public async Task<BikeServiceRecord> CreateServiceRecordAsync(BikeServiceRecord serviceRecord)
        {
            serviceRecord.CreatedDate = DateTime.UtcNow;
            
            _context.BikeServiceRecords.Add(serviceRecord);
            await _context.SaveChangesAsync();
            
            return serviceRecord;
        }

        public async Task<bool> UpdateServiceRecordAsync(int id, BikeServiceRecord serviceRecord)
        {
            if (id != serviceRecord.Id)
                return false;

            serviceRecord.ModifiedDate = DateTime.UtcNow;
            _context.Entry(serviceRecord).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, $"업데이트 중 오류 발생: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteServiceRecordAsync(int id)
        {
            var serviceRecord = await _context.BikeServiceRecords.FindAsync(id);
            if (serviceRecord == null)
                return false;

            _context.BikeServiceRecords.Remove(serviceRecord);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> BikeFrameExistsAsync(int bikeFrameId)
        {
            return await _context.BikeFrames.AnyAsync(b => b.Id == bikeFrameId);
        }
    }
}