using PortfolioApi.Models;

namespace PortfolioApi.Services
{
    public interface IAssetService
    {
        Task<List<Asset>> GetAllAsync();
        Task<Asset?> GetByIdAsync(int id);
        Task<Asset?> GetBySymbolAsync(string symbol);
        Task<Asset> CreateAsync(Asset asset);
        Task UpdatePriceAsync(int id, decimal newPrice);
    }
}
