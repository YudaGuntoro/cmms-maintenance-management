using Maintenance.Domain.Cmms;

namespace Maintenance.Persistence.Services.ContractorMonitoringService;

public interface IContractorMonitoringService
{
    Task<List<ContractorWorkPlan>> GetPlansAsync(ContractorWorkPlanFilter filter);
    Task<ContractorWorkPlan?> GetPlanAsync(int id);
    Task<ContractorWorkPlan> CreatePlanAsync(ContractorWorkPlan plan, string? performedBy);
    Task<ContractorWorkPlan?> UpdatePlanAsync(int id, ContractorWorkPlan plan, string? performedBy);
    Task<bool> DeletePlanAsync(int id);
    Task<ContractorWorkDocument?> GetDocumentAsync(int planId, int documentId);
    Task<ContractorWorkDocument?> AddDocumentAsync(int planId, ContractorWorkDocument document, string? performedBy);
    Task<bool> DeleteDocumentAsync(int planId, int documentId, string? performedBy);
    Task<List<ContractorWorkReminder>> GetRemindersAsync(DateTime now);
    Task<WorkOrder?> CreateSupervisionWorkOrderAsync(int planId, string? performedBy);
}
