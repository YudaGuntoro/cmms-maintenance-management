using Maintenance.Domain.Cmms;
using Maintenance.Repository.KpiRepository;

namespace Maintenance.Persistence.Services.KpiService;

public class KpiService : IKpiService
{
    protected readonly IKpiRepository Context;

    public KpiService(IKpiRepository data) => Context = data;

    public Task<ReliabilityKpiResponse> GetReliabilityKpiAsync(int? assetId, DateTime? startDate, DateTime? endDate)
    {
        return Context.GetReliabilityKpiAsync(assetId, startDate, endDate);
    }
}
