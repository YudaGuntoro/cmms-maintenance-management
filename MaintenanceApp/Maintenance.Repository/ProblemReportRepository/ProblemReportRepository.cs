using Maintenance.Domain.Cmms;
using Maintenance.Infrastructure.Context;
using Maintenance.Repository.Shared;
using Microsoft.EntityFrameworkCore;

namespace Maintenance.Repository.ProblemReportRepository;

public class ProblemReportRepository : IProblemReportRepository
{
    private readonly MaintenanceDbContext Context;

    public ProblemReportRepository(MaintenanceDbContext context) => Context = context;

    public async Task<List<ProblemReport>> GetProblemReportsAsync(int? assetId, ProblemReportStatus? status, ProblemReportCategory? category, string? reportedBy)
    {
        var query = Context.ProblemReports
            .AsNoTracking()
            .IncludeProblemReportMasters()
            .AsQueryable();

        if (assetId.HasValue)
        {
            query = query.Where(x => x.AssetId == assetId.Value);
        }

        if (status.HasValue)
        {
            var statusId = await CmmsMasterDataResolver.GetProblemReportStatusIdAsync(Context, status.Value);
            query = query.Where(x => x.StatusId == statusId);
        }

        if (category.HasValue)
        {
            var categoryId = await CmmsMasterDataResolver.GetProblemReportCategoryIdAsync(Context, category.Value);
            query = query.Where(x => x.CategoryId == categoryId);
        }

        if (!string.IsNullOrWhiteSpace(reportedBy))
        {
            var normalized = reportedBy.Trim();
            query = query.Where(x => x.ReportedBy == normalized);
        }

        return await query.OrderByDescending(x => x.ReportedAt).ThenByDescending(x => x.Id).ToListAsync();
    }

