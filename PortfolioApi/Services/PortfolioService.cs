using PortfolioApi.Constants;
using PortfolioApi.Models;
using PortfolioApi.Repositories;
using PortfolioApi.Data;

namespace PortfolioApi.Services
{
    public class PortfolioService : IPortfolioService
    {
        private readonly IPortfolioRepository _repo;
        private readonly ApplicationDbContext _ctx;

        public PortfolioService(IPortfolioRepository repo, ApplicationDbContext ctx)
        {
            _repo = repo;
            _ctx = ctx;
        }

        public async Task<Portfolio> CreateAsync(Portfolio pf)
        {
            await _repo.AddAsync(pf);
            return pf;
        }

        public async Task<Portfolio?> GetByIdAsync(int id)
        {
            return await _repo.GetByIdAsync(id);
        }

        public async Task<List<Portfolio>> GetByUserIdAsync(string userId)
        {
            return await _repo.GetByUserIdAsync(userId);
        }

        public async Task AddPositionAsync(int portfolioId, int assetId, decimal quantity, decimal averagePrice)
        {
            var pf = await _repo.GetByIdAsync(portfolioId);
            if (pf == null) throw new Exception(Messages.PortfolioNotFound);
            var pos = new Position { PortfolioId = portfolioId, AssetId = assetId, Quantity = quantity, AveragePrice = averagePrice };
            _ctx.Positions.Add(pos);
            await _ctx.SaveChangesAsync();
        }

        public async Task UpdatePositionAsync(int portfolioId, int positionId, decimal quantity, decimal averagePrice)
        {
            var pf = await _repo.GetByIdAsync(portfolioId) ?? throw new Exception(Messages.PortfolioNotFound);
            var pos = pf.Positions.FirstOrDefault(p => p.Id == positionId) ?? throw new Exception(Messages.PositionNotFound);
            pos.Quantity = quantity;
            pos.AveragePrice = averagePrice;
            await _repo.UpdateAsync(pf);
        }

        public async Task RemovePositionAsync(int portfolioId, int positionId)
        {
            var pf = await _repo.GetByIdAsync(portfolioId) ?? throw new Exception(Messages.PortfolioNotFound);
            var pos = pf.Positions.FirstOrDefault(p => p.Id == positionId) ?? throw new Exception(Messages.PositionNotFound);
            _ctx.Positions.Remove(pos);
            await _ctx.SaveChangesAsync();
        }
    }
}
