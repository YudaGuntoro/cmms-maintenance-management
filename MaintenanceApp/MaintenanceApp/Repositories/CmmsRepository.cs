using MaintenanceApp.Mapping.Cmms;
using MaintenanceApp.Services;

namespace MaintenanceApp.Repositories
{
    public interface ICmmsRepository
    {
        Task<DashboardSummaryDto?> GetDashboardSummaryAsync();
        Task<ReliabilityKpiDto?> GetReliabilityKpiAsync(int? assetId = null);
        Task<List<AssetDto>> GetAssetsAsync(string? search = null, string? status = null);
        Task<AssetDto?> GetAssetAsync(int id);
        Task<List<TechnicianDto>> GetTechniciansAsync();
        Task<List<SparepartDto>> GetSparepartsAsync(bool lowStockOnly = false);
        Task<List<WorkOrderDto>> GetWorkOrdersAsync(int? assetId = null, string? status = null, string? priority = null, string? maintenanceType = null);
        Task<WorkOrderDto?> GetWorkOrderAsync(int id);
        Task<WorkOrderDto?> CreateWorkOrderAsync(WorkOrderDto workOrder);
        Task<List<ContractorWorkPlanDto>> GetContractorWorkPlansAsync(string? vendor = null, string? area = null, string? status = null, string? risk = null, string? picMtc = null);
        Task<List<ContractorWorkReminderDto>> GetContractorWorkRemindersAsync();
        Task<WorkOrderDto?> AssignWorkOrderAsync(int id, int technicianId);
        Task<WorkOrderDto?> StartWorkOrderAsync(int id);
        Task<WorkOrderDto?> CompleteWorkOrderAsync(int id, CompleteWorkOrderRequest request);
        Task<WorkOrderDto?> CloseWorkOrderAsync(int id, CloseWorkOrderRequest request);
    }

    public class CmmsRepository : ICmmsRepository
    {
        private readonly ApiClient _apiClient;

        public CmmsRepository(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<DashboardSummaryDto?> GetDashboardSummaryAsync()
        {
            return await _apiClient.GetAsync<DashboardSummaryDto>("/api/dashboard/summary");
        }

        public async Task<ReliabilityKpiDto?> GetReliabilityKpiAsync(int? assetId = null)
        {
            var path = assetId.HasValue
                ? $"/api/kpi/reliability?asset_id={assetId.Value}"
                : "/api/kpi/reliability";
            return await _apiClient.GetAsync<ReliabilityKpiDto>(path);
        }

        public async Task<List<AssetDto>> GetAssetsAsync(string? search = null, string? status = null)
        {
            var query = BuildQuery(("search", search), ("status", status));
            return await _apiClient.GetAsync<List<AssetDto>>($"/api/assets{query}") ?? new List<AssetDto>();
        }

        public async Task<AssetDto?> GetAssetAsync(int id)
        {
            return await _apiClient.GetAsync<AssetDto>($"/api/assets/{id}");
        }

        public async Task<List<TechnicianDto>> GetTechniciansAsync()
        {
            return await _apiClient.GetAsync<List<TechnicianDto>>("/api/technicians") ?? new List<TechnicianDto>();
        }

        public async Task<List<SparepartDto>> GetSparepartsAsync(bool lowStockOnly = false)
        {
            var path = lowStockOnly ? "/api/spareparts?low_stock_only=true" : "/api/spareparts";
            return await _apiClient.GetAsync<List<SparepartDto>>(path) ?? new List<SparepartDto>();
        }

        public async Task<List<WorkOrderDto>> GetWorkOrdersAsync(int? assetId = null, string? status = null, string? priority = null, string? maintenanceType = null)
        {
            var query = BuildQuery(
                ("asset_id", assetId?.ToString()),
                ("status", status),
                ("priority", priority),
                ("maintenance_type", maintenanceType));

            return await _apiClient.GetAsync<List<WorkOrderDto>>($"/api/work-orders{query}") ?? new List<WorkOrderDto>();
        }

        public async Task<WorkOrderDto?> GetWorkOrderAsync(int id)
        {
            return await _apiClient.GetAsync<WorkOrderDto>($"/api/work-orders/{id}");
        }

        public async Task<WorkOrderDto?> CreateWorkOrderAsync(WorkOrderDto workOrder)
        {
            return await _apiClient.PostAsync<WorkOrderDto, WorkOrderDto>("/api/work-orders", workOrder);
        }

        public async Task<List<ContractorWorkPlanDto>> GetContractorWorkPlansAsync(string? vendor = null, string? area = null, string? status = null, string? risk = null, string? picMtc = null)
        {
            var query = BuildQuery(
                ("vendor", vendor),
                ("area", area),
                ("status", status),
                ("risk", risk),
                ("pic_mtc", picMtc));

            return await _apiClient.GetAsync<List<ContractorWorkPlanDto>>($"/api/contractor-monitoring{query}") ?? new List<ContractorWorkPlanDto>();
        }

        public async Task<List<ContractorWorkReminderDto>> GetContractorWorkRemindersAsync()
        {
            return await _apiClient.GetAsync<List<ContractorWorkReminderDto>>("/api/contractor-monitoring/reminders") ?? new List<ContractorWorkReminderDto>();
        }

        public async Task<WorkOrderDto?> AssignWorkOrderAsync(int id, int technicianId)
        {
            return await _apiClient.PatchAsync<AssignWorkOrderRequest, WorkOrderDto>($"/api/work-orders/{id}/assign", new AssignWorkOrderRequest
            {
                AssignedTo = technicianId
            });
        }

        public async Task<WorkOrderDto?> StartWorkOrderAsync(int id)
        {
            return await _apiClient.PatchAsync<object, WorkOrderDto>($"/api/work-orders/{id}/start", null);
        }

        public async Task<WorkOrderDto?> CompleteWorkOrderAsync(int id, CompleteWorkOrderRequest request)
        {
            return await _apiClient.PatchAsync<CompleteWorkOrderRequest, WorkOrderDto>($"/api/work-orders/{id}/complete", request);
        }

        public async Task<WorkOrderDto?> CloseWorkOrderAsync(int id, CloseWorkOrderRequest request)
        {
            return await _apiClient.PatchAsync<CloseWorkOrderRequest, WorkOrderDto>($"/api/work-orders/{id}/close", request);
        }

        private static string BuildQuery(params (string Key, string? Value)[] values)
        {
            var pairs = values
                .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                .Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value!)}")
                .ToList();

            return pairs.Count == 0 ? string.Empty : $"?{string.Join("&", pairs)}";
        }
    }
}
