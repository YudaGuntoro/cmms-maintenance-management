using Maintenance.Domain.Cmms;
using Maintenance.Repository.FailureCodeRepository;

namespace Maintenance.Persistence.Services.FailureCodeService;

public class FailureCodeService : IFailureCodeService
{
    protected readonly IFailureCodeRepository Context;

    public FailureCodeService(IFailureCodeRepository data) => Context = data;

    public Task<List<FailureCode>> GetFailureCodesAsync()
    {
        return Context.GetFailureCodesAsync();
    }

    public Task<FailureCode?> GetFailureCodeAsync(int id)
    {
        return Context.GetFailureCodeAsync(id);
    }

    public Task<FailureCode> CreateFailureCodeAsync(FailureCode failureCode)
    {
        return Context.CreateFailureCodeAsync(failureCode);
    }

    public Task<FailureCode?> UpdateFailureCodeAsync(int id, FailureCode failureCode)
    {
        return Context.UpdateFailureCodeAsync(id, failureCode);
    }

    public Task<bool> DeleteFailureCodeAsync(int id)
    {
        return Context.DeleteFailureCodeAsync(id);
    }
}
