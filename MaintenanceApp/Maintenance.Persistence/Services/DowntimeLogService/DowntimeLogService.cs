using Maintenance.Domain.Cmms;
using Maintenance.Repository.DowntimeLogRepository;

namespace Maintenance.Persistence.Services.DowntimeLogService;

public class DowntimeLogService : IDowntimeLogService
{
    protected readonly IDowntimeLogRepository Context;

    public DowntimeLogService(IDowntimeLogRepository data) => Context = data;

    public Task<List<DowntimeLog>> GetDowntimeLogsAsync(int? assetId, int? workOrderId)
    {
        return Context.GetDowntimeLogsAsync(assetId, workOrderId);
    }

    public Task<DowntimeLog?> GetDowntimeLogAsync(int id)
    {
        return Context.GetDowntimeLogAsync(id);
    }

    public Task<DowntimeLog> CreateDowntimeLogAsync(DowntimeLog downtimeLog)
    {
        return Context.CreateDowntimeLogAsync(downtimeLog);
    }

    public Task<DowntimeLog?> UpdateDowntimeLogAsync(int id, DowntimeLog downtimeLog)
    {
        return Context.UpdateDowntimeLogAsync(id, downtimeLog);
    }

    public Task<bool> DeleteDowntimeLogAsync(int id)
    {
        return Context.DeleteDowntimeLogAsync(id);
    }
}
