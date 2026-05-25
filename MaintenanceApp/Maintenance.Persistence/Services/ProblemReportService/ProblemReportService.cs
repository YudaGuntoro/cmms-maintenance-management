using Maintenance.Domain.Cmms;
using Maintenance.Repository.ProblemReportRepository;

namespace Maintenance.Persistence.Services.ProblemReportService;

public class ProblemReportService : IProblemReportService
{
    protected readonly IProblemReportRepository Context;

    public ProblemReportService(IProblemReportRepository data) => Context = data;

    public Task<List<ProblemReport>> GetProblemReportsAsync(int? assetId, ProblemReportStatus? status, ProblemReportCategory? category, string? reportedBy)
    {
        return Context.GetProblemReportsAsync(assetId, status, category, reportedBy);
    }

    public Task<ProblemReport?> GetProblemReportAsync(int id)
    {
        return Context.GetProblemReportAsync(id);
    }

    public Task<string> GetNextReportNumberAsync()
    {
        return Context.GetNextReportNumberAsync();
    }

    public Task<ProblemReport> CreateProblemReportAsync(ProblemReport report)
    {
        return Context.CreateProblemReportAsync(report);
    }

    public Task<ProblemReport?> UpdateProblemReportAsync(int id, ProblemReport report)
    {
        return Context.UpdateProblemReportAsync(id, report);
    }

    public Task<bool> DeleteProblemReportAsync(int id)
    {
        return Context.DeleteProblemReportAsync(id);
    }
}
