using Maintenance.Domain.Cmms;

namespace Maintenance.Persistence.Services.AssetService;

public interface IAssetService
{
    Task<List<Asset>> GetAssetsAsync(string? search, AssetStatus? status);
    Task<Asset?> GetAssetAsync(int id);
    Task<Asset> CreateAssetAsync(Asset asset);
    Task<Asset?> UpdateAssetAsync(int id, Asset asset);
    Task<bool> DeleteAssetAsync(int id);
}
