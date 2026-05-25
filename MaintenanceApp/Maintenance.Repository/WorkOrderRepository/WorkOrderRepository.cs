using Maintenance.Domain.Cmms;
using Maintenance.Infrastructure.Context;
using Maintenance.Repository.Shared;
using Microsoft.EntityFrameworkCore;

namespace Maintenance.Repository.WorkOrderRepository;

public class WorkOrderRepository : IWorkOrderRepository
{
    private readonly MaintenanceDbContext Context;

    public WorkOrderRepository(MaintenanceDbContext context) => Context = context;

    public async Task<List<WorkOrder>> GetWorkOrdersAsync(int? assetId, WorkOrderStatus? status, WorkOrderPriority? priority, MaintenanceType? maintenanceType)
    {
        var query = Context.WorkOrders
            .AsNoTracking()
            .IncludeWorkOrderMasters()
            .AsQueryable();

        if (assetId.HasValue)
        {
            query = query.Where(x => x.AssetId == assetId.Value);
        }

        if (status.HasValue)
        {
            var statusId = await CmmsMasterDataResolver.GetWorkOrderStatusIdAsync(Context, status.Value);
            query = query.Where(x => x.StatusId == statusId);
        }

        if (priority.HasValue)
        {
            var priorityId = await CmmsMasterDataResolver.GetWorkOrderPriorityIdAsync(Context, priority.Value);
            query = query.Where(x => x.PriorityId == priorityId);
        }

        if (maintenanceType.HasValue)
        {
            var maintenanceTypeId = await CmmsMasterDataResolver.GetMaintenanceTypeIdAsync(Context, maintenanceType.Value);
            query = query.Where(x => x.MaintenanceTypeId == maintenanceTypeId);
        }

        return await query.OrderByDescending(x => x.ReportedAt ?? x.CreatedAt).ToListAsync();
    }

