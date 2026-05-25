using Maintenance.Domain.Cmms;
using Maintenance.Repository.WorkOrderSparepartRepository;

namespace Maintenance.Persistence.Services.WorkOrderSparepartService;

public class WorkOrderSparepartService : IWorkOrderSparepartService
{
    protected readonly IWorkOrderSparepartRepository Context;

    public WorkOrderSparepartService(IWorkOrderSparepartRepository data) => Context = data;

    public Task<WorkOrderSparepartUsage?> UseSparepartAsync(int workOrderId, WorkOrderSparepartRequest request)
    {
        return Context.UseSparepartAsync(workOrderId, request);
    }
}
