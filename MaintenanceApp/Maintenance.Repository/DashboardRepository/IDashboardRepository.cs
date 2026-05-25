using Maintenance.Domain.Cmms;

namespace Maintenance.Repository.DashboardRepository;

public interface IDashboardRepository
{
    Task<DashboardSummaryResponse> GetDashboardSummaryAsync();
}
