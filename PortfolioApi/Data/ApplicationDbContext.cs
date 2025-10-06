using Microsoft.EntityFrameworkCore;
using PortfolioApi.Models;

namespace PortfolioApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> opts) : base(opts) { }

        public DbSet<Asset> Assets => Set<Asset>();
        public DbSet<PriceHistory> PriceHistories => Set<PriceHistory>();
        public DbSet<Portfolio> Portfolios => Set<Portfolio>();
        public DbSet<Position> Positions => Set<Position>();
        public DbSet<Transaction> Transactions => Set<Transaction>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Asset>()
                .HasMany(a => a.PriceHistory)
                .WithOne(ph => ph.Asset!)
                .HasForeignKey(ph => ph.AssetId);

            modelBuilder.Entity<Portfolio>()
                .HasMany(p => p.Positions)
                .WithOne(pos => pos.Portfolio!)
                .HasForeignKey(pos => pos.PortfolioId);

            modelBuilder.Entity<Portfolio>()
                .HasMany(p => p.Transactions)
                .WithOne(t => t.Portfolio!)
                .HasForeignKey(t => t.PortfolioId);
        }
    }
}
