using PortfolioApi.Models;

namespace PortfolioApi.Repositories
{
    public interface IAssetRepository
    {
        Task<List<Asset>> GetAllAsync();
        Task<Asset?> GetByIdAsync(int id);
        Task<Asset?> GetBySymbolAsync(string symbol);
        Task AddAsync(Asset asset);
        Task UpdateAsync(Asset asset);
        Task SaveChangesAsync();
    }
}
