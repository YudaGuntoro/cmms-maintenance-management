using Maintenance.Domain.Cmms;
using Maintenance.Repository.RootCauseRepository;

namespace Maintenance.Persistence.Services.RootCauseService;

public class RootCauseService : IRootCauseService
{
    protected readonly IRootCauseRepository Context;

    public RootCauseService(IRootCauseRepository data) => Context = data;

    public Task<List<RootCause>> GetRootCausesAsync()
    {
        return Context.GetRootCausesAsync();
    }

    public Task<RootCause?> GetRootCauseAsync(int id)
    {
        return Context.GetRootCauseAsync(id);
    }

    public Task<RootCause> CreateRootCauseAsync(RootCause rootCause)
    {
        return Context.CreateRootCauseAsync(rootCause);
    }

    public Task<RootCause?> UpdateRootCauseAsync(int id, RootCause rootCause)
    {
        return Context.UpdateRootCauseAsync(id, rootCause);
    }

    public Task<bool> DeleteRootCauseAsync(int id)
    {
        return Context.DeleteRootCauseAsync(id);
    }
}
