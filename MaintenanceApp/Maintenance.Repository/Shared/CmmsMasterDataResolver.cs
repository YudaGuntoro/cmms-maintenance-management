using Maintenance.Domain.Cmms;
using Maintenance.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Maintenance.Repository.Shared;

public static class CmmsMasterDataResolver
{
    public static IQueryable<WorkOrder> IncludeWorkOrderMasters(this IQueryable<WorkOrder> query)
    {
        return query
            .Include(x => x.MaintenanceTypeDetail)
            .Include(x => x.PriorityDetail)
            .Include(x => x.StatusDetail);
    }

    public static IQueryable<ProblemReport> IncludeProblemReportMasters(this IQueryable<ProblemReport> query)
    {
        return query
            .Include(x => x.CategoryDetail)
            .Include(x => x.PriorityDetail)
            .Include(x => x.StatusDetail);
    }

    public static IQueryable<DowntimeLog> IncludeDowntimeLogMasters(this IQueryable<DowntimeLog> query)
    {
        return query.Include(x => x.DowntimeCategoryDetail);
    }

    public static IQueryable<PreventiveSchedule> IncludePreventiveScheduleMasters(this IQueryable<PreventiveSchedule> query)
    {
        return query
            .Include(x => x.ScheduleTypeDetail)
            .Include(x => x.FrequencyTypeDetail);
    }

    public static async Task ApplyWorkOrderMastersAsync(MaintenanceDbContext db, WorkOrder workOrder)
    {
        var maintenanceType = await ResolveMaintenanceTypeAsync(db, workOrder.MaintenanceTypeId, workOrder.MaintenanceType);
        var priority = await ResolveWorkOrderPriorityAsync(db, workOrder.PriorityId, workOrder.Priority);
        var status = await ResolveWorkOrderStatusAsync(db, workOrder.StatusId, workOrder.Status);

        workOrder.MaintenanceTypeId = maintenanceType.id;
        workOrder.MaintenanceType = maintenanceType.value;
        workOrder.PriorityId = priority.id;
        workOrder.Priority = priority.value;
        workOrder.StatusId = status.id;
        workOrder.Status = status.value;
    }

    public static async Task ApplyProblemReportMastersAsync(MaintenanceDbContext db, ProblemReport report)
    {
        var category = await ResolveProblemReportCategoryAsync(db, report.CategoryId, report.Category);
        var priority = await ResolveWorkOrderPriorityAsync(db, report.PriorityId, report.Priority);
        var status = await ResolveProblemReportStatusAsync(db, report.StatusId, report.Status);

        report.CategoryId = category.id;
        report.Category = category.value;
        report.PriorityId = priority.id;
        report.Priority = priority.value;
        report.StatusId = status.id;
        report.Status = status.value;
    }

    public static async Task ApplyDowntimeLogMastersAsync(MaintenanceDbContext db, DowntimeLog downtimeLog)
    {
        var category = await ResolveDowntimeCategoryAsync(db, downtimeLog.DowntimeCategoryId, downtimeLog.DowntimeCategory);
        downtimeLog.DowntimeCategoryId = category.id;
        downtimeLog.DowntimeCategory = category.value;
    }

    public static async Task ApplyPreventiveScheduleMastersAsync(MaintenanceDbContext db, PreventiveSchedule schedule)
    {
        var scheduleType = await ResolvePreventiveScheduleTypeAsync(db, schedule.ScheduleTypeId, schedule.ScheduleType);
        var frequencyType = await ResolveFrequencyTypeAsync(db, schedule.FrequencyTypeId, schedule.FrequencyType);

        schedule.ScheduleTypeId = scheduleType.id;
        schedule.ScheduleType = scheduleType.value;
        schedule.FrequencyTypeId = frequencyType.id;
        schedule.FrequencyType = frequencyType.value;
    }

    public static async Task SetWorkOrderStatusAsync(MaintenanceDbContext db, WorkOrder workOrder, WorkOrderStatus status)
    {
        var resolved = await ResolveWorkOrderStatusAsync(db, null, status);
        workOrder.StatusDetail = await RequireTrackedWorkOrderStatusAsync(db, resolved.id);
        workOrder.StatusId = resolved.id;
        workOrder.Status = resolved.value;
    }

