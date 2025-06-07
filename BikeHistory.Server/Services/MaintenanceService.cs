using BikeHistory.Server.Data;
using BikeHistory.Server.Models;
using BikeHistory.Server.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BikeHistory.Server.Services
{
    public class MaintenanceService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MaintenanceService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // 모든 유지보수 기록 가져오기
        public async Task<List<Maintenance>> GetAllMaintenancesAsync()
        {
            return await _context.Maintenances
                .Include(m => m.BikeFrame)
                .Include(m => m.Owner)
                .Include(m => m.Store)
                .Include(m => m.MaintenanceDetails)
                .ToListAsync();
        }

        // 특정 ID의 유지보수 기록 가져오기
        public async Task<Maintenance?> GetMaintenanceByIdAsync(Guid id)
        {
            return await _context.Maintenances
                .Include(m => m.BikeFrame)
                .Include(m => m.Owner)
                .Include(m => m.Store)
                .Include(m => m.MaintenanceDetails)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        // 특정 자전거의 유지보수 기록 가져오기
        public async Task<List<Maintenance>> GetMaintenancesByBikeFrameIdAsync(int bikeFrameId)
        {
            return await _context.Maintenances
                .Include(m => m.BikeFrame)
                .Include(m => m.Owner)
                .Include(m => m.Store)
                .Include(m => m.MaintenanceDetails)
                .Where(m => m.BikeFrameId == bikeFrameId)
                .OrderByDescending(m => m.MaintenanceDate)
                .ToListAsync();
        }

        // 특정 소유자의 유지보수 기록 가져오기
        public async Task<List<Maintenance>> GetMaintenancesByOwnerIdAsync(string ownerId)
        {
            if (string.IsNullOrEmpty(ownerId))
            {
                throw new ArgumentException("소유자 ID는 비어 있을 수 없습니다", nameof(ownerId));
            }

            return await _context.Maintenances
                .Include(m => m.BikeFrame)
                .Include(m => m.Owner)
                .Include(m => m.Store)
                .Include(m => m.MaintenanceDetails)
                .Where(m => m.OwnerId == ownerId)
                .OrderByDescending(m => m.MaintenanceDate)
                .ToListAsync();
        }

        // 특정 상점의 유지보수 기록 가져오기
        public async Task<List<Maintenance>> GetMaintenancesByStoreIdAsync(string storeId)
        {
            if (string.IsNullOrEmpty(storeId))
            {
                throw new ArgumentException("상점 ID는 비어 있을 수 없습니다", nameof(storeId));
            }

            return await _context.Maintenances
                .Include(m => m.BikeFrame)
                .Include(m => m.Owner)
                .Include(m => m.Store)
                .Include(m => m.MaintenanceDetails)
                .Where(m => m.StoreId == storeId)
                .OrderByDescending(m => m.MaintenanceDate)
                .ToListAsync();
        }

        // 유지보수 기록 생성
        public async Task<Maintenance> CreateMaintenanceAsync(Maintenance maintenance, List<MaintenanceDetail> details)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 유지보수 기록 저장
                _context.Maintenances.Add(maintenance);
                await _context.SaveChangesAsync();

                // 상세 항목 추가
                int seq = 1;
                foreach (var detail in details)
                {
                    detail.MaintenanceId = maintenance.Id;
                    detail.Seq = seq++;
                    _context.MaintenanceDetails.Add(detail);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return maintenance;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // 유지보수 기록 수정
        public async Task<Maintenance> UpdateMaintenanceAsync(Maintenance maintenance, List<MaintenanceDetail>? details = null)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 유지보수 기록 업데이트
                _context.Entry(maintenance).State = EntityState.Modified;
                
                // 상세 항목이 제공된 경우 기존 항목을 삭제하고 새 항목 추가
                if (details != null)
                {
                    // 기존 상세 항목 삭제
                    var existingDetails = await _context.MaintenanceDetails
                        .Where(d => d.MaintenanceId == maintenance.Id)
                        .ToListAsync();
                    
                    _context.MaintenanceDetails.RemoveRange(existingDetails);
                    
                    // 새 상세 항목 추가
                    int seq = 1;
                    foreach (var detail in details)
                    {
                        detail.MaintenanceId = maintenance.Id;
                        detail.Seq = seq++;
                        _context.MaintenanceDetails.Add(detail);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return maintenance;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // 유지보수 기록 삭제
        public async Task DeleteMaintenanceAsync(Guid id)
        {
            var maintenance = await _context.Maintenances.FindAsync(id);
            if (maintenance == null)
            {
                throw new InvalidOperationException("유지보수 기록을 찾을 수 없습니다");
            }

            _context.Maintenances.Remove(maintenance);
            await _context.SaveChangesAsync();
        }

        // 유지보수 상세 항목 가져오기
        public async Task<MaintenanceDetail?> GetMaintenanceDetailAsync(Guid maintenanceId, int seq)
        {
            return await _context.MaintenanceDetails
                .FirstOrDefaultAsync(d => d.MaintenanceId == maintenanceId && d.Seq == seq);
        }
        
        // 유지보수 상세 항목 추가
        public async Task<MaintenanceDetail> AddMaintenanceDetailAsync(MaintenanceDetail detail)
        {
            // 현재 가장 큰 seq 값 찾기
            var maxSeq = await _context.MaintenanceDetails
                .Where(d => d.MaintenanceId == detail.MaintenanceId)
                .MaxAsync(d => (int?)d.Seq) ?? 0;
            
            detail.Seq = maxSeq + 1;
            
            _context.MaintenanceDetails.Add(detail);
            await _context.SaveChangesAsync();
            
            return detail;
        }
        
        // 유지보수 상세 항목 수정
        public async Task<MaintenanceDetail> UpdateMaintenanceDetailAsync(MaintenanceDetail detail)
        {
            _context.Entry(detail).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return detail;
        }
        
        // 유지보수 상세 항목 삭제
        public async Task DeleteMaintenanceDetailAsync(Guid maintenanceId, int seq)
        {
            var detail = await _context.MaintenanceDetails
                .FirstOrDefaultAsync(d => d.MaintenanceId == maintenanceId && d.Seq == seq);
                
            if (detail == null)
            {
                throw new InvalidOperationException("유지보수 상세 항목을 찾을 수 없습니다");
            }
            
            _context.MaintenanceDetails.Remove(detail);
            await _context.SaveChangesAsync();
        }
    }
}