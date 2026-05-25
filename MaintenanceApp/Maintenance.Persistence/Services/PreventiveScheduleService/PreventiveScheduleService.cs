using Maintenance.Domain.Cmms;
using Maintenance.Repository.PreventiveScheduleRepository;

namespace Maintenance.Persistence.Services.PreventiveScheduleService;

public class PreventiveScheduleService : IPreventiveScheduleService
{
    protected readonly IPreventiveScheduleRepository Context;

    public PreventiveScheduleService(IPreventiveScheduleRepository data) => Context = data;

    public Task<List<PreventiveSchedule>> GetPreventiveSchedulesAsync(bool? isActive)
    {
        return Context.GetPreventiveSchedulesAsync(isActive);
    }

    public Task<PreventiveSchedule?> GetPreventiveScheduleAsync(int id)
    {
        return Context.GetPreventiveScheduleAsync(id);
    }

    public Task<PreventiveSchedule> CreatePreventiveScheduleAsync(PreventiveSchedule schedule)
    {
        return Context.CreatePreventiveScheduleAsync(schedule);
    }

    public Task<PreventiveSchedule?> UpdatePreventiveScheduleAsync(int id, PreventiveSchedule schedule)
    {
        return Context.UpdatePreventiveScheduleAsync(id, schedule);
    }

    public Task<bool> DeletePreventiveScheduleAsync(int id)
    {
        return Context.DeletePreventiveScheduleAsync(id);
    }

    public Task<List<WorkOrder>> GenerateDuePreventiveWorkOrdersAsync(DateTime? dueDate)
    {
        return Context.GenerateDuePreventiveWorkOrdersAsync(dueDate);
    }
}
