using PortfolioApi.Models;

namespace PortfolioApi.Services
{
    public interface IPortfolioService
    {
        Task<List<Portfolio>> GetByUserIdAsync(string userId);
        Task<Portfolio?> GetByIdAsync(int id);
        Task<Portfolio> CreateAsync(Portfolio pf);
        Task AddPositionAsync(int portfolioId, int assetId, decimal quantity, decimal averagePrice);
        Task UpdatePositionAsync(int portfolioId, int positionId, decimal quantity, decimal averagePrice);
        Task RemovePositionAsync(int portfolioId, int positionId);
    }
}
