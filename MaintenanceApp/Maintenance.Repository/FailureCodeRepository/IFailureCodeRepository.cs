using Maintenance.Domain.Cmms;

namespace Maintenance.Repository.FailureCodeRepository;

public interface IFailureCodeRepository
{
    Task<List<FailureCode>> GetFailureCodesAsync();
    Task<FailureCode?> GetFailureCodeAsync(int id);
    Task<FailureCode> CreateFailureCodeAsync(FailureCode failureCode);
    Task<FailureCode?> UpdateFailureCodeAsync(int id, FailureCode failureCode);
    Task<bool> DeleteFailureCodeAsync(int id);
}