    public Task<WorkOrder?> GetWorkOrderAsync(int id)
    {
        return Context.WorkOrders
            .AsNoTracking()
            .IncludeWorkOrderMasters()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<WorkOrder> CreateWorkOrderAsync(WorkOrder workOrder)
    {
        if (!await Context.Assets.AnyAsync(x => x.Id == workOrder.AssetId))
        {
            throw new InvalidOperationException("Asset tidak ditemukan.");
        }

        await ValidateProblemReportReferenceAsync(workOrder.AssetId, workOrder.ProblemReportId);
        await CmmsMasterDataResolver.ApplyWorkOrderMastersAsync(Context, workOrder);

        workOrder.Id = 0;
        workOrder.WoNumber = string.IsNullOrWhiteSpace(workOrder.WoNumber)
            ? await WorkOrderRules.GenerateWorkOrderNumberAsync(Context, workOrder.MaintenanceType)
            : workOrder.WoNumber;
        workOrder.ReportedAt ??= DateTime.Now;
        workOrder.CreatedAt = DateTime.Now;
        workOrder.UpdatedAt = DateTime.Now;

        WorkOrderRules.CalculateWorkOrderDurations(workOrder);
        WorkOrderRules.ValidateWorkOrderForSave(workOrder);

        Context.WorkOrders.Add(workOrder);
        await Context.SaveChangesAsync();
        await SyncProblemReportStatusAsync(workOrder);
        await SyncDowntimeLogAsync(workOrder);
        await LoadWorkOrderMastersAsync(workOrder);
        return workOrder;
    }

    public async Task<WorkOrder?> UpdateWorkOrderAsync(int id, WorkOrder workOrder)
    {
        var existing = await Context.WorkOrders.FirstOrDefaultAsync(x => x.Id == id);
        if (existing == null)
        {
            return null;
        }

        var previousProblemReportId = existing.ProblemReportId;
        await ValidateProblemReportReferenceAsync(workOrder.AssetId, workOrder.ProblemReportId);
        await CmmsMasterDataResolver.ApplyWorkOrderMastersAsync(Context, workOrder);

        existing.AssetId = workOrder.AssetId;
        existing.ProblemReportId = workOrder.ProblemReportId;
        existing.WoNumber = string.IsNullOrWhiteSpace(workOrder.WoNumber) ? existing.WoNumber : workOrder.WoNumber;
        existing.Title = workOrder.Title;
        existing.Description = workOrder.Description;
        existing.MaintenanceTypeId = workOrder.MaintenanceTypeId;
        existing.MaintenanceType = workOrder.MaintenanceType;
        existing.MaintenanceTypeDetail = null;
        existing.PriorityId = workOrder.PriorityId;
        existing.Priority = workOrder.Priority;
        existing.PriorityDetail = null;
        existing.StatusId = workOrder.StatusId;
        existing.Status = workOrder.Status;
        existing.StatusDetail = null;
        existing.ReportedBy = workOrder.ReportedBy;
        existing.AssignedTo = workOrder.AssignedTo;
        existing.ReportedAt = workOrder.ReportedAt;
        existing.ScheduledAt = workOrder.ScheduledAt;
        existing.StartedAt = workOrder.StartedAt;
        existing.CompletedAt = workOrder.CompletedAt;
        existing.ClosedAt = workOrder.ClosedAt;
        existing.DowntimeStart = workOrder.DowntimeStart;
        existing.DowntimeEnd = workOrder.DowntimeEnd;
        existing.RepairStart = workOrder.RepairStart;
        existing.RepairEnd = workOrder.RepairEnd;
        existing.FailureCode = workOrder.FailureCode;
        existing.RootCause = workOrder.RootCause;
        existing.ActionTaken = workOrder.ActionTaken;
        existing.Result = workOrder.Result;
        existing.UpdatedAt = DateTime.Now;

        if (existing.Status == WorkOrderStatus.COMPLETED && existing.DowntimeStart.HasValue && !existing.DowntimeEnd.HasValue)
        {
            existing.DowntimeEnd = existing.CompletedAt ?? DateTime.Now;
        }

        WorkOrderRules.CalculateWorkOrderDurations(existing);
        WorkOrderRules.ValidateWorkOrderForSave(existing);

        await Context.SaveChangesAsync();
        await SyncProblemReportStatusAsync(existing, previousProblemReportId);
        await SyncDowntimeLogAsync(existing);
        await LoadWorkOrderMastersAsync(existing);
        return existing;
    }

    public async Task<bool> DeleteWorkOrderAsync(int id)
    {
        var existing = await Context.WorkOrders.FirstOrDefaultAsync(x => x.Id == id);
        if (existing == null)
        {
            return false;
        }

        if (existing.ProblemReportId.HasValue)
        {
            var report = await Context.ProblemReports.FirstOrDefaultAsync(x => x.Id == existing.ProblemReportId.Value);
            if (report != null)
            {
                await CmmsMasterDataResolver.SetProblemReportStatusAsync(Context, report, ProblemReportStatus.PENDING);
                report.UpdatedAt = DateTime.Now;
            }
        }

        Context.WorkOrders.Remove(existing);
        await Context.SaveChangesAsync();
        return true;
    }

    public async Task<WorkOrder?> AssignWorkOrderAsync(int id, WorkOrderAssignRequest request)
    {
        var workOrder = await Context.WorkOrders
            .IncludeWorkOrderMasters()
            .FirstOrDefaultAsync(x => x.Id == id);
        if (workOrder == null)
        {
            return null;
        }

        WorkOrderRules.EnsureStatus(workOrder, WorkOrderStatus.OPEN, WorkOrderStatus.ASSIGNED);
        if (!await Context.Technicians.AnyAsync(x => x.Id == request.AssignedTo && x.Status == TechnicianStatus.ACTIVE))
        {
            throw new InvalidOperationException("Technician aktif tidak ditemukan.");
        }

        workOrder.AssignedTo = request.AssignedTo;
        workOrder.ScheduledAt = request.ScheduledAt ?? workOrder.ScheduledAt;
        await CmmsMasterDataResolver.SetWorkOrderStatusAsync(Context, workOrder, WorkOrderStatus.ASSIGNED);
        workOrder.UpdatedAt = DateTime.Now;
        await Context.SaveChangesAsync();
        await SyncProblemReportStatusAsync(workOrder);
        await LoadWorkOrderMastersAsync(workOrder);
        return workOrder;
    }

    public async Task<WorkOrder?> StartWorkOrderAsync(int id)
    {
        var workOrder = await Context.WorkOrders
            .IncludeWorkOrderMasters()
            .FirstOrDefaultAsync(x => x.Id == id);
        if (workOrder == null)
        {
            return null;
        }

        if (workOrder.Status != WorkOrderStatus.ASSIGNED)
        {
            throw new InvalidOperationException("Work order hanya bisa start dari status ASSIGNED.");
        }

        await CmmsMasterDataResolver.SetWorkOrderStatusAsync(Context, workOrder, WorkOrderStatus.IN_PROGRESS);
        workOrder.StartedAt ??= DateTime.Now;
        workOrder.RepairStart ??= workOrder.StartedAt;
        workOrder.UpdatedAt = DateTime.Now;
        await Context.SaveChangesAsync();
        await SyncProblemReportStatusAsync(workOrder);
        await LoadWorkOrderMastersAsync(workOrder);
        return workOrder;
    }

    public async Task<WorkOrder?> CompleteWorkOrderAsync(int id, WorkOrderCompleteRequest request)
    {
        var workOrder = await Context.WorkOrders
            .IncludeWorkOrderMasters()
            .FirstOrDefaultAsync(x => x.Id == id);
        if (workOrder == null)
        {
            return null;
        }

        if (workOrder.Status != WorkOrderStatus.IN_PROGRESS)
        {
            throw new InvalidOperationException("Work order hanya bisa complete dari status IN_PROGRESS.");
        }

        workOrder.CompletedAt = request.CompletedAt ?? DateTime.Now;
        workOrder.DowntimeStart = request.DowntimeStart ?? workOrder.DowntimeStart;
        workOrder.DowntimeEnd = request.DowntimeEnd ?? workOrder.DowntimeEnd;
        workOrder.RepairStart = request.RepairStart ?? workOrder.RepairStart ?? workOrder.StartedAt;
        workOrder.RepairEnd = request.RepairEnd ?? workOrder.RepairEnd ?? workOrder.CompletedAt;
        workOrder.FailureCode = string.IsNullOrWhiteSpace(request.FailureCode) ? workOrder.FailureCode : request.FailureCode;
        workOrder.RootCause = string.IsNullOrWhiteSpace(request.RootCause) ? workOrder.RootCause : request.RootCause;
        workOrder.ActionTaken = string.IsNullOrWhiteSpace(request.ActionTaken) ? workOrder.ActionTaken : request.ActionTaken;
        workOrder.Result = string.IsNullOrWhiteSpace(request.Result) ? workOrder.Result : request.Result;
        if (workOrder.DowntimeStart.HasValue && !workOrder.DowntimeEnd.HasValue)
        {
            workOrder.DowntimeEnd = workOrder.CompletedAt;
        }
        await CmmsMasterDataResolver.SetWorkOrderStatusAsync(Context, workOrder, WorkOrderStatus.COMPLETED);
        workOrder.UpdatedAt = DateTime.Now;

        WorkOrderRules.CalculateWorkOrderDurations(workOrder);
        WorkOrderRules.ValidateWorkOrderForSave(workOrder);
        await Context.SaveChangesAsync();
        await SyncProblemReportStatusAsync(workOrder);
        await SyncDowntimeLogAsync(workOrder);
        await LoadWorkOrderMastersAsync(workOrder);
        return workOrder;
    }

    public async Task<WorkOrder?> CloseWorkOrderAsync(int id, WorkOrderCloseRequest request)
    {
        var workOrder = await Context.WorkOrders
            .IncludeWorkOrderMasters()
            .FirstOrDefaultAsync(x => x.Id == id);
        if (workOrder == null)
        {
            return null;
        }

        if (workOrder.Status != WorkOrderStatus.COMPLETED)
        {
            throw new InvalidOperationException("Work order tidak boleh CLOSED jika belum COMPLETED.");
        }

        workOrder.FailureCode = string.IsNullOrWhiteSpace(request.FailureCode) ? workOrder.FailureCode : request.FailureCode;
        workOrder.RootCause = string.IsNullOrWhiteSpace(request.RootCause) ? workOrder.RootCause : request.RootCause;
        workOrder.ActionTaken = string.IsNullOrWhiteSpace(request.ActionTaken) ? workOrder.ActionTaken : request.ActionTaken;
        workOrder.Result = string.IsNullOrWhiteSpace(request.Result) ? workOrder.Result : request.Result;
        workOrder.ClosedAt = request.ClosedAt ?? DateTime.Now;
        await CmmsMasterDataResolver.SetWorkOrderStatusAsync(Context, workOrder, WorkOrderStatus.CLOSED);
        workOrder.UpdatedAt = DateTime.Now;

        WorkOrderRules.ValidateWorkOrderForSave(workOrder);
        await Context.SaveChangesAsync();
        await SyncProblemReportStatusAsync(workOrder);
        await LoadWorkOrderMastersAsync(workOrder);
        return workOrder;
    }

    public Task<List<WorkOrderPhoto>> GetWorkOrderPhotosAsync(int workOrderId)
    {
        return Context.WorkOrderPhotos
            .AsNoTracking()
            .Where(x => x.WorkOrderId == workOrderId)
            .OrderByDescending(x => x.UploadedAt)
            .Select(x => new WorkOrderPhoto
            {
                Id = x.Id,
                WorkOrderId = x.WorkOrderId,
                FileName = x.FileName,
                ContentType = x.ContentType,
                SizeBytes = x.SizeBytes,
                UploadedBy = x.UploadedBy,
                UploadedAt = x.UploadedAt
            })
            .ToListAsync();
    }

    public Task<WorkOrderPhoto?> GetWorkOrderPhotoAsync(int workOrderId, int photoId)
    {
        return Context.WorkOrderPhotos
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.WorkOrderId == workOrderId && x.Id == photoId);
    }

