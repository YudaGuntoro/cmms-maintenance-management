using Maintenance.Domain.Cmms;
using Maintenance.Repository.SparepartRepository;

namespace Maintenance.Persistence.Services.SparepartService;

public class SparepartService : ISparepartService
{
    protected readonly ISparepartRepository Context;

    public SparepartService(ISparepartRepository data) => Context = data;

    public Task<List<Sparepart>> GetSparepartsAsync(string? search, bool? lowStockOnly)
    {
        return Context.GetSparepartsAsync(search, lowStockOnly);
    }

    public Task<Sparepart?> GetSparepartAsync(int id)
    {
        return Context.GetSparepartAsync(id);
    }

    public Task<Sparepart> CreateSparepartAsync(Sparepart sparepart)
    {
        return Context.CreateSparepartAsync(sparepart);
    }

    public Task<Sparepart?> UpdateSparepartAsync(int id, Sparepart sparepart)
    {
        return Context.UpdateSparepartAsync(id, sparepart);
    }

    public Task<bool> DeleteSparepartAsync(int id)
    {
        return Context.DeleteSparepartAsync(id);
    }
}
