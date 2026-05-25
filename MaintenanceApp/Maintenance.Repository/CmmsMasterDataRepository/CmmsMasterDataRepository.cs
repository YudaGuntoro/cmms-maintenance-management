using Maintenance.Domain.Cmms;
using Maintenance.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Maintenance.Repository.CmmsMasterDataRepository;

public class CmmsMasterDataRepository : ICmmsMasterDataRepository
{
    private readonly MaintenanceDbContext Context;

    public CmmsMasterDataRepository(MaintenanceDbContext context) => Context = context;

    public Task<List<MaintenanceTypeMaster>> GetMaintenanceTypesAsync()
    {
        return Context.MaintenanceTypes.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync();
    }

    public Task<MaintenanceTypeMaster?> GetMaintenanceTypeAsync(int id)
    {
        return Context.MaintenanceTypes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<MaintenanceTypeMaster> CreateMaintenanceTypeAsync(MaintenanceTypeMaster item)
    {
        await ValidateCodeAsync(Context.MaintenanceTypes, item.Code);
        Normalize(item);
        Context.MaintenanceTypes.Add(item);
        await Context.SaveChangesAsync();
        return item;
    }

    public async Task<MaintenanceTypeMaster?> UpdateMaintenanceTypeAsync(int id, MaintenanceTypeMaster item)
    {
        var existing = await Context.MaintenanceTypes.FirstOrDefaultAsync(x => x.Id == id);
        if (existing == null)
        {
            return null;
        }

        await ValidateCodeAsync(Context.MaintenanceTypes, item.Code, id);
        CopyBase(existing, item);
        await Context.SaveChangesAsync();
        return existing;
    }

    public Task<bool> DeleteMaintenanceTypeAsync(int id) => SoftDeleteAsync(Context.MaintenanceTypes, id);

    public Task<List<WorkOrderPriorityMaster>> GetWorkOrderPrioritiesAsync()
    {
        return Context.WorkOrderPriorities.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Level).ToListAsync();
    }

    public Task<WorkOrderPriorityMaster?> GetWorkOrderPriorityAsync(int id)
    {
        return Context.WorkOrderPriorities.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<WorkOrderPriorityMaster> CreateWorkOrderPriorityAsync(WorkOrderPriorityMaster item)
    {
        await ValidateCodeAsync(Context.WorkOrderPriorities, item.Code);
        Normalize(item);
        item.Level = item.Level <= 0 ? 1 : item.Level;
        Context.WorkOrderPriorities.Add(item);
        await Context.SaveChangesAsync();
        return item;
    }

    public async Task<WorkOrderPriorityMaster?> UpdateWorkOrderPriorityAsync(int id, WorkOrderPriorityMaster item)
    {
        var existing = await Context.WorkOrderPriorities.FirstOrDefaultAsync(x => x.Id == id);
        if (existing == null)
        {
            return null;
        }

        await ValidateCodeAsync(Context.WorkOrderPriorities, item.Code, id);
        CopyBase(existing, item);
        existing.Level = item.Level <= 0 ? existing.Level : item.Level;
        await Context.SaveChangesAsync();
        return existing;
    }

    public Task<bool> DeleteWorkOrderPriorityAsync(int id) => SoftDeleteAsync(Context.WorkOrderPriorities, id);

    public Task<List<WorkOrderStatusMaster>> GetWorkOrderStatusesAsync()
    {
        return Context.WorkOrderStatuses.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Sequence).ToListAsync();
    }

