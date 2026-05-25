using Maintenance.Domain.Cmms;

namespace Maintenance.Persistence.Services.KpiService;

public interface IKpiService
{
    Task<ReliabilityKpiResponse> GetReliabilityKpiAsync(int? assetId, DateTime? startDate, DateTime? endDate);
}
