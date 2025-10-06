using PortfolioApi.Models;

namespace PortfolioApi.Repositories
{
    public interface IPortfolioRepository
    {
        Task<List<Portfolio>> GetByUserIdAsync(string userId);
        Task<Portfolio?> GetByIdAsync(int id);
        Task AddAsync(Portfolio pf);
        Task UpdateAsync(Portfolio pf);
        Task SaveChangesAsync();
    }
}
