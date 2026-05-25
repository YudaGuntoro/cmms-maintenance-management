using Maintenance.Domain.Cmms;
using Maintenance.Infrastructure.Context;
using Maintenance.Repository.Shared;
using Microsoft.EntityFrameworkCore;

namespace Maintenance.Repository.DowntimeLogRepository;

public class DowntimeLogRepository : IDowntimeLogRepository
{
    private readonly MaintenanceDbContext Context;

    public DowntimeLogRepository(MaintenanceDbContext context) => Context = context;

    public async Task<List<DowntimeLog>> GetDowntimeLogsAsync(int? assetId, int? workOrderId)
    {
        var query = Context.DowntimeLogs
            .AsNoTracking()
            .IncludeDowntimeLogMasters()
            .AsQueryable();
        if (assetId.HasValue)
        {
            query = query.Where(x => x.AssetId == assetId.Value);
        }

        if (workOrderId.HasValue)
        {
            query = query.Where(x => x.WorkOrderId == workOrderId.Value);
        }

        return await query.OrderByDescending(x => x.StartTime).ToListAsync();
    }

    public Task<DowntimeLog?> GetDowntimeLogAsync(int id)
    {
        return Context.DowntimeLogs
            .AsNoTracking()
            .IncludeDowntimeLogMasters()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<DowntimeLog> CreateDowntimeLogAsync(DowntimeLog downtimeLog)
    {
        await CmmsMasterDataResolver.ApplyDowntimeLogMastersAsync(Context, downtimeLog);
        downtimeLog.Id = 0;
        downtimeLog.CreatedAt = DateTime.Now;
        WorkOrderRules.CalculateDowntimeDuration(downtimeLog);
        Context.DowntimeLogs.Add(downtimeLog);
        await Context.SaveChangesAsync();
        await LoadDowntimeLogMastersAsync(downtimeLog);
        return downtimeLog;
    }

    public async Task<DowntimeLog?> UpdateDowntimeLogAsync(int id, DowntimeLog downtimeLog)
    {
        var existing = await Context.DowntimeLogs.FirstOrDefaultAsync(x => x.Id == id);
        if (existing == null)
        {
            return null;
        }

        existing.AssetId = downtimeLog.AssetId;
        existing.WorkOrderId = downtimeLog.WorkOrderId;
        await CmmsMasterDataResolver.ApplyDowntimeLogMastersAsync(Context, downtimeLog);
        existing.DowntimeCategoryId = downtimeLog.DowntimeCategoryId;
        existing.DowntimeCategory = downtimeLog.DowntimeCategory;
        existing.DowntimeCategoryDetail = null;
        existing.StartTime = downtimeLog.StartTime;
        existing.EndTime = downtimeLog.EndTime;
        existing.Description = downtimeLog.Description;
        WorkOrderRules.CalculateDowntimeDuration(existing);
        await Context.SaveChangesAsync();
        await LoadDowntimeLogMastersAsync(existing);
        return existing;
    }

    public async Task<bool> DeleteDowntimeLogAsync(int id)
    {
        var existing = await Context.DowntimeLogs.FirstOrDefaultAsync(x => x.Id == id);
        if (existing == null)
        {
            return false;
        }

        Context.DowntimeLogs.Remove(existing);
        await Context.SaveChangesAsync();
        return true;
    }

    private async Task LoadDowntimeLogMastersAsync(DowntimeLog downtimeLog)
    {
        await Context.Entry(downtimeLog).Reference(x => x.DowntimeCategoryDetail).LoadAsync();
    }
}
