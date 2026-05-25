using Maintenance.Domain.Cmms;
using Maintenance.Infrastructure.Context;
using Maintenance.Repository.Shared;
using Microsoft.EntityFrameworkCore;

namespace Maintenance.Repository.KpiRepository;

public class KpiRepository : IKpiRepository
{
    private readonly MaintenanceDbContext Context;

    public KpiRepository(MaintenanceDbContext context) => Context = context;

    public async Task<ReliabilityKpiResponse> GetReliabilityKpiAsync(int? assetId, DateTime? startDate, DateTime? endDate)
    {
        var periodStart = (startDate ?? DateTime.Now.Date.AddDays(-30)).Date;
        var periodEnd = (endDate ?? DateTime.Now.Date).Date;
        var periodEndExclusive = periodEnd.AddDays(1);
        var closedStatusId = await CmmsMasterDataResolver.GetWorkOrderStatusIdAsync(Context, WorkOrderStatus.CLOSED);
        var breakdownTypeId = await CmmsMasterDataResolver.GetMaintenanceTypeIdAsync(Context, MaintenanceType.BREAKDOWN);
        var correctiveTypeId = await CmmsMasterDataResolver.GetMaintenanceTypeIdAsync(Context, MaintenanceType.CORRECTIVE);

        var workOrders = Context.WorkOrders.AsNoTracking()
            .Where(x => x.StatusId == closedStatusId && x.ClosedAt >= periodStart && x.ClosedAt < periodEndExclusive);

        var downtimeLogs = Context.DowntimeLogs.AsNoTracking()
            .Where(x => x.StartTime >= periodStart && x.StartTime < periodEndExclusive);

        if (assetId.HasValue)
        {
            workOrders = workOrders.Where(x => x.AssetId == assetId.Value);
            downtimeLogs = downtimeLogs.Where(x => x.AssetId == assetId.Value);
        }

        var failureCount = await workOrders.CountAsync(x => x.MaintenanceTypeId == breakdownTypeId);
        var totalRepairMinutes = await workOrders
            .Where(x => x.MaintenanceTypeId == breakdownTypeId || x.MaintenanceTypeId == correctiveTypeId)
            .SumAsync(x => x.RepairMinutes ?? 0);
        var totalDowntimeMinutes = await downtimeLogs.SumAsync(x => x.DurationMinutes ?? 0);

        var periodMinutes = (int)Math.Max(0, (periodEndExclusive - periodStart).TotalMinutes);
        var operatingMinutes = Math.Max(0, periodMinutes - totalDowntimeMinutes);
        var mttr = failureCount == 0 ? 0 : Math.Round((decimal)totalRepairMinutes / failureCount, 2);
        var mtbf = failureCount == 0 ? 0 : Math.Round((decimal)operatingMinutes / failureCount, 2);
        var availability = mtbf + mttr == 0 ? 0 : Math.Round(mtbf / (mtbf + mttr) * 100, 2);

        return new ReliabilityKpiResponse
        {
            AssetId = assetId,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            FailureCount = failureCount,
            TotalDowntimeMinutes = totalDowntimeMinutes,
            TotalRepairMinutes = totalRepairMinutes,
            OperatingMinutes = operatingMinutes,
            MttrMinutes = mttr,
            MtbfMinutes = mtbf,
            AvailabilityPercent = availability
        };
    }
}
