using Maintenance.Domain.Cmms;

namespace Maintenance.Repository.RootCauseRepository;

public interface IRootCauseRepository
{
    Task<List<RootCause>> GetRootCausesAsync();
    Task<RootCause?> GetRootCauseAsync(int id);
    Task<RootCause> CreateRootCauseAsync(RootCause rootCause);
    Task<RootCause?> UpdateRootCauseAsync(int id, RootCause rootCause);
    Task<bool> DeleteRootCauseAsync(int id);
}