    public Task<WorkOrderStatusMaster?> GetWorkOrderStatusAsync(int id)
    {
        return Context.WorkOrderStatuses.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<WorkOrderStatusMaster> CreateWorkOrderStatusAsync(WorkOrderStatusMaster item)
    {
        await ValidateCodeAsync(Context.WorkOrderStatuses, item.Code);
        Normalize(item);
        item.Sequence = item.Sequence <= 0 ? 1 : item.Sequence;
        Context.WorkOrderStatuses.Add(item);
        await Context.SaveChangesAsync();
        return item;
    }

    public async Task<WorkOrderStatusMaster?> UpdateWorkOrderStatusAsync(int id, WorkOrderStatusMaster item)
    {
        var existing = await Context.WorkOrderStatuses.FirstOrDefaultAsync(x => x.Id == id);
        if (existing == null)
        {
            return null;
        }

        await ValidateCodeAsync(Context.WorkOrderStatuses, item.Code, id);
        CopyBase(existing, item);
        existing.Sequence = item.Sequence <= 0 ? existing.Sequence : item.Sequence;
        await Context.SaveChangesAsync();
        return existing;
    }

    public Task<bool> DeleteWorkOrderStatusAsync(int id) => SoftDeleteAsync(Context.WorkOrderStatuses, id);

    public Task<List<DowntimeCategoryMaster>> GetDowntimeCategoriesAsync()
    {
        return Context.DowntimeCategories.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync();
    }

    public Task<DowntimeCategoryMaster?> GetDowntimeCategoryAsync(int id)
    {
        return Context.DowntimeCategories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<DowntimeCategoryMaster> CreateDowntimeCategoryAsync(DowntimeCategoryMaster item)
    {
        await ValidateCodeAsync(Context.DowntimeCategories, item.Code);
        Normalize(item);
        Context.DowntimeCategories.Add(item);
        await Context.SaveChangesAsync();
        return item;
    }

    public async Task<DowntimeCategoryMaster?> UpdateDowntimeCategoryAsync(int id, DowntimeCategoryMaster item)
    {
        var existing = await Context.DowntimeCategories.FirstOrDefaultAsync(x => x.Id == id);
        if (existing == null)
        {
            return null;
        }

        await ValidateCodeAsync(Context.DowntimeCategories, item.Code, id);
        CopyBase(existing, item);
        await Context.SaveChangesAsync();
        return existing;
    }

    public Task<bool> DeleteDowntimeCategoryAsync(int id) => SoftDeleteAsync(Context.DowntimeCategories, id);

    public Task<List<ProblemReportCategoryMaster>> GetProblemReportCategoriesAsync()
    {
        return Context.ProblemReportCategories.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync();
    }

    public Task<ProblemReportCategoryMaster?> GetProblemReportCategoryAsync(int id)
    {
        return Context.ProblemReportCategories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<ProblemReportCategoryMaster> CreateProblemReportCategoryAsync(ProblemReportCategoryMaster item)
    {
        await ValidateCodeAsync(Context.ProblemReportCategories, item.Code);
        Normalize(item);
        Context.ProblemReportCategories.Add(item);
        await Context.SaveChangesAsync();
        return item;
    }

    public async Task<ProblemReportCategoryMaster?> UpdateProblemReportCategoryAsync(int id, ProblemReportCategoryMaster item)
    {
        var existing = await Context.ProblemReportCategories.FirstOrDefaultAsync(x => x.Id == id);
        if (existing == null)
        {
            return null;
        }

        await ValidateCodeAsync(Context.ProblemReportCategories, item.Code, id);
        CopyBase(existing, item);
        await Context.SaveChangesAsync();
        return existing;
    }

    public Task<bool> DeleteProblemReportCategoryAsync(int id) => SoftDeleteAsync(Context.ProblemReportCategories, id);

    public Task<List<PreventiveScheduleTypeMaster>> GetPreventiveScheduleTypesAsync()
    {
        return Context.PreventiveScheduleTypes.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Id).ToListAsync();
    }

    public Task<PreventiveScheduleTypeMaster?> GetPreventiveScheduleTypeAsync(int id)
    {
        return Context.PreventiveScheduleTypes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<PreventiveScheduleTypeMaster> CreatePreventiveScheduleTypeAsync(PreventiveScheduleTypeMaster item)
    {
        await ValidateCodeAsync(Context.PreventiveScheduleTypes, item.Code);
        Normalize(item);
        Context.PreventiveScheduleTypes.Add(item);
        await Context.SaveChangesAsync();
        return item;
    }

    public async Task<PreventiveScheduleTypeMaster?> UpdatePreventiveScheduleTypeAsync(int id, PreventiveScheduleTypeMaster item)
    {
        var existing = await Context.PreventiveScheduleTypes.FirstOrDefaultAsync(x => x.Id == id);
        if (existing == null)
        {
            return null;
        }

        await ValidateCodeAsync(Context.PreventiveScheduleTypes, item.Code, id);
        CopyBase(existing, item);
        await Context.SaveChangesAsync();
        return existing;
    }

    public Task<bool> DeletePreventiveScheduleTypeAsync(int id) => SoftDeleteAsync(Context.PreventiveScheduleTypes, id);

    public Task<List<FrequencyTypeMaster>> GetFrequencyTypesAsync()
    {
        return Context.FrequencyTypes.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.IntervalDays).ToListAsync();
    }

    public Task<FrequencyTypeMaster?> GetFrequencyTypeAsync(int id)
    {
        return Context.FrequencyTypes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<FrequencyTypeMaster> CreateFrequencyTypeAsync(FrequencyTypeMaster item)
    {
        await ValidateCodeAsync(Context.FrequencyTypes, item.Code);
        Normalize(item);
        item.IntervalDays = item.IntervalDays <= 0 ? 1 : item.IntervalDays;
        Context.FrequencyTypes.Add(item);
        await Context.SaveChangesAsync();
        return item;
    }

    public async Task<FrequencyTypeMaster?> UpdateFrequencyTypeAsync(int id, FrequencyTypeMaster item)
    {
        var existing = await Context.FrequencyTypes.FirstOrDefaultAsync(x => x.Id == id);
        if (existing == null)
        {
            return null;
        }

        await ValidateCodeAsync(Context.FrequencyTypes, item.Code, id);
        CopyBase(existing, item);
        existing.IntervalDays = item.IntervalDays <= 0 ? existing.IntervalDays : item.IntervalDays;
        await Context.SaveChangesAsync();
        return existing;
    }

    public Task<bool> DeleteFrequencyTypeAsync(int id) => SoftDeleteAsync(Context.FrequencyTypes, id);

    private static void Normalize(MaintenanceTypeMaster item) => NormalizeBase(item);
    private static void Normalize(WorkOrderPriorityMaster item) => NormalizeBase(item);
    private static void Normalize(WorkOrderStatusMaster item) => NormalizeBase(item);
    private static void Normalize(DowntimeCategoryMaster item) => NormalizeBase(item);
    private static void Normalize(ProblemReportCategoryMaster item) => NormalizeBase(item);
    private static void Normalize(PreventiveScheduleTypeMaster item) => NormalizeBase(item);
    private static void Normalize(FrequencyTypeMaster item) => NormalizeBase(item);

    private static void CopyBase(MaintenanceTypeMaster target, MaintenanceTypeMaster source) => CopyBaseValues(target, source);
    private static void CopyBase(WorkOrderPriorityMaster target, WorkOrderPriorityMaster source) => CopyBaseValues(target, source);
    private static void CopyBase(WorkOrderStatusMaster target, WorkOrderStatusMaster source) => CopyBaseValues(target, source);
    private static void CopyBase(DowntimeCategoryMaster target, DowntimeCategoryMaster source) => CopyBaseValues(target, source);
    private static void CopyBase(ProblemReportCategoryMaster target, ProblemReportCategoryMaster source) => CopyBaseValues(target, source);
    private static void CopyBase(PreventiveScheduleTypeMaster target, PreventiveScheduleTypeMaster source) => CopyBaseValues(target, source);
    private static void CopyBase(FrequencyTypeMaster target, FrequencyTypeMaster source) => CopyBaseValues(target, source);

    private static void NormalizeBase(dynamic item)
    {
        if (string.IsNullOrWhiteSpace(item.Code))
        {
            throw new InvalidOperationException("Code wajib diisi.");
        }

        if (string.IsNullOrWhiteSpace(item.Name))
        {
            throw new InvalidOperationException("Name wajib diisi.");
        }

        item.Id = 0;
        item.Code = NormalizeCode(item.Code);
        item.Name = item.Name.Trim();
        item.IsActive = true;
        item.CreatedAt = DateTime.Now;
    }

    private static void CopyBaseValues(dynamic target, dynamic source)
    {
        if (string.IsNullOrWhiteSpace(source.Code))
        {
            throw new InvalidOperationException("Code wajib diisi.");
        }

        if (string.IsNullOrWhiteSpace(source.Name))
        {
            throw new InvalidOperationException("Name wajib diisi.");
        }

        target.Code = NormalizeCode(source.Code);
        target.Name = source.Name.Trim();
        target.IsActive = source.IsActive;
    }

    private static string NormalizeCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new InvalidOperationException("Code wajib diisi.");
        }

        return code.Trim().Replace(" ", "_").ToUpperInvariant();
    }

    private static async Task ValidateCodeAsync<TEntity>(DbSet<TEntity> set, string code, int? excludeId = null)
        where TEntity : class
    {
        var normalizedCode = NormalizeCode(code);
        var exists = await set.AsNoTracking().AnyAsync(x =>
            EF.Property<string>(x, nameof(MaintenanceTypeMaster.Code)) == normalizedCode &&
            (!excludeId.HasValue || EF.Property<int>(x, nameof(MaintenanceTypeMaster.Id)) != excludeId.Value));

        if (exists)
        {
            throw new InvalidOperationException($"Code {normalizedCode} sudah digunakan.");
        }
    }

    private async Task<bool> SoftDeleteAsync<TEntity>(DbSet<TEntity> set, int id)
        where TEntity : class
    {
        var existing = await set.FirstOrDefaultAsync(x => EF.Property<int>(x, nameof(MaintenanceTypeMaster.Id)) == id);
        if (existing == null)
        {
            return false;
        }

        typeof(TEntity).GetProperty(nameof(MaintenanceTypeMaster.IsActive))?.SetValue(existing, false);
        await Context.SaveChangesAsync();
        return true;
    }
}
