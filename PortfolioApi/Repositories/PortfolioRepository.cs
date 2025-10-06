using Microsoft.EntityFrameworkCore;
using PortfolioApi.Data;
using PortfolioApi.Models;

namespace PortfolioApi.Repositories
{
    public class PortfolioRepository : IPortfolioRepository
    {
        private readonly ApplicationDbContext _ctx;
        public PortfolioRepository(ApplicationDbContext ctx) { _ctx = ctx; }

        public async Task AddAsync(Portfolio pf)
        {
            _ctx.Portfolios.Add(pf);
            await _ctx.SaveChangesAsync();
        }

        public async Task<Portfolio?> GetByIdAsync(int id)
        {
            return await _ctx.Portfolios
                .Include(p => p.Positions)
                    .ThenInclude(pos => pos.Asset)
                .Include(p => p.Transactions)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Portfolio>> GetByUserIdAsync(string userId)
        {
            return await _ctx.Portfolios
                .Where(p => p.UserId == userId)
                .Include(p => p.Positions)
                    .ThenInclude(pos => pos.Asset)
                .ToListAsync();
        }

        public async Task UpdateAsync(Portfolio pf)
        {
            _ctx.Portfolios.Update(pf);
            await _ctx.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _ctx.SaveChangesAsync();
        }
    }
}
