using Maintenance.Domain.Cmms;

namespace Maintenance.Persistence.Services.SparepartService;

public interface ISparepartService
{
    Task<List<Sparepart>> GetSparepartsAsync(string? search, bool? lowStockOnly);
    Task<Sparepart?> GetSparepartAsync(int id);
    Task<Sparepart> CreateSparepartAsync(Sparepart sparepart);
    Task<Sparepart?> UpdateSparepartAsync(int id, Sparepart sparepart);
    Task<bool> DeleteSparepartAsync(int id);
}
