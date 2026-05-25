using Maintenance.Domain.Cmms;

namespace Maintenance.Repository.PreventiveScheduleRepository;

public interface IPreventiveScheduleRepository
{
    Task<List<PreventiveSchedule>> GetPreventiveSchedulesAsync(bool? isActive);
    Task<PreventiveSchedule?> GetPreventiveScheduleAsync(int id);
    Task<PreventiveSchedule> CreatePreventiveScheduleAsync(PreventiveSchedule schedule);
    Task<PreventiveSchedule?> UpdatePreventiveScheduleAsync(int id, PreventiveSchedule schedule);
    Task<bool> DeletePreventiveScheduleAsync(int id);
    Task<List<WorkOrder>> GenerateDuePreventiveWorkOrdersAsync(DateTime? dueDate);
}
