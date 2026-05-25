using Maintenance.Domain.Cmms;
using Maintenance.Infrastructure.Context;
using Maintenance.Repository.Shared;
using Microsoft.EntityFrameworkCore;

namespace Maintenance.Repository.DashboardRepository;

public class DashboardRepository : IDashboardRepository
{
    private readonly MaintenanceDbContext Context;

    public DashboardRepository(MaintenanceDbContext context) => Context = context;

    public async Task<DashboardSummaryResponse> GetDashboardSummaryAsync()
    {
        var openStatuses = new[]
        {
            WorkOrderStatus.OPEN,
            WorkOrderStatus.ASSIGNED,
            WorkOrderStatus.IN_PROGRESS,
            WorkOrderStatus.PENDING
        };
        var openStatusCodes = openStatuses.Select(x => x.ToString()).ToArray();

        var today = DateTime.Now.Date;
        var periodStart = new DateTime(today.Year, today.Month, 1);
        var periodEnd = periodStart.AddMonths(1);
        var assetLookup = await Context.Assets.AsNoTracking().ToDictionaryAsync(x => x.Id);
        var statusLookup = await Context.WorkOrderStatuses.AsNoTracking().ToDictionaryAsync(x => x.Id, x => x.Code);
        var downtimeCategoryLookup = await Context.DowntimeCategories.AsNoTracking().ToDictionaryAsync(x => x.Id, x => x.Code);
        var openStatusIds = await Context.WorkOrderStatuses.AsNoTracking()
            .Where(x => openStatusCodes.Contains(x.Code))
            .Select(x => x.Id)
            .ToListAsync();
        var closedStatusId = await CmmsMasterDataResolver.GetWorkOrderStatusIdAsync(Context, WorkOrderStatus.CLOSED);
        var breakdownMaintenanceTypeId = await CmmsMasterDataResolver.GetMaintenanceTypeIdAsync(Context, MaintenanceType.BREAKDOWN);
        var workOrdersInPeriod = Context.WorkOrders.AsNoTracking()
            .Where(x =>
                (x.CreatedAt >= periodStart && x.CreatedAt < periodEnd) ||
                (x.ReportedAt >= periodStart && x.ReportedAt < periodEnd) ||
                (x.ScheduledAt >= periodStart && x.ScheduledAt < periodEnd) ||
                (x.CompletedAt >= periodStart && x.CompletedAt < periodEnd) ||
                (x.ClosedAt >= periodStart && x.ClosedAt < periodEnd));
        var downtimeLogsInPeriod = Context.DowntimeLogs.AsNoTracking()
            .Where(x => x.StartTime >= periodStart && x.StartTime < periodEnd);

        var downtimeByAsset = await Context.DowntimeLogs.AsNoTracking()
            .Where(x => x.StartTime >= periodStart && x.StartTime < periodEnd)
            .GroupBy(x => x.AssetId)
            .Select(x => new { AssetId = x.Key, Value = x.Sum(y => y.DurationMinutes ?? 0) })
            .OrderByDescending(x => x.Value)
            .Take(5)
            .ToListAsync();

        var failuresByAsset = await workOrdersInPeriod
            .Where(x => x.StatusId == closedStatusId && x.MaintenanceTypeId == breakdownMaintenanceTypeId)
            .GroupBy(x => x.AssetId)
            .Select(x => new { AssetId = x.Key, Value = x.Count() })
            .OrderByDescending(x => x.Value)
            .Take(5)
            .ToListAsync();

        var statusRows = await workOrdersInPeriod
            .GroupBy(x => x.StatusId)
            .Select(x => new { Name = x.Key, Value = x.Count() })
            .ToListAsync();

        var downtimeCategoryRows = await downtimeLogsInPeriod
            .GroupBy(x => x.DowntimeCategoryId)
            .Select(x => new { Name = x.Key, Value = x.Sum(y => y.DurationMinutes ?? 0) })
            .ToListAsync();

        return new DashboardSummaryResponse
        {
            TotalAssets = await Context.Assets.CountAsync(),
            OpenWorkOrders = await workOrdersInPeriod.CountAsync(x => x.StatusId.HasValue && openStatusIds.Contains(x.StatusId.Value)),
            OverduePreventiveMaintenance = await Context.PreventiveSchedules.CountAsync(x => x.IsActive && x.NextDueDate.Date < today),
            LowStockSpareparts = await Context.Spareparts.CountAsync(x => x.StockQty <= x.MinimumStock),
            TopAssetsByDowntime = downtimeByAsset.Select(x => BuildAssetMetric(assetLookup, x.AssetId, x.Value)).ToList(),
            TopAssetsByFailureCount = failuresByAsset.Select(x => BuildAssetMetric(assetLookup, x.AssetId, x.Value)).ToList(),
            WorkOrderStatusSummary = statusRows.Select(x => new NameValueSummary { Name = GetLookupCode(statusLookup, x.Name), Value = x.Value }).ToList(),
            DowntimeCategorySummary = downtimeCategoryRows.Select(x => new NameValueSummary { Name = GetLookupCode(downtimeCategoryLookup, x.Name), Value = x.Value }).ToList()
        };
    }

    private static string GetLookupCode(IReadOnlyDictionary<int, string> lookup, int? id)
    {
        return id.HasValue && lookup.TryGetValue(id.Value, out var code) ? code : string.Empty;
    }

    private static AssetMetricSummary BuildAssetMetric(IReadOnlyDictionary<int, Asset> assets, int assetId, int value)
    {
        assets.TryGetValue(assetId, out var asset);
        return new AssetMetricSummary
        {
            AssetId = assetId,
            AssetCode = asset?.AssetCode ?? string.Empty,
            AssetName = asset?.AssetName ?? string.Empty,
            Value = value
        };
    }
}
