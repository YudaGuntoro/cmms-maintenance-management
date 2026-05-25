using Maintenance.Domain.Cmms;

namespace Maintenance.Persistence.Services.DashboardService;

public interface IDashboardService
{
    Task<DashboardSummaryResponse> GetDashboardSummaryAsync();
}
