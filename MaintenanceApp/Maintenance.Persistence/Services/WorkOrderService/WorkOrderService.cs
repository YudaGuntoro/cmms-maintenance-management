using Maintenance.Domain.Cmms;
using Maintenance.Repository.WorkOrderRepository;

namespace Maintenance.Persistence.Services.WorkOrderService;

public class WorkOrderService : IWorkOrderService
{
    protected readonly IWorkOrderRepository Context;

    public WorkOrderService(IWorkOrderRepository data) => Context = data;

    public Task<List<WorkOrder>> GetWorkOrdersAsync(int? assetId, WorkOrderStatus? status, WorkOrderPriority? priority, MaintenanceType? maintenanceType)
    {
        return Context.GetWorkOrdersAsync(assetId, status, priority, maintenanceType);
    }

    public Task<WorkOrder?> GetWorkOrderAsync(int id)
    {
        return Context.GetWorkOrderAsync(id);
    }

    public Task<WorkOrder> CreateWorkOrderAsync(WorkOrder workOrder)
    {
        return Context.CreateWorkOrderAsync(workOrder);
    }

    public Task<WorkOrder?> UpdateWorkOrderAsync(int id, WorkOrder workOrder)
    {
        return Context.UpdateWorkOrderAsync(id, workOrder);
    }

    public Task<bool> DeleteWorkOrderAsync(int id)
    {
        return Context.DeleteWorkOrderAsync(id);
    }

    public Task<WorkOrder?> AssignWorkOrderAsync(int id, WorkOrderAssignRequest request)
    {
        return Context.AssignWorkOrderAsync(id, request);
    }

    public Task<WorkOrder?> StartWorkOrderAsync(int id)
    {
        return Context.StartWorkOrderAsync(id);
    }

    public Task<WorkOrder?> CompleteWorkOrderAsync(int id, WorkOrderCompleteRequest request)
    {
        return Context.CompleteWorkOrderAsync(id, request);
    }

    public Task<WorkOrder?> CloseWorkOrderAsync(int id, WorkOrderCloseRequest request)
    {
        return Context.CloseWorkOrderAsync(id, request);
    }

    public Task<List<WorkOrderPhoto>> GetWorkOrderPhotosAsync(int workOrderId)
    {
        return Context.GetWorkOrderPhotosAsync(workOrderId);
    }

    public Task<WorkOrderPhoto?> GetWorkOrderPhotoAsync(int workOrderId, int photoId)
    {
        return Context.GetWorkOrderPhotoAsync(workOrderId, photoId);
    }

    public Task<WorkOrderPhoto?> AddWorkOrderPhotoAsync(int workOrderId, WorkOrderPhoto photo)
    {
        return Context.AddWorkOrderPhotoAsync(workOrderId, photo);
    }

    public Task<WorkOrderPhoto?> DeleteWorkOrderPhotoAsync(int workOrderId, int photoId)
    {
        return Context.DeleteWorkOrderPhotoAsync(workOrderId, photoId);
    }
}
