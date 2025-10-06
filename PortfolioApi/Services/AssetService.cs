using PortfolioApi.Constants;
using PortfolioApi.Models;
using PortfolioApi.Repositories;

namespace PortfolioApi.Services
{
    public class AssetService : IAssetService
    {
        private readonly IAssetRepository _repo;
        public AssetService(IAssetRepository repo) { _repo = repo; }

        public async Task<Asset> CreateAsync(Asset asset)
        {
            await _repo.AddAsync(asset);
            return asset;
        }

        public async Task<List<Asset>> GetAllAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<Asset?> GetByIdAsync(int id)
        {
            return await _repo.GetByIdAsync(id);
        }

        public async Task<Asset?> GetBySymbolAsync(string symbol)
        {
            return await _repo.GetBySymbolAsync(symbol);
        }

        public async Task UpdatePriceAsync(int id, decimal newPrice)
        {
            var asset = await _repo.GetByIdAsync(id);
            if (asset == null) throw new Exception(Messages.AssetNotFound);
            asset.CurrentPrice = newPrice;
            // opcional: adicionar PriceHistory
            asset.PriceHistory.Add(new PriceHistory { Date = DateTime.UtcNow, Price = newPrice, AssetId = asset.Id });
            await _repo.UpdateAsync(asset);
        }
    }
}
