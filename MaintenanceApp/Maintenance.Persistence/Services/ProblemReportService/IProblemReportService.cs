using Maintenance.Domain.Cmms;

namespace Maintenance.Persistence.Services.ProblemReportService;

public interface IProblemReportService
{
    Task<List<ProblemReport>> GetProblemReportsAsync(int? assetId, ProblemReportStatus? status, ProblemReportCategory? category, string? reportedBy);
    Task<ProblemReport?> GetProblemReportAsync(int id);
    Task<string> GetNextReportNumberAsync();
    Task<ProblemReport> CreateProblemReportAsync(ProblemReport report);
    Task<ProblemReport?> UpdateProblemReportAsync(int id, ProblemReport report);
    Task<bool> DeleteProblemReportAsync(int id);
}
