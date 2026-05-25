using Maintenance.Domain.Cmms;

namespace Maintenance.Persistence.Services.RootCauseService;

public interface IRootCauseService
{
    Task<List<RootCause>> GetRootCausesAsync();
    Task<RootCause?> GetRootCauseAsync(int id);
    Task<RootCause> CreateRootCauseAsync(RootCause rootCause);
    Task<RootCause?> UpdateRootCauseAsync(int id, RootCause rootCause);
    Task<bool> DeleteRootCauseAsync(int id);
}
