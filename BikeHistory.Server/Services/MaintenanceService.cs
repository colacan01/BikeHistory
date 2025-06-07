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

        // ��� �������� ��� ��������
        public async Task<List<Maintenance>> GetAllMaintenancesAsync()
        {
            return await _context.Maintenances
                .Include(m => m.BikeFrame)
                .Include(m => m.Owner)
                .Include(m => m.Store)
                .Include(m => m.MaintenanceDetails)
                .ToListAsync();
        }

        // Ư�� ID�� �������� ��� ��������
        public async Task<Maintenance?> GetMaintenanceByIdAsync(Guid id)
        {
            return await _context.Maintenances
                .Include(m => m.BikeFrame)
                .Include(m => m.Owner)
                .Include(m => m.Store)
                .Include(m => m.MaintenanceDetails)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        // Ư�� �������� �������� ��� ��������
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

        // Ư�� �������� �������� ��� ��������
        public async Task<List<Maintenance>> GetMaintenancesByOwnerIdAsync(string ownerId)
        {
            if (string.IsNullOrEmpty(ownerId))
            {
                throw new ArgumentException("������ ID�� ��� ���� �� �����ϴ�", nameof(ownerId));
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

        // Ư�� ������ �������� ��� ��������
        public async Task<List<Maintenance>> GetMaintenancesByStoreIdAsync(string storeId)
        {
            if (string.IsNullOrEmpty(storeId))
            {
                throw new ArgumentException("���� ID�� ��� ���� �� �����ϴ�", nameof(storeId));
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

        // �������� ��� ����
        public async Task<Maintenance> CreateMaintenanceAsync(Maintenance maintenance, List<MaintenanceDetail> details)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // �������� ��� ����
                _context.Maintenances.Add(maintenance);
                await _context.SaveChangesAsync();

                // �� �׸� �߰�
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

        // �������� ��� ����
        public async Task<Maintenance> UpdateMaintenanceAsync(Maintenance maintenance, List<MaintenanceDetail>? details = null)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // �������� ��� ������Ʈ
                _context.Entry(maintenance).State = EntityState.Modified;
                
                // �� �׸��� ������ ��� ���� �׸��� �����ϰ� �� �׸� �߰�
                if (details != null)
                {
                    // ���� �� �׸� ����
                    var existingDetails = await _context.MaintenanceDetails
                        .Where(d => d.MaintenanceId == maintenance.Id)
                        .ToListAsync();
                    
                    _context.MaintenanceDetails.RemoveRange(existingDetails);
                    
                    // �� �� �׸� �߰�
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

        // �������� ��� ����
        public async Task DeleteMaintenanceAsync(Guid id)
        {
            var maintenance = await _context.Maintenances.FindAsync(id);
            if (maintenance == null)
            {
                throw new InvalidOperationException("�������� ����� ã�� �� �����ϴ�");
            }

            _context.Maintenances.Remove(maintenance);
            await _context.SaveChangesAsync();
        }

        // �������� �� �׸� ��������
        public async Task<MaintenanceDetail?> GetMaintenanceDetailAsync(Guid maintenanceId, int seq)
        {
            return await _context.MaintenanceDetails
                .FirstOrDefaultAsync(d => d.MaintenanceId == maintenanceId && d.Seq == seq);
        }
        
        // �������� �� �׸� �߰�
        public async Task<MaintenanceDetail> AddMaintenanceDetailAsync(MaintenanceDetail detail)
        {
            // ���� ���� ū seq �� ã��
            var maxSeq = await _context.MaintenanceDetails
                .Where(d => d.MaintenanceId == detail.MaintenanceId)
                .MaxAsync(d => (int?)d.Seq) ?? 0;
            
            detail.Seq = maxSeq + 1;
            
            _context.MaintenanceDetails.Add(detail);
            await _context.SaveChangesAsync();
            
            return detail;
        }
        
        // �������� �� �׸� ����
        public async Task<MaintenanceDetail> UpdateMaintenanceDetailAsync(MaintenanceDetail detail)
        {
            _context.Entry(detail).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return detail;
        }
        
        // �������� �� �׸� ����
        public async Task DeleteMaintenanceDetailAsync(Guid maintenanceId, int seq)
        {
            var detail = await _context.MaintenanceDetails
                .FirstOrDefaultAsync(d => d.MaintenanceId == maintenanceId && d.Seq == seq);
                
            if (detail == null)
            {
                throw new InvalidOperationException("�������� �� �׸��� ã�� �� �����ϴ�");
            }
            
            _context.MaintenanceDetails.Remove(detail);
            await _context.SaveChangesAsync();
        }
    }
}