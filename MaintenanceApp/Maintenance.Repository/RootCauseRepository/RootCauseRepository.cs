using Maintenance.Domain.Cmms;
using Maintenance.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Maintenance.Repository.RootCauseRepository;

public class RootCauseRepository : IRootCauseRepository
{
    private readonly MaintenanceDbContext Context;

    public RootCauseRepository(MaintenanceDbContext context) => Context = context;

    public Task<List<RootCause>> GetRootCausesAsync()
    {
        return Context.RootCauses.AsNoTracking().OrderBy(x => x.Code).ToListAsync();
    }

    public Task<RootCause?> GetRootCauseAsync(int id)
    {
        return Context.RootCauses.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<RootCause> CreateRootCauseAsync(RootCause rootCause)
    {
        rootCause.Id = 0;
        Context.RootCauses.Add(rootCause);
        await Context.SaveChangesAsync();
        return rootCause;
    }

    public async Task<RootCause?> UpdateRootCauseAsync(int id, RootCause rootCause)
    {
        var existing = await Context.RootCauses.FirstOrDefaultAsync(x => x.Id == id);
        if (existing == null)
        {
            return null;
        }

        existing.Code = rootCause.Code;
        existing.Name = rootCause.Name;
        existing.Description = rootCause.Description;
        await Context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteRootCauseAsync(int id)
    {
        var existing = await Context.RootCauses.FirstOrDefaultAsync(x => x.Id == id);
        if (existing == null)
        {
            return false;
        }

        Context.RootCauses.Remove(existing);
        await Context.SaveChangesAsync();
        return true;
    }
}