    public async Task<WorkOrderPhoto?> AddWorkOrderPhotoAsync(int workOrderId, WorkOrderPhoto photo)
    {
        if (!await Context.WorkOrders.AnyAsync(x => x.Id == workOrderId))
        {
            return null;
        }

        photo.Id = 0;
        photo.WorkOrderId = workOrderId;
        photo.UploadedAt = DateTime.Now;
        Context.WorkOrderPhotos.Add(photo);
        await Context.SaveChangesAsync();
        return photo;
    }

    public async Task<WorkOrderPhoto?> DeleteWorkOrderPhotoAsync(int workOrderId, int photoId)
    {
        var photo = await Context.WorkOrderPhotos.FirstOrDefaultAsync(x => x.WorkOrderId == workOrderId && x.Id == photoId);
        if (photo == null)
        {
            return null;
        }

        Context.WorkOrderPhotos.Remove(photo);
        await Context.SaveChangesAsync();
        return photo;
    }

    private async Task SyncDowntimeLogAsync(WorkOrder workOrder)
    {
        if (!workOrder.DowntimeStart.HasValue || !workOrder.DowntimeEnd.HasValue)
        {
            return;
        }

        var log = await Context.DowntimeLogs.FirstOrDefaultAsync(x =>
            x.WorkOrderId == workOrder.Id ||
            (workOrder.ProblemReportId.HasValue && x.ProblemReportId == workOrder.ProblemReportId.Value));
        if (log == null)
        {
            log = new DowntimeLog
            {
                AssetId = workOrder.AssetId,
                WorkOrderId = workOrder.Id,
                ProblemReportId = workOrder.ProblemReportId,
                DowntimeCategory = WorkOrderRules.MapDowntimeCategory(workOrder),
                Description = workOrder.Title,
                CreatedAt = DateTime.Now
            };
            Context.DowntimeLogs.Add(log);
        }

        log.AssetId = workOrder.AssetId;
        log.WorkOrderId = workOrder.Id;
        log.ProblemReportId = workOrder.ProblemReportId;
        log.DowntimeCategory = WorkOrderRules.MapDowntimeCategory(workOrder);
        log.StartTime = workOrder.DowntimeStart.Value;
        log.EndTime = workOrder.DowntimeEnd.Value;
        log.DurationMinutes = workOrder.DowntimeMinutes;
        log.Description = workOrder.Title;
        await CmmsMasterDataResolver.ApplyDowntimeLogMastersAsync(Context, log);
        await Context.SaveChangesAsync();
    }