    public static async Task SetProblemReportStatusAsync(MaintenanceDbContext db, ProblemReport report, ProblemReportStatus status)
    {
        var resolved = await ResolveProblemReportStatusAsync(db, null, status);
        report.StatusDetail = await RequireTrackedWorkOrderStatusAsync(db, resolved.id);
        report.StatusId = resolved.id;
        report.Status = resolved.value;
    }

    public static Task<int> GetMaintenanceTypeIdAsync(MaintenanceDbContext db, MaintenanceType value)
    {
        return RequireIdByCodeAsync(db.MaintenanceTypes, value.ToString(), "maintenance_type");
    }

    public static Task<int> GetWorkOrderPriorityIdAsync(MaintenanceDbContext db, WorkOrderPriority value)
    {
        return RequireIdByCodeAsync(db.WorkOrderPriorities, value.ToString(), "priority");
    }

    public static Task<int> GetWorkOrderStatusIdAsync(MaintenanceDbContext db, WorkOrderStatus value)
    {
        return RequireIdByCodeAsync(db.WorkOrderStatuses, value.ToString(), "status");
    }

    public static Task<int> GetProblemReportStatusIdAsync(MaintenanceDbContext db, ProblemReportStatus value)
    {
        return RequireIdByCodeAsync(db.WorkOrderStatuses, value.ToString(), "status");
    }

    public static Task<int> GetProblemReportCategoryIdAsync(MaintenanceDbContext db, ProblemReportCategory value)
    {
        return RequireIdByCodeAsync(db.ProblemReportCategories, value.ToString(), "category");
    }

    public static Task<int> GetDowntimeCategoryIdAsync(MaintenanceDbContext db, DowntimeCategory value)
    {
        return RequireIdByCodeAsync(db.DowntimeCategories, value.ToString(), "downtime_category");
    }

    private static async Task<(int id, MaintenanceType value)> ResolveMaintenanceTypeAsync(MaintenanceDbContext db, int? id, MaintenanceType fallback)
    {
        var resolvedId = id.GetValueOrDefault();
        var master = resolvedId > 0
            ? await db.MaintenanceTypes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == resolvedId)
            : await db.MaintenanceTypes.AsNoTracking().FirstOrDefaultAsync(x => x.Code == fallback.ToString());

        if (master == null)
        {
            throw new InvalidOperationException("maintenance_type tidak ditemukan di master data.");
        }

