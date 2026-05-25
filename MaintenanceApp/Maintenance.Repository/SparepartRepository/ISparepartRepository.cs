using Maintenance.Domain.Cmms;

namespace Maintenance.Repository.SparepartRepository;

public interface ISparepartRepository
{
    Task<List<Sparepart>> GetSparepartsAsync(string? search, bool? lowStockOnly);
    Task<Sparepart?> GetSparepartAsync(int id);
    Task<Sparepart> CreateSparepartAsync(Sparepart sparepart);
    Task<Sparepart?> UpdateSparepartAsync(int id, Sparepart sparepart);
    Task<bool> DeleteSparepartAsync(int id);
}