    private async Task ValidateProblemReportReferenceAsync(int assetId, int? problemReportId)
    {
        if (!problemReportId.HasValue)
        {
            return;
        }

        var report = await Context.ProblemReports.AsNoTracking().FirstOrDefaultAsync(x => x.Id == problemReportId.Value);
        if (report == null)
        {
            throw new InvalidOperationException("Report problem tidak ditemukan.");
        }

        if (report.AssetId != assetId)
        {
            throw new InvalidOperationException("Asset work order harus sama dengan asset report problem.");
        }
    }

    private async Task SyncProblemReportStatusAsync(WorkOrder workOrder, int? previousProblemReportId = null)
    {
        if (previousProblemReportId.HasValue && previousProblemReportId != workOrder.ProblemReportId)
        {
            var previousReport = await Context.ProblemReports.FirstOrDefaultAsync(x => x.Id == previousProblemReportId.Value);
            if (previousReport != null)
            {
                await CmmsMasterDataResolver.SetProblemReportStatusAsync(Context, previousReport, ProblemReportStatus.PENDING);
                previousReport.UpdatedAt = DateTime.Now;
            }
        }

        if (!workOrder.ProblemReportId.HasValue)
        {
            await Context.SaveChangesAsync();
            return;
        }

        var report = await Context.ProblemReports
            .IncludeProblemReportMasters()
            .FirstOrDefaultAsync(x => x.Id == workOrder.ProblemReportId.Value);
        if (report == null)
        {
            return;
        }

        await CmmsMasterDataResolver.SetProblemReportStatusAsync(Context, report, MapProblemReportStatus(workOrder.Status));
        SyncProblemReportDowntime(report, workOrder);
        report.UpdatedAt = DateTime.Now;
        await Context.SaveChangesAsync();
    }

