using BikeHistory.Server.Models;
using BikeHistory.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace BikeHistory.Server.Services
{
    public interface IUserActivityLogger
    {
        Task LogUserActivityAsync(UserActivityLog log);
        Task<IEnumerable<UserActivityLog>> GetUserActivityLogsAsync(string userId, DateTime? startDate, DateTime? endDate);
        Task<IEnumerable<UserActivityLog>> GetAllActivityLogsAsync(DateTime? startDate, DateTime? endDate);
        Task<(IEnumerable<UserActivityLog> Logs, int TotalItems, int TotalPages)> GetAllActivityLogsPagedAsync(int page, int pageSize, DateTime? startDate, DateTime? endDate);
        Task<(IEnumerable<UserActivityLog> Logs, int TotalItems, int TotalPages)> GetUserActivityLogsPagedAsync(string userId, int page, int pageSize, DateTime? startDate, DateTime? endDate);
    }

    public class UserActivityLogger : IUserActivityLogger
    {
        private readonly ApplicationDbContext _context;

        // �⺻ ������ �߰�
        public UserActivityLogger() : this(new ApplicationDbContext())
        {
        }

        public UserActivityLogger(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogUserActivityAsync(UserActivityLog log)
        {
            log.Timestamp = DateTime.UtcNow;
            _context.UserActivityLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<UserActivityLog>> GetUserActivityLogsAsync(string userId, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.UserActivityLogs.Where(l => l.UserId == userId);
            
            if (startDate.HasValue)
                query = query.Where(l => l.Timestamp >= startDate.Value);
                
            if (endDate.HasValue)
                query = query.Where(l => l.Timestamp <= endDate.Value);
                
            return await query.OrderByDescending(l => l.Timestamp).ToListAsync();
        }

        public async Task<IEnumerable<UserActivityLog>> GetAllActivityLogsAsync(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.UserActivityLogs.AsQueryable();
            
            if (startDate.HasValue)
                query = query.Where(l => l.Timestamp >= startDate.Value);
                
            if (endDate.HasValue)
                query = query.Where(l => l.Timestamp <= endDate.Value);
                
            return await query.OrderByDescending(l => l.Timestamp).ToListAsync();
        }

        public async Task<(IEnumerable<UserActivityLog> Logs, int TotalItems, int TotalPages)> GetAllActivityLogsPagedAsync(
            int page, int pageSize, DateTime? startDate, DateTime? endDate)
        {
            // ������ �ε��� ��ȿ�� �˻�
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _context.UserActivityLogs.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(l => l.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(l => l.Timestamp <= endDate.Value);

            // ��ü �׸� �� �� ������ �� ���
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // ����¡ ó���� �α� ��������
            var logs = await query
                .OrderByDescending(l => l.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (logs, totalItems, totalPages);
        }

        public async Task<(IEnumerable<UserActivityLog> Logs, int TotalItems, int TotalPages)> GetUserActivityLogsPagedAsync(
            string userId, int page, int pageSize, DateTime? startDate, DateTime? endDate)
        {
            // ������ �ε��� ��ȿ�� �˻�
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _context.UserActivityLogs
                .Where(l => l.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(l => l.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(l => l.Timestamp <= endDate.Value);

            // ��ü �׸� �� �� ������ �� ���
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // ����¡ ó���� �α� ��������
            var logs = await query
                .OrderByDescending(l => l.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (logs, totalItems, totalPages);
        }
    }
}