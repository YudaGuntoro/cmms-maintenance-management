using Maintenance.Domain.Cmms;

namespace Maintenance.Repository.AssetRepository;

public interface IAssetRepository
{
    Task<List<Asset>> GetAssetsAsync(string? search, AssetStatus? status);
    Task<Asset?> GetAssetAsync(int id);
    Task<Asset> CreateAssetAsync(Asset asset);
    Task<Asset?> UpdateAssetAsync(int id, Asset asset);
    Task<bool> DeleteAssetAsync(int id);
}
