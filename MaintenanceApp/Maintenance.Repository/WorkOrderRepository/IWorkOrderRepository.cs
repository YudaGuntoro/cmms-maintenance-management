using Maintenance.Domain.Cmms;

namespace Maintenance.Repository.WorkOrderRepository;

public interface IWorkOrderRepository
{
    Task<List<WorkOrder>> GetWorkOrdersAsync(int? assetId, WorkOrderStatus? status, WorkOrderPriority? priority, MaintenanceType? maintenanceType);
    Task<WorkOrder?> GetWorkOrderAsync(int id);
    Task<WorkOrder> CreateWorkOrderAsync(WorkOrder workOrder);
    Task<WorkOrder?> UpdateWorkOrderAsync(int id, WorkOrder workOrder);
    Task<bool> DeleteWorkOrderAsync(int id);
    Task<WorkOrder?> AssignWorkOrderAsync(int id, WorkOrderAssignRequest request);
    Task<WorkOrder?> StartWorkOrderAsync(int id);
    Task<WorkOrder?> CompleteWorkOrderAsync(int id, WorkOrderCompleteRequest request);
    Task<WorkOrder?> CloseWorkOrderAsync(int id, WorkOrderCloseRequest request);
    Task<List<WorkOrderPhoto>> GetWorkOrderPhotosAsync(int workOrderId);
    Task<WorkOrderPhoto?> GetWorkOrderPhotoAsync(int workOrderId, int photoId);
    Task<WorkOrderPhoto?> AddWorkOrderPhotoAsync(int workOrderId, WorkOrderPhoto photo);
    Task<WorkOrderPhoto?> DeleteWorkOrderPhotoAsync(int workOrderId, int photoId);
}