    public Task<ProblemReport?> GetProblemReportAsync(int id)
    {
        return Context.ProblemReports
            .AsNoTracking()
            .IncludeProblemReportMasters()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public Task<string> GetNextReportNumberAsync()
    {
        return GenerateReportNumberAsync();
    }

    public async Task<ProblemReport> CreateProblemReportAsync(ProblemReport report)
    {
        await CmmsMasterDataResolver.ApplyProblemReportMastersAsync(Context, report);
        await ValidateReportAsync(report);

        report.Id = 0;
        report.ReportNumber = string.IsNullOrWhiteSpace(report.ReportNumber)
            ? await GenerateReportNumberAsync()
            : report.ReportNumber.Trim();
        report.Title = report.Title.Trim();
        report.ReportedBy = string.IsNullOrWhiteSpace(report.ReportedBy) ? null : report.ReportedBy.Trim();
        report.ReportedAt = NormalizeDate(report.ReportedAt);
        await CmmsMasterDataResolver.SetProblemReportStatusAsync(Context, report, ProblemReportStatus.PENDING);
        report.CreatedAt = DateTime.Now;
        report.UpdatedAt = DateTime.Now;

        CalculateDowntime(report);

        Context.ProblemReports.Add(report);
        await Context.SaveChangesAsync();
        await SyncDowntimeLogAsync(report);
        await LoadProblemReportMastersAsync(report);
        return report;
    }

    public async Task<ProblemReport?> UpdateProblemReportAsync(int id, ProblemReport report)
    {
        var existing = await Context.ProblemReports.FirstOrDefaultAsync(x => x.Id == id);
        if (existing == null)
        {
            return null;
        }

        await CmmsMasterDataResolver.ApplyProblemReportMastersAsync(Context, report);
        await ValidateReportAsync(report);

        existing.AssetId = report.AssetId;
        existing.ReportNumber = string.IsNullOrWhiteSpace(report.ReportNumber) ? existing.ReportNumber : report.ReportNumber.Trim();
        existing.Title = report.Title.Trim();
        existing.Description = report.Description;
        existing.CategoryId = report.CategoryId;
        existing.Category = report.Category;
        existing.CategoryDetail = null;
        existing.PriorityId = report.PriorityId;
        existing.Priority = report.Priority;
        existing.PriorityDetail = null;
        existing.StatusId = report.StatusId;
        existing.Status = report.Status;
        existing.StatusDetail = null;
        existing.ReportedBy = string.IsNullOrWhiteSpace(report.ReportedBy) ? existing.ReportedBy : report.ReportedBy.Trim();
        existing.ReportedAt = NormalizeDate(report.ReportedAt);
        existing.DowntimeStart = report.DowntimeStart;
        existing.DowntimeEnd = report.DowntimeEnd;
        existing.UpdatedAt = DateTime.Now;

        CalculateDowntime(existing);

        await Context.SaveChangesAsync();
        await SyncDowntimeLogAsync(existing);
        await LoadProblemReportMastersAsync(existing);
        return existing;
    }

    public async Task<bool> DeleteProblemReportAsync(int id)
    {
        var existing = await Context.ProblemReports.FirstOrDefaultAsync(x => x.Id == id);
        if (existing == null)
        {
            return false;
        }

        var relatedWorkOrders = await Context.WorkOrders.Where(x => x.ProblemReportId == id).ToListAsync();
        foreach (var workOrder in relatedWorkOrders)
        {
            workOrder.ProblemReportId = null;
            workOrder.UpdatedAt = DateTime.Now;
        }

        var generatedLogs = await Context.DowntimeLogs.Where(x => x.ProblemReportId == id).ToListAsync();
        Context.DowntimeLogs.RemoveRange(generatedLogs);
        Context.ProblemReports.Remove(existing);
        await Context.SaveChangesAsync();
        return true;
    }

    private async Task ValidateReportAsync(ProblemReport report)
    {
        if (report.AssetId <= 0 || !await Context.Assets.AnyAsync(x => x.Id == report.AssetId))
        {
            throw new InvalidOperationException("Asset tidak ditemukan.");
        }

        if (string.IsNullOrWhiteSpace(report.Title))
        {
            throw new InvalidOperationException("Title wajib diisi.");
        }

        if (report.Category == ProblemReportCategory.DOWNTIME && !report.DowntimeStart.HasValue)
        {
            throw new InvalidOperationException("Downtime start wajib diisi untuk report downtime.");
        }

        if (report.Category == ProblemReportCategory.DOWNTIME && report.DowntimeEnd.HasValue && !report.DowntimeStart.HasValue)
        {
            throw new InvalidOperationException("Downtime start wajib diisi jika downtime end diisi.");
        }

        if (report.DowntimeStart.HasValue && report.DowntimeEnd.HasValue && report.DowntimeEnd.Value < report.DowntimeStart.Value)
        {
            throw new InvalidOperationException("Downtime end tidak boleh lebih awal dari downtime start.");
        }
    }

    private async Task<string> GenerateReportNumberAsync()
    {
        var now = DateTime.Now;
        var prefix = $"RPT-{now:yyyyMMdd}";
        var countToday = await Context.ProblemReports.CountAsync(x => x.ReportNumber.StartsWith(prefix));
        return $"{prefix}-{countToday + 1:0000}";
    }

    private static DateTime NormalizeDate(DateTime date)
    {
        return date <= new DateTime(1900, 1, 1) ? DateTime.Now : date;
    }

    private static void CalculateDowntime(ProblemReport report)
    {
        if (report.DowntimeStart.HasValue && report.DowntimeEnd.HasValue)
        {
            report.DowntimeMinutes = Math.Max(0, (int)Math.Round((report.DowntimeEnd.Value - report.DowntimeStart.Value).TotalMinutes));
            return;
        }

        report.DowntimeMinutes = null;
    }

    private async Task SyncDowntimeLogAsync(ProblemReport report)
    {
        var log = await Context.DowntimeLogs.FirstOrDefaultAsync(x => x.ProblemReportId == report.Id);

        if (report.Category != ProblemReportCategory.DOWNTIME)
        {
            if (log != null)
            {
                Context.DowntimeLogs.Remove(log);
                await Context.SaveChangesAsync();
            }

            return;
        }

        if (log == null)
        {
            log = new DowntimeLog
            {
                AssetId = report.AssetId,
                ProblemReportId = report.Id,
                DowntimeCategory = DowntimeCategory.OPERATIONAL,
                CreatedAt = DateTime.Now
            };
            Context.DowntimeLogs.Add(log);
        }

        log.AssetId = report.AssetId;
        log.DowntimeCategory = DowntimeCategory.OPERATIONAL;
        log.StartTime = report.DowntimeStart ?? report.ReportedAt;
        log.EndTime = report.DowntimeEnd;
        log.DurationMinutes = report.DowntimeMinutes;
        log.Description = $"{report.ReportNumber} - {report.Title}";
        WorkOrderRules.CalculateDowntimeDuration(log);
        await CmmsMasterDataResolver.ApplyDowntimeLogMastersAsync(Context, log);
        await Context.SaveChangesAsync();
    }

    private async Task LoadProblemReportMastersAsync(ProblemReport report)
    {
        await Context.Entry(report).Reference(x => x.CategoryDetail).LoadAsync();
        await Context.Entry(report).Reference(x => x.PriorityDetail).LoadAsync();
        await Context.Entry(report).Reference(x => x.StatusDetail).LoadAsync();
    }
}
