using Maintenance.Domain.Cmms;
using Maintenance.Repository.AssetRepository;

namespace Maintenance.Persistence.Services.AssetService;

public class AssetService : IAssetService
{
    protected readonly IAssetRepository Context;

    public AssetService(IAssetRepository data) => Context = data;

    public Task<List<Asset>> GetAssetsAsync(string? search, AssetStatus? status)
    {
        return Context.GetAssetsAsync(search, status);
    }

    public Task<Asset?> GetAssetAsync(int id)
    {
        return Context.GetAssetAsync(id);
    }

    public Task<Asset> CreateAssetAsync(Asset asset)
    {
        return Context.CreateAssetAsync(asset);
    }

    public Task<Asset?> UpdateAssetAsync(int id, Asset asset)
    {
        return Context.UpdateAssetAsync(id, asset);
    }

    public Task<bool> DeleteAssetAsync(int id)
    {
        return Context.DeleteAssetAsync(id);
    }
}
