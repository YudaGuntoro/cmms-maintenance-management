using Maintenance.Domain.Cmms;

namespace Maintenance.Persistence.Services.DowntimeLogService;

public interface IDowntimeLogService
{
    Task<List<DowntimeLog>> GetDowntimeLogsAsync(int? assetId, int? workOrderId);
    Task<DowntimeLog?> GetDowntimeLogAsync(int id);
    Task<DowntimeLog> CreateDowntimeLogAsync(DowntimeLog downtimeLog);
    Task<DowntimeLog?> UpdateDowntimeLogAsync(int id, DowntimeLog downtimeLog);
    Task<bool> DeleteDowntimeLogAsync(int id);
}
