using BikeHistory.Server.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BikeHistory.Server.Services
{
    public interface IBikeServiceRecordService
    {
        Task<IEnumerable<BikeServiceRecord>> GetAllServiceRecordsAsync();
        Task<BikeServiceRecord> GetServiceRecordByIdAsync(int id);
        Task<IEnumerable<BikeServiceRecord>> GetServiceRecordsByBikeAsync(int bikeFrameId);
        Task<IEnumerable<BikeServiceRecord>> GetServiceRecordsByShopAsync(string shopId);
        Task<BikeServiceRecord> CreateServiceRecordAsync(BikeServiceRecord serviceRecord);
        Task<bool> UpdateServiceRecordAsync(int id, BikeServiceRecord serviceRecord);
        Task<bool> DeleteServiceRecordAsync(int id);
        Task<bool> BikeFrameExistsAsync(int bikeFrameId);
    }
}