    private static void SyncProblemReportDowntime(ProblemReport report, WorkOrder workOrder)
    {
        if (report.Category != ProblemReportCategory.DOWNTIME || !workOrder.DowntimeStart.HasValue)
        {
            return;
        }

        report.DowntimeStart ??= workOrder.DowntimeStart;
        report.DowntimeEnd = workOrder.DowntimeEnd;

        if (report.DowntimeStart.HasValue && report.DowntimeEnd.HasValue)
        {
            report.DowntimeMinutes = Math.Max(0, (int)Math.Round((report.DowntimeEnd.Value - report.DowntimeStart.Value).TotalMinutes));
            return;
        }

        report.DowntimeMinutes = null;
    }

    private static ProblemReportStatus MapProblemReportStatus(WorkOrderStatus status)
    {
        return status switch
        {
            WorkOrderStatus.IN_PROGRESS => ProblemReportStatus.IN_PROGRESS,
            WorkOrderStatus.COMPLETED => ProblemReportStatus.COMPLETED,
            WorkOrderStatus.CLOSED => ProblemReportStatus.COMPLETED,
            WorkOrderStatus.CANCELLED => ProblemReportStatus.CANCELLED,
            _ => ProblemReportStatus.PENDING
        };
    }

    private async Task LoadWorkOrderMastersAsync(WorkOrder workOrder)
    {
        await Context.Entry(workOrder).Reference(x => x.MaintenanceTypeDetail).LoadAsync();
        await Context.Entry(workOrder).Reference(x => x.PriorityDetail).LoadAsync();
        await Context.Entry(workOrder).Reference(x => x.StatusDetail).LoadAsync();
    }
}