        return (master.Id, Parse(master.Code, fallback));
    }

    private static async Task<(int id, WorkOrderPriority value)> ResolveWorkOrderPriorityAsync(MaintenanceDbContext db, int? id, WorkOrderPriority fallback)
    {
        var resolvedId = id.GetValueOrDefault();
        var master = resolvedId > 0
            ? await db.WorkOrderPriorities.AsNoTracking().FirstOrDefaultAsync(x => x.Id == resolvedId)
            : await db.WorkOrderPriorities.AsNoTracking().FirstOrDefaultAsync(x => x.Code == fallback.ToString());

        if (master == null)
        {
            throw new InvalidOperationException("priority tidak ditemukan di master data.");
        }

        return (master.Id, Parse(master.Code, fallback));
    }

    private static async Task<(int id, WorkOrderStatus value)> ResolveWorkOrderStatusAsync(MaintenanceDbContext db, int? id, WorkOrderStatus fallback)
    {
        var resolvedId = id.GetValueOrDefault();
        var master = resolvedId > 0
            ? await db.WorkOrderStatuses.AsNoTracking().FirstOrDefaultAsync(x => x.Id == resolvedId)
            : await db.WorkOrderStatuses.AsNoTracking().FirstOrDefaultAsync(x => x.Code == fallback.ToString());

        if (master == null)
        {
            throw new InvalidOperationException("status tidak ditemukan di master data.");
        }

        return (master.Id, Parse(master.Code, fallback));
    }

    private static async Task<(int id, ProblemReportStatus value)> ResolveProblemReportStatusAsync(MaintenanceDbContext db, int? id, ProblemReportStatus fallback)
    {
        var resolvedId = id.GetValueOrDefault();
        var master = resolvedId > 0
            ? await db.WorkOrderStatuses.AsNoTracking().FirstOrDefaultAsync(x => x.Id == resolvedId)
            : await db.WorkOrderStatuses.AsNoTracking().FirstOrDefaultAsync(x => x.Code == fallback.ToString());

        if (master == null)
        {
            throw new InvalidOperationException("status report tidak ditemukan di master data.");
        }

        return (master.Id, Parse(master.Code, fallback));
    }

    private static async Task<(int id, ProblemReportCategory value)> ResolveProblemReportCategoryAsync(MaintenanceDbContext db, int? id, ProblemReportCategory fallback)
    {
        var resolvedId = id.GetValueOrDefault();
        var master = resolvedId > 0
            ? await db.ProblemReportCategories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == resolvedId)
            : await db.ProblemReportCategories.AsNoTracking().FirstOrDefaultAsync(x => x.Code == fallback.ToString());

        if (master == null)
        {
            throw new InvalidOperationException("category report tidak ditemukan di master data.");
        }

        return (master.Id, Parse(master.Code, fallback));
    }

    private static async Task<(int id, DowntimeCategory value)> ResolveDowntimeCategoryAsync(MaintenanceDbContext db, int? id, DowntimeCategory fallback)
    {
        var resolvedId = id.GetValueOrDefault();
        var master = resolvedId > 0
            ? await db.DowntimeCategories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == resolvedId)
            : await db.DowntimeCategories.AsNoTracking().FirstOrDefaultAsync(x => x.Code == fallback.ToString());

        if (master == null)
        {
            throw new InvalidOperationException("downtime_category tidak ditemukan di master data.");
        }

        return (master.Id, Parse(master.Code, fallback));
    }

    private static async Task<(int id, PreventiveScheduleType value)> ResolvePreventiveScheduleTypeAsync(MaintenanceDbContext db, int? id, PreventiveScheduleType fallback)
    {
        var resolvedId = id.GetValueOrDefault();
        var master = resolvedId > 0
            ? await db.PreventiveScheduleTypes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == resolvedId)
            : await db.PreventiveScheduleTypes.AsNoTracking().FirstOrDefaultAsync(x => x.Code == fallback.ToString());

        if (master == null)
        {
            throw new InvalidOperationException("schedule_type tidak ditemukan di master data.");
        }

        return (master.Id, Parse(master.Code, fallback));
    }

    private static async Task<(int id, FrequencyType value)> ResolveFrequencyTypeAsync(MaintenanceDbContext db, int? id, FrequencyType fallback)
    {
        var resolvedId = id.GetValueOrDefault();
        var master = resolvedId > 0
            ? await db.FrequencyTypes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == resolvedId)
            : await db.FrequencyTypes.AsNoTracking().FirstOrDefaultAsync(x => x.Code == fallback.ToString());

        if (master == null)
        {
            throw new InvalidOperationException("frequency_type tidak ditemukan di master data.");
        }

        return (master.Id, Parse(master.Code, fallback));
    }

    private static async Task<int> RequireIdByCodeAsync<TMaster>(IQueryable<TMaster> query, string code, string fieldName)
        where TMaster : class
    {
        var id = await query
            .AsNoTracking()
            .Where(x => EF.Property<string>(x, nameof(MaintenanceTypeMaster.Code)) == code)
            .Select(x => EF.Property<int>(x, nameof(MaintenanceTypeMaster.Id)))
            .FirstOrDefaultAsync();

        if (id <= 0)
        {
            throw new InvalidOperationException($"{fieldName} {code} tidak ditemukan di master data.");
        }

        return id;
    }

    private static async Task<WorkOrderStatusMaster> RequireTrackedWorkOrderStatusAsync(MaintenanceDbContext db, int id)
    {
        var master = await db.WorkOrderStatuses.FirstOrDefaultAsync(x => x.Id == id);
        if (master == null)
        {
            throw new InvalidOperationException("status tidak ditemukan di master data.");
        }

        return master;
    }

    private static TEnum Parse<TEnum>(string code, TEnum fallback) where TEnum : struct, Enum
    {
        return Enum.TryParse<TEnum>(code, true, out var parsed) ? parsed : fallback;
    }
}
