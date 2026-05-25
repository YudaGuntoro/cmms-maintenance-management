using Maintenance.Domain.Cmms;

namespace Maintenance.Persistence.Services.WorkOrderSparepartService;

public interface IWorkOrderSparepartService
{
    Task<WorkOrderSparepartUsage?> UseSparepartAsync(int workOrderId, WorkOrderSparepartRequest request);
}
