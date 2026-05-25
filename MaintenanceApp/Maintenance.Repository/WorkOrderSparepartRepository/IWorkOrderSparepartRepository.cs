using Maintenance.Domain.Cmms;

namespace Maintenance.Repository.WorkOrderSparepartRepository;

public interface IWorkOrderSparepartRepository
{
    Task<WorkOrderSparepartUsage?> UseSparepartAsync(int workOrderId, WorkOrderSparepartRequest request);
}
