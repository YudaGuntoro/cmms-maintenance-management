using Maintenance.Domain.Cmms;
using Maintenance.Repository.ContractorMonitoringRepository;

namespace Maintenance.Persistence.Services.ContractorMonitoringService;

public class ContractorMonitoringService : IContractorMonitoringService
{
    private readonly IContractorMonitoringRepository Context;

    public ContractorMonitoringService(IContractorMonitoringRepository context) => Context = context;

    public Task<List<ContractorWorkPlan>> GetPlansAsync(ContractorWorkPlanFilter filter)
    {
        return Context.GetPlansAsync(filter);
    }

    public Task<ContractorWorkPlan?> GetPlanAsync(int id)
    {
        return Context.GetPlanAsync(id);
    }

    public Task<ContractorWorkPlan> CreatePlanAsync(ContractorWorkPlan plan, string? performedBy)
    {
        return Context.CreatePlanAsync(plan, performedBy);
    }

    public Task<ContractorWorkPlan?> UpdatePlanAsync(int id, ContractorWorkPlan plan, string? performedBy)
    {
        return Context.UpdatePlanAsync(id, plan, performedBy);
    }

    public Task<bool> DeletePlanAsync(int id)
    {
        return Context.DeletePlanAsync(id);
    }

    public Task<ContractorWorkDocument?> GetDocumentAsync(int planId, int documentId)
    {
        return Context.GetDocumentAsync(planId, documentId);
    }

    public Task<ContractorWorkDocument?> AddDocumentAsync(int planId, ContractorWorkDocument document, string? performedBy)
    {
        return Context.AddDocumentAsync(planId, document, performedBy);
    }

    public Task<bool> DeleteDocumentAsync(int planId, int documentId, string? performedBy)
    {
        return Context.DeleteDocumentAsync(planId, documentId, performedBy);
    }

    public Task<List<ContractorWorkReminder>> GetRemindersAsync(DateTime now)
    {
        return Context.GetRemindersAsync(now);
    }

    public Task<WorkOrder?> CreateSupervisionWorkOrderAsync(int planId, string? performedBy)
    {
        return Context.CreateSupervisionWorkOrderAsync(planId, performedBy);
    }
}
