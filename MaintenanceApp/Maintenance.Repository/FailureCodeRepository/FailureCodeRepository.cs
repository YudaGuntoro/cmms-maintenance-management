using Maintenance.Domain.Cmms;
using Maintenance.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Maintenance.Repository.FailureCodeRepository;

public class FailureCodeRepository : IFailureCodeRepository
{
    private readonly MaintenanceDbContext Context;

    public FailureCodeRepository(MaintenanceDbContext context) => Context = context;

    public Task<List<FailureCode>> GetFailureCodesAsync()
    {
        return Context.FailureCodes.AsNoTracking().OrderBy(x => x.Code).ToListAsync();
    }

    public Task<FailureCode?> GetFailureCodeAsync(int id)
    {
        return Context.FailureCodes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<FailureCode> CreateFailureCodeAsync(FailureCode failureCode)
    {
        failureCode.Id = 0;
        Context.FailureCodes.Add(failureCode);
        await Context.SaveChangesAsync();
        return failureCode;
    }

    public async Task<FailureCode?> UpdateFailureCodeAsync(int id, FailureCode failureCode)
    {
        var existing = await Context.FailureCodes.FirstOrDefaultAsync(x => x.Id == id);
        if (existing == null)
        {
            return null;
        }

        existing.Code = failureCode.Code;
        existing.Name = failureCode.Name;
        existing.Category = failureCode.Category;
        existing.Description = failureCode.Description;
        await Context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteFailureCodeAsync(int id)
    {
        var existing = await Context.FailureCodes.FirstOrDefaultAsync(x => x.Id == id);
        if (existing == null)
        {
            return false;
        }

        Context.FailureCodes.Remove(existing);
        await Context.SaveChangesAsync();
        return true;
    }
}
