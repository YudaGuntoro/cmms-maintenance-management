using Maintenance.Domain.Cmms;
using Maintenance.Infrastructure.Context;
using Maintenance.Repository.Shared;
using Microsoft.EntityFrameworkCore;

namespace Maintenance.Repository.PreventiveScheduleRepository;

public class PreventiveScheduleRepository : IPreventiveScheduleRepository
{
    private readonly MaintenanceDbContext Context;

    public PreventiveScheduleRepository(MaintenanceDbContext context) => Context = context;

    public async Task<List<PreventiveSchedule>> GetPreventiveSchedulesAsync(bool? isActive)
    {
        var query = Context.PreventiveSchedules
            .AsNoTracking()
            .IncludePreventiveScheduleMasters()
            .AsQueryable();
        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        return await query.OrderBy(x => x.NextDueDate).ToListAsync();
    }

    public Task<PreventiveSchedule?> GetPreventiveScheduleAsync(int id)
    {
        return Context.PreventiveSchedules
            .AsNoTracking()
            .IncludePreventiveScheduleMasters()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<PreventiveSchedule> CreatePreventiveScheduleAsync(PreventiveSchedule schedule)
    {
        if (!await Context.Assets.AnyAsync(x => x.Id == schedule.AssetId))
        {
            throw new InvalidOperationException("Asset tidak ditemukan.");
        }

        await CmmsMasterDataResolver.ApplyPreventiveScheduleMastersAsync(Context, schedule);
        schedule.Id = 0;
        schedule.CreatedAt = DateTime.Now;
        schedule.UpdatedAt = DateTime.Now;
        Context.PreventiveSchedules.Add(schedule);
        await Context.SaveChangesAsync();
        await LoadPreventiveScheduleMastersAsync(schedule);
        return schedule;
    }

    public async Task<PreventiveSchedule?> UpdatePreventiveScheduleAsync(int id, PreventiveSchedule schedule)
    {
        var existing = await Context.PreventiveSchedules.FirstOrDefaultAsync(x => x.Id == id);
        if (existing == null)
        {
            return null;
        }

        existing.AssetId = schedule.AssetId;
        existing.ScheduleName = schedule.ScheduleName;
        await CmmsMasterDataResolver.ApplyPreventiveScheduleMastersAsync(Context, schedule);
        existing.ScheduleTypeId = schedule.ScheduleTypeId;
        existing.ScheduleType = schedule.ScheduleType;
        existing.ScheduleTypeDetail = null;
        existing.FrequencyTypeId = schedule.FrequencyTypeId;
        existing.FrequencyType = schedule.FrequencyType;
        existing.FrequencyTypeDetail = null;
        existing.FrequencyValue = schedule.FrequencyValue;
        existing.NextDueDate = schedule.NextDueDate;
        existing.LastGeneratedAt = schedule.LastGeneratedAt;
        existing.EstimatedDurationMinutes = schedule.EstimatedDurationMinutes;
        existing.ChecklistTemplate = schedule.ChecklistTemplate;
        existing.IsActive = schedule.IsActive;
        existing.UpdatedAt = DateTime.Now;
        await Context.SaveChangesAsync();
        await LoadPreventiveScheduleMastersAsync(existing);
        return existing;
    }

    public async Task<bool> DeletePreventiveScheduleAsync(int id)
    {
        var existing = await Context.PreventiveSchedules.FirstOrDefaultAsync(x => x.Id == id);
        if (existing == null)
        {
            return false;
        }

        Context.PreventiveSchedules.Remove(existing);
        await Context.SaveChangesAsync();
        return true;
    }

    public async Task<List<WorkOrder>> GenerateDuePreventiveWorkOrdersAsync(DateTime? dueDate)
    {
        var today = (dueDate ?? DateTime.Now).Date;
        var schedules = await Context.PreventiveSchedules
            .IncludePreventiveScheduleMasters()
            .Where(x => x.IsActive && x.NextDueDate.Date <= today)
            .OrderBy(x => x.NextDueDate)
            .ToListAsync();

        var generated = new List<WorkOrder>();

        foreach (var schedule in schedules)
        {
            var periodKey = WorkOrderRules.BuildPreventivePeriodKey(schedule);
            var exists = await Context.WorkOrders.AnyAsync(x =>
                x.PreventiveScheduleId == schedule.Id &&
                x.PreventiveSchedulePeriodKey == periodKey);

            if (!exists)
            {
                var workOrder = new WorkOrder
                {
                    WoNumber = await WorkOrderRules.GenerateWorkOrderNumberAsync(Context, MaintenanceType.PREVENTIVE),
                    AssetId = schedule.AssetId,
                    Title = schedule.ScheduleName,
                    Description = schedule.ChecklistTemplate,
                    MaintenanceType = MaintenanceType.PREVENTIVE,
                    Priority = WorkOrderPriority.MEDIUM,
                    Status = WorkOrderStatus.OPEN,
                    ReportedBy = "SYSTEM",
                    ReportedAt = DateTime.Now,
                    ScheduledAt = schedule.NextDueDate,
                    PreventiveScheduleId = schedule.Id,
                    PreventiveSchedulePeriodKey = periodKey,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                await CmmsMasterDataResolver.ApplyWorkOrderMastersAsync(Context, workOrder);
                Context.WorkOrders.Add(workOrder);
                generated.Add(workOrder);
            }

            schedule.LastGeneratedAt = DateTime.Now;
            schedule.NextDueDate = WorkOrderRules.CalculateNextDueDate(schedule);
            schedule.UpdatedAt = DateTime.Now;
        }

        await Context.SaveChangesAsync();
        foreach (var workOrder in generated)
        {
            await Context.Entry(workOrder).Reference(x => x.MaintenanceTypeDetail).LoadAsync();
            await Context.Entry(workOrder).Reference(x => x.PriorityDetail).LoadAsync();
            await Context.Entry(workOrder).Reference(x => x.StatusDetail).LoadAsync();
        }

        return generated;
    }

    private async Task LoadPreventiveScheduleMastersAsync(PreventiveSchedule schedule)
    {
        await Context.Entry(schedule).Reference(x => x.ScheduleTypeDetail).LoadAsync();
        await Context.Entry(schedule).Reference(x => x.FrequencyTypeDetail).LoadAsync();
    }
}
