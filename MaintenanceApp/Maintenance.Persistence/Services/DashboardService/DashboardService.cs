using Maintenance.Domain.Cmms;
using Maintenance.Repository.DashboardRepository;

namespace Maintenance.Persistence.Services.DashboardService;

public class DashboardService : IDashboardService
{
    protected readonly IDashboardRepository Context;

    public DashboardService(IDashboardRepository data) => Context = data;

    public Task<DashboardSummaryResponse> GetDashboardSummaryAsync()
    {
        return Context.GetDashboardSummaryAsync();
    }
}
