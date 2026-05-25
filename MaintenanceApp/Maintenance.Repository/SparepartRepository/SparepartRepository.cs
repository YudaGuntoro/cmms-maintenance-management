using Maintenance.Domain.Cmms;
using Maintenance.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Maintenance.Repository.SparepartRepository;

public class SparepartRepository : ISparepartRepository
{
    private readonly MaintenanceDbContext Context;

    public SparepartRepository(MaintenanceDbContext context) => Context = context;

    public async Task<List<Sparepart>> GetSparepartsAsync(string? search, bool? lowStockOnly)
    {
        var query = Context.Spareparts.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.PartCode.Contains(search) || x.PartName.Contains(search));
        }

        if (lowStockOnly == true)
        {
            query = query.Where(x => x.StockQty <= x.MinimumStock);
        }

        return await query.OrderBy(x => x.PartCode).ToListAsync();
    }

    public Task<Sparepart?> GetSparepartAsync(int id)
    {
        return Context.Spareparts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Sparepart> CreateSparepartAsync(Sparepart sparepart)
    {
        sparepart.Id = 0;
        sparepart.CreatedAt = DateTime.Now;
        sparepart.UpdatedAt = DateTime.Now;
        Context.Spareparts.Add(sparepart);
        await Context.SaveChangesAsync();
        return sparepart;
    }

    public async Task<Sparepart?> UpdateSparepartAsync(int id, Sparepart sparepart)
    {
        var existing = await Context.Spareparts.FirstOrDefaultAsync(x => x.Id == id);
        if (existing == null)
        {
            return null;
        }

        existing.PartCode = sparepart.PartCode;
        existing.PartName = sparepart.PartName;
        existing.Category = sparepart.Category;
        existing.Unit = sparepart.Unit;
        existing.StockQty = sparepart.StockQty;
        existing.MinimumStock = sparepart.MinimumStock;
        existing.Location = sparepart.Location;
        existing.Supplier = sparepart.Supplier;
        existing.LeadTimeDays = sparepart.LeadTimeDays;
        existing.Price = sparepart.Price;
        existing.IsCritical = sparepart.IsCritical;
        existing.UpdatedAt = DateTime.Now;
        await Context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteSparepartAsync(int id)
    {
        var existing = await Context.Spareparts.FirstOrDefaultAsync(x => x.Id == id);
        if (existing == null)
        {
            return false;
        }

        Context.Spareparts.Remove(existing);
        await Context.SaveChangesAsync();
        return true;
    }
}
