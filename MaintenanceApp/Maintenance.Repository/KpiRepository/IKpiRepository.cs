using Maintenance.Domain.Cmms;

namespace Maintenance.Repository.KpiRepository;

public interface IKpiRepository
{
    Task<ReliabilityKpiResponse> GetReliabilityKpiAsync(int? assetId, DateTime? startDate, DateTime? endDate);
}
