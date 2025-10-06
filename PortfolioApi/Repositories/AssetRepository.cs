using Microsoft.EntityFrameworkCore;
using PortfolioApi.Data;
using PortfolioApi.Models;

namespace PortfolioApi.Repositories
{
    public class AssetRepository : IAssetRepository
    {
        private readonly ApplicationDbContext _ctx;
        public AssetRepository(ApplicationDbContext ctx) { _ctx = ctx; }

        public async Task AddAsync(Asset asset)
        {
            _ctx.Assets.Add(asset);
            await _ctx.SaveChangesAsync();
        }

        public async Task<List<Asset>> GetAllAsync()
        {
            return await _ctx.Assets.Include(a => a.PriceHistory).ToListAsync();
        }

        public async Task<Asset?> GetByIdAsync(int id)
        {
            return await _ctx.Assets.Include(a => a.PriceHistory).FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Asset?> GetBySymbolAsync(string symbol)
        {
            return await _ctx.Assets.Include(a => a.PriceHistory).FirstOrDefaultAsync(a => a.Symbol == symbol);
        }

        public async Task UpdateAsync(Asset asset)
        {
            _ctx.Assets.Update(asset);
            await _ctx.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _ctx.SaveChangesAsync();
        }
    }
}
