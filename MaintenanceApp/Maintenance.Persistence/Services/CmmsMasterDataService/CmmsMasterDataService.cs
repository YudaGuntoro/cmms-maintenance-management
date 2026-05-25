using Maintenance.Domain.Cmms;
using Maintenance.Repository.CmmsMasterDataRepository;

namespace Maintenance.Persistence.Services.CmmsMasterDataService;

public class CmmsMasterDataService : ICmmsMasterDataService
{
    protected readonly ICmmsMasterDataRepository Context;

    public CmmsMasterDataService(ICmmsMasterDataRepository data) => Context = data;

    public Task<List<MaintenanceTypeMaster>> GetMaintenanceTypesAsync()
    {
        return Context.GetMaintenanceTypesAsync();
    }

    public Task<MaintenanceTypeMaster?> GetMaintenanceTypeAsync(int id) => Context.GetMaintenanceTypeAsync(id);
    public Task<MaintenanceTypeMaster> CreateMaintenanceTypeAsync(MaintenanceTypeMaster item) => Context.CreateMaintenanceTypeAsync(item);
    public Task<MaintenanceTypeMaster?> UpdateMaintenanceTypeAsync(int id, MaintenanceTypeMaster item) => Context.UpdateMaintenanceTypeAsync(id, item);
    public Task<bool> DeleteMaintenanceTypeAsync(int id) => Context.DeleteMaintenanceTypeAsync(id);

    public Task<List<WorkOrderPriorityMaster>> GetWorkOrderPrioritiesAsync()
    {
        return Context.GetWorkOrderPrioritiesAsync();
    }

    public Task<WorkOrderPriorityMaster?> GetWorkOrderPriorityAsync(int id) => Context.GetWorkOrderPriorityAsync(id);
    public Task<WorkOrderPriorityMaster> CreateWorkOrderPriorityAsync(WorkOrderPriorityMaster item) => Context.CreateWorkOrderPriorityAsync(item);
    public Task<WorkOrderPriorityMaster?> UpdateWorkOrderPriorityAsync(int id, WorkOrderPriorityMaster item) => Context.UpdateWorkOrderPriorityAsync(id, item);
    public Task<bool> DeleteWorkOrderPriorityAsync(int id) => Context.DeleteWorkOrderPriorityAsync(id);

    public Task<List<WorkOrderStatusMaster>> GetWorkOrderStatusesAsync()
    {
        return Context.GetWorkOrderStatusesAsync();
    }

    public Task<WorkOrderStatusMaster?> GetWorkOrderStatusAsync(int id) => Context.GetWorkOrderStatusAsync(id);
    public Task<WorkOrderStatusMaster> CreateWorkOrderStatusAsync(WorkOrderStatusMaster item) => Context.CreateWorkOrderStatusAsync(item);
    public Task<WorkOrderStatusMaster?> UpdateWorkOrderStatusAsync(int id, WorkOrderStatusMaster item) => Context.UpdateWorkOrderStatusAsync(id, item);
    public Task<bool> DeleteWorkOrderStatusAsync(int id) => Context.DeleteWorkOrderStatusAsync(id);

    public Task<List<DowntimeCategoryMaster>> GetDowntimeCategoriesAsync()
    {
        return Context.GetDowntimeCategoriesAsync();
    }

    public Task<DowntimeCategoryMaster?> GetDowntimeCategoryAsync(int id) => Context.GetDowntimeCategoryAsync(id);
    public Task<DowntimeCategoryMaster> CreateDowntimeCategoryAsync(DowntimeCategoryMaster item) => Context.CreateDowntimeCategoryAsync(item);
    public Task<DowntimeCategoryMaster?> UpdateDowntimeCategoryAsync(int id, DowntimeCategoryMaster item) => Context.UpdateDowntimeCategoryAsync(id, item);
    public Task<bool> DeleteDowntimeCategoryAsync(int id) => Context.DeleteDowntimeCategoryAsync(id);

    public Task<List<ProblemReportCategoryMaster>> GetProblemReportCategoriesAsync()
    {
        return Context.GetProblemReportCategoriesAsync();
    }

    public Task<ProblemReportCategoryMaster?> GetProblemReportCategoryAsync(int id) => Context.GetProblemReportCategoryAsync(id);
    public Task<ProblemReportCategoryMaster> CreateProblemReportCategoryAsync(ProblemReportCategoryMaster item) => Context.CreateProblemReportCategoryAsync(item);
    public Task<ProblemReportCategoryMaster?> UpdateProblemReportCategoryAsync(int id, ProblemReportCategoryMaster item) => Context.UpdateProblemReportCategoryAsync(id, item);
    public Task<bool> DeleteProblemReportCategoryAsync(int id) => Context.DeleteProblemReportCategoryAsync(id);

    public Task<List<PreventiveScheduleTypeMaster>> GetPreventiveScheduleTypesAsync()
    {
        return Context.GetPreventiveScheduleTypesAsync();
    }

    public Task<PreventiveScheduleTypeMaster?> GetPreventiveScheduleTypeAsync(int id) => Context.GetPreventiveScheduleTypeAsync(id);
    public Task<PreventiveScheduleTypeMaster> CreatePreventiveScheduleTypeAsync(PreventiveScheduleTypeMaster item) => Context.CreatePreventiveScheduleTypeAsync(item);
    public Task<PreventiveScheduleTypeMaster?> UpdatePreventiveScheduleTypeAsync(int id, PreventiveScheduleTypeMaster item) => Context.UpdatePreventiveScheduleTypeAsync(id, item);
    public Task<bool> DeletePreventiveScheduleTypeAsync(int id) => Context.DeletePreventiveScheduleTypeAsync(id);

    public Task<List<FrequencyTypeMaster>> GetFrequencyTypesAsync()
    {
        return Context.GetFrequencyTypesAsync();
    }

    public Task<FrequencyTypeMaster?> GetFrequencyTypeAsync(int id) => Context.GetFrequencyTypeAsync(id);
    public Task<FrequencyTypeMaster> CreateFrequencyTypeAsync(FrequencyTypeMaster item) => Context.CreateFrequencyTypeAsync(item);
    public Task<FrequencyTypeMaster?> UpdateFrequencyTypeAsync(int id, FrequencyTypeMaster item) => Context.UpdateFrequencyTypeAsync(id, item);
    public Task<bool> DeleteFrequencyTypeAsync(int id) => Context.DeleteFrequencyTypeAsync(id);
}
