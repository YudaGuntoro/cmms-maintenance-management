using Maintenance.Domain.Cmms;
using Maintenance.Infrastructure.Context;
using Maintenance.Repository.Shared;
using Microsoft.EntityFrameworkCore;

namespace Maintenance.Repository.ContractorMonitoringRepository;

public class ContractorMonitoringRepository : IContractorMonitoringRepository
{
    private readonly MaintenanceDbContext Context;

    public ContractorMonitoringRepository(MaintenanceDbContext context) => Context = context;

    public async Task<List<ContractorWorkPlan>> GetPlansAsync(ContractorWorkPlanFilter filter)
    {
        var query = Context.ContractorWorkPlans
            .AsNoTracking()
            .Include(x => x.Documents)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Vendor))
        {
            query = query.Where(x => x.VendorName.Contains(filter.Vendor));
        }

        if (!string.IsNullOrWhiteSpace(filter.Area))
        {
            query = query.Where(x => x.WorkArea.Contains(filter.Area) || x.DepartmentArea.Contains(filter.Area));
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(x => x.Status == filter.Status.Value);
        }

        if (filter.StartDate.HasValue)
        {
            var start = filter.StartDate.Value.Date;
            query = query.Where(x => x.EndAt >= start);
        }

        if (filter.EndDate.HasValue)
        {
            var endExclusive = filter.EndDate.Value.Date.AddDays(1);
            query = query.Where(x => x.StartAt < endExclusive);
        }

        if (!string.IsNullOrWhiteSpace(filter.PicMtc))
        {
            query = query.Where(x => x.InternalPicName.Contains(filter.PicMtc));
        }

        query = ApplyRiskFilter(query, filter.Risk);

        return await query
            .OrderBy(x => x.StartAt)
            .ThenBy(x => x.VendorName)
            .ToListAsync();
    }

    public Task<ContractorWorkPlan?> GetPlanAsync(int id)
    {
        return Context.ContractorWorkPlans
            .AsNoTracking()
            .Include(x => x.Documents)
            .Include(x => x.Audits.OrderByDescending(a => a.CreatedAt))
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<ContractorWorkPlan> CreatePlanAsync(ContractorWorkPlan plan, string? performedBy)
    {
        await ValidatePlanAsync(plan);
        plan.Id = 0;
        plan.CreatedBy = CleanUser(performedBy) ?? plan.CreatedBy;
        plan.UpdatedBy = plan.CreatedBy;
        plan.CreatedAt = DateTime.Now;
        plan.UpdatedAt = DateTime.Now;
        CalculateDuration(plan);

        Context.ContractorWorkPlans.Add(plan);
        await Context.SaveChangesAsync();
        AddAudit(plan.Id, "CREATE", null, null, $"Created contractor plan for {plan.VendorName}", performedBy);
        await Context.SaveChangesAsync();
        return plan;
    }

    public async Task<ContractorWorkPlan?> UpdatePlanAsync(int id, ContractorWorkPlan plan, string? performedBy)
    {
        var existing = await Context.ContractorWorkPlans.FirstOrDefaultAsync(x => x.Id == id);
        if (existing == null)
        {
            return null;
        }

        plan.AssetId = plan.AssetId == 0 ? null : plan.AssetId;
        plan.WorkOrderId = existing.WorkOrderId;
        await ValidatePlanAsync(plan);
        CalculateDuration(plan);

        TrackChange(existing.Id, "vendor_name", existing.VendorName, plan.VendorName, performedBy);
        TrackChange(existing.Id, "work_title", existing.WorkTitle, plan.WorkTitle, performedBy);
        TrackChange(existing.Id, "work_area", existing.WorkArea, plan.WorkArea, performedBy);
        TrackChange(existing.Id, "start_at", existing.StartAt, plan.StartAt, performedBy);
        TrackChange(existing.Id, "end_at", existing.EndAt, plan.EndAt, performedBy);
        TrackChange(existing.Id, "status", existing.Status, plan.Status, performedBy);
        TrackChange(existing.Id, "permit_document_status", existing.PermitDocumentStatus, plan.PermitDocumentStatus, performedBy);

        existing.VendorName = plan.VendorName.Trim();
        existing.VendorPicName = plan.VendorPicName.Trim();
        existing.VendorPicPhone = Clean(plan.VendorPicPhone);
        existing.WorkerCount = plan.WorkerCount;
        existing.InternalPicName = plan.InternalPicName.Trim();
        existing.DepartmentArea = plan.DepartmentArea.Trim();
        existing.WorkTitle = plan.WorkTitle.Trim();
        existing.WorkDescription = Clean(plan.WorkDescription);
        existing.WorkArea = plan.WorkArea.Trim();
        existing.WorkLocation = Clean(plan.WorkLocation);
        existing.AssetId = plan.AssetId;
        existing.AdditionalNotes = Clean(plan.AdditionalNotes);
        existing.StartAt = plan.StartAt;
        existing.EndAt = plan.EndAt;
        existing.EstimatedDurationMinutes = plan.EstimatedDurationMinutes;
        existing.Status = plan.Status;
        existing.PermitDocumentStatus = plan.PermitDocumentStatus;
        existing.WorkingAtHeight = plan.WorkingAtHeight;
        existing.HotWork = plan.HotWork;
        existing.Welding = plan.Welding;
        existing.ElectricalWork = plan.ElectricalWork;
        existing.ConfinedSpace = plan.ConfinedSpace;
        existing.HeavyEquipmentActivity = plan.HeavyEquipmentActivity;
        existing.ChemicalHandling = plan.ChemicalHandling;
        existing.ShutdownActivity = plan.ShutdownActivity;
        existing.LotoRequired = plan.LotoRequired;
        existing.NeedSafetyStandby = plan.NeedSafetyStandby;
        existing.UpdatedBy = CleanUser(performedBy);
        existing.UpdatedAt = DateTime.Now;

        await Context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeletePlanAsync(int id)
    {
        var existing = await Context.ContractorWorkPlans.FirstOrDefaultAsync(x => x.Id == id);
        if (existing == null)
        {
            return false;
        }

        Context.ContractorWorkPlans.Remove(existing);
        await Context.SaveChangesAsync();
        return true;
    }

    public Task<ContractorWorkDocument?> GetDocumentAsync(int planId, int documentId)
    {
        return Context.ContractorWorkDocuments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ContractorWorkPlanId == planId && x.Id == documentId);
    }

    public async Task<ContractorWorkDocument?> AddDocumentAsync(int planId, ContractorWorkDocument document, string? performedBy)
    {
        var plan = await Context.ContractorWorkPlans.FirstOrDefaultAsync(x => x.Id == planId);
        if (plan == null)
        {
            return null;
        }

        document.Id = 0;
        document.ContractorWorkPlanId = planId;
        document.FileName = Path.GetFileName(document.FileName);
        document.UploadedBy = CleanUser(performedBy);
        document.UploadedAt = DateTime.Now;
        if (document.ExpiresAt.HasValue && document.ExpiresAt.Value.Date < DateTime.Today)
        {
            document.DocumentStatus = ContractorDocumentStatus.EXPIRED;
        }

        Context.ContractorWorkDocuments.Add(document);

        if (document.DocumentType == ContractorDocumentType.PERMIT)
        {
            TrackChange(plan.Id, "permit_document_status", plan.PermitDocumentStatus, document.DocumentStatus, performedBy);
            plan.PermitDocumentStatus = document.DocumentStatus;
            plan.PermitUploadedBy = document.UploadedBy;
            plan.PermitUploadedAt = document.UploadedAt;
        }

        plan.UpdatedBy = CleanUser(performedBy);
        plan.UpdatedAt = DateTime.Now;
        AddAudit(plan.Id, "UPLOAD_DOCUMENT", "document", null, document.FileName, performedBy);
        await Context.SaveChangesAsync();
        return document;
    }

    public async Task<bool> DeleteDocumentAsync(int planId, int documentId, string? performedBy)
    {
        var document = await Context.ContractorWorkDocuments.FirstOrDefaultAsync(x => x.ContractorWorkPlanId == planId && x.Id == documentId);
        if (document == null)
        {
            return false;
        }

        var plan = await Context.ContractorWorkPlans.FirstOrDefaultAsync(x => x.Id == planId);
        Context.ContractorWorkDocuments.Remove(document);

        if (plan != null)
        {
            AddAudit(plan.Id, "DELETE_DOCUMENT", "document", document.FileName, null, performedBy);
            plan.UpdatedBy = CleanUser(performedBy);
            plan.UpdatedAt = DateTime.Now;
        }

        await Context.SaveChangesAsync();
        return true;
    }

    public async Task<List<ContractorWorkReminder>> GetRemindersAsync(DateTime now)
    {
        var activePlans = await Context.ContractorWorkPlans
            .AsNoTracking()
            .Where(x => x.Status != ContractorWorkStatus.FINISHED && x.Status != ContractorWorkStatus.CANCELLED)
            .OrderBy(x => x.StartAt)
            .ToListAsync();

        var reminders = new List<ContractorWorkReminder>();
        foreach (var plan in activePlans)
        {
            if (plan.CreatedAt >= now.AddDays(-1))
            {
                reminders.Add(BuildReminder(plan, "NEW_PLAN", "INFO", "Rencana pekerjaan vendor baru sudah tercatat."));
            }

            if (plan.StartAt > now && plan.StartAt <= now.AddHours(24))
            {
                reminders.Add(BuildReminder(plan, "STARTING_SOON", "WARNING", "Pekerjaan vendor akan dimulai dalam 24 jam."));
            }

            if (plan.PermitDocumentStatus is ContractorDocumentStatus.NOT_UPLOADED or ContractorDocumentStatus.NEED_REVISION or ContractorDocumentStatus.EXPIRED)
            {
                reminders.Add(BuildReminder(plan, "PERMIT_ATTENTION", "WARNING", $"Status permit: {plan.PermitDocumentStatus}."));
            }

            if (IsHighRisk(plan))
            {
                reminders.Add(BuildReminder(plan, "HIGH_RISK", "DANGER", $"High-risk activity: {BuildRiskText(plan)}."));
            }

            if (plan.EndAt < now && plan.Status is not ContractorWorkStatus.FINISHED and not ContractorWorkStatus.EXPIRED)
            {
                reminders.Add(BuildReminder(plan, "OVERDUE_STATUS", "DANGER", "Jadwal pekerjaan sudah lewat, status belum diperbarui."));
            }
        }

        return reminders
            .OrderByDescending(x => x.Severity == "DANGER")
            .ThenByDescending(x => x.Severity == "WARNING")
            .ThenBy(x => x.StartAt)
            .ToList();
    }

    public async Task<WorkOrder?> CreateSupervisionWorkOrderAsync(int planId, string? performedBy)
    {
        var plan = await Context.ContractorWorkPlans.FirstOrDefaultAsync(x => x.Id == planId);
        if (plan == null)
        {
            return null;
        }

        if (plan.WorkOrderId.HasValue)
        {
            return await Context.WorkOrders
                .AsNoTracking()
                .IncludeWorkOrderMasters()
                .FirstOrDefaultAsync(x => x.Id == plan.WorkOrderId.Value);
        }

        if (!plan.AssetId.HasValue)
        {
            throw new InvalidOperationException("Asset wajib dipilih sebelum membuat WO Contractor Supervision.");
        }

        if (!await Context.Assets.AnyAsync(x => x.Id == plan.AssetId.Value))
        {
            throw new InvalidOperationException("Asset tidak ditemukan.");
        }

        var workOrder = new WorkOrder
        {
            AssetId = plan.AssetId.Value,
            WoNumber = await WorkOrderRules.GenerateWorkOrderNumberAsync(Context, MaintenanceType.CONTRACTOR_SUPERVISION),
            Title = $"Contractor Supervision - {plan.WorkTitle}",
            Description = BuildWorkOrderDescription(plan),
            MaintenanceType = MaintenanceType.CONTRACTOR_SUPERVISION,
            Priority = IsHighRisk(plan) ? WorkOrderPriority.HIGH : WorkOrderPriority.MEDIUM,
            Status = WorkOrderStatus.OPEN,
            ReportedBy = CleanUser(performedBy) ?? plan.InternalPicName,
            ReportedAt = DateTime.Now,
            ScheduledAt = plan.StartAt,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        await CmmsMasterDataResolver.ApplyWorkOrderMastersAsync(Context, workOrder);
        Context.WorkOrders.Add(workOrder);
        await Context.SaveChangesAsync();
        await Context.Entry(workOrder).Reference(x => x.MaintenanceTypeDetail).LoadAsync();
        await Context.Entry(workOrder).Reference(x => x.PriorityDetail).LoadAsync();
        await Context.Entry(workOrder).Reference(x => x.StatusDetail).LoadAsync();

        plan.WorkOrderId = workOrder.Id;
        plan.UpdatedBy = CleanUser(performedBy);
        plan.UpdatedAt = DateTime.Now;
        AddAudit(plan.Id, "CREATE_SUPERVISION_WORK_ORDER", "work_order_id", null, workOrder.WoNumber, performedBy);
        await Context.SaveChangesAsync();
        return workOrder;
    }

    private async Task ValidatePlanAsync(ContractorWorkPlan plan)
    {
        if (string.IsNullOrWhiteSpace(plan.VendorName))
        {
            throw new InvalidOperationException("Nama vendor wajib diisi.");
        }

        if (string.IsNullOrWhiteSpace(plan.VendorPicName))
        {
            throw new InvalidOperationException("PIC vendor wajib diisi.");
        }

        if (string.IsNullOrWhiteSpace(plan.InternalPicName))
        {
            throw new InvalidOperationException("PIC MTC wajib diisi.");
        }

        if (string.IsNullOrWhiteSpace(plan.DepartmentArea))
        {
            throw new InvalidOperationException("Departemen / area terkait wajib diisi.");
        }

        if (string.IsNullOrWhiteSpace(plan.WorkTitle))
        {
            throw new InvalidOperationException("Judul pekerjaan wajib diisi.");
        }

        if (string.IsNullOrWhiteSpace(plan.WorkArea))
        {
            throw new InvalidOperationException("Area kerja wajib diisi.");
        }

        if (plan.WorkerCount < 0)
        {
            throw new InvalidOperationException("Jumlah pekerja tidak boleh minus.");
        }

        if (plan.EndAt <= plan.StartAt)
        {
            throw new InvalidOperationException("Tanggal selesai harus lebih besar dari tanggal mulai.");
        }

        if (plan.AssetId.HasValue && !await Context.Assets.AnyAsync(x => x.Id == plan.AssetId.Value))
        {
            throw new InvalidOperationException("Asset tidak ditemukan.");
        }
    }

    private static void CalculateDuration(ContractorWorkPlan plan)
    {
        plan.EstimatedDurationMinutes = Math.Max(0, (int)Math.Round((plan.EndAt - plan.StartAt).TotalMinutes, MidpointRounding.AwayFromZero));
    }

    private static IQueryable<ContractorWorkPlan> ApplyRiskFilter(IQueryable<ContractorWorkPlan> query, string? risk)
    {
        return risk?.Trim().ToLowerInvariant() switch
        {
            "working_at_height" => query.Where(x => x.WorkingAtHeight),
            "hot_work" => query.Where(x => x.HotWork),
            "welding" => query.Where(x => x.Welding),
            "electrical_work" => query.Where(x => x.ElectricalWork),
            "confined_space" => query.Where(x => x.ConfinedSpace),
            "heavy_equipment_activity" => query.Where(x => x.HeavyEquipmentActivity),
            "chemical_handling" => query.Where(x => x.ChemicalHandling),
            "shutdown_activity" => query.Where(x => x.ShutdownActivity),
            "loto_required" => query.Where(x => x.LotoRequired),
            "need_safety_standby" => query.Where(x => x.NeedSafetyStandby),
            "high_risk" => query.Where(x => x.WorkingAtHeight || x.HotWork || x.Welding || x.ElectricalWork || x.ConfinedSpace || x.HeavyEquipmentActivity || x.ChemicalHandling || x.ShutdownActivity || x.LotoRequired || x.NeedSafetyStandby),
            _ => query
        };
    }

    private static bool IsHighRisk(ContractorWorkPlan plan)
    {
        return plan.WorkingAtHeight || plan.HotWork || plan.Welding || plan.ElectricalWork ||
            plan.ConfinedSpace || plan.HeavyEquipmentActivity || plan.ChemicalHandling ||
            plan.ShutdownActivity || plan.LotoRequired || plan.NeedSafetyStandby;
    }

    private static string BuildRiskText(ContractorWorkPlan plan)
    {
        var risks = plan.RiskTags;
        return risks.Count == 0 ? "-" : string.Join(", ", risks);
    }

    private static string BuildWorkOrderDescription(ContractorWorkPlan plan)
    {
        return string.Join(Environment.NewLine, new[]
        {
            $"Vendor: {plan.VendorName}",
            $"PIC Vendor: {plan.VendorPicName} ({plan.VendorPicPhone ?? "-"})",
            $"PIC MTC: {plan.InternalPicName}",
            $"Area: {plan.WorkArea}",
            $"Schedule: {plan.StartAt:dd MMM yyyy HH:mm} - {plan.EndAt:dd MMM yyyy HH:mm}",
            $"Permit: {plan.PermitDocumentStatus}",
            $"Risks: {BuildRiskText(plan)}",
            "",
            plan.WorkDescription ?? string.Empty,
            plan.AdditionalNotes ?? string.Empty
        }.Where(x => x.Length > 0));
    }

    private static ContractorWorkReminder BuildReminder(ContractorWorkPlan plan, string type, string severity, string message)
    {
        return new ContractorWorkReminder
        {
            ContractorWorkPlanId = plan.Id,
            VendorName = plan.VendorName,
            WorkTitle = plan.WorkTitle,
            WorkArea = plan.WorkArea,
            StartAt = plan.StartAt,
            Type = type,
            Severity = severity,
            Message = message
        };
    }

    private void AddAudit(int planId, string action, string? fieldName, string? oldValue, string? newValue, string? performedBy)
    {
        Context.ContractorWorkAudits.Add(new ContractorWorkAudit
        {
            ContractorWorkPlanId = planId,
            Action = action,
            FieldName = fieldName,
            OldValue = oldValue,
            NewValue = newValue,
            PerformedBy = CleanUser(performedBy),
            CreatedAt = DateTime.Now
        });
    }

    private void TrackChange<T>(int planId, string fieldName, T oldValue, T newValue, string? performedBy)
    {
        if (EqualityComparer<T>.Default.Equals(oldValue, newValue))
        {
            return;
        }

        AddAudit(planId, "UPDATE", fieldName, oldValue?.ToString(), newValue?.ToString(), performedBy);
    }

    private static string? Clean(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string? CleanUser(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
