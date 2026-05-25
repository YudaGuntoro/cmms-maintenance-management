using Maintenance.Domain.Cmms;

namespace Maintenance.Repository.CmmsMasterDataRepository;

public interface ICmmsMasterDataRepository
{
    Task<List<MaintenanceTypeMaster>> GetMaintenanceTypesAsync();
    Task<MaintenanceTypeMaster?> GetMaintenanceTypeAsync(int id);
    Task<MaintenanceTypeMaster> CreateMaintenanceTypeAsync(MaintenanceTypeMaster item);
    Task<MaintenanceTypeMaster?> UpdateMaintenanceTypeAsync(int id, MaintenanceTypeMaster item);
    Task<bool> DeleteMaintenanceTypeAsync(int id);

    Task<List<WorkOrderPriorityMaster>> GetWorkOrderPrioritiesAsync();
    Task<WorkOrderPriorityMaster?> GetWorkOrderPriorityAsync(int id);
    Task<WorkOrderPriorityMaster> CreateWorkOrderPriorityAsync(WorkOrderPriorityMaster item);
    Task<WorkOrderPriorityMaster?> UpdateWorkOrderPriorityAsync(int id, WorkOrderPriorityMaster item);
    Task<bool> DeleteWorkOrderPriorityAsync(int id);

    Task<List<WorkOrderStatusMaster>> GetWorkOrderStatusesAsync();
    Task<WorkOrderStatusMaster?> GetWorkOrderStatusAsync(int id);
    Task<WorkOrderStatusMaster> CreateWorkOrderStatusAsync(WorkOrderStatusMaster item);
    Task<WorkOrderStatusMaster?> UpdateWorkOrderStatusAsync(int id, WorkOrderStatusMaster item);
    Task<bool> DeleteWorkOrderStatusAsync(int id);

    Task<List<DowntimeCategoryMaster>> GetDowntimeCategoriesAsync();
    Task<DowntimeCategoryMaster?> GetDowntimeCategoryAsync(int id);
    Task<DowntimeCategoryMaster> CreateDowntimeCategoryAsync(DowntimeCategoryMaster item);
    Task<DowntimeCategoryMaster?> UpdateDowntimeCategoryAsync(int id, DowntimeCategoryMaster item);
    Task<bool> DeleteDowntimeCategoryAsync(int id);

    Task<List<ProblemReportCategoryMaster>> GetProblemReportCategoriesAsync();
    Task<ProblemReportCategoryMaster?> GetProblemReportCategoryAsync(int id);
    Task<ProblemReportCategoryMaster> CreateProblemReportCategoryAsync(ProblemReportCategoryMaster item);
    Task<ProblemReportCategoryMaster?> UpdateProblemReportCategoryAsync(int id, ProblemReportCategoryMaster item);
    Task<bool> DeleteProblemReportCategoryAsync(int id);

    Task<List<PreventiveScheduleTypeMaster>> GetPreventiveScheduleTypesAsync();
    Task<PreventiveScheduleTypeMaster?> GetPreventiveScheduleTypeAsync(int id);
    Task<PreventiveScheduleTypeMaster> CreatePreventiveScheduleTypeAsync(PreventiveScheduleTypeMaster item);
    Task<PreventiveScheduleTypeMaster?> UpdatePreventiveScheduleTypeAsync(int id, PreventiveScheduleTypeMaster item);
    Task<bool> DeletePreventiveScheduleTypeAsync(int id);

    Task<List<FrequencyTypeMaster>> GetFrequencyTypesAsync();
    Task<FrequencyTypeMaster?> GetFrequencyTypeAsync(int id);
    Task<FrequencyTypeMaster> CreateFrequencyTypeAsync(FrequencyTypeMaster item);
    Task<FrequencyTypeMaster?> UpdateFrequencyTypeAsync(int id, FrequencyTypeMaster item);
    Task<bool> DeleteFrequencyTypeAsync(int id);
}
