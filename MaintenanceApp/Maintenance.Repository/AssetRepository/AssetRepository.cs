using Maintenance.Domain.Cmms;
using Maintenance.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Maintenance.Repository.AssetRepository;

public class AssetRepository : IAssetRepository
{
    private readonly MaintenanceDbContext Context;

    public AssetRepository(MaintenanceDbContext context) => Context = context;

    public async Task<List<Asset>> GetAssetsAsync(string? search, AssetStatus? status)
    {
        var query = Context.Assets.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.AssetCode.Contains(search) || x.AssetName.Contains(search));
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        return await query.OrderBy(x => x.AssetCode).ToListAsync();
    }

    public Task<Asset?> GetAssetAsync(int id)
    {
        return Context.Assets.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Asset> CreateAssetAsync(Asset asset)
    {
        asset.Id = 0;
        asset.CreatedAt = DateTime.Now;
        asset.UpdatedAt = DateTime.Now;
        Context.Assets.Add(asset);
        await Context.SaveChangesAsync();
        return asset;
    }

    public async Task<Asset?> UpdateAssetAsync(int id, Asset asset)
    {
        var existing = await Context.Assets.FirstOrDefaultAsync(x => x.Id == id);
        if (existing == null)
        {
            return null;
        }

        existing.AssetCode = asset.AssetCode;
        existing.AssetName = asset.AssetName;
        existing.AssetType = asset.AssetType;
        existing.Plant = asset.Plant;
        existing.Area = asset.Area;
        existing.ProductionLine = asset.ProductionLine;
        existing.Location = asset.Location;
        existing.Manufacturer = asset.Manufacturer;
        existing.Model = asset.Model;
        existing.SerialNumber = asset.SerialNumber;
        existing.InstallationDate = asset.InstallationDate;
        existing.CriticalityLevel = asset.CriticalityLevel;
        existing.Status = asset.Status;
        existing.UpdatedAt = DateTime.Now;

        await Context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAssetAsync(int id)
    {
        var existing = await Context.Assets.FirstOrDefaultAsync(x => x.Id == id);
        if (existing == null)
        {
            return false;
        }

        Context.Assets.Remove(existing);
        await Context.SaveChangesAsync();
        return true;
    }
}
